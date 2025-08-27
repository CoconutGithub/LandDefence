using UnityEngine;

// 타워의 전반적인 동작(적 감지, 공격 등)을 제어하는 스크립트입니다.
public class TowerController : MonoBehaviour
{
    [Header("타워 능력치")]
    [SerializeField]
    private float attackRange = 3f;
    [SerializeField]
    private float timeBetweenAttacks = 1f;

    // (수정) 발사체의 세부 정보를 설정하는 헤더와 변수를 추가합니다.
    [Header("발사체 정보")]
    [SerializeField]
    private float projectileDamage = 25f;
    [SerializeField]
    private float projectileSpeed = 10f;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Transform firePoint;

    private Transform currentTarget;
    private float attackCountdown = 0f;

    void Update()
    {
        FindClosestEnemy();
        attackCountdown -= Time.deltaTime;

        if (currentTarget != null && attackCountdown <= 0f)
        {
            Attack();
            attackCountdown = timeBetweenAttacks;
        }
    }

    void Attack()
    {
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();

        if (projectile != null)
        {
            // (수정) projectile.SetTarget 대신 새로운 Setup 함수를 호출하여 모든 정보를 전달합니다.
            projectile.Setup(currentTarget, projectileDamage, projectileSpeed);
        }
    }

    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null && closestDistance <= attackRange)
        {
            currentTarget = closestEnemy.transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
