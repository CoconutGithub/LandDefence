using UnityEngine;

// 타워 스킬의 설계도 역할을 하는 데이터 클래스입니다.
[System.Serializable]
public class TowerSkillBlueprint
{
    [Header("스킬 기본 정보")]
    public string skillName;
    [TextArea]
    public string skillDescription;

    [Header("스킬 레벨별 데이터")]
    public int maxLevel = 3;
    public int[] costs; 

    public float[] values1;
    public float[] values2;
    public float[] values3;

    [Header("스킬 발사체 오버라이드 (선택 사항)")]
    public Sprite overrideProjectileSprite; // 이 스킬 발동 시, 기본 발사체의 이미지를 이것으로 교체합니다. (애니메이션이 없을 경우)
    public GameObject overrideProjectilePrefab; // 이 스킬 발동 시, 기본 발사체 대신 이 프리팹을 생성합니다. (애니메이션이 있을 경우)
}
