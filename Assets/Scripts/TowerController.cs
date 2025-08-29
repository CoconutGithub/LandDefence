using System.Collections;
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
    
    [Header("특수 효과")]
    [SerializeField]
    private float slowAmount = 0.5f;
    [SerializeField]
    private float slowDuration = 2f;
    [SerializeField]
    private float explosionRadius = 2f;
    [SerializeField]
    private int bulletsPerBurst = 1;
    [SerializeField]
    private float timeBetweenShots = 0.1f;
    
    private float finalProjectileDamage;

    [Header("업그레이드 정보")]
    public TowerBlueprint[] upgradePaths;

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
        int damageLevel = DataManager.LoadDamageLevel(towerType);
        finalProjectileDamage = baseProjectileDamage * (1f + (damageLevel * 0.1f));
    }

    public void SetParentSpot(TowerSpotController spot)
    {
        parentSpot = spot;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // (수정) 통합된 TowerUpgradeUI를 호출합니다.
        if (upgradePaths != null && upgradePaths.Length > 0)
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
            StartCoroutine(AttackBurst());
            attackCountdown = timeBetweenAttacks;
        }
    }

    IEnumerator AttackBurst()
    {
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            if (currentTarget == null) yield break;
            Shoot();
            yield return new WaitForSeconds(timeBetweenShots);
        }
    }

    void Shoot()
    {
        SoundManager.instance.PlayAttackSound();
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (towerType == TowerType.Bomb)
        {
            BombProjectileController bomb = projectileGO.GetComponent<BombProjectileController>();
            if (bomb != null) bomb.Setup(currentTarget, finalProjectileDamage, projectileSpeed, explosionRadius, towerType, damageType);
        }
        else
        {
            ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();
            if (projectile != null) projectile.Setup(currentTarget, finalProjectileDamage, projectileSpeed, towerType, damageType, slowAmount, slowDuration);
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
