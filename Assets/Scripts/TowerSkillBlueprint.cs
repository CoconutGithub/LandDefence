//TowerSkillBlueprint.cs
using UnityEngine;

// 타워 스킬의 설계도 역할을 하는 데이터 클래스입니다.
// 스킬 이름, 설명, 레벨별 비용 및 능력치 정보를 담습니다.
// 이 스크립트는 게임 오브젝트에 붙이지 않고, 최종 진화 타워의 Inspector 창에서 설정합니다.
[System.Serializable]
public class TowerSkillBlueprint
{
    [Header("스킬 기본 정보")]
    public string skillName; // UI에 표시될 스킬 이름 (내부적으로도 이 이름으로 스킬을 구분합니다)
    [TextArea]
    public string skillDescription; // UI에 표시될 스킬 설명

    [Header("스킬 레벨별 데이터")]
    public int maxLevel = 3; // 스킬의 최대 레벨
    public int[] costs; // 각 레벨로 업그레이드하는 데 필요한 골드 (0번 인덱스 = 1레벨 비용)

    // 스킬의 효과를 위한 값들. 스킬마다 사용하는 값이 다릅니다.
    // 예: 불화살 -> value1: 발동 확률, value2: 초당 데미지, value3: 지속 시간
    public float[] values1;
    public float[] values2;
    public float[] values3;
}
