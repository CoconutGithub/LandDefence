using UnityEngine;
using UnityEngine.EventSystems;

// IPointerClickHandler 인터페이스를 상속받아 클릭을 감지할 수 있게 합니다.
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

    // 업그레이드 정보를 두 개의 경로로 나누어 설정합니다.
    [Header("업그레이드 정보")]
    public TowerBlueprint upgradeKR_Blueprint; // 한국 타워 업그레이드 정보
    public TowerBlueprint upgradeJP_Blueprint; // 일본 타워 업그레이드 정보

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Transform firePoint;

    private Transform currentTarget;
    private float attackCountdown = 0f;
    private TowerSpotController parentSpot; // 이 타워가 서 있는 부지 정보

    // 타워가 처음 생성될 때 부모 부지 정보를 저장하는 함수
    public void SetParentSpot(TowerSpotController spot)
    {
        parentSpot = spot;
    }

    // 클릭되었을 때 업그레이드 UI를 엽니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        // 업그레이드 경로가 하나라도 설정되어 있을 때만 UI를 엽니다.
        if (upgradeKR_Blueprint.prefab != null || upgradeJP_Blueprint.prefab != null)
        {
            TowerUpgradeUI.instance.Show(this);
        }
    }

    // 실제로 타워를 업그레이드하는 함수
    public void Upgrade(TowerBlueprint blueprint)
    {
        if (GameManager.instance.SpendGold(blueprint.cost))
        {
            // 새로운 업그레이드 타워를 같은 위치에 생성합니다.
            GameObject newTowerGO = Instantiate(blueprint.prefab, transform.position, transform.rotation);
            
            // 새로 생긴 타워에게도 부모 부지 정보를 넘겨줍니다.
            newTowerGO.GetComponent<TowerController>().SetParentSpot(parentSpot);

            // 부모 부지가 기억하는 현재 타워를 새로운 타워로 갱신해줍니다.
            parentSpot.SetCurrentTower(newTowerGO);

            // 기존 타워(자기 자신)를 파괴합니다.
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
