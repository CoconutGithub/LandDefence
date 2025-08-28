using UnityEngine;
using UnityEngine.UI;

// 적의 체력과 관련된 모든 것을 관리하는 스크립트입니다.
public class EnemyHealth : MonoBehaviour
{
    [Header("능력치")]
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private int goldValue = 10;
    [SerializeField]
    private float physicalDefense = 10f;
    [SerializeField]
    private float magicalDefense = 5f;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject experienceOrbPrefab;
    [SerializeField]
    private Slider healthBarSlider;
    [SerializeField]
    private GameObject healthBarCanvas;

    private float currentHealth;
    private TowerType lastAttackerType;

    void Start()
    {
        currentHealth = maxHealth;
        healthBarSlider.maxValue = 1f;
        healthBarSlider.value = 1f;
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void TakeDamage(float damageAmount, TowerType attackerType, DamageType damageType)
    {
        lastAttackerType = attackerType;

        float finalDamage = 0f;
        if (damageType == DamageType.Physical)
        {
            float damageReduction = Mathf.Clamp(physicalDefense / 100f, 0f, 0.9f);
            finalDamage = damageAmount * (1 - damageReduction);
        }
        else if (damageType == DamageType.Magical)
        {
            float damageReduction = Mathf.Clamp(magicalDefense / 100f, 0f, 0.9f);
            finalDamage = damageAmount * (1 - damageReduction);
        }

        currentHealth -= finalDamage;
        healthBarSlider.value = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        SoundManager.instance.PlayDeathSound();
        GameManager.instance.AddGold(goldValue);
        
        // (수정) 마지막 공격자가 영웅(Hero)이 아닐 경우에만 경험치 구슬을 드랍합니다.
        if (lastAttackerType != TowerType.Hero)
        {
            GameObject orbGO = Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity);
            orbGO.GetComponent<ExperienceController>().Setup(5, lastAttackerType);
        }

        GameManager.instance.EnemyDefeated();
        Destroy(gameObject);
    }
}
