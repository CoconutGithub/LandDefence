using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// (추가) 둔화 효과의 정보(강도, 지속시간)를 담는 클래스
public class SlowEffect
{
    public float Amount;
    public float Duration;
}

public class EnemyMovement : MonoBehaviour
{
    [Header("이동 관련")]
    [SerializeField]
    private float moveSpeed = 2f;
    [SerializeField]
    private float knockbackSpeed = 8f;

    [Header("전투 관련")]
    [SerializeField]
    private float attackDamage = 15f;
    [SerializeField]
    private float timeBetweenAttacks = 1.5f;
    // (추가) 이 적이 병사/영웅에게 막히는지 여부 (true = 막힘, false = 통과)
    [SerializeField]
    private bool canBeBlocked = true;

    [Header("광역 공격 (선택 사항)")]
    // (추가) 광역 공격을 하는 적인지 여부
    [SerializeField]
    private bool isAreaOfEffect = false;
    // (추가) 광역 공격의 범위
    [SerializeField]
    private float aoeRadius = 1.5f;
    // (추가) 공격할 아군을 식별하기 위한 레이어
    [SerializeField]
    private LayerMask friendlyLayer;


    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private float attackCountdown = 0f;

    private List<SoldierController> blockingSoldiers = new List<SoldierController>();
    private HeroController blockingHero;
    
    private float originalSpeed;
    private List<SlowEffect> activeSlows = new List<SlowEffect>();
    
    private bool isRooted = false;
    private float rootTimer = 0f;

    private bool isBeingKnockedBack = false;

    void Start()
    {
        originalSpeed = moveSpeed;
    }

    public int GetCurrentWaypointIndex()
    {
        return currentWaypointIndex;
    }

    public void SetWaypointHolder(Transform _waypointHolder)
    {
        waypoints = new Transform[_waypointHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = _waypointHolder.GetChild(i);
        }
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
    }

    void Update()
    {
        if (isBeingKnockedBack) return;
        
        if (isRooted)
        {
            rootTimer -= Time.deltaTime;
            if (rootTimer <= 0)
            {
                isRooted = false;
            }
            return;
        }

        HandleEffects();
        
        // (수정) canBeBlocked가 false이거나, 막는 유닛이 없으면 이동하고, 아니면 공격합니다.
        if (!canBeBlocked || (blockingSoldiers.Count == 0 && blockingHero == null))
        {
            Move();
        }
        else
        {
            Attack();
        }
    }

    private void HandleEffects()
    {
        if (activeSlows.Count > 0)
        {
            for (int i = activeSlows.Count - 1; i >= 0; i--)
            {
                activeSlows[i].Duration -= Time.deltaTime;
                if (activeSlows[i].Duration <= 0)
                {
                    activeSlows.RemoveAt(i);
                }
            }
        }

        if (activeSlows.Count > 0)
        {
            float maxSlowAmount = 0f;
            foreach (var slow in activeSlows)
            {
                if (slow.Amount > maxSlowAmount)
                {
                    maxSlowAmount = slow.Amount;
                }
            }
            moveSpeed = originalSpeed * (1 - maxSlowAmount);
        }
        else
        {
            moveSpeed = originalSpeed;
        }
    }

    public void ApplySlow(float slowAmount, float duration)
    {
        var existingSlow = activeSlows.Find(s => s.Amount == slowAmount);
        if (existingSlow != null)
        {
            existingSlow.Duration = Mathf.Max(existingSlow.Duration, duration);
        }
        else
        {
            activeSlows.Add(new SlowEffect { Amount = slowAmount, Duration = duration });
        }
    }

    public void ApplyRoot(float duration)
    {
        isRooted = true;
        rootTimer = duration;
    }

    public void Knockback(float distance)
    {
        if (isBeingKnockedBack || waypoints == null || waypoints.Length == 0) return;
        StartCoroutine(KnockbackCoroutine(distance));
    }

