using UnityEngine;
using UnityEngine.EventSystems;

// UI 버튼 위에 마우스가 올라오거나 나가는 이벤트를 감지하는 스크립트입니다.
public class SkillButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 이 버튼이 표시해야 할 스킬 설명을 저장할 변수입니다.
    public string skillDescription;

    // 마우스 커서가 버튼 영역 안으로 들어왔을 때 호출됩니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // (수정) 스킬 설명이 비어있지 않을 때만 툴팁을 보여줍니다.
        if (!string.IsNullOrEmpty(skillDescription))
        {
            // TowerUpgradeUI에 툴팁을 보여달라고 요청합니다.
            TowerUpgradeUI.instance.ShowSkillTooltip(skillDescription);
        }
    }

    // 마우스 커서가 버튼 영역 밖으로 나갔을 때 호출됩니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        // (수정) 스킬 설명이 비어있지 않을 때만 툴팁을 숨깁니다.
        if (!string.IsNullOrEmpty(skillDescription))
        {
            // TowerUpgradeUI에 툴팁을 숨겨달라고 요청합니다.
            TowerUpgradeUI.instance.HideSkillTooltip();
        }
    }
}

