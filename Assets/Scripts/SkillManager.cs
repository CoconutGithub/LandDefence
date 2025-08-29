using UnityEngine;
using UnityEngine.UI; // Image, Button을 사용하기 위해 필요합니다.
using TMPro; // TextMeshPro를 사용하기 위해 필요합니다.
using System.Collections.Generic; // List를 사용하기 위해 필요합니다.

// (수정) 각 스킬의 정보를 하나로 묶어 관리하는 클래스를 새로 정의합니다.
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

    [Header("스킬 목록")]
    // (수정) 여러 스킬을 관리하기 위해 List를 사용합니다.
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
    private LayerMask enemyLayer;

    private bool isSelectingTarget = false;

    void Awake()
    {
        if (instance != null) { return; }
        instance = this;
    }

    void Start()
    {
        // 모든 스킬의 쿨다운 UI를 초기화합니다.
        foreach (Skill skill in skills)
        {
            if (skill.cooldownImage != null)
            {
                skill.cooldownImage.fillAmount = 0;
            }
            skill.currentCooldown = 0;
        }
    }

    void Update()
    {
        // (수정) 모든 스킬의 쿨다운을 한 번에 관리합니다.
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

        if (isSelectingTarget)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CastLightning(Input.mousePosition);
                isSelectingTarget = false;
            }
        }
    }
    
    // --- 각 버튼에 연결될 함수들 ---

    public void OnLightningSkillButton()
    {
        // (수정) "Lightning" 이라는 이름의 스킬을 찾아 쿨다운을 확인합니다.
        Skill lightningSkill = FindSkill("Lightning");
        if (lightningSkill != null && lightningSkill.currentCooldown <= 0)
        {
            isSelectingTarget = true;
        }
    }

    public void OnHealLivesButton()
    {
        // (수정) "HealLives" 라는 이름의 스킬을 찾아 쿨다운을 확인하고 사용합니다.
        Skill healSkill = FindSkill("HealLives");
        if (healSkill != null && healSkill.currentCooldown <= 0)
        {
            GameManager.instance.HealLives(3);
            StartCooldown(healSkill);
        }
    }

    public void OnHealLivesPlusButton()
    {
        // (수정) "HealLivesPlus" 라는 이름의 스킬을 찾아 쿨다운을 확인하고 사용합니다.
        Skill healPlusSkill = FindSkill("HealLivesPlus");
        if (healPlusSkill != null && healPlusSkill.currentCooldown <= 0)
        {
            GameManager.instance.HealLives(5);
            StartCooldown(healPlusSkill);
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
                // 기절 효과 (둔화 100%)
                enemyMovement.ApplySlow(1f, lightningStunDuration);
            }
        }
        
        StartCooldown(lightningSkill);
    }

    // (추가) 스킬 이름을 통해 리스트에서 특정 스킬을 찾는 헬퍼 함수
    private Skill FindSkill(string name)
    {
        return skills.Find(skill => skill.skillName == name);
    }
    
    // (추가) 스킬 사용 후 쿨다운을 시작하는 헬퍼 함수
    private void StartCooldown(Skill skill)
    {
        skill.currentCooldown = skill.cooldown;
        if (skill.skillButton != null)
        {
            skill.skillButton.interactable = false;
        }
    }
}

