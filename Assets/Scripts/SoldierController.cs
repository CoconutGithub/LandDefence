using UnityEngine;
using UnityEngine.UI;

public class SoldierController : MonoBehaviour
{
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

    private float currentHealth;
    private float attackCountdown = 0f;
    private EnemyMovement currentTarget;
    private Vector3 rallyPointPosition;
    private BarracksController ownerBarracks;
    private float timeSinceLastCombat = 0f;

    void Start()
    {
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
        attackCountdown -= Time.deltaTime;
        
        if (currentTarget == null)
        {
            MoveToRallyPoint();
            timeSinceLastCombat += Time.deltaTime;
            if (timeSinceLastCombat >= timeToStartRegen && currentHealth < maxHealth)
            {
                RegenerateHealth();
            }
        }
        else
        {
            timeSinceLastCombat = 0f;
        }

        if (currentTarget != null && attackCountdown <= 0f)
        {
            Attack();
            attackCountdown = timeBetweenAttacks;
        }
    }
    
    void RegenerateHealth()
    {
        currentHealth += healthRegenRate * Time.deltaTime;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }

    void MoveToRallyPoint()
    {
        if (Vector3.Distance(transform.position, rallyPointPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, rallyPointPosition, moveSpeed * Time.deltaTime);
        }
    }
    
    public void SetRallyPointPosition(Vector3 newPosition)
    {
        rallyPointPosition = newPosition;
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
        ReleaseEnemyBeforeDeath(); // 죽을 때도 적을 풀어줍니다.
        if (ownerBarracks != null)
        {
            ownerBarracks.RemoveSoldier(this);
        }
        Destroy(gameObject);
    }

    // (추가) 병영이 업그레이드되거나, 병사가 죽기 직전에 호출되어 적을 풀어주는 함수입니다.
    public void ReleaseEnemyBeforeDeath()
    {
        if (currentTarget != null)
        {
            currentTarget.ResumeMovement();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && currentTarget == null)
        {
            EnemyMovement enemy = other.GetComponent<EnemyMovement>();
            if (enemy != null && !enemy.IsBlocked())
            {
                enemy.BlockMovement(this, null);
                currentTarget = enemy;
            }
        }
    }
    
    void FixedUpdate()
    {
        if (currentTarget != null && !currentTarget.gameObject.activeInHierarchy)
        {
             currentTarget = null;
        }
    }
}
