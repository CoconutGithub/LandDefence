using UnityEngine;
using UnityEngine.UI; // (추가) Button을 사용하기 위해 추가합니다.
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

    [Header("UI Components")]
    [SerializeField]
    private GameObject uiPanel;
    [SerializeField]
    private GameObject upgradeButtonPrefab; // (추가) 복제해서 사용할 버튼의 원본(프리팹)입니다.
    [SerializeField]
    private Transform buttonContainer; // (추가) 생성된 버튼들이 들어갈 부모 패널입니다.

    private TowerController selectedTower;

    void Start()
    {
        uiPanel.SetActive(false);
    }

    // TowerController에서 호출하여 UI를 보여주는 함수입니다.
    public void Show(TowerController tower)
    {
        selectedTower = tower;
        
        // 이전에 생성된 버튼들을 모두 삭제하여 UI를 깨끗하게 비웁니다.
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 선택된 타워의 모든 업그레이드 경로를 가져와 각각 버튼을 생성합니다.
        foreach (TowerBlueprint blueprint in tower.upgradePaths)
        {
            GameObject buttonGO = Instantiate(upgradeButtonPrefab, buttonContainer);
            
            // 버튼의 텍스트를 설정합니다.
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"{blueprint.towerName}\n({blueprint.cost}G)";
            }

            // 버튼의 클릭 이벤트를 설정합니다.
            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                // (중요) 버튼을 누르면 어떤 'blueprint'로 업그레이드할지 알려주도록 이벤트를 설정합니다.
                button.onClick.AddListener(() => {
                    UpgradeTo(blueprint);
                });
            }
        }
        
        transform.position = tower.transform.position;
        uiPanel.SetActive(true);
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }

    // (수정) 모든 업그레이드 버튼이 이 하나의 함수를 호출합니다.
    private void UpgradeTo(TowerBlueprint blueprint)
    {
        if (selectedTower != null)
        {
            selectedTower.Upgrade(blueprint);
        }
        Hide();
    }
}
