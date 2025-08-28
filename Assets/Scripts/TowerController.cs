using UnityEngine;
using UnityEngine.EventSystems;

public class TowerController : MonoBehaviour, IPointerClickHandler
{
    [Header("타워 기본 정보")]
    public TowerType towerType = TowerType.Archer;

    [Header("타워 능력치")]
    [SerializeField]
    private float attackRange = 3f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;

    [Header("발사체 정보")]
    [SerializeField]
    private float baseProjectileDamage = 25f; // (수정) '기본' 공격력으로 이름 변경
    [SerializeField]
    private float projectileSpeed = 10f;
    
    private float finalProjectileDamage; // (추가) 업그레이드가 적용된 최종 공격력

    [Header("업그레이드 정보")]
    public TowerBlueprint upgradeKR_Blueprint;
    public TowerBlueprint upgradeJP_Blueprint;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Transform firePoint;

    private Transform currentTarget;
    private float attackCountdown = 0f;
    private TowerSpotController parentSpot;

    // (추가) Start 함수: 게임 시작 시 한 번만 호출됩니다.
    void Start()
    {
        // 저장된 업그레이드 레벨을 불러옵니다.
        int damageLevel = DataManager.LoadArcherDamageLevel();
        // 최종 공격력을 계산합니다. (예: 1레벨당 10%씩 증가)
        finalProjectileDamage = baseProjectileDamage * (1f + (damageLevel * 0.1f));
    }

    public void SetParentSpot(TowerSpotController spot)
    {
        parentSpot = spot;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (upgradeKR_Blueprint.prefab != null || upgradeJP_Blueprint.prefab != null)
        {
            TowerUpgradeUI.instance.Show(this);
        }
    }

    public void Upgrade(TowerBlueprint blueprint)
    {
        if (GameManager.instance.SpendGold(blueprint.cost))
        {
            SoundManager.instance.PlayBuildSound();
            GameObject newTowerGO = Instantiate(blueprint.prefab, transform.position, transform.rotation);
            newTowerGO.GetComponent<TowerController>().SetParentSpot(parentSpot);
            parentSpot.SetCurrentTower(newTowerGO);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        FindClosestEnemy();
        attackCountdown -= Time.deltaTime;

        if (currentTarget != null && attackCountdown <= 0f)
        {
            Attack();
            attackCountdown = timeBetweenAttacks;
        }
    }

    void Attack()
    {
        SoundManager.instance.PlayAttackSound();
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();

        if (projectile != null)
        {
            // (수정) 발사체에게 '기본' 공격력이 아닌 '최종' 공격력을 전달합니다.
            projectile.Setup(currentTarget, finalProjectileDamage, projectileSpeed, towerType);
        }
    }

    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }
        if (closestEnemy != null && closestDistance <= attackRange)
        {
            currentTarget = closestEnemy.transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
