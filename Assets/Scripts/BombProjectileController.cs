using UnityEngine;

// 폭탄 발사체의 이동과 폭발을 담당하는 스크립트입니다.
public class BombProjectileController : MonoBehaviour
{
    private float moveSpeed;
    private float damage;
    private float explosionRadius; // 폭발 반경
    private Transform target;
    private TowerType ownerTowerType;
    private DamageType damageType;

    // 타워로부터 필요한 모든 정보를 전달받는 함수입니다.
    public void Setup(Transform _target, float _damage, float _speed, float _explosionRadius, TowerType _ownerType, DamageType _damageType)
    {
        target = _target;
        damage = _damage;
        moveSpeed = _speed;
        explosionRadius = _explosionRadius;
        ownerTowerType = _ownerType;
        damageType = _damageType;
    }

    void Update()
    {
        // 목표가 사라졌다면, 그 자리에 즉시 폭발합니다.
        if (target == null)
        {
            Explode();
            return;
        }

        // 목표를 향해 이동합니다.
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // 목표에 거의 도달했다면 폭발합니다.
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            Explode();
        }
    }

    // 폭발하여 주변의 모든 적에게 피해를 주는 함수입니다.
    void Explode()
    {
        // 폭발 사운드를 재생합니다. (SoundManager에 추가 필요)
        // SoundManager.instance.PlayExplosionSound();

        // 지정된 폭발 반경 내에 있는 모든 Collider를 찾습니다.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        // 찾은 모든 Collider들을 순회합니다.
        foreach (Collider2D hit in colliders)
        {
            // 만약 Collider가 "Enemy" 태그를 가지고 있다면
            if (hit.CompareTag("Enemy"))
            {
                // 해당 적의 EnemyHealth 스크립트를 가져와 데미지를 줍니다.
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, ownerTowerType, damageType);
                }
            }
        }

        // 폭발 후, 폭탄 자신은 파괴됩니다.
        Destroy(gameObject);
    }

    // (추가) Scene 뷰에서 폭발 범위를 시각적으로 확인하기 위한 기즈모입니다.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
