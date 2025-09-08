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
    
    [Header("애니메이션 정보")]
    [SerializeField]
    private AnimationClip attackAnimationClip;
    private float attackAnimLength;

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

    private float reflectionChance = 0f;
    private float reflectionDuration = 0f;
    private bool isReflectingDamage = false;
    private float reflectionTimer = 0f;
    
    private AnimationController animationController;
    
    // (추가) 병사의 원래 스케일 값을 저장할 변수
    private Vector3 originalScale;

    void Awake()
    {
        animationController = GetComponent<AnimationController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        recognitionCollider = GetComponent<CircleCollider2D>();
        originalMaxHealth = maxHealth;
        originalAttackDamage = attackDamage;
        if (recognitionCollider != null)
        {
            originalRecognitionRadius = recognitionCollider.radius;
        }
        
        // (추가) 시작할 때 현재 스케일 값을 저장합니다.
        originalScale = transform.localScale;
    }

    void Start()
    {
        if (attackAnimationClip != null)
        {
            attackAnimLength = attackAnimationClip.length;
        }

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
        HandleReflection();
        
        // (수정) 방향을 바꿀 때, 저장해 둔 원래 스케일 값을 사용하도록 변경합니다.
        if (currentTarget != null)
        {
            if (currentTarget.transform.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            }
            else
            {
                transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            }
        }
        else
        {
            // 타겟이 없을 때는 오른쪽(기본 방향)을 보도록 설정합니다.
            transform.localScale = originalScale;
        }

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
                    StartAttackSequence();
                    attackCountdown = timeBetweenAttacks;
                }
                break;
        }
    }
    
    void StartAttackSequence()
    {
        if (animationController != null)
        {
            if (attackAnimLength > 0 && timeBetweenAttacks > 0)
            {
                float speedMultiplier = attackAnimLength / timeBetweenAttacks;
                animationController.SetAnimationSpeed(speedMultiplier);
            }
            else
            {
                animationController.SetAnimationSpeed(1f);
            }
            animationController.PlayAttackAnimation();
        }
        else
        {
            DealDamage();
        }
    }
    
    public void DealDamage()
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

    void HandleReflection()
    {
        if (isReflectingDamage)
        {
            reflectionTimer -= Time.deltaTime;
            if (reflectionTimer <= 0f)
            {
                isReflectingDamage = false;
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

    public void TakeDamage(float damage, EnemyMovement attacker)
    {
        if (isReflectingDamage && attacker != null)
        {
            attacker.GetComponent<EnemyHealth>().TakeDamage(damage, TowerType.Barracks, damageType);
        }

        currentHealth -= damage;
        timeSinceLastCombat = 0f;

        if (!isReflectingDamage && reflectionChance > 0)
        {
            if (Random.Range(0f, 100f) < reflectionChance)
            {
                isReflectingDamage = true;
                reflectionTimer = reflectionDuration;
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

