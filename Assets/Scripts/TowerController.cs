using System.Collections;
using System.Collections.Generic;
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
    
    [Header("레이저 정보 (영국 마법사 전용)")]
    public bool isLaserTower = false;
    [SerializeField]
    private float laserDps = 30f;
    [SerializeField]
    private float laserDpsRamp = 10f;
    
    private float finalProjectileDamage;

    [Header("업그레이드 및 스킬 정보")]
    public TowerBlueprint[] upgradePaths;
    public TowerSkillBlueprint[] towerSkills;
    
    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private LineRenderer laserLineRenderer;
    [SerializeField]
    private GameObject haetaePrefab; 

    private Transform currentTarget;
    private float attackCountdown = 0f;
    private TowerSpotController parentSpot;
    private float currentDpsRamp = 0f;
    private Transform lastTarget;
    private float laserTimer = 0f;

    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    private SoldierController spawnedHaetae; 

    void Start()
    {
        int damageLevel = DataManager.LoadDamageLevel(towerType);
        finalProjectileDamage = baseProjectileDamage * (1f + (damageLevel * 0.1f));

        if (isLaserTower && laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }

        foreach (var skill in towerSkills)
        {
            skillLevels[skill.skillName] = 0;
        }
    }

    public void SetParentSpot(TowerSpotController spot)
    {
        parentSpot = spot;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TowerUpgradeUI.instance.Show(this);
    }
    
    public int GetSkillLevel(string skillName)
    {
        if (skillLevels.ContainsKey(skillName))
        {
            return skillLevels[skillName];
        }
        return 0;
    }

    public void UpgradeSkill(TowerSkillBlueprint skillToUpgrade)
    {
        int currentLevel = GetSkillLevel(skillToUpgrade.skillName);
        if (currentLevel >= skillToUpgrade.maxLevel)
        {
            Debug.Log("이 스킬은 이미 마스터했습니다!");
            return;
        }

        int cost = skillToUpgrade.costs[currentLevel];
        if (GameManager.instance.SpendGold(cost))
        {
            SoundManager.instance.PlayBuildSound();
            skillLevels[skillToUpgrade.skillName]++;
            int newLevel = skillLevels[skillToUpgrade.skillName];
            Debug.Log($"{skillToUpgrade.skillName} 스킬 레벨 업! -> {newLevel}");

            if (skillToUpgrade.skillName == "해치")
            {
                HandleHaetaeSkill(skillToUpgrade, newLevel);
            }
            
            TowerUpgradeUI.instance.Show(this);
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
        }
    }
    
    // (수정) 해치 소환 위치를 가장 가까운 '경로 위'로 변경하는 로직
    private void HandleHaetaeSkill(TowerSkillBlueprint haetaeSkill, int newLevel)
    {
        if (newLevel == 1)
        {
            Transform waypoints = GameManager.WaypointHolder;
            if (waypoints == null || waypoints.childCount < 2) return;

            Vector3 bestSpawnPoint = Vector3.zero;
            float minDistanceSqr = float.MaxValue;

            // 모든 웨이포인트 '선분'을 순회합니다. (마지막 지점 제외)
            for (int i = 0; i < waypoints.childCount - 1; i++)
            {
                Vector3 p1 = waypoints.GetChild(i).position;
                Vector3 p2 = waypoints.GetChild(i + 1).position;
                
                // 타워와 가장 가까운 선분 위의 점을 찾습니다.
                Vector3 closestPointOnSegment = GetClosestPointOnLineSegment(p1, p2, transform.position);
                
                float distSqr = (transform.position - closestPointOnSegment).sqrMagnitude;

                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    bestSpawnPoint = closestPointOnSegment;
                }
            }
            
            if (bestSpawnPoint != Vector3.zero)
            {
                GameObject haetaeGO = Instantiate(haetaePrefab, bestSpawnPoint, Quaternion.identity);
                spawnedHaetae = haetaeGO.GetComponent<SoldierController>();
                spawnedHaetae.SetRallyPointPosition(bestSpawnPoint); 
            }
        }
        
        if (spawnedHaetae != null)
        {
            float health = haetaeSkill.values1[newLevel - 1];
            float damage = haetaeSkill.values2[newLevel - 1];
            spawnedHaetae.SetupAsHaetae(health, damage);
        }
    }

    // (추가) 두 점으로 이루어진 선분 위에서 특정 점과 가장 가까운 점을 찾는 함수
    private Vector3 GetClosestPointOnLineSegment(Vector3 p1, Vector3 p2, Vector3 point)
    {
        Vector3 lineDir = p2 - p1;
        float lineLengthSqr = lineDir.sqrMagnitude;
        if (lineLengthSqr < 0.0001f) return p1; // 선분의 길이가 거의 0이면 시작점을 반환

        // 점이 선분 위에 투영된 위치를 계산 (0.0 ~ 1.0 사이 값)
        float t = Mathf.Clamp01(Vector3.Dot(point - p1, lineDir) / lineLengthSqr);
        
        // 투영된 위치에 해당하는 실제 좌표를 반환
        return p1 + t * lineDir;
    }

    public void Upgrade(TowerBlueprint blueprint)
    {
        if (GameManager.instance.SpendGold(blueprint.cost))
        {
            SoundManager.instance.PlayBuildSound();
            GameObject newTowerGO = Instantiate(blueprint.prefab, transform.position, transform.rotation);

            var newTowerController = newTowerGO.GetComponent<TowerController>();
            if (newTowerController != null) newTowerController.SetParentSpot(parentSpot);
            
            var newBarracksController = newTowerGO.GetComponent<BarracksController>();
            if (newBarracksController != null) newBarracksController.SetParentSpot(parentSpot);
            
            parentSpot.SetCurrentTower(newTowerGO);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        FindClosestEnemy();
        attackCountdown -= Time.deltaTime;

        if (isLaserTower)
        {
            HandleLaserAttack();
        }
        else
        {
            if (currentTarget != null && attackCountdown <= 0f)
            {
                StartCoroutine(AttackBurst());
                attackCountdown = timeBetweenAttacks;
            }
        }
    }

    void HandleLaserAttack()
    {
        if (currentTarget != null)
        {
            laserLineRenderer.enabled = true;
            laserLineRenderer.SetPosition(0, firePoint.position);
            laserLineRenderer.SetPosition(1, currentTarget.position);

            if (currentTarget == lastTarget)
            {
                laserTimer += Time.deltaTime;
                if (laserTimer >= 1f)
                {
                    currentDpsRamp += laserDpsRamp; 
                    laserTimer -= 1f;
                }
            }
            else
            {
                currentDpsRamp = 0f;
                laserTimer = 0f;
            }
            lastTarget = currentTarget;

            float totalDps = laserDps + currentDpsRamp;
            float damageToSend = totalDps * Time.deltaTime;

            currentTarget.GetComponent<EnemyHealth>().TakeDamage(damageToSend, towerType, damageType);
        }
        else
        {
            if (laserLineRenderer.enabled)
            {
                laserLineRenderer.enabled = false;
                currentDpsRamp = 0f;
                laserTimer = 0f; 
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
                bomb.Setup(currentTarget.position, finalProjectileDamage, projectileSpeed, explosionRadius, towerType, damageType);
            }
        }
        else
        {
            ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                float dotDamage = 0;
                float dotDuration = 0;

                int fireArrowLevel = GetSkillLevel("불화살");
                if (fireArrowLevel > 0)
                {
                    TowerSkillBlueprint fireArrowSkill = System.Array.Find(towerSkills, skill => skill.skillName == "불화살");
                    if (fireArrowSkill != null)
                    {
                        float procChance = fireArrowSkill.values1[fireArrowLevel - 1];

                        if (Random.Range(0f, 100f) < procChance)
                        {
                            dotDamage = fireArrowSkill.values2[fireArrowLevel - 1];
                            dotDuration = fireArrowSkill.values3[fireArrowLevel - 1];
                        }
                    }
                }
                
                projectile.Setup(currentTarget, finalProjectileDamage, projectileSpeed, towerType, damageType, slowAmount, slowDuration, dotDamage, dotDuration);
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

