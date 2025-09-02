using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // (추가) List 사용을 위해 추가

// 적의 체력과 방어력을 관리하는 스크립트입니다.
public class EnemyHealth : MonoBehaviour
{
    // (추가) 지속 데미지 효과(DOT)의 정보를 담는 내부 클래스입니다.
    private class DamageOverTimeEffect
    {
        public float DamagePerSecond;
        public float RemainingDuration;
    }

    [Header("능력치")]
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private int goldValue = 10;
    [SerializeField]
    private int experienceValue = 5;
    [SerializeField]
    private float physicalDefense = 0f;
    [SerializeField]
    private float magicalDefense = 0f;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject experienceOrbPrefab;
    [SerializeField]
    private Slider healthBarSlider;
    [SerializeField]
    private GameObject healthBarCanvas;

    public float MaxHealth { get { return maxHealth; } }
    
    private float currentHealth;
    private TowerType lastAttackerType = TowerType.None;
    private bool isDead = false; // (추가) 중복 사망 처리를 방지하기 위한 플래그

    // (추가) 이 적에게 적용된 모든 지속 데미지 효과를 관리하는 리스트입니다.
    private List<DamageOverTimeEffect> activeDotEffects = new List<DamageOverTimeEffect>();

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarCanvas.SetActive(false);
            healthBarSlider.maxValue = 1f;
            healthBarSlider.value = 1f;
        }
    }

    // (추가) Update 함수에서 지속 데미지 효과를 매 프레임 처리합니다.
    void Update()
    {
        if (isDead) return;

        // 지속 데미지 효과 처리
        if (activeDotEffects.Count > 0)
        {
            // 리스트를 뒤에서부터 순회해야 요소를 안전하게 제거할 수 있습니다.
            for (int i = activeDotEffects.Count - 1; i >= 0; i--)
            {
                var dot = activeDotEffects[i];
                currentHealth -= dot.DamagePerSecond * Time.deltaTime;
                dot.RemainingDuration -= Time.deltaTime;

                if (dot.RemainingDuration <= 0)
                {
                    activeDotEffects.RemoveAt(i);
                }
            }
            UpdateHealthBar();
        }

        // 지속 데미지로 인해 체력이 0 이하가 되었는지 확인합니다.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null && healthBarCanvas.activeInHierarchy)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    // (추가) 외부(발사체 등)에서 이 적에게 지속 데미지를 적용하기 위한 함수입니다.
    public void ApplyDot(float damagePerSecond, float duration)
    {
        activeDotEffects.Add(new DamageOverTimeEffect
        {
            DamagePerSecond = damagePerSecond,
            RemainingDuration = duration
        });
    }

    public void TakeDamage(float rawDamage, TowerType attackerType, DamageType damageType)
    {
        if (currentHealth <= 0 || isDead) return;

        lastAttackerType = attackerType;

        float defense = 0;
        if (damageType == DamageType.Physical)
        {
            defense = physicalDefense;
        }
        else if (damageType == DamageType.Magical)
        {
            defense = magicalDefense;
        }

        float finalDamage = rawDamage * (1 - defense / 100);

        // (수정) 의도된 피해량(rawDamage)이 1보다 작고 0보다 클 때는 최소 데미지 보정을 적용하지 않습니다.
        if (rawDamage > 0 && rawDamage < 1) 
        {
             // 레이저 같은 지속적인 약한 데미지는 그대로 적용
        }
        else if (rawDamage > 0)
        {
            finalDamage = Mathf.Max(finalDamage, 1);
        }
        else
        {
            finalDamage = 0;
        }

        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;
            UpdateHealthBar();
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }
    
    public void InstantKill()
    {
        currentHealth = 0;
        UpdateHealthBar();
        Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (lastAttackerType != TowerType.None && lastAttackerType != TowerType.Hero)
        {
            GameObject orb = Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity);
            orb.GetComponent<ExperienceController>().Setup(experienceValue, lastAttackerType);
        }
        
        GameManager.instance.AddGold(goldValue);
        GameManager.instance.EnemyDefeated();
        SoundManager.instance.PlayDeathSound();
        Destroy(gameObject);
    }
    
    void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarCanvas.SetActive(currentHealth < maxHealth && currentHealth > 0);
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }
}

