using UnityEngine;
using UnityEngine.UI;
using TMPro;

// (수정) 모든 타워의 업그레이드 및 '스킬' UI를 제어하는 통합 스크립트입니다.
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
    private GameObject upgradeButtonPrefab; // 업그레이드/스킬 버튼으로 재사용될 프리팹
    [SerializeField]
    private Transform buttonContainer;
    [SerializeField]
    private Button setRallyPointButton;

    private TowerController selectedTower;
    private BarracksController selectedBarracks;

    void Start()
    {
        uiPanel.SetActive(false);
    }

    // (수정) TowerController에서 호출 시, 업그레이드 경로와 스킬 경로를 모두 확인하여 적절한 UI를 표시합니다.
    public void Show(TowerController tower)
    {
        selectedTower = tower;
        selectedBarracks = null;
        
        setRallyPointButton.gameObject.SetActive(false);

        // 기존 버튼들을 모두 삭제합니다.
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 업그레이드 경로가 있으면 업그레이드 버튼들을 생성합니다.
        if (tower.upgradePaths != null && tower.upgradePaths.Length > 0)
        {
            UpdateButtonsForUpgrades(tower.upgradePaths);
        }
        // 업그레이드 경로는 없고 스킬이 있다면 스킬 버튼들을 생성합니다.
        else if (tower.towerSkills != null && tower.towerSkills.Length > 0)
        {
            UpdateButtonsForSkills(tower.towerSkills);
        }
        
        transform.position = tower.transform.position;
        uiPanel.SetActive(true);
    }

    public void Show(BarracksController barracks)
    {
        selectedBarracks = barracks;
        selectedTower = null;

        setRallyPointButton.gameObject.SetActive(true);

        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        if (barracks.upgradePaths != null && barracks.upgradePaths.Length > 0)
        {
            UpdateButtonsForUpgrades(barracks.upgradePaths);
        }
        // (추가) 병영도 최종 티어에서는 스킬을 가질 수 있으므로, 해당 로직을 추가합니다. (현재는 해당사항 없음)
        // else if (barracks.towerSkills != null && barracks.towerSkills.Length > 0) { ... }

        transform.position = barracks.transform.position;
        uiPanel.SetActive(true);
    }

    // 타워 '업그레이드' 버튼들을 생성하는 함수
    private void UpdateButtonsForUpgrades(TowerBlueprint[] blueprints)
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

    // (추가) 타워 '스킬' 버튼들을 생성하는 새로운 함수
    private void UpdateButtonsForSkills(TowerSkillBlueprint[] skills)
    {
        buttonContainer.gameObject.SetActive(true);
        foreach (TowerSkillBlueprint skillBlueprint in skills)
        {
            GameObject buttonGO = Instantiate(upgradeButtonPrefab, buttonContainer);
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            Button button = buttonGO.GetComponent<Button>();

            int currentLevel = selectedTower.GetSkillLevel(skillBlueprint.skillName);
            
            // 스킬 레벨에 따라 버튼 텍스트와 상호작용 여부를 다르게 설정합니다.
            if (currentLevel >= skillBlueprint.maxLevel)
            {
                buttonText.text = $"{skillBlueprint.skillName}\n(MAX)";
                button.interactable = false; // 최대 레벨이면 버튼 비활성화
            }
            else
            {
                int cost = skillBlueprint.costs[currentLevel];
                buttonText.text = $"{skillBlueprint.skillName} ({currentLevel}/{skillBlueprint.maxLevel})\n({cost}G)";
                button.onClick.AddListener(() => {
                    selectedTower.UpgradeSkill(skillBlueprint);
                });
            }
        }
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }

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
