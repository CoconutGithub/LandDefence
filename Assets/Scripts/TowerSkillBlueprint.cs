using UnityEngine;

// 타워 스킬의 설계도 역할을 하는 데이터 클래스입니다.
[System.Serializable]
public class TowerSkillBlueprint
{
    [Header("스킬 기본 정보")]
    public string skillName;
    [TextArea]
    public string skillDescription;
    public Sprite icon; // (추가) UI에 표시될 아이콘 이미지입니다.

    [Header("스킬 레벨별 데이터")]
    public int maxLevel = 3;
    public int[] costs; 

    public float[] values1;
    public float[] values2;
    public float[] values3;

    [Header("스킬 발사체 오버라이드 (선택 사항)")]
    public Sprite overrideProjectileSprite;
    public GameObject overrideProjectilePrefab;
}

