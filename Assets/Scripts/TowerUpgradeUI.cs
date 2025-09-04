using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    private GameObject upgradeButtonPrefab; // (수정) 이제 이 프리팹 하나만 사용합니다.
    [SerializeField]
    private Transform buttonContainer;
    [SerializeField]
    private Button setRallyPointButton;
    // [SerializeField] private GameObject skillButtonPrefab; // (제거) 더 이상 필요 없습니다.

    private TowerController selectedTower;
    private BarracksController selectedBarracks;

    void Start()
    {
        uiPanel.SetActive(false);
    }

    public void Show(TowerController tower)
    {
        selectedTower = tower;
        selectedBarracks = null;
        
        setRallyPointButton.gameObject.SetActive(false);

        if (tower.upgradePaths != null && tower.upgradePaths.Length > 0)
        {
            UpdateUpgradeButtons(tower.upgradePaths);
        }
        else
        {
            UpdateSkillButtons(tower.towerSkills, tower);
        }
        
        transform.position = tower.transform.position;
        uiPanel.SetActive(true);
    }

    public void Show(BarracksController barracks)
    {
        selectedBarracks = barracks;
        selectedTower = null;

        setRallyPointButton.gameObject.SetActive(true);

        if (barracks.upgradePaths != null && barracks.upgradePaths.Length > 0)
        {
            UpdateUpgradeButtons(barracks.upgradePaths);
        }
        else
        {
            UpdateSkillButtons(barracks.towerSkills, null, barracks);
        }

        transform.position = barracks.transform.position;
        uiPanel.SetActive(true);
    }

    void ClearAllButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateUpgradeButtons(TowerBlueprint[] blueprints)
    {
        ClearAllButtons();
        buttonContainer.gameObject.SetActive(true);

        foreach (TowerBlueprint blueprint in blueprints)
        {
            GameObject buttonGO = Instantiate(upgradeButtonPrefab, buttonContainer);
            
            Image buttonImage = buttonGO.GetComponent<Image>();
            if (buttonImage != null && blueprint.icon != null)
            {
                buttonImage.sprite = blueprint.icon;
            }
            
            // (수정) 이름으로 'CostText'를 찾아 비용을 표시합니다.
            Transform costTextTransform = buttonGO.transform.Find("CostText");
            if (costTextTransform != null)
            {
                costTextTransform.GetComponent<TextMeshProUGUI>().text = $"{blueprint.cost}G";
            }
            
            // (추가) 업그레이드 버튼에서는 'LevelText'를 비활성화합니다.
            Transform levelTextTransform = buttonGO.transform.Find("LevelText");
            if (levelTextTransform != null)
            {
                levelTextTransform.gameObject.SetActive(false);
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
    
    private void UpdateSkillButtons(TowerSkillBlueprint[] skills, TowerController tower = null, BarracksController barracks = null)
    {
        ClearAllButtons();
        buttonContainer.gameObject.SetActive(true);

        foreach (TowerSkillBlueprint skill in skills)
        {
            GameObject buttonGO = Instantiate(upgradeButtonPrefab, buttonContainer); // (수정) UpgradeButtonPrefab 사용
            
            Image buttonImage = buttonGO.GetComponent<Image>();
            if (buttonImage != null && skill.icon != null)
            {
                buttonImage.sprite = skill.icon;
            }

            Button button = buttonGO.GetComponent<Button>();
            
            // 이름으로 특정 Text 컴포넌트를 찾습니다.
            TextMeshProUGUI costText = null;
            Transform costTextTransform = buttonGO.transform.Find("CostText");
            if (costTextTransform != null)
            {
                costText = costTextTransform.GetComponent<TextMeshProUGUI>();
            }

            TextMeshProUGUI levelText = null;
            Transform levelTextTransform = buttonGO.transform.Find("LevelText");
            if (levelTextTransform != null)
            {
                levelText = levelTextTransform.GetComponent<TextMeshProUGUI>();
                levelText.gameObject.SetActive(true); // 스킬 버튼에서는 활성화
            }

            int currentLevel = 0;
            if(tower != null) currentLevel = tower.GetSkillLevel(skill.skillName);
            else if(barracks != null) currentLevel = barracks.GetSkillLevel(skill.skillName);

            if (levelText != null)
            {
                levelText.text = $"{currentLevel}/{skill.maxLevel}";
            }

            if (currentLevel >= skill.maxLevel)
            {
                if (costText != null) costText.text = "마스터";
                button.interactable = false;
            }
            else
            {
                if (costText != null) costText.text = $"{skill.costs[currentLevel]}G";
                button.interactable = true;
                button.onClick.AddListener(() => {
                    if(tower != null) tower.UpgradeSkill(skill);
                    else if(barracks != null) barracks.UpgradeSkill(skill);
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

