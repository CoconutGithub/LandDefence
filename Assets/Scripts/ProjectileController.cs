using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private float moveSpeed;
    private float damage;
    private Transform target;
    private TowerType ownerTowerType;
    private DamageType damageType;
    private float slowAmount;   // (추가) 감속량
    private float slowDuration; // (추가) 감속 지속 시간

    // (수정) Setup 함수에서 감속 효과 정보도 함께 전달받습니다.
    public void Setup(Transform _target, float _damage, float _speed, TowerType _ownerType, DamageType _damageType, float _slowAmount, float _slowDuration)
    {
        target = _target;
        damage = _damage;
        moveSpeed = _speed;
        ownerTowerType = _ownerType;
        damageType = _damageType;
        slowAmount = _slowAmount;
        slowDuration = _slowDuration;
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

        // (추가) 만약 감속 효과가 있는 공격이었다면, 적의 이동 스크립트에 감속을 요청합니다.
        if (slowAmount > 0f)
        {
            EnemyMovement enemyMovement = target.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.ApplySlow(slowAmount, slowDuration);
            }
        }

        Destroy(gameObject);
    }
}
