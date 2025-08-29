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
    private float timeBetweenAttacks = 1f; // 점사 타워의 재장전 시간

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

    // (추가) 레이저 타워 전용 설정입니다.
    [Header("레이저 정보 (영국 마법사 전용)")]
    public bool isLaserTower = false; // 이 타워가 레이저 타워인지 확인
    [SerializeField]
    private float laserDps = 30f; // 초당 데미지 (Damage Per Second)
    [SerializeField]
    private float laserDpsRamp = 10f; // 초당 증가하는 추가 데미지
    
    private float finalProjectileDamage;

    [Header("업그레이드 정보")]
    public TowerBlueprint[] upgradePaths;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private LineRenderer laserLineRenderer; // (추가) 레이저를 그릴 Line Renderer
    
    private Transform currentTarget;
    private float attackCountdown = 0f;
    private TowerSpotController parentSpot;
    private float currentDpsRamp = 0f; // (추가) 현재 증가된 레이저 데미지
    private Transform lastTarget; // (추가) 이전에 공격했던 목표

    void Start()
    {
        int damageLevel = DataManager.LoadDamageLevel(towerType);
        finalProjectileDamage = baseProjectileDamage * (1f + (damageLevel * 0.1f));

        // (추가) 레이저 타워라면 시작할 때 레이저를 숨깁니다.
        if (isLaserTower && laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }
    }

    public void SetParentSpot(TowerSpotController spot)
    {
        parentSpot = spot;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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

        // (수정) 레이저 타워일 경우의 로직을 추가합니다.
        if (isLaserTower)
        {
            HandleLaserAttack();
        }
        else // 기존 발사체 타워 로직
        {
            if (currentTarget != null && attackCountdown <= 0f)
            {
                StartCoroutine(AttackBurst());
                attackCountdown = timeBetweenAttacks;
            }
        }
    }

    // (추가) 레이저 공격을 처리하는 함수입니다.
    void HandleLaserAttack()
    {
        if (currentTarget != null)
        {
            // 레이저를 켜고 위치를 설정합니다.
            laserLineRenderer.enabled = true;
            laserLineRenderer.SetPosition(0, firePoint.position);
            laserLineRenderer.SetPosition(1, currentTarget.position);

            // 목표가 이전과 같은지 확인하여 데미지 증가를 결정합니다.
            if (currentTarget == lastTarget)
            {
                currentDpsRamp += laserDpsRamp * Time.deltaTime;
            }
            else // 목표가 바뀌었다면 데미지 증가량을 초기화합니다.
            {
                currentDpsRamp = 0f;
            }
            lastTarget = currentTarget;

            // 최종 데미지를 계산하여 적용합니다.
            float totalDps = laserDps + currentDpsRamp;
            currentTarget.GetComponent<EnemyHealth>().TakeDamage(totalDps * Time.deltaTime, towerType, damageType);
        }
        else // 목표가 없으면 레이저를 끕니다.
        {
            if (laserLineRenderer.enabled)
            {
                laserLineRenderer.enabled = false;
                currentDpsRamp = 0f;
                lastTarget = null;
            }
        }
    }

    IEnumerator AttackBurst()
    {
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            if (currentTarget == null)
            {
                yield break;
            }
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
            if (bomb != null)
            {
                bomb.Setup(currentTarget, finalProjectileDamage, projectileSpeed, explosionRadius, towerType, damageType);
            }
        }
        else
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
