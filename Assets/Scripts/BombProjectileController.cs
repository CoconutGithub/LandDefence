//BombProjectileController.cs
using UnityEngine;
using System.Collections.Generic; // List 사용을 위해 추가

// 폭탄 발사체의 이동과 폭발을 담당하는 스크립트입니다.
public class BombProjectileController : MonoBehaviour
{
    private float moveSpeed;
    private float damage;
    private float explosionRadius;
    private Vector3 targetPosition;
    private TowerType ownerTowerType;
    private DamageType damageType;
    private float slowAmount;
    private float slowDuration;

    // (추가) 욕심쟁이! 스킬을 위한 변수
    private TowerSkillBlueprint greedySkill;
    private int greedySkillLevel;

    // (수정) 욕심쟁이! 스킬 정보를 함께 전달받도록 함수를 변경합니다.
    public void Setup(Vector3 _targetPosition, float _damage, float _speed, float _explosionRadius, TowerType _ownerType, DamageType _damageType, float _slowAmount, float _slowDuration, TowerSkillBlueprint _greedySkill = null, int _greedySkillLevel = 0)
    {
        targetPosition = _targetPosition;
        damage = _damage;
        moveSpeed = _speed;
        explosionRadius = _explosionRadius;
        ownerTowerType = _ownerType;
        damageType = _damageType;
        slowAmount = _slowAmount;
        slowDuration = _slowDuration;
        greedySkill = _greedySkill;
        greedySkillLevel = _greedySkillLevel;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    // (수정) 폭발 시 욕심쟁이! 스킬 로직을 적용하여 피해량을 계산합니다.
    void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        List<EnemyHealth> enemiesToHit = new List<EnemyHealth>();

        // 1. 유효한 적 타겟의 목록을 먼저 만듭니다.
        foreach (Collider2D hit in colliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemiesToHit.Add(enemyHealth);
                }
            }
        }

        float damageToApply = damage; // 기본 피해량으로 시작

        // 2. 욕심쟁이! 스킬이 있고, 레벨이 0보다 크면 피해량을 재계산합니다.
        if (greedySkill != null && greedySkillLevel > 0)
        {
            int enemyCount = enemiesToHit.Count;
            if (enemyCount == 1 && greedySkill.values1.Length >= greedySkillLevel)
            {
                damageToApply = greedySkill.values1[greedySkillLevel - 1]; // 1명 명중 시 데미지
            }
            else if (enemyCount == 2 && greedySkill.values2.Length >= greedySkillLevel)
            {
                damageToApply = greedySkill.values2[greedySkillLevel - 1]; // 2명 명중 시 데미지
            }
            else if (enemyCount >= 3 && greedySkill.values3.Length >= greedySkillLevel)
            {
                damageToApply = greedySkill.values3[greedySkillLevel - 1]; // 3명 이상 명중 시 데미지
            }
        }

        // 3. 계산된 최종 피해량을 모든 유효 타겟에게 적용합니다.
        foreach (EnemyHealth enemy in enemiesToHit)
        {
            enemy.TakeDamage(damageToApply, ownerTowerType, damageType);

            if (slowAmount > 0)
            {
                EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
                if (enemyMovement != null)
                {
                    enemyMovement.ApplySlow(slowAmount, slowDuration);
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

