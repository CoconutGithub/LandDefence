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

    [Header("광역 공격")]
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

    private float originalMaxHealth;
    private float originalAttackDamage;
    private float lifeStealPercentage = 0f;
    private float originalRecognitionRadius;
    private float aoeChance = 0f;

    // (추가) 방패 공격 스킬 관련 변수
    private float reflectionChance = 0f;
    private float reflectionDuration = 0f;
    private bool isReflectingDamage = false;
    private float reflectionTimer = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        recognitionCollider = GetComponent<CircleCollider2D>();
        originalMaxHealth = maxHealth;
        originalAttackDamage = attackDamage;
        if (recognitionCollider != null)
        {
            originalRecognitionRadius = recognitionCollider.radius;
        }
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
        // (추가) 피해 반사 지속 시간 관리
        HandleReflection();

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

    // (추가) 피해 반사 상태를 관리하는 함수
    void HandleReflection()
    {
        if (isReflectingDamage)
        {
            reflectionTimer -= Time.deltaTime;
            if (reflectionTimer <= 0f)
            {
                isReflectingDamage = false;
                // (선택 사항) 반사 효과가 끝났음을 시각적으로 표시할 수 있습니다.
            }
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
        originalMaxHealth = newMaxHealth;
        originalAttackDamage = newAttackDamage;
        maxHealth = newMaxHealth;
        attackDamage = newAttackDamage;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    // (수정) 피해 반사 관련 능력치도 함께 받도록 함수 확장
    public void ApplyStatModification(float healthModifier, float damageModifier, float lifeSteal, float recognitionRadiusBonus, float newAoeChance, float newReflectionChance, float newReflectionDuration)
    {
        maxHealth = originalMaxHealth + healthModifier;
        attackDamage = originalAttackDamage + damageModifier;
        lifeStealPercentage = lifeSteal;
        aoeChance = newAoeChance;
        reflectionChance = newReflectionChance;
        reflectionDuration = newReflectionDuration;

        if (recognitionCollider != null)
        {
            recognitionCollider.radius = originalRecognitionRadius + recognitionRadiusBonus;
        }

        currentHealth = Mathf.Min(currentHealth, maxHealth);
        if (maxHealth <= 0) Die();
        
        UpdateHealthBar();
    }

    void Attack()
    {
        float totalDamageDealt = 0;
        bool isAoeAttackThisTurn = isAreaOfEffect;

        if (!isAoeAttackThisTurn && aoeChance > 0)
        {
            if (Random.Range(0f, 100f) < aoeChance)
            {
                isAoeAttackThisTurn = true;
            }
        }

        if (isAoeAttackThisTurn)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
            foreach (Collider2D hit in colliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    hit.GetComponent<EnemyHealth>().TakeDamage(attackDamage, TowerType.Barracks, damageType);
                    totalDamageDealt += attackDamage;
                }
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.GetComponent<EnemyHealth>().TakeDamage(attackDamage, TowerType.Barracks, damageType);
                totalDamageDealt = attackDamage;
            }
        }

        if (lifeStealPercentage > 0 && totalDamageDealt > 0)
        {
            Heal(totalDamageDealt * (lifeStealPercentage / 100f));
        }
    }

    // (수정) 공격자의 정보를 받고, 피해 반사 로직을 추가
    public void TakeDamage(float damage, EnemyMovement attacker)
    {
        // 피해 반사 상태라면, 받은 피해를 공격자에게 되돌려줍니다.
        if (isReflectingDamage && attacker != null)
        {
            attacker.GetComponent<EnemyHealth>().TakeDamage(damage, TowerType.Barracks, damageType);
        }

        currentHealth -= damage;
        timeSinceLastCombat = 0f;

        // 피해 반사 상태가 아닐 때, 확률적으로 피해 반사를 활성화합니다.
        if (!isReflectingDamage && reflectionChance > 0)
        {
            if (Random.Range(0f, 100f) < reflectionChance)
            {
                isReflectingDamage = true;
                reflectionTimer = reflectionDuration;
                // (선택 사항) 반사 효과가 시작되었음을 시각적으로 표시할 수 있습니다. (예: 방패 아이콘 활성화)
            }
        }

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

