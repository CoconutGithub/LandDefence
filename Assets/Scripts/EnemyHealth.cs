using UnityEngine;
using UnityEngine.UI; // (추가) UI 요소를 사용하기 위해 추가합니다.

// 적의 체력과 관련된 모든 것을 관리하는 스크립트입니다.
public class EnemyHealth : MonoBehaviour
{
    [Header("능력치")]
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private int goldValue = 10;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject experienceOrbPrefab;
    [SerializeField]
    private Slider healthBarSlider; // (추가) 적의 체력을 표시할 UI 슬라이더입니다.
    [SerializeField]
    private GameObject healthBarCanvas; // (추가) 체력바를 담고 있는 캔버스입니다.

    private float currentHealth;
    private TowerType lastAttackerType;

    void Start()
    {
        currentHealth = maxHealth;
        // (추가) 체력바의 최대값을 1로 설정하고, 현재값을 1로 채웁니다. (비율로 제어)
        healthBarSlider.maxValue = 1f;
        healthBarSlider.value = 1f;
    }

    // (추가) LateUpdate는 모든 Update가 끝난 후 호출됩니다. 카메라를 따라가는 UI에 적합합니다.
    void LateUpdate()
    {
        // 체력바가 항상 메인 카메라를 바라보도록 하여 글자가 깨지거나 눕지 않게 합니다.
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void TakeDamage(float damageAmount, TowerType attackerType)
    {
        currentHealth -= damageAmount;
        lastAttackerType = attackerType;

        // (추가) 체력이 깎일 때마다 체력바의 값을 갱신합니다. (현재체력 / 최대체력)
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
