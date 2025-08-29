using UnityEngine;
using UnityEngine.UI;

// 병사의 모든 행동(이동, 공격, 죽음)과 능력치를 관리하는 스크립트입니다.
public class SoldierController : MonoBehaviour
{
    // (추가) 병사의 현재 행동 상태를 정의합니다.
    private enum SoldierState { ReturningToRallyPoint, IdleAtRallyPoint, ChasingEnemy, Fighting }
    private SoldierState currentState;

    [Header("능력치")]
    [SerializeField]
    private float maxHealth = 150f;
    [SerializeField]
    private float attackDamage = 10f;
    [SerializeField]
    private float timeBetweenAttacks = 1.5f;
    [SerializeField]
    private float moveSpeed = 3f;
    [SerializeField]
    private float healthRegenRate = 5f;
    [SerializeField]
    private float timeToStartRegen = 3f;
    [SerializeField]
    private DamageType damageType = DamageType.Physical;

    [Header("광역 공격 (바이킹 전용)")]
    [SerializeField]
    private bool isAreaOfEffect = false;
    [SerializeField]
    private float aoeRadius = 1.5f;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private Slider healthBarSlider;
    [SerializeField]
    private GameObject healthBarCanvas;
    [SerializeField]
    private LayerMask enemyLayer; // (추가) 적을 감지할 때 사용할 레이어 마스크

    private float currentHealth;
    private float attackCountdown = 0f;
    private EnemyMovement currentTarget;
    private Vector3 rallyPointPosition;
    private BarracksController ownerBarracks;
    private float timeSinceLastCombat = 0f;
    private CircleCollider2D recognitionCollider; // (추가) 적 인식 범위

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = 1f;
            healthBarSlider.value = 1f;
        }
        recognitionCollider = GetComponent<CircleCollider2D>();
        currentState = SoldierState.ReturningToRallyPoint; // 처음에는 집결 지점으로 이동 시작
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    void Update()
    {
        // (수정) 상태에 따라 다른 행동을 하도록 상태 머신을 사용합니다.
        switch (currentState)
        {
            case SoldierState.ReturningToRallyPoint:
                MoveToRallyPoint();
                if (Vector3.Distance(transform.position, rallyPointPosition) <= 0.1f)
                {
                    currentState = SoldierState.IdleAtRallyPoint;
                }
                break;

            case SoldierState.IdleAtRallyPoint:
                FindEnemyToChase();
                RegenerateHealthIfNeeded();
                break;

            case SoldierState.ChasingEnemy:
                if (currentTarget == null)
                {
                    currentState = SoldierState.ReturningToRallyPoint;
                    return;
                }
                // 적과의 거리가 공격 가능 거리보다 가까워지면 전투 시작
                if (Vector3.Distance(transform.position, currentTarget.transform.position) <= 1.0f) // 1.0f는 공격 사거리
                {
                    currentState = SoldierState.Fighting;
                    currentTarget.BlockMovement(this, null);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, currentTarget.transform.position, moveSpeed * Time.deltaTime);
                }
                break;

            case SoldierState.Fighting:
                attackCountdown -= Time.deltaTime;
                if (currentTarget == null)
                {
                    currentState = SoldierState.ReturningToRallyPoint;
                    return;
                }
                if (attackCountdown <= 0f)
                {
                    Attack();
                    attackCountdown = timeBetweenAttacks;
                }
                break;
        }
    }
    
    // (추가) 인식 범위 내의 적을 찾는 함수
    void FindEnemyToChase()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, recognitionCollider.radius, enemyLayer);
        EnemyMovement closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            EnemyMovement enemy = hitCollider.GetComponent<EnemyMovement>();
            // 적이 아직 3명 미만의 병사에게 막혀있을 때만 타겟으로 삼습니다.
            if (enemy != null && enemy.GetBlockerCount() < 3)
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
            currentState = SoldierState.ChasingEnemy;
        }
    }

    void RegenerateHealthIfNeeded()
    {
        timeSinceLastCombat += Time.deltaTime;
        if (timeSinceLastCombat >= timeToStartRegen && currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            if (healthBarSlider != null)
            {
                healthBarSlider.value = currentHealth / maxHealth;
            }
        }
    }

    void MoveToRallyPoint()
    {
        transform.position = Vector3.MoveTowards(transform.position, rallyPointPosition, moveSpeed * Time.deltaTime);
    }
    
    public void SetRallyPointPosition(Vector3 newPosition)
    {
        rallyPointPosition = newPosition;
        // 새로운 집결 지점을 받으면, 전투 중이 아닐 경우 즉시 복귀를 시작합니다.
        if (currentState == SoldierState.IdleAtRallyPoint)
        {
            currentState = SoldierState.ReturningToRallyPoint;
        }
    }
    
    public void SetBarracks(BarracksController barracks)
    {
        ownerBarracks = barracks;
    }

    void Attack()
    {
        if (isAreaOfEffect)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
            foreach (Collider2D hit in colliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    hit.GetComponent<EnemyHealth>().TakeDamage(attackDamage, TowerType.Barracks, damageType);
                }
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.GetComponent<EnemyHealth>().TakeDamage(attackDamage, TowerType.Barracks, damageType);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        timeSinceLastCombat = 0f;
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
        ReleaseEnemyBeforeDeath(); // (수정) 죽을 때도 적을 풀어줍니다.
        if (ownerBarracks != null)
        {
            ownerBarracks.RemoveSoldier(this);
        }
        Destroy(gameObject);
    }

    // (추가) BarracksController가 호출할 수 있도록 public으로 선언된 함수입니다.
    public void ReleaseEnemyBeforeDeath()
    {
        if (currentTarget != null)
        {
            currentTarget.UnblockBySoldier(this);
        }
    }
}
