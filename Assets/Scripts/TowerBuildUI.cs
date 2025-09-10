using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [Header("UI 설정")]
    public GameObject uiPanel;
    public Sprite confirmIcon;

    [Header("타워 설계도")]
    public TowerBlueprint archerBlueprint;
    public TowerBlueprint mageBlueprint;
    public TowerBlueprint barracksBlueprint;
    public TowerBlueprint bombBlueprint;
    public TowerBlueprint gunBlueprint;

    [Header("UI 버튼 연결")]
    public Button archerButton;
    public Button mageButton;
    public Button barracksButton;
    public Button bombButton;
    public Button gunButton;
    
    private Dictionary<TowerBlueprint, Button> blueprintButtonMap;
    private Dictionary<Button, Sprite> originalButtonIcons;

    private TowerBlueprint pendingBlueprint = null;
    private GameObject previewTower = null;
    private TowerSpotController currentSpot;
    private Transform currentSpotTransform;

    void Start()
    {
        uiPanel.SetActive(false);

        blueprintButtonMap = new Dictionary<TowerBlueprint, Button>
        {
            { archerBlueprint, archerButton },
            { mageBlueprint, mageButton },
            { barracksBlueprint, barracksButton },
            { bombBlueprint, bombButton },
            { gunBlueprint, gunButton }
        };

        originalButtonIcons = new Dictionary<Button, Sprite>();
        foreach (var entry in blueprintButtonMap)
        {
            if (entry.Value != null)
            {
                // (수정) 버튼의 'Target Graphic'을 직접 참조하여 어떤 구조든 안전하게 아이콘을 가져옵니다.
                Image targetImage = entry.Value.targetGraphic as Image;
                if (targetImage != null)
                {
                    originalButtonIcons[entry.Value] = targetImage.sprite;
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (pendingBlueprint != null)
            {
                CancelBuildPreview();
            }
            else
            {
                Hide();
            }
        }
    }

    void LateUpdate()
    {
        if (uiPanel.activeSelf && currentSpotTransform != null)
        {
            transform.position = currentSpotTransform.position;
        }
    }

    public void Show(TowerSpotController spot)
    {
        if (currentSpot != spot)
        {
            CancelBuildPreview();
        }
        
        currentSpot = spot;
        currentSpotTransform = spot.transform;

        UpdateCostText(archerButton, archerBlueprint);
        UpdateCostText(mageButton, mageBlueprint);
        UpdateCostText(barracksButton, barracksBlueprint);
        UpdateCostText(bombButton, bombBlueprint);
        UpdateCostText(gunButton, gunBlueprint);

        transform.position = spot.transform.position;
        uiPanel.SetActive(true);
    }

    private void UpdateCostText(Button button, TowerBlueprint blueprint)
    {
        if (button != null && blueprint != null)
        {
            // 버튼의 자식 계층 어디에 있든 TextMeshPro 컴포넌트를 찾아 비용을 업데이트합니다.
            button.GetComponentInChildren<TextMeshProUGUI>().text = blueprint.cost + "G";
        }
    }

    public void Hide()
    {
        CancelBuildPreview();
        uiPanel.SetActive(false);
        currentSpotTransform = null;
    }
    
    public void RequestBuild(TowerBlueprint blueprint)
    {
        Button clickedButton = blueprintButtonMap[blueprint];

        if (pendingBlueprint == blueprint)
        {
            currentSpot.BuildTower(pendingBlueprint);
            Hide();
        }
        else
        {
            CancelBuildPreview();
            pendingBlueprint = blueprint;
            
            // (수정) 버튼의 'Target Graphic'을 직접 찾아 아이콘을 변경합니다.
            if (clickedButton != null && confirmIcon != null)
            {
                Image targetImage = clickedButton.targetGraphic as Image;
                if (targetImage != null)
                {
                    targetImage.sprite = confirmIcon;
                }
            }

            previewTower = Instantiate(blueprint.prefab, currentSpot.transform.position, Quaternion.identity);
            
            if (previewTower.GetComponent<TowerController>() != null) previewTower.GetComponent<TowerController>().enabled = false;
            if (previewTower.GetComponent<BarracksController>() != null) previewTower.GetComponent<BarracksController>().enabled = false;

            SetObjectTransparency(previewTower, 0.7f);
        }
    }
    
    private void CancelBuildPreview()
    {
        if (previewTower != null)
        {
            Destroy(previewTower);
            previewTower = null;
        }
        pendingBlueprint = null;
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
                // (수정) 버튼의 'Target Graphic'을 직접 찾아 아이콘을 원래대로 되돌립니다.
                Image targetImage = button.targetGraphic as Image;
                if (targetImage != null)
                {
                    targetImage.sprite = originalIcon;
                }
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
    
    public void BuildArcherTower() { RequestBuild(archerBlueprint); }
    public void BuildMageTower() { RequestBuild(mageBlueprint); }
    public void BuildBarracksTower() { RequestBuild(barracksBlueprint); }
    public void BuildBombTower() { RequestBuild(bombBlueprint); }
    public void BuildGunTower() { RequestBuild(gunBlueprint); }
}

