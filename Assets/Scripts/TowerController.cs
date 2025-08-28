using UnityEngine;
using UnityEngine.EventSystems;

public class TowerController : MonoBehaviour, IPointerClickHandler
{
    [Header("타워 기본 정보")]
    public TowerType towerType = TowerType.Archer;
    public DamageType damageType = DamageType.Physical;

    [Header("타워 능력치")]
    [SerializeField]
    private float attackRange = 3f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;

    [Header("발사체 정보")]
    [SerializeField]
    private float baseProjectileDamage = 25f;
    [SerializeField]
    private float projectileSpeed = 10f;
    
    [Header("특수 효과 (마법사/폭탄 전용)")]
    [SerializeField]
    private float slowAmount = 0.5f;
    [SerializeField]
    private float slowDuration = 2f;
    [SerializeField]
    private float explosionRadius = 2f; // (추가) 폭탄 타워의 폭발 반경
    
    private float finalProjectileDamage;

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

    void Start()
    {
        // (수정) 타워 종류에 따라 다른 업그레이드 데이터를 불러오도록 개선이 필요하지만, 지금은 궁수만 적용합니다.
        if (towerType == TowerType.Archer)
        {
            int damageLevel = DataManager.LoadArcherDamageLevel();
            finalProjectileDamage = baseProjectileDamage * (1f + (damageLevel * 0.1f));
        }
        else
        {
            finalProjectileDamage = baseProjectileDamage;
        }
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

        // (수정) 타워 종류에 따라 다른 발사체 스크립트를 제어합니다.
        if (towerType == TowerType.Bomb)
        {
            BombProjectileController bomb = projectileGO.GetComponent<BombProjectileController>();
            if (bomb != null)
            {
                bomb.Setup(currentTarget, finalProjectileDamage, projectileSpeed, explosionRadius, towerType, damageType);
            }
        }
        else // 궁수, 마법사 타워
        {
            ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                projectile.Setup(currentTarget, finalProjectileDamage, projectileSpeed, towerType, damageType, slowAmount, slowDuration);
            }
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
