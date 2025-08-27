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
        // (수정) 적이 죽었을 때 사운드를 재생합니다.
        SoundManager.instance.PlayDeathSound();

        GameManager.instance.AddGold(goldValue);
        Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity);
        GameManager.instance.EnemyDefeated();
        Destroy(gameObject);
    }
}
