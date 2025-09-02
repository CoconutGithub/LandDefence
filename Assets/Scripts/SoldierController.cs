using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    private float respawnTime = 10f;

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
    private bool isHaetae = false;
    private SpriteRenderer spriteRenderer;

    // (추가) 스킬 효과 계산을 위한 원본 능력치
    private float originalMaxHealth;
    private float originalAttackDamage;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        recognitionCollider = GetComponent<CircleCollider2D>();
        // (추가) 스킬이 적용되기 전의 순수 능력치를 저장합니다.
        originalMaxHealth = maxHealth;
        originalAttackDamage = attackDamage;
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
            UpdateHealthBar();
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
    
    public void SetBarracks(BarracksController barracks)
    {
        ownerBarracks = barracks;
    }

    public void SetupAsHaetae(float newMaxHealth, float newAttackDamage)
    {
        isHaetae = true;
        originalMaxHealth = newMaxHealth; // 해치의 기본 스탯을 원본으로 저장
        originalAttackDamage = newAttackDamage;
        maxHealth = newMaxHealth;
        attackDamage = newAttackDamage;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    // (추가) 병영에서 스킬 효과를 적용하기 위해 호출하는 함수
    public void ApplyStatModification(float healthModifier, float damageModifier)
    {
        maxHealth = originalMaxHealth + healthModifier;
        attackDamage = originalAttackDamage + damageModifier;

        // 최대 체력이 변경되었으므로, 현재 체력도 비율에 맞게 조정하거나, 최대치를 넘지 않도록 보정
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        if (maxHealth <= 0) Die(); // 체력 감소로 인해 죽는 경우
        
        UpdateHealthBar();
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
        UpdateHealthBar();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (currentHealth <= 0) return;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthBar();
    }
    
    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        ReleaseEnemyBeforeDeath();
        
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

    IEnumerator RespawnCoroutine()
    {
        spriteRenderer.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        healthBarCanvas.SetActive(false);
        currentState = SoldierState.IdleAtRallyPoint;

        yield return new WaitForSeconds(respawnTime);

        transform.position = rallyPointPosition;
        currentHealth = maxHealth;
        
        spriteRenderer.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        UpdateHealthBar();

        Debug.Log("해치가 부활했습니다!");
    }

    public void ReleaseEnemyBeforeDeath()
    {
        if (currentTarget != null)
        {
            currentTarget.UnblockBySoldier(this);
            currentTarget = null;
        }
    }
}

