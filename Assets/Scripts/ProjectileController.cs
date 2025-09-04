using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private float moveSpeed;
    private float damage;
    private Transform target;
    private TowerType ownerTowerType;
    private DamageType damageType;
    private float slowAmount;
    private float slowDuration;
    private float dotDamagePerSecond;
    private float dotDuration;
    private float knockbackDistance;
    private float knockbackRadius;

    // (수정) 더 이상 필요 없으므로 rotationOffset 변수를 제거했습니다.

    public void Setup(Transform _target, float _damage, float _speed, TowerType _ownerType, DamageType _damageType, float _slowAmount, float _slowDuration, float _dotDamage, float _dotDuration, float _knockbackDistance = 0, float _knockbackRadius = 0)
    {
        target = _target;
        damage = _damage;
        moveSpeed = _speed;
        ownerTowerType = _ownerType;
        damageType = _damageType;
        slowAmount = _slowAmount;
        slowDuration = _slowDuration;
        dotDamagePerSecond = _dotDamage;
        this.dotDuration = _dotDuration;
        knockbackDistance = _knockbackDistance;
        knockbackRadius = _knockbackRadius;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // --- (수정) 발사체 회전 로직 변경 ---
        // 1. 타겟을 향하는 방향 벡터를 계산하고 정규화(normalized)합니다.
        Vector3 direction = (target.position - transform.position).normalized;
        // 2. 발사체의 오른쪽 방향(transform.right)을 계산된 방향 벡터로 직접 설정합니다.
        //    이렇게 하면 스프라이트의 오른쪽(일반적인 2D 이미지의 앞쪽)이 항상 타겟을 향하게 됩니다.
        transform.right = direction;
        // --- (수정 끝) ---

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
            if(damage > 0)
            {
                enemyHealth.TakeDamage(damage, ownerTowerType, damageType);
            }
            if(dotDamagePerSecond > 0 && dotDuration > 0)
            {
                enemyHealth.ApplyDotEffect(dotDamagePerSecond, dotDuration);
            }
        }

        if (slowAmount > 0f)
        {
            EnemyMovement enemyMovement = target.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.ApplySlow(slowAmount, slowDuration);
            }
        }
        
        if (knockbackDistance > 0f)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, knockbackRadius);
            foreach (var hitCollider in colliders)
            {
                EnemyMovement enemyMovement = hitCollider.GetComponent<EnemyMovement>();
                if (enemyMovement != null)
                {
                    enemyMovement.Knockback(knockbackDistance);
                }
            }
        }

        Destroy(gameObject);
    }
}

