using UnityEngine;
using UnityEngine.UI;
using System.Collections; // (추가) 코루틴 사용을 위해 추가

// 병사의 모든 행동(이동, 공격, 죽음)과 능력치를 관리하는 스크립트입니다.
public class SoldierController : MonoBehaviour
{
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
    [SerializeField]
    private float respawnTime = 10f; // (추가) 해치 전용 부활 시간

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
    private LayerMask enemyLayer;

    private float currentHealth;
    private float attackCountdown = 0f;
    private EnemyMovement currentTarget;
    private Vector3 rallyPointPosition;
    private BarracksController ownerBarracks;
    private float timeSinceLastCombat = 0f;
    private CircleCollider2D recognitionCollider;
    private bool isHaetae = false; // (추가) 이 유닛이 해치인지 구분하는 플래그
    private SpriteRenderer spriteRenderer; // (추가) 부활 시 모습을 감추기 위함

    void Awake() // (수정) Start 대신 Awake에서 컴포넌트를 미리 찾아둡니다.
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        recognitionCollider = GetComponent<CircleCollider2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = 1f;
            healthBarSlider.value = 1f;
        }
        currentState = SoldierState.ReturningToRallyPoint;
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
                if (Vector3.Distance(transform.position, currentTarget.transform.position) <= 1.0f)
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
    
    void FindEnemyToChase()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, recognitionCollider.radius, enemyLayer);
        EnemyMovement closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            EnemyMovement enemy = hitCollider.GetComponent<EnemyMovement>();
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
        if (currentState == SoldierState.IdleAtRallyPoint)
        {
            currentState = SoldierState.ReturningToRallyPoint;
        }
    }

    // (추가) 해치로 설정하고 능력치를 부여하는 함수
    public void SetupAsHaetae(float newMaxHealth, float newAttackDamage)
    {
        isHaetae = true;
        maxHealth = newMaxHealth;
        attackDamage = newAttackDamage;
        currentHealth = maxHealth;
        // 해치는 체력 회복 로직도 공유하므로, healthRegenRate도 설정할 수 있습니다 (필요 시).
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
        ReleaseEnemyBeforeDeath();
        
        // (수정) 해치일 경우와 일반 병사일 경우를 분리합니다.
        if (isHaetae)
        {
            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            if (ownerBarracks != null)
            {
                ownerBarracks.RemoveSoldier(this);
            }
            Destroy(gameObject);
        }
    }

    // (추가) 해치 전용 부활 코루틴
    IEnumerator RespawnCoroutine()
    {
        // 모습을 감추고 충돌을 비활성화합니다.
        spriteRenderer.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        healthBarCanvas.SetActive(false);

        yield return new WaitForSeconds(respawnTime);

        // 부활 위치(고정된 집결지)로 이동하고 상태를 초기화합니다.
        transform.position = rallyPointPosition;
        currentHealth = maxHealth;
        currentState = SoldierState.IdleAtRallyPoint;

        // 다시 모습을 드러내고 충돌을 활성화합니다.
        spriteRenderer.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        if (healthBarSlider != null) healthBarSlider.value = 1f;

        Debug.Log("해치가 부활했습니다!");
    }

    public void ReleaseEnemyBeforeDeath()
    {
        if (currentTarget != null)
        {
            currentTarget.UnblockBySoldier(this);
        }
    }
}
