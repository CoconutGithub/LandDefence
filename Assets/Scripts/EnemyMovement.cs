using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("이동 능력치")]
    [SerializeField]
    private float moveSpeed = 2f;

    // (추가) 적의 공격 능력치입니다.
    [Header("공격 능력치")]
    [SerializeField]
    private float attackDamage = 15f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;

    private Transform waypointHolder;
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private float originalSpeed;
    private bool isBlockedBySoldier = false;
    private SoldierController blockingSoldier;
    private float attackCountdown = 0f; // (추가) 공격 쿨타임을 계산하기 위한 변수

    void Start()
    {
        originalSpeed = moveSpeed;
    }

    void Update()
    {
        attackCountdown -= Time.deltaTime;

        // (수정) 병사에게 막혔을 때의 행동을 추가합니다.
        if (isBlockedBySoldier)
        {
            // 나를 막던 병사가 죽었는지 확인합니다.
            if (blockingSoldier == null)
            {
                ResumeMovement();
                return; // 즉시 이동을 재개하도록 함수를 종료합니다.
            }

            // 공격할 준비가 되었다면 공격합니다.
            if (attackCountdown <= 0f)
            {
                Attack();
                attackCountdown = timeBetweenAttacks;
            }
            return; // 공격 중에는 이동하지 않도록 함수를 여기서 종료합니다.
        }

        // (이하 이동 로직은 동일)
        if (waypoints == null || currentWaypointIndex >= waypoints.Length)
        {
            // 길 끝에 도달했는지 확인
            if (waypoints != null && currentWaypointIndex >= waypoints.Length)
            {
                GameManager.instance.EnemyReachedEnd();
                GameManager.instance.EnemyDefeated();
                Destroy(gameObject);
            }
            return;
        }

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            currentWaypointIndex++;
        }
    }

    // (추가) 병사를 공격하는 함수입니다.
    void Attack()
    {
        if (blockingSoldier != null)
        {
            blockingSoldier.TakeDamage(attackDamage);
        }
    }
    
    public void ApplySlow(float amount, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SlowDown(amount, duration));
    }

    IEnumerator SlowDown(float amount, float duration)
    {
        moveSpeed = originalSpeed * (1 - amount);
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
    }
    
    public void BlockMovement(SoldierController soldier)
    {
        isBlockedBySoldier = true;
        blockingSoldier = soldier;
    }
    
    public void ResumeMovement()
    {
        isBlockedBySoldier = false;
        blockingSoldier = null;
    }
    
    public bool IsBlocked()
    {
        return isBlockedBySoldier;
    }

    public void SetWaypointHolder(Transform _waypointHolder)
    {
        waypointHolder = _waypointHolder;
        waypoints = new Transform[waypointHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = waypointHolder.GetChild(i);
        }
        transform.position = waypoints[currentWaypointIndex].position;
    }
}
