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
    private float physicalDefense = 10f; // 물리 방어력
    [SerializeField]
    private float magicalDefense = 5f;  // 마법 방어력

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

    // (수정) TakeDamage 함수에서 데미지 타입도 함께 받도록 수정합니다.
    public void TakeDamage(float damageAmount, TowerType attackerType, DamageType damageType)
    {
        lastAttackerType = attackerType;

        // 데미지 타입에 따라 최종 데미지를 계산합니다.
        float finalDamage = 0f;
        if (damageType == DamageType.Physical)
        {
            // 방어력 1당 1%의 데미지 감소 효과 (최대 90%까지)
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
        
        GameObject orbGO = Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity);
        orbGO.GetComponent<ExperienceController>().Setup(5, lastAttackerType);

        GameManager.instance.EnemyDefeated();
        Destroy(gameObject);
    }
}
