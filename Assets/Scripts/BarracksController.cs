using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BarracksController : MonoBehaviour, IPointerClickHandler
{
    public static BarracksController selectedBarracks;

    [Header("병영 능력치")]
    [SerializeField]
    private int maxSoldiers = 3;
    [SerializeField]
    private float spawnRate = 10f;
    [SerializeField]
    private float spreadRadius = 1.0f; // (추가) 병사들이 퍼지는 반경입니다.

    [Header("필요한 컴포넌트")]
    [SerializeField]
    private GameObject soldierPrefab;
    [SerializeField]
    private GameObject rallyPointPrefab;

    private List<SoldierController> spawnedSoldiers = new List<SoldierController>();
    private float spawnCountdown = 0f;
    private Transform rallyPointInstance;

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

        if (selectedBarracks == this)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                rallyPointInstance.position = new Vector3(mousePos.x, mousePos.y, 0);
                UpdateSoldierRallyPoints(); // (수정) 집결 지점 이동 시 진형을 다시 계산합니다.
                Deselect();
            }
        }
    }
    
    void SpawnSoldier()
    {
        GameObject soldierGO = Instantiate(soldierPrefab, transform.position, Quaternion.identity);
        SoldierController soldier = soldierGO.GetComponent<SoldierController>();

        if (soldier != null)
        {
            spawnedSoldiers.Add(soldier);
            soldier.SetBarracks(this);
            UpdateSoldierRallyPoints(); // (수정) 새 병사가 스폰되면 진형을 다시 계산합니다.
        }
    }
    
    public void RemoveSoldier(SoldierController soldier)
    {
        if (spawnedSoldiers.Contains(soldier))
        {
            spawnedSoldiers.Remove(soldier);
            UpdateSoldierRallyPoints(); // (수정) 병사가 죽으면 진형을 다시 계산합니다.
        }
    }
    
    // (수정) 모든 병사들의 목표 위치를 원형으로 다시 계산하고 할당하는 함수입니다.
    void UpdateSoldierRallyPoints()
    {
        // 현재 살아있는 병사 수
        int soldierCount = spawnedSoldiers.Count;
        if (soldierCount == 0) return;

        // 360도를 병사 수로 나누어 각 병사 사이의 각도를 계산합니다.
        float angleStep = 360f / soldierCount;

        for (int i = 0; i < soldierCount; i++)
        {
            // 현재 병사의 각도를 계산합니다. (Mathf.Deg2Rad는 각도를 라디안으로 변환합니다)
            float angle = i * angleStep * Mathf.Deg2Rad;
            
            // 삼각함수를 이용해 집결 지점 중심으로부터의 смещение(offset)를 계산합니다.
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * spreadRadius;
            
            // 최종 목표 위치를 계산하고 병사에게 알려줍니다.
            Vector3 targetPosition = rallyPointInstance.position + offset;
            spawnedSoldiers[i].SetRallyPointPosition(targetPosition);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectedBarracks != null && selectedBarracks != this)
        {
            selectedBarracks.Deselect();
        }
        
        selectedBarracks = this;
        rallyPointInstance.gameObject.SetActive(true);
    }
    
    void Deselect()
    {
        selectedBarracks = null;
    }
}
