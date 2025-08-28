using UnityEngine;
using UnityEngine.UI;

// 병사의 모든 행동(이동, 공격, 죽음)과 능력치를 관리하는 스크립트입니다.
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
    private float healthRegenRate = 5f; // (추가) 초당 체력 회복량
    [SerializeField]
    private float timeToStartRegen = 3f; // (추가) 전투 후 회복 시작까지 걸리는 시간

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
    private float timeSinceLastCombat = 0f; // (추가) 마지막 전투 후 지난 시간

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
        
        // (수정) 전투 중이 아닐 때의 로직
        if (currentTarget == null)
        {
            MoveToRallyPoint();

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
    }
    
    // (추가) 체력을 회복하고 체력바를 업데이트하는 함수
    void RegenerateHealth()
    {
        currentHealth += healthRegenRate * Time.deltaTime;
        // 체력이 최대 체력을 넘지 않도록 합니다.
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

        if (ownerBarracks != null)
        {
            ownerBarracks.RemoveSoldier(this);
        }
        Destroy(gameObject);
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
