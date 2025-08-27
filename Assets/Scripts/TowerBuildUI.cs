using UnityEngine;
using UnityEngine.EventSystems;

// 타워 건설 UI를 제어하는 스크립트입니다.
public class TowerBuildUI : MonoBehaviour
{
    public static TowerBuildUI instance;

    public GameObject uiPanel;

    public TowerBlueprint towerKR;
    public TowerBlueprint towerJP;

    private TowerSpotController currentSpot;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("TowerBuildUI 인스턴스가 둘 이상입니다!");
            return;
        }
        instance = this;

        canvasGroup = uiPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("BuildUIPanel에 CanvasGroup 컴포넌트가 없습니다! Inspector에서 추가해주세요.");
        }
    }

    void Start()
    {
        Hide();
    }

    void Update()
    {
        // UI가 활성화 상태(alpha > 0)이고, 마우스 왼쪽 버튼을 클릭했을 때
        if (canvasGroup.alpha > 0 && Input.GetMouseButtonDown(0))
        {
            // 마우스 포인터가 UI 요소(버튼 등) 위에 있지 않다면 UI를 숨깁니다.
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Hide();
            }
        }
    }

    public void Show(TowerSpotController spot)
    {
        currentSpot = spot;
        
        // (수정) UI의 위치를 타워 부지의 월드 좌표로 직접 설정합니다.
        // Canvas의 Render Mode가 'Screen Space - Camera'일 때 이 방식이 더 안정적입니다.
        uiPanel.transform.position = spot.transform.position;
        
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void BuildTowerKR()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(towerKR);
        }
        Hide();
    }

    public void BuildTowerJP()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(towerJP);
        }
        Hide();
    }
}
