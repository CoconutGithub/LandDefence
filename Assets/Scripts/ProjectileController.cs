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
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // (추가) 스프라이트 렌더러 참조

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // (추가) 컴포넌트 찾기
        if (rb == null)
        {
            Debug.LogError("ProjectileController: Rigidbody 2D 컴포넌트를 찾을 수 없습니다! 프리팹에 추가해주세요.");
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("ProjectileController: Sprite Renderer 컴포넌트를 찾을 수 없습니다!");
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

    void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector2 direction = (Vector2)target.position - rb.position;
        direction.Normalize();

        rb.linearVelocity = direction * moveSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rb.rotation = angle;
    }
    
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (!this.enabled) return;
        this.enabled = false;

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

