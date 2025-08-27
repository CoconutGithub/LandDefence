using UnityEngine;

// 적의 체력과 관련된 모든 것을 관리하는 스크립트입니다.
public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private int goldValue = 10;
    [SerializeField]
    private GameObject experienceOrbPrefab; // (수정) 드랍할 경험치 구슬 프리팹입니다.

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

        // (수정) 경험치 구슬을 현재 위치에 생성합니다. Quaternion.identity는 회전 없음을 의미합니다.
        Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
