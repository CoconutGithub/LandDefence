using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Wave Settings")]
    public Wave[] waves;
    [SerializeField]
    private float timeBetweenWaves = 5f;

    [Header("Required Components")]
    [SerializeField]
    private Transform waypointHolder;
    [SerializeField]
    private TextMeshProUGUI goldText;
    [SerializeField]
    private TextMeshProUGUI experienceText;

    [Header("Player Stats")]
    [SerializeField]
    private int startGold = 100;
    private int currentGold;
    private int currentExperience = 0;

    private int waveIndex = 0;
    private float countdown = 2f;

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        currentGold = startGold;
        UpdateGoldText();
        UpdateExperienceText();
    }

    void Update()
    {
        if (waveIndex >= waves.Length) { return; }

        if (countdown <= 0f)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }
        countdown -= Time.deltaTime;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldText();
    }
    
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldText();
            return true;
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
            return false;
        }
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;
        UpdateExperienceText();
    }

    void UpdateGoldText()
    {
        goldText.text = "Gold: " + currentGold;
    }

    void UpdateExperienceText()
    {
        experienceText.text = "EXP: " + currentExperience;
    }

    // (수정) 새로운 Wave 구조에 맞춰 웨이브 생성 로직을 변경합니다.
    IEnumerator SpawnWave()
    {
        if (waveIndex >= waves.Length)
        {
            Debug.Log("모든 웨이브를 클리어했습니다!");
            this.enabled = false;
            yield break;
        }

        Wave wave = waves[waveIndex];

        // 현재 웨이브에 포함된 모든 '적 그룹'을 순서대로 실행합니다.
        foreach (EnemyGroup group in wave.enemyGroups)
        {
            // 현재 그룹에 포함된 적들을 생성합니다.
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(group.rate);
            }

            // 이 그룹이 끝나고 다음 그룹이 시작되기 전까지 대기합니다.
            yield return new WaitForSeconds(wave.timeBetweenGroups);
        }

        waveIndex++;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        GameObject enemyGO = Instantiate(enemyPrefab, waypointHolder.GetChild(0).position, Quaternion.identity);
        enemyGO.GetComponent<EnemyMovement>().SendMessage("SetWaypointHolder", waypointHolder);
    }
}
