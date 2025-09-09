using UnityEngine;
using System.Collections.Generic;

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
    private SpriteRenderer spriteRenderer;

    private TowerSkillBlueprint greedySkill;
    private int greedySkillLevel;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

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
        // (추가) 목표 지점을 향해 날아가도록 방향을 계산하고 이미지를 회전시킵니다.
        if (moveSpeed > 0)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.right = direction; // 이미지의 오른쪽이 날아가는 방향을 보도록 설정
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        List<EnemyHealth> enemiesToHit = new List<EnemyHealth>();

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

        float damageToApply = damage;

        if (greedySkill != null && greedySkillLevel > 0)
        {
            int enemyCount = enemiesToHit.Count;
            if (enemyCount == 1 && greedySkill.values1.Length >= greedySkillLevel)
            {
                damageToApply = greedySkill.values1[greedySkillLevel - 1];
            }
            else if (enemyCount == 2 && greedySkill.values2.Length >= greedySkillLevel)
            {
                damageToApply = greedySkill.values2[greedySkillLevel - 1];
            }
            else if (enemyCount >= 3 && greedySkill.values3.Length >= greedySkillLevel)
            {
                damageToApply = greedySkill.values3[greedySkillLevel - 1];
            }
        }

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

