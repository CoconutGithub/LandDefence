//EnemyMovement.cs
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

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private float attackCountdown = 0f;

    private List<SoldierController> blockingSoldiers = new List<SoldierController>();
    private HeroController blockingHero;
    
    private float originalSpeed;
    // (수정) 단일 둔화 변수 대신, 여러 둔화 효과를 관리할 리스트를 사용합니다.
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
        
        if (blockingSoldiers.Count == 0 && blockingHero == null)
        {
            Move();
        }
        else
        {
            Attack();
        }
    }

    // (수정) 여러 둔화 효과를 관리하고, 가장 강력한 효과를 적용하도록 로직을 변경합니다.
    private void HandleEffects()
    {
        if (activeSlows.Count > 0)
        {
            // 만료된 둔화 효과를 리스트에서 제거합니다.
            for (int i = activeSlows.Count - 1; i >= 0; i--)
            {
                activeSlows[i].Duration -= Time.deltaTime;
                if (activeSlows[i].Duration <= 0)
                {
                    activeSlows.RemoveAt(i);
                }
            }
        }

        // 남아있는 둔화 효과 중 가장 강력한 것을 찾아 적용합니다.
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
            // 둔화 효과가 없으면 원래 속도로 되돌립니다.
            moveSpeed = originalSpeed;
        }
    }

    // (수정) 둔화 효과를 리스트에 추가하도록 로직을 변경합니다.
    public void ApplySlow(float slowAmount, float duration)
    {
        // 이미 같은 강도의 둔화 효과가 있다면, 지속시간을 더 긴 쪽으로 갱신합니다.
        var existingSlow = activeSlows.Find(s => s.Amount == slowAmount);
        if (existingSlow != null)
        {
            existingSlow.Duration = Mathf.Max(existingSlow.Duration, duration);
        }
        else // 없다면 새로 추가합니다.
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

    void Attack()
    {
        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            if (blockingSoldiers.Count > 0 && blockingSoldiers[0] != null)
            {
                blockingSoldiers[0].TakeDamage(attackDamage, this);
            }
            else if (blockingHero != null)
            {
                blockingHero.TakeDamage(attackDamage);
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
}

