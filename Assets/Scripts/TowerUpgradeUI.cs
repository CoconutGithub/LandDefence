using UnityEngine;
using UnityEngine.UI;
using TMPro;

// (수정) 모든 타워의 업그레이드 UI를 제어하는 통합 스크립트입니다.
public class TowerUpgradeUI : MonoBehaviour
{
    public static TowerUpgradeUI instance;

    void Awake()
    {
        if (instance != null) { return; }
        instance = this;
    }

    [Header("UI Components")]
    [SerializeField]
    private GameObject uiPanel;
    [SerializeField]
    private GameObject upgradeButtonPrefab;
    [SerializeField]
    private Transform buttonContainer;
    [SerializeField]
    private Button setRallyPointButton; // (추가) 병영 전용 '집결 지점 설정' 버튼

    // (수정) 어떤 종류의 타워가 선택되었는지 저장하기 위한 변수들입니다.
    private TowerController selectedTower;
    private BarracksController selectedBarracks;

    void Start()
    {
        uiPanel.SetActive(false);
    }

    // TowerController에서 호출하여 UI를 보여주는 함수입니다.
    public void Show(TowerController tower)
    {
        selectedTower = tower;
        selectedBarracks = null; // 다른 타워 선택지는 초기화합니다.
        
        // (추가) 병영 타워가 아니므로, 집결 지점 설정 버튼을 숨깁니다.
        setRallyPointButton.gameObject.SetActive(false);

        UpdateButtons(tower.upgradePaths);
        
        transform.position = tower.transform.position;
        uiPanel.SetActive(true);
    }

    // BarracksController에서 호출하여 UI를 보여주는 함수입니다.
    public void Show(BarracksController barracks)
    {
        selectedBarracks = barracks;
        selectedTower = null; // 다른 타워 선택지는 초기화합니다.

        // (추가) 병영 타워이므로, 집결 지점 설정 버튼을 보여줍니다.
        setRallyPointButton.gameObject.SetActive(true);

        UpdateButtons(barracks.upgradePaths);

        transform.position = barracks.transform.position;
        uiPanel.SetActive(true);
    }

    // 버튼을 동적으로 생성하는 로직입니다.
    private void UpdateButtons(TowerBlueprint[] blueprints)
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 업그레이드 경로가 있을 때만 버튼을 생성합니다.
        if (blueprints != null && blueprints.Length > 0)
        {
            buttonContainer.gameObject.SetActive(true);
            foreach (TowerBlueprint blueprint in blueprints)
            {
                GameObject buttonGO = Instantiate(upgradeButtonPrefab, buttonContainer);
                
                TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"{blueprint.towerName}\n({blueprint.cost}G)";
                }

                Button button = buttonGO.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => {
                        UpgradeTo(blueprint);
                    });
                }
            }
        }
        else
        {
            // 업그레이드 경로가 없으면 버튼 컨테이너를 숨깁니다.
            buttonContainer.gameObject.SetActive(false);
        }
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }

    // (추가) "집결 지점 설정" 버튼에 연결될 함수입니다.
    public void OnSetRallyPointButton()
    {
        if (selectedBarracks != null)
        {
            selectedBarracks.EnterRallyPointMode();
        }
        Hide();
    }

    private void UpgradeTo(TowerBlueprint blueprint)
    {
        if (selectedTower != null)
        {
            selectedTower.Upgrade(blueprint);
        }
        else if (selectedBarracks != null)
        {
            selectedBarracks.Upgrade(blueprint);
        }
        Hide();
    }
}
