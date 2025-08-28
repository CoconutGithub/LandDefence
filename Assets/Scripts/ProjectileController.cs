using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private float moveSpeed;
    private float damage;
    private Transform target;
    private TowerType ownerTowerType;
    private DamageType damageType; // 이 발사체의 데미지 타입

    // Setup 함수에서 데미지 타입도 함께 전달받습니다.
    public void Setup(Transform _target, float _damage, float _speed, TowerType _ownerType, DamageType _damageType)
    {
        target = _target;
        damage = _damage;
        moveSpeed = _speed;
        ownerTowerType = _ownerType;
        damageType = _damageType;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        SoundManager.instance.PlayHitSound();
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // 적에게 데미지를 줄 때, 데미지 타입도 함께 알려줍니다.
            enemyHealth.TakeDamage(damage, ownerTowerType, damageType);
        }
        Destroy(gameObject);
    }
}
