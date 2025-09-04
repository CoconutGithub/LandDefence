//TechTreemanager.cs
using UnityEngine;
using TMPro;

// 테크 트리의 모든 UI와 로직을 관리하는 스크립트입니다.
public class TechTreeManager : MonoBehaviour
{
    // (수정) 각 타워 종류별 UI 요소들을 묶어서 관리하기 위한 클래스입니다.
    [System.Serializable]
    public class TowerUpgradeUI
    {
        public TowerType type;
        public TextMeshProUGUI expText;
        public TextMeshProUGUI buttonText;
    }

    [Header("UI Components")]
    public TowerUpgradeUI[] towerUIs; // (수정) 여러 타워의 UI를 배열로 관리합니다.

    void OnEnable()
    {
        // 패널이 보일 때마다 모든 타워 UI를 최신 정보로 업데이트합니다.
        UpdateAllTowerUIs();
    }

    // 모든 타워 UI를 업데이트하는 함수입니다.
    void UpdateAllTowerUIs()
    {
        foreach (var ui in towerUIs)
        {
            UpdateSingleTowerUI(ui);
        }
    }

    // 하나의 타워 UI를 업데이트하는 함수입니다.
    void UpdateSingleTowerUI(TowerUpgradeUI ui)
    {
        int totalExp = DataManager.LoadExperience(ui.type);
        int damageLevel = DataManager.LoadDamageLevel(ui.type);

        ui.expText.text = $"보유 {ui.type} 경험치: {totalExp}";

        int upgradeCost = 100 + (damageLevel * 50);
        ui.buttonText.text = $"{ui.type} 공격력 강화 ({damageLevel + 1}레벨)\n(비용: {upgradeCost} EXP)";
    }

    // (수정) 이제 버튼에서 직접 어떤 타워를 업그레이드할지 알려줘야 합니다.
    // Unity 에디터의 버튼 OnClick() 이벤트에서 int 값을 전달합니다.
    // 0: Archer, 1: Mage, 2: Barracks, 3: Bomb, 4: Gun
    public void UpgradeTowerDamage(int towerTypeIndex)
    {
        TowerType type = (TowerType)towerTypeIndex; // 정수를 TowerType으로 변환합니다.

        int totalExp = DataManager.LoadExperience(type);
        int damageLevel = DataManager.LoadDamageLevel(type);
        int upgradeCost = 100 + (damageLevel * 50);

        if (totalExp >= upgradeCost)
        {
            totalExp -= upgradeCost;
            damageLevel++;

            DataManager.SaveExperience(type, totalExp);
            DataManager.SaveDamageLevel(type, damageLevel);

            Debug.Log($"{type} 공격력 강화 완료! 현재 레벨: {damageLevel}");
            
            // 변경된 UI만 다시 업데이트합니다.
            UpdateAllTowerUIs();
        }
        else
        {
            Debug.Log($"{type} 경험치가 부족합니다!");
        }
    }
}
