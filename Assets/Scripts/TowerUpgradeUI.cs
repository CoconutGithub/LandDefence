using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

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
    private Sprite confirmIcon;

    [Header("Skill Tooltip")]
    [SerializeField]
    private GameObject skillTooltipPanel;
    [SerializeField]
    private TextMeshProUGUI skillTooltipText;

    private TowerController selectedTower;
    private BarracksController selectedBarracks;
    private Transform selectedTowerTransform;
    
    private object pendingUpgrade = null;
    private GameObject previewTower = null;
    
    private Dictionary<Button, Sprite> originalButtonIcons = new Dictionary<Button, Sprite>();

    void Start()
    {
        uiPanel.SetActive(false);
        if (skillTooltipPanel != null)
        {
            skillTooltipPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (pendingUpgrade != null)
            {
                CancelPreview();
            }
            else if (uiPanel.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint(uiPanel.GetComponent<RectTransform>(), Input.mousePosition))
            {
                Hide();
            }
        }

        // (수정) World Space Canvas에서 마우스 위치를 올바르게 따라가도록 수정합니다.
        if (skillTooltipPanel != null && skillTooltipPanel.activeSelf)
        {
            // 이 코드는 카메라에서 마우스 위치로 광선을 쏴서 UI 캔버스 평면과 만나는 지점을 계산합니다.
            Plane canvasPlane = new Plane(transform.forward, transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (canvasPlane.Raycast(ray, out float enter))
            {
                // 광선이 캔버스 평면과 만나는 정확한 3D 월드 좌표를 툴팁의 위치로 설정합니다.
                skillTooltipPanel.transform.position = ray.GetPoint(enter);
            }
        }
    }
    
    void LateUpdate()
    {
        if (uiPanel.activeSelf && selectedTowerTransform != null)
        {
            transform.position = selectedTowerTransform.position;
        }
    }
    
    // ... (이 아래의 다른 함수들은 변경되지 않았습니다) ...

    public void Show(TowerController tower)
    {
        if (selectedTower != tower)
        {
            CancelPreview();
        }
        
        selectedTower = tower;
        selectedBarracks = null;
        selectedTowerTransform = tower.transform;
        
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
        if (selectedBarracks != barracks)
        {
            CancelPreview();
        }

        selectedBarracks = barracks;
        selectedTower = null;
        selectedTowerTransform = barracks.transform;

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
        originalButtonIcons.Clear();
    }

    private void UpdateUpgradeButtons(TowerBlueprint[] blueprints)
    {
        ClearAllButtons();
        buttonContainer.gameObject.SetActive(true);

        foreach (TowerBlueprint blueprint in blueprints)
        {
            GameObject buttonGO = Instantiate(upgradeButtonPrefab, buttonContainer);
            Button button = buttonGO.GetComponent<Button>();
            Image iconImage = button.GetComponent<Image>();
            iconImage.sprite = blueprint.icon;

            originalButtonIcons[button] = blueprint.icon;

            Transform levelTextTransform = buttonGO.transform.Find("LevelText");
            if (levelTextTransform != null)
            {
                levelTextTransform.gameObject.SetActive(false);
            }

            TextMeshProUGUI costText = buttonGO.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
            costText.text = blueprint.cost + "G";
            
            button.onClick.AddListener(() => {
                RequestUpgrade(blueprint, button);
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
            Image iconImage = button.GetComponent<Image>();
            iconImage.sprite = skill.icon;
            
            originalButtonIcons[button] = skill.icon;
            
            SkillButtonHover hoverHandler = buttonGO.AddComponent<SkillButtonHover>();
            hoverHandler.skillDescription = skill.skillDescription;
            
            int currentLevel = (tower != null) ? tower.GetSkillLevel(skill.skillName) : barracks.GetSkillLevel(skill.skillName);
            buttonGO.transform.Find("LevelText").GetComponent<TextMeshProUGUI>().text = $"{currentLevel}/{skill.maxLevel}";
            if (currentLevel >= skill.maxLevel)
            {
                buttonGO.transform.Find("CostText").GetComponent<TextMeshProUGUI>().text = "마스터";
                button.interactable = false;
            }
            else
            {
                buttonGO.transform.Find("CostText").GetComponent<TextMeshProUGUI>().text = skill.costs[currentLevel] + "G";
            }
            
            button.onClick.AddListener(() => {
                RequestSkillUpgrade(skill, button);
            });
        }
    }

    public void Hide()
    {
        CancelPreview();
        uiPanel.SetActive(false);
        selectedTowerTransform = null;
        HideSkillTooltip();
    }

    public void OnSetRallyPointButton()
    {
        if (selectedBarracks != null)
        {
            selectedBarracks.EnterRallyPointMode();
        }
        Hide();
    }
    
    private void RequestUpgrade(TowerBlueprint blueprint, Button clickedButton)
    {
        if (pendingUpgrade as TowerBlueprint == blueprint)
        {
            if (selectedTower != null) selectedTower.Upgrade(blueprint);
            else if (selectedBarracks != null) selectedBarracks.Upgrade(blueprint);
            Hide();
        }
        else
        {
            CancelPreview();
            pendingUpgrade = blueprint;
            
            clickedButton.GetComponent<Image>().sprite = confirmIcon;
            
            previewTower = Instantiate(blueprint.prefab, selectedTowerTransform.position, Quaternion.identity);
            if (previewTower.GetComponent<TowerController>() != null) previewTower.GetComponent<TowerController>().enabled = false;
            if (previewTower.GetComponent<BarracksController>() != null) previewTower.GetComponent<BarracksController>().enabled = false;
            SetObjectTransparency(previewTower, 0.7f);
        }
    }

    private void RequestSkillUpgrade(TowerSkillBlueprint skill, Button clickedButton)
    {
        if (pendingUpgrade as TowerSkillBlueprint == skill)
        {
            if(selectedTower != null) selectedTower.UpgradeSkill(skill);
            else if(selectedBarracks != null) selectedBarracks.UpgradeSkill(skill);
            
            CancelPreview();
            if(selectedTower != null) Show(selectedTower);
            else if(selectedBarracks != null) Show(selectedBarracks);
        }
        else
        {
            CancelPreview();
            pendingUpgrade = skill;
            clickedButton.GetComponent<Image>().sprite = confirmIcon;
        }
    }
    
    private void CancelPreview()
    {
        if (previewTower != null)
        {
            Destroy(previewTower);
            previewTower = null;
        }
        pendingUpgrade = null;
        ResetAllButtonIcons();
    }
    
    private void ResetAllButtonIcons()
    {
        foreach (var entry in originalButtonIcons)
        {
            Button button = entry.Key;
            Sprite originalIcon = entry.Value;
            if (button != null)
            {
                button.GetComponent<Image>().sprite = originalIcon;
            }
        }
    }

    private void SetObjectTransparency(GameObject obj, float alpha)
    {
        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;
        }
    }
    
    public void ShowSkillTooltip(string description)
    {
        if (skillTooltipPanel != null && skillTooltipText != null)
        {
            skillTooltipText.text = description;
            skillTooltipPanel.SetActive(true);
        }
    }

    public void HideSkillTooltip()
    {
        if (skillTooltipPanel != null)
        {
            skillTooltipPanel.SetActive(false);
        }
    }
}

