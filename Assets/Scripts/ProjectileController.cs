using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private float moveSpeed;
    private float damage;
    private Transform target;

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
        // (수정) 적에게 맞았을 때 사운드를 재생합니다.
        SoundManager.instance.PlayHitSound();

        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
