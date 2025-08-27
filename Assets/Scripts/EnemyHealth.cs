using UnityEngine;

// 적의 체력과 관련된 모든 것을 관리하는 스크립트입니다.
public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private int goldValue = 10;
    [SerializeField]
    private GameObject experienceOrbPrefab;

    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameManager.instance.AddGold(goldValue);
        Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity);

        // (수정) 적이 죽었음을 GameManager에 알립니다.
        GameManager.instance.EnemyDefeated();

        Destroy(gameObject);
    }
}
