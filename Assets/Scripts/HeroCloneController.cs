using UnityEngine;
using UnityEngine.UI;

// (수정) 병사처럼 상태 기반 AI를 가지도록 대폭 수정된 영웅 분신 컨트롤러입니다.
public class HeroCloneController : MonoBehaviour
{
    // (수정) 본체에게 복귀하는 상태(ReturningToHero)를 추가하여 행동을 명확히 분리합니다.
    private enum CloneState { FollowingHero, ReturningToHero, ChasingEnemy, Fighting }
    private CloneState currentState;

    [Header("분신 능력치")]
    [SerializeField]
    private float maxHealth = 300f;
    [SerializeField]
    private float attackDamage = 30f;
    [SerializeField]
    private float timeBetweenAttacks = 0.5f;
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float attackRange = 1f;
    [SerializeField]
    private float recognitionRadius = 3f;
    [SerializeField]
    private float leashDistance = 3f;
    [SerializeField]
    private float returnCompleteDistance = 1f; // (추가) 이 거리 안으로 들어와야 복귀 완료로 인정됩니다.

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private Slider healthBarSlider;
    [SerializeField]
    private GameObject healthBarCanvas;
    [SerializeField]
    private LayerMask enemyLayer;

    private float currentHealth;
    private float attackCountdown = 0f;
    private EnemyMovement currentTarget;
    private Transform heroTransform;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = 1f;
            healthBarSlider.value = 1f;
        }
        currentState = CloneState.FollowingHero;
    }

    public void Setup(Transform _heroTransform)
    {
        heroTransform = _heroTransform;
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null && healthBarCanvas.activeInHierarchy)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    void Update()
    {
        if (heroTransform == null) return;

        // (수정) 본체와 너무 멀어지면, 현재 상태가 '복귀중'이 아닐 경우에만 '복귀' 상태로 강제 전환합니다.
        if (Vector3.Distance(transform.position, heroTransform.position) > leashDistance && currentState != CloneState.ReturningToHero)
        {
            currentTarget = null;
            currentState = CloneState.ReturningToHero;
        }
        
        switch (currentState)
        {
            case CloneState.FollowingHero:
                // '따라가기'(대기) 상태일 때만 적을 탐색합니다.
                FindEnemyToChase();
                break;
            case CloneState.ReturningToHero:
                // (수정) '복귀' 상태일 때는 적을 탐색하지 않고, 본체와의 거리만 확인합니다.
                if (Vector3.Distance(transform.position, heroTransform.position) < returnCompleteDistance)
                {
                    // 충분히 가까워지면 '따라가기'(대기) 상태로 전환하여 다시 적을 찾기 시작합니다.
                    currentState = CloneState.FollowingHero;
                }
                break;
            case CloneState.ChasingEnemy:
                ChaseTarget();
                break;
            case CloneState.Fighting:
                FightTarget();
                break;
        }
    }
    
    void FixedUpdate()
    {
        // (수정) 상태에 따라 이동 로직을 명확히 구분합니다.
        // 기본적으로는 멈춰있도록 설정합니다.
        rb.linearVelocity = Vector2.zero; 

        switch (currentState)
        {
            case CloneState.ReturningToHero:
                MoveTowards(heroTransform.position);
                break;
            case CloneState.ChasingEnemy:
                if (currentTarget != null) MoveTowards(currentTarget.transform.position);
                break;
            // 'FollowingHero'와 'Fighting' 상태에서는 물리적인 이동을 하지 않습니다.
        }
    }
    
    // 목표 지점을 향해 이동하는 함수입니다.
    void MoveTowards(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    void FindEnemyToChase()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, recognitionRadius, enemyLayer);
        EnemyMovement closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            EnemyMovement enemy = hitCollider.GetComponent<EnemyMovement>();
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        if (closestEnemy != null)
        {
            currentTarget = closestEnemy;
            currentState = CloneState.ChasingEnemy;
        }
    }

    void ChaseTarget()
    {
        if (currentTarget == null)
        {
            currentState = CloneState.FollowingHero;
            return;
        }
        
        if (Vector3.Distance(transform.position, currentTarget.transform.position) <= attackRange)
        {
            currentState = CloneState.Fighting;
        }
    }

    void FightTarget()
    {
        attackCountdown -= Time.deltaTime;
        if (currentTarget == null)
        {
            currentState = CloneState.FollowingHero;
            return;
        }
        
        if(Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange)
        {
            currentState = CloneState.ChasingEnemy;
            return;
        }

        if (attackCountdown <= 0f)
        {
            Attack();
            attackCountdown = timeBetweenAttacks;
        }
    }

    void Attack()
    {
        if (currentTarget != null)
        {
            currentTarget.GetComponent<EnemyHealth>().TakeDamage(attackDamage, TowerType.Hero, DamageType.Physical);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, recognitionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if(heroTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(heroTransform.position, leashDistance);
        }
    }
}

