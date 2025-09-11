using UnityEngine;
using UnityEngine.UI;

public class HeroCloneController : MonoBehaviour
{
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
    private float returnCompleteDistance = 1f;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private Slider healthBarSlider;
    [SerializeField]
    private GameObject healthBarCanvas;
    [SerializeField]
    private LayerMask enemyLayer;

    [Header("애니메이션 정보")]
    [SerializeField]
    private AnimationClip attackAnimationClip;
    private AnimationController animationController;
    private float attackAnimLength;
    private Vector3 originalScale;


    private float currentHealth;
    private float attackCountdown = 0f;
    private EnemyMovement currentTarget;
    private Transform heroTransform;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animationController = GetComponent<AnimationController>();
        originalScale = transform.localScale;
        if (attackAnimationClip != null)
        {
            attackAnimLength = attackAnimationClip.length;
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

        if (currentTarget == null && (currentState == CloneState.ChasingEnemy || currentState == CloneState.Fighting))
        {
            currentState = CloneState.FollowingHero;
        }

        if (animationController != null)
        {
            bool isMoving = (currentState == CloneState.ChasingEnemy || currentState == CloneState.ReturningToHero);
            animationController.SetAnimationBool("IsMoving", isMoving);
        }

        HandleSpriteDirection();

        if (Vector3.Distance(transform.position, heroTransform.position) > leashDistance && currentState != CloneState.ReturningToHero)
        {
            if (currentTarget != null)
            {
                currentTarget.ResumeMovement(); 
                currentTarget = null;
            }
            currentState = CloneState.ReturningToHero;
        }

        switch (currentState)
        {
            case CloneState.FollowingHero:
                FindEnemyToChase();
                break;
            case CloneState.ReturningToHero:
                if (Vector3.Distance(transform.position, heroTransform.position) < returnCompleteDistance)
                {
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
        rb.linearVelocity = Vector2.zero;

        switch (currentState)
        {
            case CloneState.ReturningToHero:
                MoveTowards(heroTransform.position);
                break;
            case CloneState.ChasingEnemy:
                if (currentTarget != null) MoveTowards(currentTarget.transform.position);
                break;
        }
    }

    void HandleSpriteDirection()
    {
        Transform target = null;
        if (currentState == CloneState.ChasingEnemy || currentState == CloneState.Fighting)
        {
            if (currentTarget != null)
            {
                target = currentTarget.transform;
            }
        }
        else if (currentState == CloneState.ReturningToHero)
        {
            target = heroTransform;
        }

        if (target != null)
        {
            float directionX = target.position.x - transform.position.x;
            if (Mathf.Abs(directionX) > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Sign(directionX) * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
        }
    }

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
            if (enemy != null && !enemy.IsBlocked())
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
            // (핵심 수정) 적에게 자신(분신)을 알리기 위해 새로운 함수를 호출합니다.
            currentTarget.BlockMovementByClone(this);
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
        
        if (Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange)
        {
            currentState = CloneState.ChasingEnemy;
            currentTarget.ResumeMovement();
            return;
        }

        if (attackCountdown <= 0f)
        {
            Attack();
            attackCountdown = timeBetweenAttacks;
        }
    }
    
    // (추가) 적이 강제로 전투를 중단시킬 때 호출하는 함수
    public void ResumeFromBlock()
    {
        currentTarget = null;
        currentState = CloneState.FollowingHero;
    }

    void Attack()
    {
        if (currentTarget != null && animationController != null)
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
        else if (currentTarget != null)
        {
            DealDamageToTarget();
        }
    }

    public void DealDamageToTarget()
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
        if (currentTarget != null)
        {
            currentTarget.ResumeMovement();
        }
        Destroy(gameObject);
    }
}

