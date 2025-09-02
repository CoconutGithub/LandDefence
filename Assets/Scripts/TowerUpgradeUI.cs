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
    private GameObject upgradeButtonPrefab;
    [SerializeField]
    private Transform buttonContainer;
    [SerializeField]
    private Button setRallyPointButton;
    [SerializeField]
    private GameObject skillButtonPrefab; // (추가) 스킬 버튼 프리팹

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

        // (수정) 타워의 상태(업그레이드/스킬)에 따라 다른 버튼을 표시
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

        // (수정) 병영의 상태(업그레이드/스킬)에 따라 다른 버튼을 표시
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

    // (수정) 스킬 버튼을 생성하고 설정하는 함수
    private void UpdateSkillButtons(TowerSkillBlueprint[] skills, TowerController tower = null, BarracksController barracks = null)
    {
        ClearAllButtons();
        buttonContainer.gameObject.SetActive(true);

        foreach (TowerSkillBlueprint skill in skills)
        {
            GameObject buttonGO = Instantiate(skillButtonPrefab, buttonContainer);
            Button button = buttonGO.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

            int currentLevel = 0;
            if(tower != null) currentLevel = tower.GetSkillLevel(skill.skillName);
            else if(barracks != null) currentLevel = barracks.GetSkillLevel(skill.skillName);

            if (currentLevel >= skill.maxLevel)
            {
                buttonText.text = $"{skill.skillName}\n(마스터)";
                button.interactable = false;
            }
            else
            {
                buttonText.text = $"{skill.skillName} ({currentLevel + 1}LV)\n({skill.costs[currentLevel]}G)";
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

