using UnityEngine;
using TMPro;

// 타워 업그레이드 UI를 제어하는 스크립트입니다.
public class TowerUpgradeUI : MonoBehaviour
{
    public static TowerUpgradeUI instance;

    void Awake()
    {
        if (instance != null) { return; }
        instance = this;
    }

    public GameObject uiPanel;
    // (수정) 버튼의 자식 Text(TMP)를 연결할 변수입니다.
    public TextMeshProUGUI upgradeKR_ButtonText;
    public TextMeshProUGUI upgradeJP_ButtonText;

    private TowerController selectedTower;

    void Start()
    {
        uiPanel.SetActive(false);
    }

    public void Show(TowerController tower)
    {
        selectedTower = tower;
        
        transform.position = tower.transform.position;
        uiPanel.SetActive(true);

        // (수정) 버튼 텍스트에 타워 이름과 비용을 함께 표시합니다.
        upgradeKR_ButtonText.text = $"{tower.upgradeKR_Blueprint.towerName}\n({tower.upgradeKR_Blueprint.cost}G)";
        upgradeJP_ButtonText.text = $"{tower.upgradeJP_Blueprint.towerName}\n({tower.upgradeJP_Blueprint.cost}G)";
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }

    public void UpgradeToKR()
    {
        if (selectedTower != null)
        {
            selectedTower.Upgrade(selectedTower.upgradeKR_Blueprint);
        }
        Hide();
    }

    public void UpgradeToJP()
    {
        if (selectedTower != null)
        {
            selectedTower.Upgrade(selectedTower.upgradeJP_Blueprint);
        }
        Hide();
    }
}
