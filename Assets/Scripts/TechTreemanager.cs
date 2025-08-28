using UnityEngine;
using TMPro;

// 테크 트리의 모든 UI와 로직을 관리하는 스크립트입니다.
public class TechTreeManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField]
    private TextMeshProUGUI archerExpText;
    [SerializeField]
    private TextMeshProUGUI upgradeArcherButtonText; // (추가) 업그레이드 버튼의 텍스트

    private int totalArcherExp;
    private int archerDamageLevel; // (추가) 현재 궁수 공격력 레벨

    void OnEnable()
    {
        // 패널이 보일 때마다 최신 데이터를 불러옵니다.
        totalArcherExp = DataManager.LoadArcherExperience();
        archerDamageLevel = DataManager.LoadArcherDamageLevel();
        UpdateUI();
    }

    // UI 텍스트를 현재 데이터에 맞게 업데이트하는 함수입니다.
    void UpdateUI()
    {
        archerExpText.text = "보유 궁수 경험치: " + totalArcherExp;

        // (수정) 업그레이드 비용을 레벨에 따라 동적으로 계산합니다.
        int upgradeCost = 100 + (archerDamageLevel * 50); // 예: 레벨당 비용 50씩 증가
        upgradeArcherButtonText.text = $"궁수 공격력 강화 ({archerDamageLevel + 1}레벨)\n(비용: {upgradeCost} EXP)";
    }

    // "궁수 공격력 강화" 버튼에 연결될 함수입니다.
    public void UpgradeArcherDamage()
    {
        int upgradeCost = 100 + (archerDamageLevel * 50);

        if (totalArcherExp >= upgradeCost)
        {
            totalArcherExp -= upgradeCost;
            archerDamageLevel++; // 레벨 1 증가

            // 변경된 데이터를 저장합니다.
            DataManager.SaveArcherExperience(totalArcherExp);
            DataManager.SaveArcherDamageLevel(archerDamageLevel);

            Debug.Log($"궁수 공격력 강화 완료! 현재 레벨: {archerDamageLevel}");
            UpdateUI();
        }
        else
        {
            Debug.Log("경험치가 부족합니다!");
        }
    }
}
