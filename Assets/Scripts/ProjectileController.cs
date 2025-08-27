using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private float moveSpeed;
    private float damage;
    private Transform target;
    private TowerType ownerTowerType; // (수정) 이 발사체를 쏜 타워의 종류를 저장할 변수

    // (수정) Setup 함수에서 타워 종류(TowerType)도 함께 전달받습니다.
    public void Setup(Transform _target, float _damage, float _speed, TowerType _ownerType)
    {
        target = _target;
        damage = _damage;
        moveSpeed = _speed;
        ownerTowerType = _ownerType;
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
            // (수정) 적에게 데미지를 줄 때, 어떤 종류의 타워가 공격했는지도 알려줍니다.
            enemyHealth.TakeDamage(damage, ownerTowerType);
        }
        Destroy(gameObject);
    }
}
