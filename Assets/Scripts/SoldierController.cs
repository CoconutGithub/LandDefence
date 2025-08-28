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

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private Slider healthBarSlider;
    [SerializeField]
    private GameObject healthBarCanvas;

    private float currentHealth;
    private float attackCountdown = 0f;
    private EnemyMovement currentTarget;
    private Vector3 rallyPointPosition; // (수정) Transform 대신 Vector3 위치 값을 목표로 삼습니다.
    private BarracksController ownerBarracks;

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
        }

        if (currentTarget != null && attackCountdown <= 0f)
        {
            Attack();
            attackCountdown = timeBetweenAttacks;
        }
    }
    
    void MoveToRallyPoint()
    {
        // (수정) Vector3 위치로 이동합니다.
        if (Vector3.Distance(transform.position, rallyPointPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, rallyPointPosition, moveSpeed * Time.deltaTime);
        }
    }
    
    // (수정) 외부에서 Vector3 위치 값을 받아 목표를 설정하는 함수입니다. (이전 SetRallyPoint 함수를 대체합니다)
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
        // 목표가 있는데, 그 목표가 죽었거나 범위를 벗어났는지 확인
        if (currentTarget != null && !currentTarget.gameObject.activeInHierarchy)
        {
             currentTarget = null;
        }
    }
}
