using UnityEngine;

// 폭탄 발사체의 이동과 폭발을 담당하는 스크립트입니다.
public class BombProjectileController : MonoBehaviour
{
    private float moveSpeed;
    private float damage;
    private float explosionRadius;
    private Vector3 targetPosition; // (수정) 움직이는 목표(Transform) 대신 고정된 위치(Vector3)를 저장합니다.
    private TowerType ownerTowerType;
    private DamageType damageType;

    // (수정) 타워로부터 목표의 '위치'를 전달받도록 함수를 변경합니다.
    public void Setup(Vector3 _targetPosition, float _damage, float _speed, float _explosionRadius, TowerType _ownerType, DamageType _damageType)
    {
        targetPosition = _targetPosition;
        damage = _damage;
        moveSpeed = _speed;
        explosionRadius = _explosionRadius;
        ownerTowerType = _ownerType;
        damageType = _damageType;
    }

    void Update()
    {
        // (수정) 더 이상 목표가 사라졌는지 확인할 필요 없이, 고정된 목표 지점을 향해 이동합니다.
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 목표 지점에 거의 도달했다면 폭발합니다.
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    // 폭발하여 주변의 모든 적에게 피해를 주는 함수입니다.
    void Explode()
    {
        // 폭발 사운드를 재생합니다. (SoundManager에 추가 필요)
        // SoundManager.instance.PlayExplosionSound();

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
