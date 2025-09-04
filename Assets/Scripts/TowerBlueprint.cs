//TowerBlueprint.cs
using UnityEngine;

// 타워의 설계도 역할을 하는 데이터 클래스입니다.
// 프리팹, 건설 비용 등의 정보를 담습니다.
// 이 스크립트는 게임 오브젝트에 붙이지 않습니다.
[System.Serializable]
public class TowerBlueprint
{
    public GameObject prefab;
    public int cost;
    public string towerName; // (추가) UI에 표시될 타워의 이름입니다.
}
