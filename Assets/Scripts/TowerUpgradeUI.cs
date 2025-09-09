using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems; // (추가) UI 클릭 이벤트를 감지하기 위해 필요합니다.

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

    private TowerController selectedTower;
    private BarracksController selectedBarracks;
    private Transform selectedTowerTransform; // (추가) 현재 선택된 타워의 Transform을 저장합니다.

    void Start()
    {
        uiPanel.SetActive(false);
    }
    
    // (추가) 패널이 활성화되어 있을 때, 매 프레임 실행됩니다.
    void Update()
    {
        // 만약 마우스 왼쪽 버튼을 클릭했고, 그 클릭이 UI 요소(버튼 등) 위가 아니라면
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // 그리고 현재 마우스 위치가 이 패널의 영역 밖이라면
            if (!RectTransformUtility.RectangleContainsScreenPoint(uiPanel.GetComponent<RectTransform>(), Input.mousePosition))
            {
                 Hide(); // 패널을 숨깁니다.
            }
        }
    }
    
    // (추가) 모든 렌더링이 끝난 후, 매 프레임 실행됩니다.
    void LateUpdate()
    {
        // 패널이 활성화되어 있고, 타워가 선택되어 있다면
        if (uiPanel.activeSelf && selectedTowerTransform != null)
        {
            // UI의 위치를 타워의 월드 위치와 동일하게 계속 업데이트합니다.
            transform.position = selectedTowerTransform.position;
        }
    }

    public void Show(TowerController tower)
    {
        selectedTower = tower;
        selectedBarracks = null;
        selectedTowerTransform = tower.transform; // (추가) 타워의 Transform을 저장합니다.
        
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
        selectedTowerTransform = barracks.transform; // (추가) 병영의 Transform을 저장합니다.

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
            Button button = buttonGO.GetComponent<Button>();
            Image icon = buttonGO.GetComponent<Image>();
            icon.sprite = blueprint.icon;

            Transform levelTextTransform = buttonGO.transform.Find("LevelText");
            if (levelTextTransform != null)
            {
                levelTextTransform.gameObject.SetActive(false);
            }

            TextMeshProUGUI costText = buttonGO.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
            if (costText != null)
            {
                costText.text = blueprint.cost + "G";
            }
            
            button.onClick.AddListener(() => {
                UpgradeTo(blueprint);
            });
        }
    }

    private void UpdateSkillButtons(TowerSkillBlueprint[] skills, TowerController tower = null, BarracksController barracks = null)
    {
        ClearAllButtons();
        buttonContainer.gameObject.SetActive(true);

        foreach (TowerSkillBlueprint skill in skills)
        {
            GameObject buttonGO = Instantiate(upgradeButtonPrefab, buttonContainer);
            Button button = buttonGO.GetComponent<Button>();
            Image icon = buttonGO.GetComponent<Image>();
            icon.sprite = skill.icon;
            
            Transform levelTextTransform = buttonGO.transform.Find("LevelText");
            Transform costTextTransform = buttonGO.transform.Find("CostText");

            int currentLevel = 0;
            if(tower != null) currentLevel = tower.GetSkillLevel(skill.skillName);
            else if(barracks != null) currentLevel = barracks.GetSkillLevel(skill.skillName);

            if (levelTextTransform != null)
            {
                levelTextTransform.gameObject.SetActive(true);
                levelTextTransform.GetComponent<TextMeshProUGUI>().text = $"{currentLevel}/{skill.maxLevel}";
            }

            if (costTextTransform != null)
            {
                if (currentLevel >= skill.maxLevel)
                {
                    costTextTransform.GetComponent<TextMeshProUGUI>().text = "마스터";
                    button.interactable = false;
                }
                else
                {
                    costTextTransform.GetComponent<TextMeshProUGUI>().text = skill.costs[currentLevel] + "G";
                    button.interactable = true;
                }
            }
            
            button.onClick.AddListener(() => {
                if(tower != null) tower.UpgradeSkill(skill);
                else if(barracks != null) barracks.UpgradeSkill(skill);
            });
        }
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
        selectedTowerTransform = null; // (추가) 패널이 닫힐 때 참조를 비워줍니다.
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

