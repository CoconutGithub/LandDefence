using UnityEngine;
using UnityEngine.EventSystems;

public class TowerController : MonoBehaviour, IPointerClickHandler
{
    [Header("타워 능력치")]
    [SerializeField]
    private float attackRange = 3f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;

    [Header("발사체 정보")]
    [SerializeField]
    private float projectileDamage = 25f;
    [SerializeField]
    private float projectileSpeed = 10f;

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
        // (수정) 공격 시 사운드를 재생합니다.
        SoundManager.instance.PlayAttackSound();

        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();

        if (projectile != null)
        {
            projectile.Setup(currentTarget, projectileDamage, projectileSpeed);
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
