//HeroController.cs
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


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
    
    public void ActivateCloneSkill(float duration)
    {
        StartCoroutine(CloneSkillCoroutine(duration));
    }
    
    private IEnumerator CloneSkillCoroutine(float duration)
    {
        foreach(var clone in activeClones)
        {
            if (clone != null) Destroy(clone);
        }
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

        foreach(var clone in activeClones)
        {
            if (clone != null) Destroy(clone);
        }
        activeClones.Clear();
    }
    
    // (추가) 외부(EnemyMovement)에서 호출하여 영웅의 타겟을 초기화하는 함수
    public void ResumeMovement()
    {
        currentTarget = null;
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }

    void RegenerateHealth()
    {
        currentHealth += healthRegenRate * Time.deltaTime;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementInput * moveSpeed;
    }

    void Attack()
    {
        if (currentTarget != null)
        {
            currentTarget.GetComponent<EnemyHealth>().TakeDamage(attackDamage, TowerType.Hero, DamageType.Physical);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        timeSinceLastCombat = 0f; 

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (currentTarget != null)
        {
            currentTarget.ResumeMovement();
        }
        Debug.Log("영웅이 쓰러졌습니다!");
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ExperienceOrb"))
        {
            ExperienceController orb = other.GetComponent<ExperienceController>();
            if (orb != null)
            {
                orb.Collect();
            }
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

