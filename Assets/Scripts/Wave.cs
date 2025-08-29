using UnityEngine;

// 웨이브 정보를 담는 스크립트입니다.
[System.Serializable]
public class Wave
{
    public EnemyGroup[] enemyGroups;
    public float timeBetweenGroups = 1f;

    [System.Serializable]
    public struct EnemyGroup
    {
        public GameObject enemyPrefab;
        public int count;
        // (수정) 'rate'(초당 스폰 수) 대신 'spawnInterval'(스폰 간격-초)을 사용하도록 변경하여 더 직관적으로 만듭니다.
        public float spawnInterval;
    }
}

