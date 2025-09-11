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
    // (추가) 광역 공격 옵션
    [SerializeField]
    private bool isAreaOfEffect = false;
    [SerializeField]
    private float aoeRadius = 1.5f;
    [SerializeField]
    private LayerMask friendlyLayer; // (수정) 이제 여러 레이어를 선택할 수 있습니다.
    [SerializeField]
    private AnimationClip attackAnimationClip; // (추가) 공격 애니메이션 클립 연결

    // (추가) 충돌 비활성화 옵션
    [Header("특성")]
    [SerializeField]
    private bool canBeBlocked = true;


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

    // (수정) 애니메이션 제어를 위한 변수들
    private AnimationController animationController;
    private Vector3 originalScale;
    private float attackAnimLength; 

    void Start()
    {
        originalSpeed = moveSpeed;
        // (수정) 애니메이션 관련 변수 초기화
        animationController = GetComponent<AnimationController>();
        originalScale = transform.localScale;
        
        // (추가) 공격 애니메이션 길이 가져오기
        if (attackAnimationClip != null)
        {
            attackAnimLength = attackAnimationClip.length;
        }
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
            // (수정) 속박 상태일 때는 움직이지 않으므로 IsMoving을 false로 설정
            if (animationController != null) animationController.SetAnimationBool("IsMoving", false);
            return;
        }

        HandleEffects();
        
        bool isFighting = blockingSoldiers.Count > 0 || blockingHero != null;
        if (animationController != null)
        {
            animationController.SetAnimationBool("IsMoving", !isFighting && !isRooted && !isBeingKnockedBack);
        }

        // (수정) 이동 방향에 따른 스프라이트 좌우 반전 로직 호출
        HandleSpriteDirection(isFighting);

        if (isFighting)
        {
            Attack();
        }
        else
        {
            Move();
        }
    }

    // (수정) 스프라이트 방향을 제어하는 새로운 함수
    private void HandleSpriteDirection(bool isFighting)
    {
        Transform currentTargetTransform = null;

        if (isFighting)
        {
            // 싸우고 있을 때는 현재 공격 대상을 바라봅니다.
            if (blockingHero != null)
            {
                currentTargetTransform = blockingHero.transform;
            }
            else if (blockingSoldiers.Count > 0 && blockingSoldiers[0] != null)
            {
                currentTargetTransform = blockingSoldiers[0].transform;
            }
        }
        else if (waypoints != null && currentWaypointIndex < waypoints.Length)
        {
            // 이동 중일 때는 다음 웨이포인트를 바라봅니다.
            currentTargetTransform = waypoints[currentWaypointIndex];
        }

        // 목표가 있을 경우에만 방향을 계산합니다.
        if (currentTargetTransform != null)
        {
            float directionX = currentTargetTransform.position.x - transform.position.x;

            if (Mathf.Abs(directionX) > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Sign(directionX) * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
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

        foreach (var soldier in new List<SoldierController>(blockingSoldiers))
        {
            if (soldier != null)
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

    // (수정) Attack 함수는 이제 애니메이션 속도를 조절하고 재생만 담당합니다.
    void Attack()
    {
        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            if (animationController != null)
            {
                // timeBetweenAttacks에 맞춰 애니메이션 속도를 계산합니다.
                if (attackAnimLength > 0 && timeBetweenAttacks > 0)
                {
                    float speedMultiplier = attackAnimLength / timeBetweenAttacks;
                    animationController.SetAnimationSpeed(speedMultiplier);
                }
                else
                {
                    animationController.SetAnimationSpeed(1f);
                }
                
                animationController.PlayAttackAnimation(); // "DoAttack" Trigger 발동
            }
            else
            {
                // 애니메이터가 없으면 즉시 데미지를 줍니다.
                DealDamage();
            }
            
            attackCountdown = timeBetweenAttacks;
        }
    }

    // (추가) 애니메이션 이벤트에서 호출될, 실제 데미지를 주는 함수
    public void DealDamage()
    {
        // (수정) 광역 공격 로직
        if (isAreaOfEffect)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, friendlyLayer);
            foreach (var hit in hits)
            {
                if (hit.GetComponent<SoldierController>() != null)
                    hit.GetComponent<SoldierController>().TakeDamage(attackDamage, this);
                if (hit.GetComponent<HeroController>() != null)
                    hit.GetComponent<HeroController>().TakeDamage(attackDamage);
                if (hit.GetComponent<HeroCloneController>() != null)
                    hit.GetComponent<HeroCloneController>().TakeDamage(attackDamage);
            }
        }
        else // 단일 공격
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
    }

    void Move()
    {
        if (waypoints != null && currentWaypointIndex >= waypoints.Length)
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
        // (수정) canBeBlocked가 false이면 함수를 즉시 종료하여 막히지 않도록 합니다.
        if (!canBeBlocked || isBeingKnockedBack) return;

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
    
    // (추가) 광역 공격 범위를 씬 뷰에서 시각적으로 보여주기 위한 기즈모
    private void OnDrawGizmosSelected()
    {
        if (isAreaOfEffect)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}

