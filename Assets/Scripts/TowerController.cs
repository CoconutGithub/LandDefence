using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerController : MonoBehaviour, IPointerClickHandler
{
    private AnimationController animationController;

    [Header("타워 기본 정보")]
    public TowerType towerType = TowerType.Archer;
    public DamageType damageType = DamageType.Physical;

    [Header("타워 능력치")]
    [SerializeField]
    private float attackRange = 3f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;

    // (추가) 연사 타워인지 구분하는 체크박스
    [Header("연사 타워 설정 (독일 총 타워 전용)")]
    [SerializeField]
    private bool isRapidFireTower = false;

    [Header("발사체 정보")]
    [SerializeField]
    private float baseProjectileDamage = 25f;
    [SerializeField]
    private float projectileSpeed = 10f;
    [SerializeField]
    private Sprite projectileSprite;

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
    [SerializeField]
    private Gradient[] laserColorGradients = new Gradient[3];

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
    private Transform characterSpriteTransform;
    [SerializeField]
    private LineRenderer laserLineRenderer;
    [SerializeField]
    private GameObject haetaePrefab;
    [SerializeField]
    private GameObject missilePrefab;
    [SerializeField]
    private Sprite missileSprite;

    private Transform currentTarget;
    private float attackCountdown = 0f;
    private TowerSpotController parentSpot;
    private float currentDpsRamp = 0f;
    private Transform lastTarget;
    private float laserTimer = 0f;

    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    private SoldierController spawnedHaetae;

    private float originalTimeBetweenAttacks;
    private int originalBulletsPerBurst;
    private float originalLaserDps;
    private float originalLaserDpsRamp;
    private float originalSlowAmount;
    private float originalSlowDuration;

    private float healCheckTimer = 0f;
    private const float HEAL_CHECK_INTERVAL = 0.5f;

    private bool isSupplyBuffActive = false;
    private float supplyBuffTimer = 0f;

    private float iceSlickCheckTimer = 0f;
    private const float ICE_SLICK_CHECK_INTERVAL = 0.5f;

    void Start()
    {
        animationController = GetComponent<AnimationController>();
        if (animationController == null)
        {
            Debug.LogWarning($"TowerController: AnimationController 컴포넌트를 찾을 수 없습니다. 오브젝트: {gameObject.name}");
        }

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

        originalTimeBetweenAttacks = timeBetweenAttacks;
        originalBulletsPerBurst = bulletsPerBurst;
        originalLaserDps = laserDps;
        originalLaserDpsRamp = laserDpsRamp;
        originalSlowAmount = slowAmount;
        originalSlowDuration = slowDuration;

        if (isLaserTower)
        {
            ApplyLaserColor(0);
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

            ApplyAllPassiveSkillEffects();

            if (isLaserTower && skillToUpgrade.skillName == "아브라카다브라!")
            {
                ApplyLaserColor(newLevel);
            }

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

    void ApplyLaserColor(int skillLevel)
    {
        if (laserLineRenderer == null || laserColorGradients == null) return;
        if (skillLevel >= 0 && skillLevel < laserColorGradients.Length)
        {
            if (laserColorGradients[skillLevel] != null)
            {
                laserLineRenderer.colorGradient = laserColorGradients[skillLevel];
            }
        }
    }

    void ApplyAllPassiveSkillEffects()
    {
        timeBetweenAttacks = originalTimeBetweenAttacks;
        bulletsPerBurst = originalBulletsPerBurst;
        laserDps = originalLaserDps;
        laserDpsRamp = originalLaserDpsRamp;
        slowAmount = originalSlowAmount;
        slowDuration = originalSlowDuration;
        int damageLevel = DataManager.LoadDamageLevel(towerType);
        finalProjectileDamage = baseProjectileDamage * (1f + (damageLevel * 0.1f));
        foreach (var skill in towerSkills)
        {
            int skillLevel = GetSkillLevel(skill.skillName);
            if (skillLevel > 0)
            {
                switch (skill.skillName)
                {
                    case "빠른 재장전":
                        float reduction = skill.values1[skillLevel - 1];
                        timeBetweenAttacks = Mathf.Max(originalTimeBetweenAttacks - reduction, 0.1f);
                        break;
                    case "기관총":
                        bulletsPerBurst = (int)skill.values1[skillLevel - 1];
                        float damageMultiplier = skill.values2[skillLevel - 1];
                        finalProjectileDamage *= damageMultiplier;
                        break;
                    case "아브라카다브라!":
                        laserDps = skill.values1[skillLevel - 1];
                        laserDpsRamp = skill.values2[skillLevel - 1];
                        break;
                    case "전선 구축":
                        slowAmount = skill.values1[skillLevel - 1];
                        slowDuration = skill.values2[skillLevel - 1];
                        break;
                }
            }
        }
    }

    private void HandleHaetaeSkill(TowerSkillBlueprint haetaeSkill, int newLevel)
    {
        if (newLevel == 1)
        {
            Transform waypoints = GameManager.WaypointHolder;
            if (waypoints == null || waypoints.childCount < 2) return;
            Vector3 bestSpawnPoint = Vector3.zero;
            float minDistanceSqr = float.MaxValue;
            for (int i = 0; i < waypoints.childCount - 1; i++)
            {
                Vector3 p1 = waypoints.GetChild(i).position;
                Vector3 p2 = waypoints.GetChild(i + 1).position;
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
                if (spawnedHaetae != null)
                {
                    spawnedHaetae.SetRallyPointPosition(bestSpawnPoint);
                }
            }
        }
        if (spawnedHaetae != null)
        {
            float health = haetaeSkill.values1[newLevel - 1];
            float damage = haetaeSkill.values2[newLevel - 1];
            spawnedHaetae.SetupAsHaetae(health, damage);
        }
    }

    private Vector3 GetClosestPointOnLineSegment(Vector3 p1, Vector3 p2, Vector3 point)
    {
        Vector3 lineDir = p2 - p1;
        float lineLengthSqr = lineDir.sqrMagnitude;
        if (lineLengthSqr < 0.0001f) return p1;
        float t = Mathf.Clamp01(Vector3.Dot(point - p1, lineDir) / lineLengthSqr);
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
        HandleSupplyBuff();
        int healLevel = GetSkillLevel("힐");
        if (healLevel > 0)
        {
            HandleHealingAura(healLevel);
        }
        int iceSlickLevel = GetSkillLevel("빙판");
        if (iceSlickLevel > 0)
        {
            HandleIceSlickAura(iceSlickLevel);
        }
        FindClosestEnemy();
        HandleSpriteDirection();
        attackCountdown -= Time.deltaTime;
        if (isLaserTower)
        {
            if (currentTarget != null)
            {
                HandleLaserAttack(true);
            }
            else
            {
                HandleLaserAttack(false);
            }
        }
        else
        {
            if (currentTarget != null && attackCountdown <= 0f)
            {
                StartAttackSequence();
                attackCountdown = timeBetweenAttacks;
            }
        }
    }

    void HandleSpriteDirection()
    {
        if (characterSpriteTransform != null && currentTarget != null)
        {
            if (currentTarget.position.x < transform.position.x)
            {
                characterSpriteTransform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                characterSpriteTransform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }

    void HandleIceSlickAura(int skillLevel)
    {
        iceSlickCheckTimer -= Time.deltaTime;
        if (iceSlickCheckTimer <= 0f)
        {
            iceSlickCheckTimer = ICE_SLICK_CHECK_INTERVAL;
            TowerSkillBlueprint iceSlickSkill = System.Array.Find(towerSkills, skill => skill.skillName == "빙판");
            if (iceSlickSkill == null || iceSlickSkill.values1.Length < skillLevel || iceSlickSkill.values2.Length < skillLevel) return;
            float slowAmountValue = iceSlickSkill.values1[skillLevel - 1] / 100f;
            float iceSlickRadius = iceSlickSkill.values2[skillLevel - 1];
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, iceSlickRadius);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    EnemyMovement enemy = collider.GetComponent<EnemyMovement>();
                    if (enemy != null)
                    {
                        enemy.ApplySlow(slowAmountValue, ICE_SLICK_CHECK_INTERVAL + 0.1f);
                    }
                }
            }
        }
    }

    void HandleSupplyBuff()
    {
        if (isSupplyBuffActive)
        {
            supplyBuffTimer -= Time.deltaTime;
            if (supplyBuffTimer <= 0)
            {
                isSupplyBuffActive = false;
                ApplyAllPassiveSkillEffects();
            }
        }
    }

    void HandleHealingAura(int skillLevel)
    {
        healCheckTimer -= Time.deltaTime;
        if (healCheckTimer <= 0f)
        {
            healCheckTimer = HEAL_CHECK_INTERVAL;
            TowerSkillBlueprint healSkill = System.Array.Find(towerSkills, skill => skill.skillName == "힐");
            if (healSkill == null) return;
            float healPerSecond = healSkill.values1[skillLevel - 1];
            float healAmount = healPerSecond * HEAL_CHECK_INTERVAL;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (var collider in colliders)
            {
                SoldierController soldier = collider.GetComponent<SoldierController>();
                if (soldier != null)
                {
                    soldier.Heal(healAmount);
                }
                HeroController hero = collider.GetComponent<HeroController>();
                if (hero != null)
                {
                    hero.Heal(healAmount);
                }
            }
        }
    }

    void HandleLaserAttack(bool isActive)
    {
        if (isActive && currentTarget != null)
        {
            laserLineRenderer.enabled = true;
            if (animationController != null)
            {
                animationController.SetAnimationBool("IsCasting", true);
            }
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
                if (animationController != null)
                {
                    animationController.SetAnimationBool("IsCasting", false);
                }
                currentDpsRamp = 0f;
                laserTimer = 0f;
                lastTarget = null;
            }
        }
    }

    // (수정) 공격 애니메이션을 시작시키는 단일 진입점
    void StartAttackSequence()
    {
        if (animationController != null)
        {
            if (isRapidFireTower)
            {
                // 연사 타워는 IsBursting 신호로 애니메이션 제어
                StartCoroutine(BurstFireCoroutine());
            }
            else
            {
                // 단발/점사 타워는 DoAttack 신호로 애니메이션 제어
                animationController.PlayAttackAnimation();
            }
        }
        else
        {
            // 애니메이션이 없는 경우, 즉시 발사
            if (isRapidFireTower)
            {
                StartCoroutine(BurstFireCoroutine());
            }
            else
            {
                FireOneShot();
            }
        }
    }

    // (수정) 애니메이션 이벤트에서 호출될 단발/점사 발사 함수
    public void FireOneShot()
    {
        SoundManager.instance.PlayAttackSound();
        Shoot();
    }

    // (수정) 연사 시작을 위한 새로운 코루틴
    private IEnumerator BurstFireCoroutine()
    {
        if (animationController != null)
        {
            animationController.SetAnimationBool("IsBursting", true);
        }

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            if (currentTarget == null)
            {
                break; // 연사 도중 타겟이 사라지면 중단
            }
            SoundManager.instance.PlayAttackSound();
            Shoot();
            yield return new WaitForSeconds(timeBetweenShots);
        }

        if (animationController != null)
        {
            animationController.SetAnimationBool("IsBursting", false);
        }
    }

    void Shoot()
    {
        // ... (기존 Shoot 함수의 내용은 그대로 유지)
        int supplyLevel = GetSkillLevel("빵 보급");
        if (supplyLevel > 0 && !isSupplyBuffActive)
        {
            TowerSkillBlueprint supplySkill = System.Array.Find(towerSkills, skill => skill.skillName == "빵 보급");
            if (supplySkill != null && supplySkill.values1.Length >= supplyLevel && supplySkill.values2.Length >= supplyLevel && supplySkill.values3.Length >= supplyLevel)
            {
                float procChance = supplySkill.values1[supplyLevel - 1];
                if (Random.Range(0f, 100f) < procChance)
                {
                    isSupplyBuffActive = true;
                    float buffDuration = supplySkill.values2[supplyLevel - 1];
                    float speedMultiplier = supplySkill.values3[supplyLevel - 1];
                    supplyBuffTimer = buffDuration;
                    timeBetweenAttacks /= speedMultiplier;
                }
            }
        }
        int poseidonLevel = GetSkillLevel("포세이돈");
        if (poseidonLevel > 0)
        {
            TowerSkillBlueprint poseidonSkill = System.Array.Find(towerSkills, skill => skill.skillName == "포세이돈");
            if (poseidonSkill != null && poseidonSkill.values1.Length >= poseidonLevel && poseidonSkill.values2.Length >= poseidonLevel && poseidonSkill.values3.Length >= poseidonLevel)
            {
                float procChance = poseidonSkill.values1[poseidonLevel - 1];
                if (Random.Range(0f, 100f) < procChance)
                {
                    Transform specialTarget = FindFurthestEnemyInRange();
                    if (specialTarget != null)
                    {
                        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                        ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();
                        if (projectile != null)
                        {
                            projectile.SetSprite(projectileSprite);
                            float knockbackDist = poseidonSkill.values2[poseidonLevel - 1];
                            float knockbackRadius = poseidonSkill.values3[poseidonLevel - 1];
                            projectile.Setup(specialTarget, 0, projectileSpeed, towerType, damageType, 0, 0, 0, 0, knockbackDist, knockbackRadius);
                        }
                        return;
                    }
                }
            }
        }
        int hadesLevel = GetSkillLevel("하데스");
        if (hadesLevel > 0)
        {
            TowerSkillBlueprint hadesSkill = System.Array.Find(towerSkills, skill => skill.skillName == "하데스");
            if (hadesSkill != null)
            {
                if (Random.Range(0f, 100f) < 5f)
                {
                    EnemyHealth enemyHealth = currentTarget.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        float healthThreshold = hadesSkill.values1[hadesLevel - 1];
                        if (enemyHealth.MaxHealth <= healthThreshold)
                        {
                            enemyHealth.InstantKill();
                            return;
                        }
                    }
                }
            }
        }
        int sniperLevel = GetSkillLevel("저격총");
        if (sniperLevel > 0)
        {
            TowerSkillBlueprint sniperSkill = System.Array.Find(towerSkills, skill => skill.skillName == "저격총");
            if (sniperSkill != null)
            {
                float procChance = sniperSkill.values1[sniperLevel - 1];
                if (Random.Range(0f, 100f) < procChance)
                {
                    Transform specialTarget = FindHighestHealthEnemyOnMap();
                    if (specialTarget != null)
                    {
                        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                        ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();
                        if (projectile != null)
                        {
                            projectile.SetSprite(projectileSprite);
                            float specialDamage = sniperSkill.values2[sniperLevel - 1];
                            projectile.Setup(specialTarget, specialDamage, projectileSpeed, towerType, damageType, 0, 0, 0, 0);
                        }
                        return;
                    }
                }
            }
        }
        int missileLevel = GetSkillLevel("미사일");
        if (missileLevel > 0 && missilePrefab != null)
        {
            TowerSkillBlueprint missileSkill = System.Array.Find(towerSkills, skill => skill.skillName == "미사일");
            if (missileSkill != null)
            {
                float procChance = missileSkill.values1[missileLevel - 1];
                if (Random.Range(0f, 100f) < procChance)
                {
                    GameObject missileGO = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);
                    BombProjectileController bomb = missileGO.GetComponent<BombProjectileController>();
                    if (bomb != null && currentTarget != null)
                    {
                        bomb.SetSprite(missileSprite);
                        float missileDamage = missileSkill.values2[missileLevel - 1];
                        float missileRadius = missileSkill.values3[missileLevel - 1];
                        bomb.Setup(currentTarget.position, missileDamage, projectileSpeed, missileRadius, towerType, damageType, 0, 0, null, 0);
                    }
                    return;
                }
            }
        }
        int longShotLevel = GetSkillLevel("장거리 사격");
        if (longShotLevel > 0)
        {
            TowerSkillBlueprint longShotSkill = System.Array.Find(towerSkills, skill => skill.skillName == "장거리 사격");
            if (longShotSkill != null)
            {
                float procChance = longShotSkill.values1[longShotLevel - 1];
                if (Random.Range(0f, 100f) < procChance)
                {
                    Transform specialTarget = FindHighestHealthEnemyInRange();
                    if (specialTarget != null)
                    {
                        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                        ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();
                        if (projectile != null)
                        {
                            if (longShotSkill.overrideProjectileSprite != null)
                            {
                                projectile.SetSprite(longShotSkill.overrideProjectileSprite);
                            }
                            else
                            {
                                projectile.SetSprite(projectileSprite);
                            }
                            float damageMultiplier = longShotSkill.values2[longShotLevel - 1];
                            float specialDamage = finalProjectileDamage * damageMultiplier;
                            projectile.Setup(specialTarget, specialDamage, projectileSpeed, towerType, damageType, 0, 0, 0, 0);
                        }
                        return;
                    }
                }
            }
        }
        GameObject prefabToSpawn = projectilePrefab;
        Sprite spriteToUse = projectileSprite;
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
                    if (fireArrowSkill.overrideProjectilePrefab != null)
                    {
                        prefabToSpawn = fireArrowSkill.overrideProjectilePrefab;
                        spriteToUse = null;
                    }
                    else if (fireArrowSkill.overrideProjectileSprite != null)
                    {
                        spriteToUse = fireArrowSkill.overrideProjectileSprite;
                    }
                }
            }
        }
        GameObject projectileGO_normal = Instantiate(prefabToSpawn, firePoint.position, Quaternion.identity);
        if (towerType == TowerType.Bomb)
        {
            BombProjectileController bomb = projectileGO_normal.GetComponent<BombProjectileController>();
            if (bomb != null)
            {
                if (spriteToUse != null) bomb.SetSprite(spriteToUse);
                int greedyLevel = GetSkillLevel("욕심쟁이!");
                TowerSkillBlueprint greedySkill = null;
                if (greedyLevel > 0)
                {
                    greedySkill = System.Array.Find(towerSkills, skill => skill.skillName == "욕심쟁이!");
                }
                bomb.Setup(currentTarget.position, finalProjectileDamage, projectileSpeed, explosionRadius, towerType, damageType, slowAmount, slowDuration, greedySkill, greedyLevel);
            }
        }
        else
        {
            ProjectileController projectile = projectileGO_normal.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                if (spriteToUse != null)
                {
                    projectile.SetSprite(spriteToUse);
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

    private Transform FindHighestHealthEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null;
        float highestHealth = 0;
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy <= attackRange)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null && enemyHealth.MaxHealth > highestHealth)
                {
                    highestHealth = enemyHealth.MaxHealth;
                    bestTarget = enemy.transform;
                }
            }
        }
        return bestTarget;
    }

    private Transform FindFurthestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null;
        int maxWaypointIndex = -1;
        float minDistanceToNextWaypoint = float.MaxValue;
        Transform waypoints = GameManager.WaypointHolder;
        if (waypoints == null) return null;
        foreach (GameObject enemyGO in enemies)
        {
            float distanceToTower = Vector3.Distance(transform.position, enemyGO.transform.position);
            if (distanceToTower <= attackRange)
            {
                EnemyMovement enemy = enemyGO.GetComponent<EnemyMovement>();
                if (enemy == null) continue;
                int enemyWaypointIndex = enemy.GetCurrentWaypointIndex();
                if (enemyWaypointIndex >= waypoints.childCount) continue;
                if (enemyWaypointIndex > maxWaypointIndex)
                {
                    maxWaypointIndex = enemyWaypointIndex;
                    bestTarget = enemy.transform;
                    minDistanceToNextWaypoint = Vector3.Distance(enemy.transform.position, waypoints.GetChild(enemyWaypointIndex).position);
                }
                else if (enemyWaypointIndex == maxWaypointIndex)
                {
                    float distanceToNext = Vector3.Distance(enemy.transform.position, waypoints.GetChild(enemyWaypointIndex).position);
                    if (distanceToNext < minDistanceToNextWaypoint)
                    {
                        minDistanceToNextWaypoint = distanceToNext;
                        bestTarget = enemy.transform;
                    }
                }
            }
        }
        return bestTarget;
    }

    private Transform FindHighestHealthEnemyOnMap()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null;
        float highestHealth = 0;
        foreach (GameObject enemy in enemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.MaxHealth > highestHealth)
            {
                highestHealth = enemyHealth.MaxHealth;
                bestTarget = enemy.transform;
            }
        }
        return bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

