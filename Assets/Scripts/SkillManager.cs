using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 각 스킬의 정보를 하나로 묶어 관리하는 클래스입니다.
[System.Serializable]
public class Skill
{
    public string skillName; // 스킬 이름 (구분용)
    public Button skillButton; // 스킬 UI 버튼
    public Image cooldownImage; // 스킬 쿨다운 이미지
    public float cooldown; // 스킬의 총 쿨다운 시간
    [HideInInspector]
    public float currentCooldown; // 현재 남은 쿨다운 시간
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;

    private enum TargetingSkill
    {
        None,
        Lightning,
        Vine,
        SkullMagic
    }
    private TargetingSkill currentTargetingSkill = TargetingSkill.None;

    [Header("스킬 목록")]
    [SerializeField]
    private List<Skill> skills = new List<Skill>();

    [Header("번개 스킬 설정")]
    [SerializeField]
    private float lightningDamage = 50f;
    [SerializeField]
    private float lightningRadius = 2f;
    [SerializeField]
    private float lightningStunDuration = 2f;
    [SerializeField]
    private GameObject lightningEffectPrefab; 
    
    [Header("나무 덩굴 스킬 설정")]
    [SerializeField]
    private float vineRadius = 2.5f;
    [SerializeField]
    private float vineDuration = 4f;
    [SerializeField]
    private GameObject vineEffectPrefab; // (추가) 덩굴 이펙트 프리팹
    
    [Header("해골 마법 스킬 설정")]
    [SerializeField]
    private float skullMagicHealthThreshold = 500f;
    [SerializeField]
    private GameObject skullMagicEffectPrefab; // (추가) 해골 마법 이펙트 프리팹

    [Header("모래 지옥 스킬 설정")]
    [SerializeField]
    private float sandHellHealthThreshold = 700f;
    [SerializeField]
    private GameObject sandHellEffectPrefab; // (추가) 모래 지옥 이펙트 프리팹

    [Header("공통 설정")]
    [SerializeField]
    private LayerMask enemyLayer;
    [SerializeField]
    private GameObject rangeIndicator;
    [SerializeField]
    private Color lightningIndicatorColor = Color.cyan;
    [SerializeField]
    private Color vineIndicatorColor = Color.green;

    private SpriteRenderer rangeIndicatorRenderer;
    
    [Header("분신술 스킬 설정")]
    [SerializeField]
    private HeroController heroController;
    [SerializeField]
    private float cloneDuration = 10f;
    [SerializeField]
    private GameObject cloneEffectPrefab; // (추가) 분신술 이펙트 프리팹


    void Awake()
    {
        if (instance != null) { return; }
        instance = this;
    }

    void Start()
    {
        foreach (Skill skill in skills)
        {
            if (skill.cooldownImage != null)
            {
                skill.cooldownImage.fillAmount = 0;
            }
            skill.currentCooldown = 0;
        }

        if (rangeIndicator != null)
        {
            rangeIndicatorRenderer = rangeIndicator.GetComponent<SpriteRenderer>();
            rangeIndicator.SetActive(false);
        }
    }

