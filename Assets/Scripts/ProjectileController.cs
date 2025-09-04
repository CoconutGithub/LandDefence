//ProjectileController.cs
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
    // (추가) 넉백 효과가 적용될 범위를 저장할 변수
    private float knockbackRadius;

    // (수정) Setup 함수에서 넉백 반경(_knockbackRadius)도 함께 전달받도록 파라미터를 추가합니다.
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
        
        // (수정) 넉백 효과 적용 로직을 범위 공격으로 변경합니다.
        if (knockbackDistance > 0f)
        {
            // 발사체가 부딪힌 위치를 중심으로 knockbackRadius 범위 내의 모든 콜라이더를 찾습니다.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, knockbackRadius);
            foreach (var hitCollider in colliders)
            {
                // 찾은 콜라이더가 적이라면 넉백 효과를 적용합니다.
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

