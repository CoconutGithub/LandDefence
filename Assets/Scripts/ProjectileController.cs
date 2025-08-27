using UnityEngine;

// 발사체의 이동과 소멸을 담당하는 스크립트입니다.
public class ProjectileController : MonoBehaviour
{
    // (수정) 이제 능력치를 타워로부터 전달받으므로, Inspector에서 설정할 필요가 없습니다.
    private float moveSpeed;
    private float damage;

    private Transform target;

    // (수정) SetTarget 함수를 Setup 함수로 변경하여 공격력과 속도도 함께 전달받습니다.
    public void Setup(Transform _target, float _damage, float _speed)
    {
        target = _target;
        damage = _damage;
        moveSpeed = _speed;
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
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // 설정된 damage 값으로 공격합니다.
            enemyHealth.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
