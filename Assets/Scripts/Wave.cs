using UnityEngine;

// 한 웨이브 내에서 특정 종류의 적 그룹을 정의하는 클래스입니다.
[System.Serializable]
public class EnemyGroup
{
    public GameObject enemyPrefab; // 이 그룹에서 생성할 적의 프리팹
    public int count;              // 이 그룹에서 생성할 적의 수
    public float rate;             // 이 그룹 내의 적 생성 간격
}

// 이제 Wave 클래스는 EnemyGroup의 배열을 가집니다.
[System.Serializable]
public class Wave
{
    public EnemyGroup[] enemyGroups; // 이 웨이브를 구성하는 적 그룹들
    public float timeBetweenGroups;  // 한 그룹의 생성이 끝나고 다음 그룹이 시작될 때까지의 대기 시간
}
