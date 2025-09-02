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

    // 어떤 스킬이 타겟팅 중인지 구분하기 위해 enum을 사용합니다.
    private enum TargetingSkill
    {
        None,
        Lightning,
        Vine,
        SkullMagic // (추가) 해골 마법 타겟팅 상태
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
    
    [Header("나무 덩굴 스킬 설정")]
    [SerializeField]
    private float vineRadius = 2.5f;
    [SerializeField]
    private float vineDuration = 4f;
    
    [Header("해골 마법 스킬 설정")] // (추가)
    [SerializeField]
    private float skullMagicHealthThreshold = 500f; // 이 체력 이하의 적만 즉사시킬 수 있습니다.

    // (추가) 분신술 스킬 설정
    [Header("분신술 스킬 설정")]
    [SerializeField]
    private float cloneDuration = 10f; // 분신 지속 시간

    [Header("공통 설정")]
    [SerializeField]
    private LayerMask enemyLayer;
    [SerializeField]
    private GameObject rangeIndicator;

    // (추가) HeroController 참조
    [Header("참조")]
    [SerializeField]
    private HeroController heroController;

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
            rangeIndicator.SetActive(false);
        }
    }

    void Update()
    {
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
            if (rangeIndicator != null && (currentTargetingSkill == TargetingSkill.Lightning || currentTargetingSkill == TargetingSkill.Vine))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                rangeIndicator.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                // (수정) 어떤 스킬이냐에 따라 다른 캐스팅 함수를 호출합니다.
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
            ShowRangeIndicator(lightningRadius);
        }
    }
    
    public void OnVineSkillButton()
    {
        Skill vineSkill = FindSkill("Vine");
        if (vineSkill != null && vineSkill.currentCooldown <= 0)
        {
            currentTargetingSkill = TargetingSkill.Vine;
            ShowRangeIndicator(vineRadius);
        }
    }

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
    
    // (추가) 해골 마법 버튼에 연결될 함수입니다.
    public void OnSkullMagicButton()
    {
        Skill skullMagicSkill = FindSkill("SkullMagic");
        if (skullMagicSkill != null && skullMagicSkill.currentCooldown <= 0)
        {
            // 이 스킬은 범위 표시기가 필요 없으므로 바로 타겟팅 모드로 들어갑니다.
            currentTargetingSkill = TargetingSkill.SkullMagic;
            Debug.Log("즉사시킬 적을 선택하세요!");
        }
    }
    
    // (추가) 분신술 버튼에 연결될 함수입니다.
    public void OnCloneSkillButton()
    {
        Skill cloneSkill = FindSkill("Clone"); // Inspector에서 skillName을 "Clone"으로 설정해야 합니다.
        if (cloneSkill != null && cloneSkill.currentCooldown <= 0)
        {
            if (heroController != null)
            {
                // HeroController에 분신술 활성화 함수를 호출합니다.
                heroController.ActivateCloneSkill(cloneDuration);
                StartCooldown(cloneSkill);
            }
            else
            {
                Debug.LogError("SkillManager에 HeroController가 연결되지 않았습니다!");
            }
        }
    }
    
    // --- 내부 로직 함수들 ---

    void CastLightning(Vector3 mousePosition)
    {
        Skill lightningSkill = FindSkill("Lightning");
        if (lightningSkill == null) return;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;

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
    
    // (추가) 해골 마법 시전 로직입니다.
    void CastSkullMagic(Vector3 mousePosition)
    {
        Skill skullMagicSkill = FindSkill("SkullMagic");
        if (skullMagicSkill == null) return;

        // 마우스 클릭 위치에 있는 적을 찾습니다.
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePosition), Vector2.zero, Mathf.Infinity, enemyLayer);

        if (hit.collider != null)
        {
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // 적의 최대 체력이 설정된 한계치 이하일 때만 즉사시킵니다.
                if (enemyHealth.MaxHealth <= skullMagicHealthThreshold)
                {
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
    
    private void ShowRangeIndicator(float radius)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(true);
            rangeIndicator.transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
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
