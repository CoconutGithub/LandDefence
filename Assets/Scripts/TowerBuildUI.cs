using UnityEngine;
using UnityEngine.EventSystems; // (추가) UI 클릭 이벤트를 감지하기 위해 필요합니다.
using TMPro;

// 타워 건설 UI를 제어하는 스크립트입니다.
public class TowerBuildUI : MonoBehaviour
{
    public static TowerBuildUI instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("TowerBuildUI 인스턴스가 둘 이상입니다!");
            return;
        }
        instance = this;
    }

    public GameObject uiPanel;

    public TowerBlueprint archerBlueprint;
    public TowerBlueprint mageBlueprint;
    public TowerBlueprint barracksBlueprint;
    public TowerBlueprint bombBlueprint;
    public TowerBlueprint gunBlueprint;

    // (수정) 비용 텍스트를 연결할 변수들
    public TextMeshProUGUI archerCostText;
    public TextMeshProUGUI mageCostText;
    public TextMeshProUGUI barracksCostText;
    public TextMeshProUGUI bombCostText;
    public TextMeshProUGUI gunCostText;

    private TowerSpotController currentSpot;
    private Transform currentSpotTransform; // (추가) 현재 타워 스팟의 Transform을 저장합니다.

    void Start()
    {
        uiPanel.SetActive(false);
    }
    
    // (추가) 패널이 활성화되어 있을 때, 매 프레임 실행됩니다.
    void Update()
    {
        // 만약 마우스 왼쪽 버튼을 클릭했고, 그 클릭이 UI 요소 위가 아니라면 (즉, 게임 월드를 클릭했다면)
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Hide(); // 패널을 숨깁니다.
        }
    }
    
    // (추가) 모든 렌더링이 끝난 후, 매 프레임 실행됩니다.
    void LateUpdate()
    {
        // 패널이 활성화되어 있고, 타워 스팟이 지정되어 있다면
        if (uiPanel.activeSelf && currentSpotTransform != null)
        {
            // UI의 위치를 타워 스팟의 월드 위치와 동일하게 계속 업데이트합니다.
            // 이렇게 하면 카메라가 움직여도 UI가 타워 스팟에 고정되어 보입니다.
            transform.position = currentSpotTransform.position;
        }
    }

    public void Show(TowerSpotController spot)
    {
        currentSpot = spot;
        currentSpotTransform = spot.transform; // (추가) 타워 스팟의 Transform을 저장합니다.
        
        // (수정) 각 버튼의 비용 텍스트를 업데이트합니다.
        UpdateCostText(archerCostText, archerBlueprint);
        UpdateCostText(mageCostText, mageBlueprint);
        UpdateCostText(barracksCostText, barracksBlueprint);
        UpdateCostText(bombCostText, bombBlueprint);
        UpdateCostText(gunCostText, gunBlueprint);
        
        transform.position = spot.transform.position;
        uiPanel.SetActive(true);
    }
    
    // (추가) 비용 텍스트를 업데이트하는 헬퍼 함수
    private void UpdateCostText(TextMeshProUGUI textElement, TowerBlueprint blueprint)
    {
        if (textElement != null && blueprint != null)
        {
            textElement.text = blueprint.cost + "G";
        }
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
        currentSpotTransform = null; // (추가) 패널이 닫힐 때 참조를 비워줍니다.
    }

    public void BuildArcherTower()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(archerBlueprint);
        }
        Hide();
    }

    public void BuildMageTower()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(mageBlueprint);
        }
        Hide();
    }

    public void BuildBarracksTower()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(barracksBlueprint);
        }
        Hide();
    }

    public void BuildBombTower()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(bombBlueprint);
        }
        Hide();
    }
    
    public void BuildGunTower()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(gunBlueprint);
        }
        Hide();
    }
}

