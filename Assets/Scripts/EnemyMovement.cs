using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    [Header("이동 관련")]
    [SerializeField]
    private float moveSpeed = 2f;

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

    void Start()
    {
        originalSpeed = moveSpeed;
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
        if (currentWaypointIndex >= waypoints.Length) return;

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
    
    // (수정) SoldierController와 HeroController의 호출을 모두 처리하는 단일 BlockMovement 함수입니다.
    public void BlockMovement(SoldierController soldier, HeroController hero)
    {
        if (soldier != null)
        {
            // 병사가 호출한 경우
            if (!blockingSoldiers.Contains(soldier))
            {
                blockingSoldiers.Add(soldier);
            }
        }
        else if (hero != null)
        {
            // 영웅이 호출한 경우
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

