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

    // (추가) 불화살 스킬을 위한 지속 데미지 정보 변수
    private float dotDamagePerSecond;
    private float dotDuration;

    // (수정) Setup 함수에서 지속 데미지 정보를 선택적 파라미터로 함께 전달받습니다.
    public void Setup(Transform _target, float _damage, float _speed, TowerType _ownerType, DamageType _damageType, float _slowAmount, float _slowDuration, float _dotDamage = 0, float _dotDuration = 0)
    {
        target = _target;
        damage = _damage;
        moveSpeed = _speed;
        ownerTowerType = _ownerType;
        damageType = _damageType;
        slowAmount = _slowAmount;
        slowDuration = _slowDuration;
        dotDamagePerSecond = _dotDamage;
        dotDuration = _dotDuration;
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
            enemyHealth.TakeDamage(damage, ownerTowerType, damageType);
        }

        if (slowAmount > 0f)
        {
            EnemyMovement enemyMovement = target.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.ApplySlow(slowAmount, slowDuration);
            }
        }

        // (추가) 만약 지속 데미지 효과가 있는 공격이었다면, 적의 체력 스크립트에 효과를 적용합니다.
        if (dotDuration > 0 && enemyHealth != null)
        {
            enemyHealth.ApplyDot(dotDamagePerSecond, dotDuration);
        }

        Destroy(gameObject);
    }
}
