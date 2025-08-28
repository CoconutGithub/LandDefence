using System.Collections;
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
    private SoldierController blockingSoldier;
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
            if ((blockingSoldier != null && !blockingSoldier.gameObject.activeInHierarchy) || 
                (blockingHero != null && !blockingHero.gameObject.activeInHierarchy))
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
        if (blockingSoldier != null)
        {
            blockingSoldier.TakeDamage(attackDamage);
        }
        else if (blockingHero != null)
        {
            blockingHero.TakeDamage(attackDamage);
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
    
    public void BlockMovement(SoldierController soldier, HeroController hero)
    {
        isBlocked = true;
        if (soldier != null) blockingSoldier = soldier;
        if (hero != null) blockingHero = hero;
    }
    
    public void ResumeMovement()
    {
        isBlocked = false;
        blockingSoldier = null;
        blockingHero = null;
    }
    
    public bool IsBlocked()
    {
        return isBlocked;
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
