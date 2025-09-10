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
        
        // (추가) 현재 상태에 따라 IsMoving 파라미터를 업데이트합니다.
        UpdateAnimationState();

        // (수정) 이동 및 공격 방향에 따라 스프라이트 방향을 제어하는 새 함수 호출
        HandleSpriteDirection();

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

    // (추가) 병사의 이동 및 공격 방향에 따라 스프라이트의 좌우를 결정하는 함수
    void HandleSpriteDirection()
    {
        Vector3 targetPosition = Vector3.zero;
        bool hasTarget = false;

        // 상태에 따라 목표 위치를 결정합니다.
        if (currentState == SoldierState.ChasingEnemy && currentTarget != null)
        {
            targetPosition = currentTarget.transform.position;
            hasTarget = true;
        }
        else if (currentState == SoldierState.ReturningToRallyPoint)
        {
            targetPosition = rallyPointPosition;
            hasTarget = true;
        }
        else if (currentState == SoldierState.Fighting && currentTarget != null)
        {
            targetPosition = currentTarget.transform.position;
            hasTarget = true;
        }

        // 목표가 있을 경우에만 방향을 바꿉니다.
        if (hasTarget)
        {
            // 목표가 왼쪽에 있으면 왼쪽을, 오른쪽에 있으면 오른쪽을 보도록 합니다.
            if (targetPosition.x < transform.position.x)
            {
                transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            }
            else if (targetPosition.x > transform.position.x)
            {
                transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            }
            // x좌표가 거의 같으면 방향을 바꾸지 않습니다.
        }
    }
    
    // (추가) 병사의 상태에 따라 애니메이터의 IsMoving 파라미터를 설정하는 함수
    void UpdateAnimationState()
    {
        if (animationController == null) return;

        bool isMoving = (currentState == SoldierState.ReturningToRallyPoint || currentState == SoldierState.ChasingEnemy);
        animationController.SetAnimationBool("IsMoving", isMoving);
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

