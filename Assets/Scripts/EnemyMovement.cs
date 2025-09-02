using UnityEngine;
using System.Collections.Generic;
using System.Collections; // (추가) 코루틴 사용을 위해 추가

public class EnemyMovement : MonoBehaviour
{
    [Header("이동 관련")]
    [SerializeField]
    private float moveSpeed = 2f;
    // (추가) 넉백 효과가 적용될 때의 이동 속도
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
    private bool isSlowed = false;
    private float slowTimer = 0f;

    private bool isRooted = false;
    private float rootTimer = 0f;

    // (추가) 넉백 상태를 관리하기 위한 플래그
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
        transform.position = waypoints[currentWaypointIndex].position;
    }

    void Update()
    {
        // (추가) 넉백, 속박 상태일 때는 다른 행동을 하지 않도록 최상단에서 확인합니다.
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

    private void HandleEffects()
    {
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                moveSpeed = originalSpeed;
                isSlowed = false;
            }
        }
    }

    public void ApplySlow(float slowAmount, float duration)
    {
        moveSpeed = originalSpeed * (1 - slowAmount);
        isSlowed = true;
        slowTimer = duration;
    }

    public void ApplyRoot(float duration)
    {
        isRooted = true;
        rootTimer = duration;
    }

    // (수정) 넉백 로직을 코루틴으로 변경하여 부드러운 이동을 구현합니다.
    public void Knockback(float distance)
    {
        if (isBeingKnockedBack || waypoints == null || waypoints.Length == 0) return;
        StartCoroutine(KnockbackCoroutine(distance));
    }

    private IEnumerator KnockbackCoroutine(float distance)
    {
        isBeingKnockedBack = true;
        
        // 넉백 시작 시, 현재 자신을 막고 있는 유닛들을 해제합니다.
        if (blockingHero != null)
        {
            blockingHero.ResumeMovement(); // 영웅은 스스로 타겟을 다시 찾을 것입니다.
        }
        foreach(var soldier in blockingSoldiers)
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
                // 시작 지점보다 더 뒤로 갈 수 없으면 넉백을 중단합니다.
                currentWaypointIndex = 0;
                transform.position = waypoints[0].position;
                break;
            }

            Vector3 previousWaypoint = waypoints[currentWaypointIndex - 1].position;
            float distanceToPrevious = Vector3.Distance(transform.position, previousWaypoint);

            if (distanceToGo > distanceToPrevious)
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

            // 계산된 목표 지점까지 부드럽게 이동합니다.
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, knockbackSpeed * Time.deltaTime);
                yield return null;
            }
        }

        transform.position = targetPosition; // 정확한 위치로 보정
        isBeingKnockedBack = false;
    }

    void Attack()
    {
        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            if (blockingSoldiers.Count > 0 && blockingSoldiers[0] != null)
            {
                blockingSoldiers[0].TakeDamage(attackDamage);
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
        if (isBeingKnockedBack) return; // 넉백 중에는 막히지 않음

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

