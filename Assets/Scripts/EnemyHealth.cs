using UnityEngine;
using UnityEngine.UI;

// 적의 체력과 방어력을 관리하는 스크립트입니다.
public class EnemyHealth : MonoBehaviour
{
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

    void LateUpdate()
    {
        if (healthBarCanvas != null && healthBarCanvas.activeInHierarchy)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void TakeDamage(float rawDamage, TowerType attackerType, DamageType damageType)
    {
        if (currentHealth <= 0) return;

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

        // (수정) 최초 데미지(rawDamage)가 1 이상인 공격에 대해서만 최소 데미지를 1로 보정합니다.
        if (rawDamage >= 1.0f)
        {
            finalDamage = Mathf.Max(finalDamage, 1);
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

