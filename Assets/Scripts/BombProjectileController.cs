using UnityEngine;

// 폭탄 발사체의 이동과 폭발을 담당하는 스크립트입니다.
public class BombProjectileController : MonoBehaviour
{
    private float moveSpeed;
    private float damage;
    private float explosionRadius;
    private Vector3 targetPosition;
    private TowerType ownerTowerType;
    private DamageType damageType;
    // (추가) 둔화 효과를 위한 변수
    private float slowAmount;
    private float slowDuration;

    // (수정) 둔화 효과 정보를 함께 전달받도록 함수를 변경합니다.
    public void Setup(Vector3 _targetPosition, float _damage, float _speed, float _explosionRadius, TowerType _ownerType, DamageType _damageType, float _slowAmount, float _slowDuration)
    {
        targetPosition = _targetPosition;
        damage = _damage;
        moveSpeed = _speed;
        explosionRadius = _explosionRadius;
        ownerTowerType = _ownerType;
        damageType = _damageType;
        slowAmount = _slowAmount;
        slowDuration = _slowDuration;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    // (수정) 폭발 시 둔화 효과를 적용하는 로직을 추가합니다.
    void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in colliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, ownerTowerType, damageType);
                }

                // 둔화 효과가 있을 경우, 적에게 둔화를 적용합니다.
                if (slowAmount > 0)
                {
                    EnemyMovement enemyMovement = hit.GetComponent<EnemyMovement>();
                    if (enemyMovement != null)
                    {
                        enemyMovement.ApplySlow(slowAmount, slowDuration);
                    }
                }
            }
        }
        
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
