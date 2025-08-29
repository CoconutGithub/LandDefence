using System.Collections;
using System.Collections.Generic; // (추가) List를 사용하기 위해 추가합니다.
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("이동 능력치")]
    [SerializeField]
    private float moveSpeed = 2f;

    [Header("공격 능력치")]
    [SerializeField]
    private float attackDamage = 15f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;

    private Transform waypointHolder;
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private float originalSpeed;
    private bool isBlocked = false;
    
    // (수정) 이제 여러 명의 병사가 이 적을 막을 수 있습니다.
    private List<SoldierController> blockingSoldiers = new List<SoldierController>();
    private HeroController blockingHero;
    private float attackCountdown = 0f;

    void Start()
    {
        originalSpeed = moveSpeed;
    }

    void Update()
    {
        attackCountdown -= Time.deltaTime;

        if (isBlocked)
        {
            // 나를 막던 유닛들이 모두 죽었는지 확인합니다.
            blockingSoldiers.RemoveAll(item => item == null); // 목록에서 죽은 병사들을 제거합니다.
            if (blockingHero != null && !blockingHero.gameObject.activeInHierarchy)
            {
                blockingHero = null;
            }

            // 막고 있는 유닛이 아무도 없다면 이동을 재개합니다.
            if (blockingSoldiers.Count == 0 && blockingHero == null)
            {
                ResumeMovement();
                return;
            }

            if (attackCountdown <= 0f)
            {
                Attack();
                attackCountdown = timeBetweenAttacks;
            }
            return;
        }

        // (이하 이동 로직은 동일)
        if (waypoints == null || currentWaypointIndex >= waypoints.Length)
        {
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

    void Attack()
    {
        // (수정) 이제 영웅이나 병사들 중 하나를 공격합니다.
        if (blockingHero != null)
        {
            blockingHero.TakeDamage(attackDamage);
        }
        else if (blockingSoldiers.Count > 0 && blockingSoldiers[0] != null)
        {
            // 가장 먼저 막아선 병사를 공격합니다.
            blockingSoldiers[0].TakeDamage(attackDamage);
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
    
    // (수정) 병사 또는 영웅이 이 함수를 호출하여 이동을 멈춥니다.
    public void BlockMovement(SoldierController soldier, HeroController hero)
    {
        isBlocked = true;
        if (soldier != null && !blockingSoldiers.Contains(soldier))
        {
            blockingSoldiers.Add(soldier);
        }
        if (hero != null)
        {
            blockingHero = hero;
        }
    }
    
    // (수정) 병사가 전투를 해제할 때 호출됩니다.
    public void UnblockBySoldier(SoldierController soldier)
    {
        if (blockingSoldiers.Contains(soldier))
        {
            blockingSoldiers.Remove(soldier);
        }
    }

    public void ResumeMovement()
    {
        isBlocked = false;
        blockingSoldiers.Clear();
        blockingHero = null;
    }
    
    public bool IsBlocked()
    {
        // (수정) 이제 자신을 막고 있는 유닛이 있는지 확인합니다.
        return isBlocked;
    }

    // (추가) 현재 이 적을 막고 있는 병사의 수를 반환합니다.
    public int GetBlockerCount()
    {
        return blockingSoldiers.Count;
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
