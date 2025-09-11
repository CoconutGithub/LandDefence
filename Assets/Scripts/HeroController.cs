using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 영웅의 이동, 전투 등 모든 것을 관리하는 스크립트입니다.
public class HeroController : MonoBehaviour
{
    [Header("영웅 능력치")]
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float maxHealth = 500f;
    [SerializeField]
    private float attackDamage = 30f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;
    [SerializeField]
    private float healthRegenRate = 10f;
    [SerializeField]
    private float timeToStartRegen = 5f;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private Slider healthBarSlider;
    [SerializeField]
    private GameObject healthBarCanvas;
    [Header("분신술 스킬")]
    [SerializeField]
    private GameObject heroClonePrefab;
    [SerializeField]
    private Transform cloneSpawnPointLeft;
    [SerializeField]
    private Transform cloneSpawnPointRight;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private float currentHealth;
    private float attackCountdown = 0f;
    private EnemyMovement currentTarget;
    private float timeSinceLastCombat = 0f;
    private List<GameObject> activeClones = new List<GameObject>();
    private AnimationController animationController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animationController = GetComponent<AnimationController>();
        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = 1f;
            healthBarSlider.value = 1f;
        }
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(moveX, moveY).normalized;

        if (animationController != null)
        {
            animationController.SetAnimationBool("IsMoving", movementInput.sqrMagnitude > 0);
        }

        if (Mathf.Abs(moveX) > 0.01f)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(moveX);
            transform.localScale = newScale;
        }

        attackCountdown -= Time.deltaTime;

        if (currentTarget == null)
        {
            timeSinceLastCombat += Time.deltaTime;
            if (timeSinceLastCombat >= timeToStartRegen && currentHealth < maxHealth)
            {
                RegenerateHealth();
            }
        }
        else
        {
            timeSinceLastCombat = 0f;
        }

        if (currentTarget != null && attackCountdown <= 0f)
        {
            Attack();
            attackCountdown = timeBetweenAttacks;
        }

        if (currentTarget != null)
        {
            if (!currentTarget.gameObject.activeInHierarchy || Vector3.Distance(transform.position, currentTarget.transform.position) > GetComponent<CircleCollider2D>().radius)
            {
                currentTarget.ResumeMovement();
                currentTarget = null;
            }
        }
    }

    // (수정) Attack 함수는 이제 애니메이션을 재생하기만 합니다.
    void Attack()
    {
        if (currentTarget == null) return;

        // 실제 공격 속도는 Update()의 attackCountdown에 의해서만 제어됩니다.
        if (animationController != null)
        {
            animationController.SetAnimationSpeed(1f); // 항상 기본 속도로 재생하도록 보장
            animationController.PlayAttackAnimation(); // "DoAttack" Trigger 발동
        }
        else
        {
            // 애니메이터가 없을 경우를 대비해 직접 데미지를 줍니다.
            DealDamageToTarget();
        }
    }
    
    // (유지) 애니메이션 이벤트에서 호출될, 실제 데미지를 주는 함수
    public void DealDamageToTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.GetComponent<EnemyHealth>().TakeDamage(attackDamage, TowerType.Hero, DamageType.Physical);
        }
    }

    public void ActivateCloneSkill(float duration)
    {
        StartCoroutine(CloneSkillCoroutine(duration));
    }

    private IEnumerator CloneSkillCoroutine(float duration)
    {
        foreach (var clone in activeClones) { if (clone != null) Destroy(clone); }
        activeClones.Clear();
        GameObject clone1 = Instantiate(heroClonePrefab, cloneSpawnPointLeft.position, Quaternion.identity);
        GameObject clone2 = Instantiate(heroClonePrefab, cloneSpawnPointRight.position, Quaternion.identity);
        HeroCloneController cloneController1 = clone1.GetComponent<HeroCloneController>();
        HeroCloneController cloneController2 = clone2.GetComponent<HeroCloneController>();
        if (cloneController1 != null) cloneController1.Setup(transform);
        if (cloneController2 != null) cloneController2.Setup(transform);
        activeClones.Add(clone1);
        activeClones.Add(clone2);
        yield return new WaitForSeconds(duration);
        foreach (var clone in activeClones) { if (clone != null) Destroy(clone); }
        activeClones.Clear();
    }

    public void ResumeMovement()
    {
        currentTarget = null;
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0) return;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        if (healthBarSlider != null) { healthBarSlider.value = currentHealth / maxHealth; }
    }

    void RegenerateHealth()
    {
        currentHealth += healthRegenRate * Time.deltaTime;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        if (healthBarSlider != null) { healthBarSlider.value = currentHealth / maxHealth; }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementInput * moveSpeed;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        timeSinceLastCombat = 0f;
        if (healthBarSlider != null) { healthBarSlider.value = currentHealth / maxHealth; }
        if (currentHealth <= 0) { Die(); }
    }

    void Die()
    {
        if (currentTarget != null) { currentTarget.ResumeMovement(); }
        Debug.Log("영웅이 쓰러졌습니다!");
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ExperienceOrb"))
        {
            ExperienceController orb = other.GetComponent<ExperienceController>();
            if (orb != null) { orb.Collect(); }
            return;
        }

        if (other.CompareTag("Enemy") && currentTarget == null)
        {
            EnemyMovement enemy = other.GetComponent<EnemyMovement>();
            if (enemy != null && !enemy.IsBlocked())
            {
                enemy.BlockMovement(null, this);
                currentTarget = enemy;
            }
        }
    }
}