//BarracksController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BarracksController : MonoBehaviour, IPointerClickHandler
{
    private bool isInRallyPointMode = false;

    [Header("병영 능력치")]
    [SerializeField]
    private int maxSoldiers = 3;
    [SerializeField]
    private float spawnRate = 10f;
    [SerializeField]
    private float spreadRadius = 1.0f;
    [SerializeField]
    private float rallyPointRange = 3f;

    [Header("업그레이드 및 스킬 정보")]
    public TowerBlueprint[] upgradePaths;
    public TowerSkillBlueprint[] towerSkills;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject soldierPrefab;
    [SerializeField]
    private GameObject rallyPointPrefab;

    private List<SoldierController> spawnedSoldiers = new List<SoldierController>();
    private float spawnCountdown = 0f;
    private Transform rallyPointInstance;
    private TowerSpotController parentSpot;

    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    private float originalSpawnRate;

    void Start()
    {
        rallyPointInstance = Instantiate(rallyPointPrefab, transform.position, Quaternion.identity).transform;
        rallyPointInstance.position = transform.position + new Vector3(0, -1.5f, 0);
        rallyPointInstance.gameObject.SetActive(false);
        
        foreach (var skill in towerSkills)
        {
            skillLevels[skill.skillName] = 0;
        }
        
        originalSpawnRate = spawnRate;

        for (int i = 0; i < maxSoldiers; i++)
        {
            SpawnSoldier();
        }
    }

    void Update()
    {
        if (spawnedSoldiers.Count < maxSoldiers)
        {
            spawnCountdown -= Time.deltaTime;
            if (spawnCountdown <= 0f)
            {
                SpawnSoldier();
                spawnCountdown = spawnRate;
            }
        }

        if (isInRallyPointMode)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 targetPos = new Vector3(mousePos.x, mousePos.y, transform.position.z);

                if (Vector3.Distance(transform.position, targetPos) <= rallyPointRange)
                {
                    rallyPointInstance.position = new Vector3(mousePos.x, mousePos.y, 0);
                    UpdateSoldierRallyPoints();
                    isInRallyPointMode = false;
                }
                else
                {
                    Debug.Log("너무 멀리 집결 지점을 설정할 수 없습니다!");
                }
            }
        }
    }
    
    public void Upgrade(TowerBlueprint blueprint)
    {
        if (GameManager.instance.SpendGold(blueprint.cost))
        {
            SoundManager.instance.PlayBuildSound();
            
            foreach (SoldierController soldier in spawnedSoldiers)
            {
                if (soldier != null)
                {
                    soldier.ReleaseEnemyBeforeDeath();
                    Destroy(soldier.gameObject);
                }
            }
            spawnedSoldiers.Clear();

            GameObject newBarracksGO = Instantiate(blueprint.prefab, transform.position, transform.rotation);
            newBarracksGO.GetComponent<BarracksController>().SetParentSpot(parentSpot);
            parentSpot.SetCurrentTower(newBarracksGO);
            
            Destroy(rallyPointInstance.gameObject);
            Destroy(gameObject);
        }
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
            Debug.Log($"{skillToUpgrade.skillName} 스킬 레벨 업! -> {skillLevels[skillToUpgrade.skillName]}");

            ApplyAllPassiveSkillEffectsToBarracks();

            foreach (var soldier in spawnedSoldiers)
            {
                if(soldier != null) ApplyAllPassiveSkillEffectsToSoldier(soldier);
            }

            TowerUpgradeUI.instance.Show(this);
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
        }
    }

    void ApplyAllPassiveSkillEffectsToBarracks()
    {
        spawnRate = originalSpawnRate;
        
        foreach (var skill in towerSkills)
        {
            int skillLevel = GetSkillLevel(skill.skillName);
            if (skillLevel > 0)
            {
                switch (skill.skillName)
                {
                    case "인해전술":
                        float reduction = skill.values1[skillLevel - 1];
                        spawnRate = Mathf.Max(originalSpawnRate - reduction, 1f);
                        break;
                }
            }
        }
    }

    // (수정) '방패 공격' 스킬 효과를 계산하도록 로직 추가
    void ApplyAllPassiveSkillEffectsToSoldier(SoldierController soldier)
    {
        float healthModifier = 0f;
        float damageModifier = 0f;
        float lifeSteal = 0f;
        float recognitionRadiusBonus = 0f;
        float aoeChance = 0f;
        float reflectionChance = 0f;
        float reflectionDuration = 0f;

        foreach (var skill in towerSkills)
        {
            int skillLevel = GetSkillLevel(skill.skillName);
            if (skillLevel > 0)
            {
                switch (skill.skillName)
                {
                    case "폭주":
                        healthModifier -= skill.values1[skillLevel - 1];
                        damageModifier += skill.values2[skillLevel - 1];
                        break;
                    case "체력 흡수":
                        lifeSteal = skill.values1[skillLevel - 1];
                        break;
                    case "우리는 하나!":
                        recognitionRadiusBonus = skill.values1[skillLevel - 1];
                        break;
                    case "빛이 있으라":
                        aoeChance = skill.values1[skillLevel - 1];
                        break;
                    case "방패 공격":
                        reflectionChance = 10f; // 10% 고정 확률
                        reflectionDuration = skill.values1[skillLevel - 1];
                        break;
                }
            }
        }
        soldier.ApplyStatModification(healthModifier, damageModifier, lifeSteal, recognitionRadiusBonus, aoeChance, reflectionChance, reflectionDuration);
    }
    
    public void SetParentSpot(TowerSpotController spot)
    {
        parentSpot = spot;
    }

    void SpawnSoldier()
    {
        GameObject soldierGO = Instantiate(soldierPrefab, transform.position, Quaternion.identity);
        SoldierController soldier = soldierGO.GetComponent<SoldierController>();

        if (soldier != null)
        {
            spawnedSoldiers.Add(soldier);
            soldier.SetBarracks(this);
            ApplyAllPassiveSkillEffectsToSoldier(soldier);
            UpdateSoldierRallyPoints();
        }
    }

    public void RemoveSoldier(SoldierController soldier)
    {
        if (spawnedSoldiers.Contains(soldier))
        {
            spawnedSoldiers.Remove(soldier);
            if(spawnedSoldiers.Count < maxSoldiers)
            {
                spawnCountdown = spawnRate;
            }
        }
    }
    
    void UpdateSoldierRallyPoints()
    {
        int soldierCount = spawnedSoldiers.Count;
        if (soldierCount == 0) return;

        float angleStep = 360f / soldierCount;

        for (int i = 0; i < soldierCount; i++)
        {
            if(spawnedSoldiers[i] == null) continue;

            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * spreadRadius;
            Vector3 targetPosition = rallyPointInstance.position + offset;
            spawnedSoldiers[i].SetRallyPointPosition(targetPosition);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        TowerUpgradeUI.instance.Show(this);
    }
    
    public void EnterRallyPointMode()
    {
        isInRallyPointMode = true;
        rallyPointInstance.gameObject.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rallyPointRange);
    }
}

