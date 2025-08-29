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
        Vine
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

    [Header("공통 설정")]
    [SerializeField]
    private LayerMask enemyLayer;
    [SerializeField] // (추가) 스킬 범위 표시기로 사용할 프리팹 또는 게임 오브젝트입니다.
    private GameObject rangeIndicator;

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

        // (추가) 게임 시작 시 범위 표시기는 보이지 않도록 합니다.
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
            // (수정) 타겟팅 중일 때 범위 표시기가 마우스를 따라다니도록 합니다.
            if (rangeIndicator != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                rangeIndicator.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
            }
            
            // 왼쪽 마우스 클릭으로 스킬 시전
            if (Input.GetMouseButtonDown(0))
            {
                if (currentTargetingSkill == TargetingSkill.Lightning)
                {
                    CastLightning(Input.mousePosition);
                }
                else if (currentTargetingSkill == TargetingSkill.Vine)
                {
                    CastVines(Input.mousePosition);
                }
                CancelTargeting();
            }
            // (추가) 오른쪽 마우스 클릭으로 스킬 선택 취소
            else if (Input.GetMouseButtonDown(1))
            {
                CancelTargeting();
            }
        }
    }

    public void OnLightningSkillButton()
    {
        Skill lightningSkill = FindSkill("Lightning");
        if (lightningSkill != null && lightningSkill.currentCooldown <= 0)
        {
            currentTargetingSkill = TargetingSkill.Lightning;
            // (추가) 범위 표시기를 활성화하고 번개 스킬의 반경에 맞게 크기를 조절합니다.
            ShowRangeIndicator(lightningRadius);
        }
    }

    public void OnVineSkillButton()
    {
        Skill vineSkill = FindSkill("Vine");
        if (vineSkill != null && vineSkill.currentCooldown <= 0)
        {
            currentTargetingSkill = TargetingSkill.Vine;
            // (추가) 범위 표시기를 활성화하고 나무 덩굴 스킬의 반경에 맞게 크기를 조절합니다.
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
    
    // --- (추가) 범위 표시기 관련 함수들 ---
    private void ShowRangeIndicator(float radius)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(true);
            // 원의 반경(radius)은 지름(scale)의 절반이므로, 2를 곱해줍니다.
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

