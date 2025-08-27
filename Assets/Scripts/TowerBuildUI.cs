using UnityEngine;

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

    // (수정) 건설할 타워를 기본 궁수 타워 하나로 고정합니다.
    public TowerBlueprint archerBlueprint;

    private TowerSpotController currentSpot;

    void Start()
    {
        uiPanel.SetActive(false);
    }

    public void Show(TowerSpotController spot)
    {
        currentSpot = spot;
        transform.position = spot.transform.position;
        uiPanel.SetActive(true);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }

    // (수정) 기본 궁수 타워를 건설하는 단일 함수입니다.
    public void BuildArcherTower()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(archerBlueprint);
        }
        Hide();
    }
}