    private IEnumerator KnockbackCoroutine(float distance)
    {
        isBeingKnockedBack = true;
        
        if (blockingHero != null)
        {
            blockingHero.ResumeMovement();
        }
        
        foreach(var soldier in new List<SoldierController>(blockingSoldiers))
        {
            if(soldier != null)
            {
                soldier.ReleaseEnemyBeforeDeath();
            }
        }

        Vector3 targetPosition = transform.position;
        float distanceToGo = distance;

        while (distanceToGo > 0)
        {
            if (currentWaypointIndex <= 0)
            {
                currentWaypointIndex = 0;
                transform.position = waypoints[0].position;
                break;
            }

            Vector3 previousWaypoint = waypoints[currentWaypointIndex - 1].position;
            float distanceToPrevious = Vector3.Distance(transform.position, previousWaypoint);

            if (distanceToGo >= distanceToPrevious)
            {
                targetPosition = previousWaypoint;
                distanceToGo -= distanceToPrevious;
                currentWaypointIndex--;
            }
            else
            {
                Vector3 direction = (previousWaypoint - transform.position).normalized;
                targetPosition = transform.position + direction * distanceToGo;
                distanceToGo = 0;
            }

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, knockbackSpeed * Time.deltaTime);
                yield return null;
            }
        }

        transform.position = targetPosition;
        isBeingKnockedBack = false;
    }

    // (수정) 광역 공격과 단일 공격 로직을 분리합니다.
    void Attack()
    {
        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            // isAreaOfEffect가 true이면 광역 공격을 수행합니다.
            if (isAreaOfEffect)
            {
                // 지정된 범위 내의 friendlyLayer에 속한 모든 콜라이더를 찾습니다.
                Collider2D[] friendlies = Physics2D.OverlapCircleAll(transform.position, aoeRadius, friendlyLayer);
                foreach (var friendlyCollider in friendlies)
                {
                    // 찾은 콜라이더가 병사 컴포넌트를 가지고 있다면 피해를 줍니다.
                    SoldierController soldier = friendlyCollider.GetComponent<SoldierController>();
                    if (soldier != null)
                    {
                        soldier.TakeDamage(attackDamage, this);
                    }

                    // 찾은 콜라이더가 영웅 컴포넌트를 가지고 있다면 피해를 줍니다.
                    HeroController hero = friendlyCollider.GetComponent<HeroController>();
                    if (hero != null)
                    {
                        hero.TakeDamage(attackDamage);
                    }
                }
            }
            else // isAreaOfEffect가 false이면 기존의 단일 대상 공격을 수행합니다.
            {
                if (blockingSoldiers.Count > 0 && blockingSoldiers[0] != null)
                {
                    blockingSoldiers[0].TakeDamage(attackDamage, this);
                }
                else if (blockingHero != null)
                {
                    blockingHero.TakeDamage(attackDamage);
                }
            }
            
            attackCountdown = timeBetweenAttacks;
        }
    }
    
    void Move()
    {
        if (currentWaypointIndex >= waypoints.Length || isRooted)
        {
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                EnemyReachedEnd();
            }
        }
    }

    void EnemyReachedEnd()
    {
        GameManager.instance.EnemyReachedEnd();
        GameManager.instance.EnemyDefeated();
        Destroy(gameObject);
    }
    
    public void BlockMovement(SoldierController soldier, HeroController hero)
    {
        // (추가) canBeBlocked가 false이면, 아예 막히지 않고 함수를 즉시 종료합니다.
        if (!canBeBlocked) return;

        if (isBeingKnockedBack) return;

        if (soldier != null)
        {
            if (!blockingSoldiers.Contains(soldier))
            {
                blockingSoldiers.Add(soldier);
            }
        }
        else if (hero != null)
        {
            blockingHero = hero;
        }
    }

    public void UnblockBySoldier(SoldierController soldier)
    {
        if (blockingSoldiers.Contains(soldier))
        {
            blockingSoldiers.Remove(soldier);
        }
    }
    
    public void ResumeMovement()
    {
        blockingHero = null;
    }
    
    public int GetBlockerCount()
    {
        int count = blockingSoldiers.Count;
        if (blockingHero != null)
        {
            count++;
        }
        return count;
    }

    public bool IsBlocked()
    {
        return blockingSoldiers.Count > 0 || blockingHero != null;
    }

    // (추가) 광역 공격 범위를 에디터에서 시각적으로 확인할 수 있도록 Gizmo를 그립니다.
    private void OnDrawGizmosSelected()
    {
        if (isAreaOfEffect)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}
