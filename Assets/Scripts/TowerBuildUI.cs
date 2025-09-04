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
    private TowerSpotController currentSpot;

    [Header("궁수 타워")]
    public TowerBlueprint archerBlueprint;
    public TextMeshProUGUI archerCostText;

    [Header("마법사 타워")]
    public TowerBlueprint mageBlueprint;
    public TextMeshProUGUI mageCostText;
    
    [Header("병영 타워")]
    public TowerBlueprint barracksBlueprint;
    public TextMeshProUGUI barracksCostText;

    [Header("폭탄 타워")]
    public TowerBlueprint bombBlueprint;
    public TextMeshProUGUI bombCostText;

    [Header("총 타워")]
    public TowerBlueprint gunBlueprint;
    public TextMeshProUGUI gunCostText;


    void Start()
    {
        uiPanel.SetActive(false);
    }

    public void Show(TowerSpotController spot)
    {
        currentSpot = spot;
        transform.position = spot.transform.position;

        // (수정) UI가 보일 때 각 버튼의 비용 텍스트를 업데이트합니다.
        if (archerCostText != null) archerCostText.text = archerBlueprint.cost + "G";
        if (mageCostText != null) mageCostText.text = mageBlueprint.cost + "G";
        if (barracksCostText != null) barracksCostText.text = barracksBlueprint.cost + "G";
        if (bombCostText != null) bombCostText.text = bombBlueprint.cost + "G";
        if (gunCostText != null) gunCostText.text = gunBlueprint.cost + "G";

        uiPanel.SetActive(true);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }
    
    public void BuildArcherTower()
    {
        if (currentSpot != null) currentSpot.BuildTower(archerBlueprint);
        Hide();
    }
    
    public void BuildMageTower()
    {
        if (currentSpot != null) currentSpot.BuildTower(mageBlueprint);
        Hide();
    }
    
    public void BuildBarracksTower()
    {
        if (currentSpot != null) currentSpot.BuildTower(barracksBlueprint);
        Hide();
    }
    
    public void BuildBombTower()
    {
        if (currentSpot != null) currentSpot.BuildTower(bombBlueprint);
        Hide();
    }
    
    public void BuildGunTower()
    {
        if (currentSpot != null) currentSpot.BuildTower(gunBlueprint);
        Hide();
    }
}
