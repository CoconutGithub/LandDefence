using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    [SerializeField]
    private bool isAreaOfEffect = false;
    [SerializeField]
    private float aoeRadius = 1.5f;
    [SerializeField]
    private LayerMask friendlyLayer;
    [SerializeField]
    private AnimationClip attackAnimationClip;

    [Header("특성")]
    [SerializeField]
    private bool canBeBlocked = true;


    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private float attackCountdown = 0f;

    private List<SoldierController> blockingSoldiers = new List<SoldierController>();
    private HeroController blockingHero;
    private HeroCloneController blockingClone; // (추가) 분신을 별도로 저장할 변수

    private float originalSpeed;
    private List<SlowEffect> activeSlows = new List<SlowEffect>();

    private bool isRooted = false;
    private float rootTimer = 0f;

    private bool isBeingKnockedBack = false;

    private AnimationController animationController;
    private Vector3 originalScale;
    private float attackAnimLength; 

    void Start()
    {
        originalSpeed = moveSpeed;
        animationController = GetComponent<AnimationController>();
        originalScale = transform.localScale;
        
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
            if (animationController != null) animationController.SetAnimationBool("IsMoving", false);
            return;
        }

        HandleEffects();
        
        // (수정) isFighting 조건에 blockingClone 추가
        bool isFighting = blockingSoldiers.Count > 0 || blockingHero != null || blockingClone != null;
        if (animationController != null)
        {
            animationController.SetAnimationBool("IsMoving", !isFighting && !isRooted && !isBeingKnockedBack);
        }

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

    private void HandleSpriteDirection(bool isFighting)
    {
        Transform currentTargetTransform = null;

        if (isFighting)
        {
            if (blockingHero != null)
            {
                currentTargetTransform = blockingHero.transform;
            }
            // (추가) 분신을 바라보도록 설정
            else if (blockingClone != null)
            {
                currentTargetTransform = blockingClone.transform;
            }
            else if (blockingSoldiers.Count > 0 && blockingSoldiers[0] != null)
            {
                currentTargetTransform = blockingSoldiers[0].transform;
            }
        }
        else if (waypoints != null && currentWaypointIndex < waypoints.Length)
        {
            currentTargetTransform = waypoints[currentWaypointIndex];
        }

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

        if (blockingHero != null) { blockingHero.ResumeMovement(); }
        if (blockingClone != null) { blockingClone.ResumeFromBlock(); } // (추가) 분신에게도 알려줌

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

    void Attack()
    {
        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            if (animationController != null)
            {
                if (attackAnimLength > 0 && timeBetweenAttacks > 0)
                {
                    float speedMultiplier = attackAnimLength / timeBetweenAttacks;
                    animationController.SetAnimationSpeed(speedMultiplier);
                }
                else
                {
                    animationController.SetAnimationSpeed(1f);
                }
                
                animationController.PlayAttackAnimation();
            }
            else
            {
                DealDamage();
            }
            
            attackCountdown = timeBetweenAttacks;
        }
    }

    public void DealDamage()
    {
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
        else
        {
            if (blockingSoldiers.Count > 0 && blockingSoldiers[0] != null)
            {
                blockingSoldiers[0].TakeDamage(attackDamage, this);
            }
            else if (blockingHero != null)
            {
                blockingHero.TakeDamage(attackDamage);
            }
            // (핵심 수정) 적이 분신을 공격하도록 로직을 추가합니다.
            else if (blockingClone != null)
            {
                blockingClone.TakeDamage(attackDamage);
            }
        }
    }

    void Move()
    {
        // NullReferenceException 방지를 위한 안전장치
        if (waypoints == null || currentWaypointIndex >= waypoints.Length)
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

    // (추가) 분신이 길을 막을 때 호출할 새로운 함수
    public void BlockMovementByClone(HeroCloneController clone)
    {
        if (!canBeBlocked || isBeingKnockedBack) return;
        blockingClone = clone;
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
        blockingClone = null; // (추가) 분신 참조도 초기화
    }

    public int GetBlockerCount()
    {
        int count = blockingSoldiers.Count;
        if (blockingHero != null) { count++; }
        if (blockingClone != null) { count++; }
        return count;
    }

    public bool IsBlocked()
    {
        return blockingSoldiers.Count > 0 || blockingHero != null || blockingClone != null;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (isAreaOfEffect)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}

