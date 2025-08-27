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
    [SerializeField]
    private TextMeshProUGUI livesText;

    [Header("Player Stats")]
    [SerializeField]
    private int startGold = 100;
    [SerializeField]
    private int startLives = 20;
    private int currentGold;
    private int currentExperience = 0;
    private int currentLives;

    private int waveIndex = 0;
    private float countdown = 2f;
    private bool isGameOver = false;
    private int enemiesAlive = 0; // 현재 살아있는 적의 수를 저장하는 변수입니다.

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        isGameOver = false;
        enemiesAlive = 0;
        currentGold = startGold;
        currentLives = startLives;
        UpdateGoldText();
        UpdateExperienceText();
        UpdateLivesText();
    }

    void Update()
    {
        if (isGameOver)
            return;

        // 모든 웨이브가 끝나고 살아있는 적이 없다면 승리 처리
        if (waveIndex >= waves.Length && enemiesAlive <= 0)
        {
            WinGame();
            return; // 승리 후에는 더 이상 Update를 실행하지 않습니다.
        }

        // 마지막 웨이브가 진행 중이면 더 이상 다음 웨이브 카운트다운을 하지 않습니다.
        if (waveIndex >= waves.Length) { return; }

        if (countdown <= 0f)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }
        countdown -= Time.deltaTime;
    }
    
    // 적이 처치되거나 도착했을 때 호출되는 함수입니다.
    public void EnemyDefeated()
    {
        enemiesAlive--;
    }

    public void EnemyReachedEnd()
    {
        currentLives--;
        UpdateLivesText();

        if (currentLives <= 0 && !isGameOver) // 게임오버가 아닐 때만 실행
        {
            GameOver();
        }
    }
    
    void GameOver()
    {
        isGameOver = true;
        Debug.Log("게임 오버!");
    }

    // 게임 승리 처리 함수입니다.
    void WinGame()
    {
        isGameOver = true;
        Debug.Log("승리!");
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
    
    void UpdateLivesText()
    {
        livesText.text = "Lives: " + currentLives;
    }

    IEnumerator SpawnWave()
    {
        // 마지막 웨이브인지 확인
        if (waveIndex >= waves.Length)
        {
            yield break;
        }

        Wave wave = waves[waveIndex];
        foreach (EnemyGroup group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(group.rate);
            }
            yield return new WaitForSeconds(wave.timeBetweenGroups);
        }
        waveIndex++;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        // 적을 생성할 때마다 살아있는 적의 수를 1 증가시킵니다.
        enemiesAlive++;
        GameObject enemyGO = Instantiate(enemyPrefab, waypointHolder.GetChild(0).position, Quaternion.identity);
        enemyGO.GetComponent<EnemyMovement>().SendMessage("SetWaypointHolder", waypointHolder);
    }
}
