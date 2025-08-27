using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private int goldValue = 10;
    [SerializeField]
    private GameObject experienceOrbPrefab;

    private float currentHealth;
    private TowerType lastAttackerType; // (수정) 마지막으로 공격한 타워의 종류를 저장

    void Start()
    {
        currentHealth = maxHealth;
    }

    // (수정) TakeDamage 함수에서 공격한 타워의 종류도 함께 받습니다.
    public void TakeDamage(float damageAmount, TowerType attackerType)
    {
        currentHealth -= damageAmount;
        lastAttackerType = attackerType; // 마지막 공격자 정보를 갱신

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        SoundManager.instance.PlayDeathSound();
        GameManager.instance.AddGold(goldValue);

        // (수정) 경험치 구슬을 드랍할 때, 어떤 종류의 경험치인지 알려줍니다.
        GameObject orbGO = Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity);
        orbGO.GetComponent<ExperienceController>().Setup(5, lastAttackerType); // 예시로 5 경험치

        GameManager.instance.EnemyDefeated();
        Destroy(gameObject);
    }
}
