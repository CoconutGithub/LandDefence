using UnityEngine;

// 타워의 설계도 역할을 하는 데이터 클래스입니다.
[System.Serializable]
public class TowerBlueprint
{
    public GameObject prefab;
    public int cost;
    public string towerName;
    public Sprite icon; // (추가) UI에 표시될 아이콘 이미지입니다.
}
