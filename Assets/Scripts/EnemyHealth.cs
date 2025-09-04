//EnemyHealth.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // List 사용을 위해 추가

public class EnemyHealth : MonoBehaviour
{
    // (추가) 지속 데미지 효과를 관리하기 위한 내부 클래스
    [System.Serializable]
    public class DotEffect
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
    private bool isDead = false; // 중복 사망 방지 플래그
    private List<DotEffect> activeDotEffects = new List<DotEffect>(); // (추가) 적용된 지속 데미지 효과 목록

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
        if (currentHealth <= 0 && !isDead)
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

    // (추가) ProjectileController에서 호출할 지속 데미지 적용 함수
    public void ApplyDotEffect(float damagePerSecond, float duration)
    {
        activeDotEffects.Add(new DotEffect { DamagePerSecond = damagePerSecond, RemainingDuration = duration });
    }

    public void TakeDamage(float rawDamage, TowerType attackerType, DamageType damageType)
    {
        if (isDead) return;

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
        
        // 레이저처럼 1 미만의 데미지는 그대로 적용하고, 그 외의 공격은 최소 1의 데미지를 보장합니다.
        if (rawDamage > 0 && rawDamage < 1)
        {
            // Do nothing, let the small damage apply as is (for lasers)
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

            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }
    }
    
    public void InstantKill()
    {
        if (isDead) return;
        currentHealth = 0;
        UpdateHealthBar();
        Die();
    }

    void Die()
    {
        isDead = true;

        if (lastAttackerType != TowerType.None && lastAttackerType != TowerType.Hero)
        {
            GameObject orb = Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity);
            if(orb.GetComponent<ExperienceController>() != null)
            {
                orb.GetComponent<ExperienceController>().Setup(experienceValue, lastAttackerType);
            }
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

