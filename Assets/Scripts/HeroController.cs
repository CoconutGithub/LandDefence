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
        // (수정) 전투 여부와 상관없이 항상 이동 입력을 받습니다.
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(moveX, moveY).normalized;

        attackCountdown -= Time.deltaTime;

        if (currentTarget != null && attackCountdown <= 0f)
        {
            Attack();
            attackCountdown = timeBetweenAttacks;
        }

        // 목표가 있는데, 그 목표가 죽었거나 범위를 벗어났는지 확인하는 로직
        if (currentTarget != null)
        {
            // 목표가 죽었거나, 너무 멀어졌다면 전투를 해제합니다.
            if (!currentTarget.gameObject.activeInHierarchy || Vector3.Distance(transform.position, currentTarget.transform.position) > GetComponent<CircleCollider2D>().radius)
            {
                currentTarget.ResumeMovement(); // 적의 이동을 재개시킵니다.
                currentTarget = null;           // 공격 목표를 초기화합니다.
            }
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

    // Is Trigger가 켜졌으므로, OnTriggerEnter2D로 모든 것을 감지합니다.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 감지된 것이 경험치 구슬이라면
        if (other.CompareTag("ExperienceOrb"))
        {
            ExperienceController orb = other.GetComponent<ExperienceController>();
            if (orb != null)
            {
                orb.Collect();
            }
            return; // 경험치를 먹었으면 아래 로직은 실행하지 않습니다.
        }

        // 감지된 것이 적이고, 아직 공격 목표가 없다면
        if (other.CompareTag("Enemy") && currentTarget == null)
        {
            EnemyMovement enemy = other.GetComponent<EnemyMovement>();
            if (enemy != null && !enemy.IsBlocked())
            {
                enemy.BlockMovement(null, this); // 적의 이동을 멈추고
                currentTarget = enemy;           // 내 공격 목표로 설정합니다.
            }
        }
    }
}