    void Update()
    {
        // ... (Update 함수 내용은 이전과 동일) ...
        foreach (Skill skill in skills)
        {
            if (skill.currentCooldown > 0)
            {
                skill.currentCooldown -= Time.deltaTime;
                if (skill.cooldownImage != null)
                {
                    skill.cooldownImage.fillAmount = skill.currentCooldown / skill.cooldown;
                }

                if (skill.currentCooldown <= 0)
                {
                    skill.currentCooldown = 0;
                    if (skill.skillButton != null)
                    {
                        skill.skillButton.interactable = true;
                    }
                }
            }
        }

        if (currentTargetingSkill != TargetingSkill.None)
        {
            if (rangeIndicator != null && rangeIndicator.activeSelf)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                rangeIndicator.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                switch (currentTargetingSkill)
                {
                    case TargetingSkill.Lightning:
                        CastLightning(Input.mousePosition);
                        break;
                    case TargetingSkill.Vine:
                        CastVines(Input.mousePosition);
                        break;
                    case TargetingSkill.SkullMagic:
                        CastSkullMagic(Input.mousePosition);
                        break;
                }
                CancelTargeting();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CancelTargeting();
            }
        }
    }
    
    // --- 각 버튼에 연결될 함수들 ---
    
    public void OnLightningSkillButton()
    {
        Skill lightningSkill = FindSkill("Lightning");
        if (lightningSkill != null && lightningSkill.currentCooldown <= 0)
        {
            currentTargetingSkill = TargetingSkill.Lightning;
            ShowRangeIndicator(lightningRadius, lightningIndicatorColor);
        }
    }
    
    public void OnVineSkillButton()
    {
        Skill vineSkill = FindSkill("Vine");
        if (vineSkill != null && vineSkill.currentCooldown <= 0)
        {
            currentTargetingSkill = TargetingSkill.Vine;
            ShowRangeIndicator(vineRadius, vineIndicatorColor);
        }
    }
    
    public void OnCloneSkillButton()
    {
        Skill cloneSkill = FindSkill("Clone");
        if (cloneSkill != null && cloneSkill.currentCooldown <= 0)
        {
            if (heroController != null)
            {
                // (수정) 분신술 이펙트 생성
                if(cloneEffectPrefab != null)
                {
                    Instantiate(cloneEffectPrefab, heroController.transform.position, Quaternion.identity);
                }
                heroController.ActivateCloneSkill(cloneDuration);
                StartCooldown(cloneSkill);
            }
            else
            {
                Debug.LogError("HeroController가 SkillManager에 연결되지 않았습니다!");
            }
        }
    }
    
    public void OnSandHellButton()
    {
        Skill sandHellSkill = FindSkill("SandHell");
        if (sandHellSkill != null && sandHellSkill.currentCooldown <= 0)
        {
            CastSandHell();
            StartCooldown(sandHellSkill);
        }
    }

    // ... (Heal, SkullMagic 버튼 함수는 이전과 동일) ...
    public void OnHealLivesButton()
    {
        Skill healSkill = FindSkill("HealLives");
        if (healSkill != null && healSkill.currentCooldown <= 0)
        {
            GameManager.instance.HealLives(3);
            StartCooldown(healSkill);
        }
    }

    public void OnHealLivesPlusButton()
    {
        Skill healPlusSkill = FindSkill("HealLivesPlus");
        if (healPlusSkill != null && healPlusSkill.currentCooldown <= 0)
        {
            GameManager.instance.HealLives(5);
            StartCooldown(healPlusSkill);
        }
    }
    
    public void OnSkullMagicButton()
    {
        Skill skullMagicSkill = FindSkill("SkullMagic");
        if (skullMagicSkill != null && skullMagicSkill.currentCooldown <= 0)
        {
            currentTargetingSkill = TargetingSkill.SkullMagic;
            Debug.Log("즉사시킬 적을 선택하세요!");
        }
    }

    // --- 내부 로직 함수들 ---

    void CastLightning(Vector3 mousePosition)
    {
        Skill lightningSkill = FindSkill("Lightning");
        if (lightningSkill == null) return;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;

        if (lightningEffectPrefab != null)
        {
            Instantiate(lightningEffectPrefab, worldPosition, Quaternion.identity);
        }

        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(worldPosition, lightningRadius, enemyLayer);
        foreach (Collider2D enemyCollider in enemiesToDamage)
        {
            EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(lightningDamage, TowerType.None, DamageType.Magical);
            }
            
            EnemyMovement enemyMovement = enemyCollider.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.ApplySlow(1f, lightningStunDuration);
            }
        }
        
        StartCooldown(lightningSkill);
    }
    
    void CastVines(Vector3 mousePosition)
    {
        Skill vineSkill = FindSkill("Vine");
        if (vineSkill == null) return;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;

        // (추가) 덩굴 이펙트 생성
        if(vineEffectPrefab != null)
        {
            Instantiate(vineEffectPrefab, worldPosition, Quaternion.identity);
        }

        Collider2D[] enemiesToRoot = Physics2D.OverlapCircleAll(worldPosition, vineRadius, enemyLayer);
        foreach (Collider2D enemyCollider in enemiesToRoot)
        {
            EnemyMovement enemyMovement = enemyCollider.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.ApplyRoot(vineDuration);
            }
        }
        
        StartCooldown(vineSkill);
    }
    
    void CastSkullMagic(Vector3 mousePosition)
    {
        Skill skullMagicSkill = FindSkill("SkullMagic");
        if (skullMagicSkill == null) return;

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePosition), Vector2.zero, Mathf.Infinity, enemyLayer);

        if (hit.collider != null)
        {
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                if (enemyHealth.MaxHealth <= skullMagicHealthThreshold)
                {
                    // (추가) 해골 마법 이펙트를 적 위치에 생성
                    if(skullMagicEffectPrefab != null)
                    {
                        Instantiate(skullMagicEffectPrefab, hit.transform.position, Quaternion.identity);
                    }
                    enemyHealth.InstantKill();
                    StartCooldown(skullMagicSkill);
                }
                else
                {
                    Debug.Log("이 적은 너무 강해서 즉사시킬 수 없습니다!");
                }
            }
        }
    }

    void CastSandHell()
    {
        // (추가) 모래 지옥 이펙트 생성 (맵 중앙에 생성하는 예시)
        if(sandHellEffectPrefab != null)
        {
            Instantiate(sandHellEffectPrefab, Vector3.zero, Quaternion.identity);
        }

        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObject in allEnemies)
        {
            EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                if (enemyHealth.MaxHealth <= sandHellHealthThreshold)
                {
                    enemyHealth.InstantKill();
                }
            }
        }
    }
    
    // ... (나머지 함수들은 이전과 동일) ...
    private Skill FindSkill(string name)
    {
        return skills.Find(skill => skill.skillName == name);
    }
    
    private void StartCooldown(Skill skill)
    {
        skill.currentCooldown = skill.cooldown;
        if (skill.skillButton != null)
        {
            skill.skillButton.interactable = false;
        }
    }
    
    private void ShowRangeIndicator(float radius, Color color)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(true);
            rangeIndicator.transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
            
            if (rangeIndicatorRenderer != null)
            {
                rangeIndicatorRenderer.color = color;
            }
        }
    }

    private void CancelTargeting()
    {
        currentTargetingSkill = TargetingSkill.None;
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }
}

