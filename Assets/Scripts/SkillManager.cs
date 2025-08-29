using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 모든 플레이어 스킬을 관리하는 스크립트입니다.
public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;

    [Header("번개 스킬 설정")]
    [SerializeField]
    private float lightningDamage = 150f;
    [SerializeField]
    private float lightningRadius = 2f;
    [SerializeField]
    private float lightningStunDuration = 2f;
    [SerializeField]
    private float lightningCooldown = 20f;
    [SerializeField]
    private LayerMask enemyLayer;

    [Header("UI 연결")]
    [SerializeField]
    private Button lightningSkillButton;
    [SerializeField]
    private Image cooldownImage;

    private bool isAimingSkill = false;
    private float currentCooldown = 0f;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0;
        }
        if (lightningSkillButton != null)
        {
            lightningSkillButton.interactable = true;
        }
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            UpdateCooldownUI();

            if (currentCooldown <= 0)
            {
                currentCooldown = 0;
                lightningSkillButton.interactable = true;
                cooldownImage.fillAmount = 0;
            }
        }

        if (isAimingSkill && Input.GetMouseButtonDown(0))
        {
            CastLightning();
        }
    }
    
    public void OnLightningSkillButton()
    {
        if (currentCooldown <= 0)
        {
            isAimingSkill = true;
            Debug.Log("번개 스킬 조준 모드 활성화. 맵을 클릭하여 스킬을 시전하세요.");
        }
    }

    void CastLightning()
    {
        // (수정) Input.mouse.position -> Input.mousePosition 오타 수정
        Vector3 castPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        castPosition.z = 0;

        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(castPosition, lightningRadius, enemyLayer);

        Debug.Log($"{enemiesToDamage.Length}명의 적에게 번개 스킬 발동!");

        foreach (Collider2D enemyCollider in enemiesToDamage)
        {
            EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // (수정) 이제 TowerType.None을 정상적으로 사용할 수 있습니다.
                enemyHealth.TakeDamage(lightningDamage, TowerType.None, DamageType.Magical);
            }

            EnemyMovement enemyMovement = enemyCollider.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.ApplySlow(1f, lightningStunDuration);
            }
        }
        
        SoundManager.instance.PlayHitSound();

        isAimingSkill = false;
        currentCooldown = lightningCooldown;
        lightningSkillButton.interactable = false;
        cooldownImage.fillAmount = 1;
    }
    
    void UpdateCooldownUI()
    {
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = currentCooldown / lightningCooldown;
        }
    }
}

