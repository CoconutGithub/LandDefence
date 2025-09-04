using UnityEngine;
using System.Collections.Generic;

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
    private TowerSkillBlueprint greedySkill;
    private int greedySkillLevel;
    
    private SpriteRenderer spriteRenderer; // (추가) 스프라이트 렌더러 참조

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // (추가) 컴포넌트 찾기
        if (spriteRenderer == null)
        {
            Debug.LogError("BombProjectileController: Sprite Renderer 컴포넌트를 찾을 수 없습니다!");
        }
    }
    
    // (추가) 외부(TowerController)에서 이 발사체의 이미지를 변경할 수 있도록 하는 함수
    public void SetSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
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
