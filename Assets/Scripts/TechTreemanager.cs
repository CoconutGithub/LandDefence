using UnityEngine;
using TMPro;

// 테크 트리의 모든 UI와 로직을 관리하는 스크립트입니다.
public class TechTreeManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField]
    private TextMeshProUGUI archerExpText; // 궁수 경험치를 표시할 텍스트

    private int totalArcherExp; // 현재 보유한 총 궁수 경험치

    // OnEnable은 이 오브젝트(패널)가 활성화될 때마다 호출됩니다.
    void OnEnable()
    {
        // 패널이 보일 때마다 최신 경험치 정보를 불러와 업데이트합니다.
        totalArcherExp = DataManager.LoadArcherExperience();
        UpdateUI();
    }

    // UI 텍스트를 현재 데이터에 맞게 업데이트하는 함수입니다.
    void UpdateUI()
    {
        archerExpText.text = "보유 궁수 경험치: " + totalArcherExp;
    }

    // "궁수 공격력 강화" 버튼에 연결될 함수입니다.
    public void UpgradeArcherDamage()
    {
        int upgradeCost = 100; // 업그레이드 비용

        if (totalArcherExp >= upgradeCost)
        {
            totalArcherExp -= upgradeCost;
            DataManager.SaveArcherExperience(totalArcherExp);

            // 여기에 실제로 궁수 공격력을 영구적으로 강화하는 코드를 추가해야 합니다.
            Debug.Log("궁수 공격력 강화 완료!");
            UpdateUI();
        }
        else
        {
            Debug.Log("경험치가 부족합니다!");
        }
    }
}
