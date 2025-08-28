using UnityEngine;
using UnityEngine.UI;

// 영웅의 이동, 전투 등 모든 것을 관리하는 스크립트입니다.
public class HeroController : MonoBehaviour
{
    [Header("영웅 능력치")]
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float maxHealth = 500f;
    [SerializeField]
    private float attackDamage = 30f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;
    [SerializeField]
    private float healthRegenRate = 10f; // (추가) 초당 체력 회복량
    [SerializeField]
    private float timeToStartRegen = 3f; // (추가) 전투 후 회복 시작까지 걸리는 시간

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private Slider healthBarSlider;
    [SerializeField]
    private GameObject healthBarCanvas;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private float currentHealth;
    private float attackCountdown = 0f;
    private EnemyMovement currentTarget;
    private float timeSinceLastCombat = 0f; // (추가) 마지막 전투 후 지난 시간

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = 1f;
            healthBarSlider.value = 1f;
        }
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
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(moveX, moveY).normalized;

        attackCountdown -= Time.deltaTime;

        // (수정) 전투 중이 아닐 때의 로직
        if (currentTarget == null)
        {
            // (추가) 체력 회복 로직
            timeSinceLastCombat += Time.deltaTime;
            if (timeSinceLastCombat >= timeToStartRegen && currentHealth < maxHealth)
            {
                RegenerateHealth();
            }
        }
        else
        {
            // (추가) 전투 중일 때는 회복 타이머를 리셋합니다.
            timeSinceLastCombat = 0f;
        }

        if (currentTarget != null && attackCountdown <= 0f)
        {
            Attack();
            attackCountdown = timeBetweenAttacks;
        }

        if (currentTarget != null)
        {
            if (!currentTarget.gameObject.activeInHierarchy || Vector3.Distance(transform.position, currentTarget.transform.position) > GetComponent<CircleCollider2D>().radius)
            {
                currentTarget.ResumeMovement();
                currentTarget = null;
            }
        }
    }

    // (추가) 체력을 회복하고 체력바를 업데이트하는 함수
    void RegenerateHealth()
    {
        currentHealth += healthRegenRate * Time.deltaTime;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementInput * moveSpeed;
    }

    void Attack()
    {
        if (currentTarget != null)
        {
            currentTarget.GetComponent<EnemyHealth>().TakeDamage(attackDamage, TowerType.Barracks, DamageType.Physical);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        timeSinceLastCombat = 0f; // (추가) 피해를 입으면 회복 타이머를 리셋합니다.

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
        Debug.Log("영웅이 쓰러졌습니다!");
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ExperienceOrb"))
        {
            ExperienceController orb = other.GetComponent<ExperienceController>();
            if (orb != null)
            {
                orb.Collect();
            }
            return;
        }

        if (other.CompareTag("Enemy") && currentTarget == null)
        {
            EnemyMovement enemy = other.GetComponent<EnemyMovement>();
            if (enemy != null && !enemy.IsBlocked())
            {
                enemy.BlockMovement(null, this);
                currentTarget = enemy;
            }
        }
    }
}
