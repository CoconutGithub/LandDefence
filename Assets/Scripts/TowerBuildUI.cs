using UnityEngine;
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

    // (수정) 건설할 타워의 종류를 늘립니다.
    public TowerBlueprint archerBlueprint;
    public TowerBlueprint mageBlueprint;
    public TowerBlueprint barracksBlueprint; // (추가) 병영 타워의 설계도입니다.

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

    // 기본 궁수 타워를 건설하는 함수입니다.
    public void BuildArcherTower()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(archerBlueprint);
        }
        Hide();
    }

    // 마법사 타워를 건설하는 함수입니다.
    public void BuildMageTower()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(mageBlueprint);
        }
        Hide();
    }

    // (추가) 병영 타워를 건설하는 새로운 함수입니다.
    public void BuildBarracksTower()
    {
        if (currentSpot != null)
        {
            currentSpot.BuildTower(barracksBlueprint);
        }
        Hide();
    }
}
