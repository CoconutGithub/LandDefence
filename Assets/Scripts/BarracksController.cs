using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BarracksController : MonoBehaviour, IPointerClickHandler
{
    private bool isInRallyPointMode = false;

    [Header("병영 능력치")]
    [SerializeField]
    private int maxSoldiers = 3;
    [SerializeField]
    private float spawnRate = 10f;
    [SerializeField]
    private float spreadRadius = 1.0f;
    [SerializeField]
    private float rallyPointRange = 3f;

    [Header("업그레이드 정보")]
    public TowerBlueprint[] upgradePaths;

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject soldierPrefab;
    [SerializeField]
    private GameObject rallyPointPrefab;

    private List<SoldierController> spawnedSoldiers = new List<SoldierController>();
    private float spawnCountdown = 0f;
    private Transform rallyPointInstance;
    private TowerSpotController parentSpot;

    void Start()
    {
        rallyPointInstance = Instantiate(rallyPointPrefab, transform.position, Quaternion.identity).transform;
        rallyPointInstance.position = transform.position + new Vector3(0, -1.5f, 0);
        rallyPointInstance.gameObject.SetActive(false);
        
        for (int i = 0; i < maxSoldiers; i++)
        {
            SpawnSoldier();
        }
    }

    void Update()
    {
        if (spawnedSoldiers.Count < maxSoldiers)
        {
            spawnCountdown -= Time.deltaTime;
            if (spawnCountdown <= 0f)
            {
                SpawnSoldier();
                spawnCountdown = spawnRate;
            }
        }

        if (isInRallyPointMode)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                // (수정) Z축 값을 타워의 Z축 값과 동일하게 맞춰서 2D 거리 계산을 정확하게 합니다.
                Vector3 targetPos = new Vector3(mousePos.x, mousePos.y, transform.position.z);

                // (수정) 이제 targetPos를 사용하여 거리를 계산합니다.
                if (Vector3.Distance(transform.position, targetPos) <= rallyPointRange)
                {
                    rallyPointInstance.position = new Vector3(mousePos.x, mousePos.y, 0);
                    UpdateSoldierRallyPoints();
                    isInRallyPointMode = false; // 위치 지정이 끝났으므로 모드를 해제합니다.
                }
                else
                {
                    Debug.Log("너무 멀리 집결 지점을 설정할 수 없습니다!");
                }
            }
        }
    }
    
    public void Upgrade(TowerBlueprint blueprint)
    {
        if (GameManager.instance.SpendGold(blueprint.cost))
        {
            SoundManager.instance.PlayBuildSound();
            
            foreach (SoldierController soldier in spawnedSoldiers)
            {
                if (soldier != null)
                {
                    soldier.ReleaseEnemyBeforeDeath();
                    Destroy(soldier.gameObject);
                }
            }
            spawnedSoldiers.Clear();

            GameObject newBarracksGO = Instantiate(blueprint.prefab, transform.position, transform.rotation);
            newBarracksGO.GetComponent<BarracksController>().SetParentSpot(parentSpot);
            parentSpot.SetCurrentTower(newBarracksGO);
            
            Destroy(rallyPointInstance.gameObject);
            Destroy(gameObject);
        }
    }
    
    public void SetParentSpot(TowerSpotController spot)
    {
        parentSpot = spot;
    }

    void SpawnSoldier()
    {
        GameObject soldierGO = Instantiate(soldierPrefab, transform.position, Quaternion.identity);
        SoldierController soldier = soldierGO.GetComponent<SoldierController>();

        if (soldier != null)
        {
            spawnedSoldiers.Add(soldier);
            soldier.SetBarracks(this);
            UpdateSoldierRallyPoints();
        }
    }
    
    public void RemoveSoldier(SoldierController soldier)
    {
        if (spawnedSoldiers.Contains(soldier))
        {
            spawnedSoldiers.Remove(soldier);
            UpdateSoldierRallyPoints();
        }
    }
    
    void UpdateSoldierRallyPoints()
    {
        int soldierCount = spawnedSoldiers.Count;
        if (soldierCount == 0) return;

        float angleStep = 360f / soldierCount;

        for (int i = 0; i < soldierCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * spreadRadius;
            Vector3 targetPosition = rallyPointInstance.position + offset;
            spawnedSoldiers[i].SetRallyPointPosition(targetPosition);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        TowerUpgradeUI.instance.Show(this);
    }
    
    public void EnterRallyPointMode()
    {
        isInRallyPointMode = true;
        rallyPointInstance.gameObject.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rallyPointRange);
    }
}
