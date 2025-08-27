using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField]
    private GameObject resultUIPanel; // (수정) 다시 하나의 결과 UI 패널로 통합합니다.
    [SerializeField]
    private TextMeshProUGUI resultText; // (수정) 결과 패널 안의 텍스트입니다.

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
    private int enemiesAlive = 0;

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
        resultUIPanel.SetActive(false); // (수정) 게임 시작 시 결과 패널을 숨깁니다.
    }

    void Update()
    {
        if (isGameOver)
            return;

        if (waveIndex >= waves.Length && enemiesAlive <= 0)
        {
            WinGame();
            return;
        }

        if (waveIndex >= waves.Length) { return; }

        if (countdown <= 0f)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }
        countdown -= Time.deltaTime;
    }
    
    public void EnemyDefeated()
    {
        enemiesAlive--;
    }

    public void EnemyReachedEnd()
    {
        currentLives--;
        UpdateLivesText();

        if (currentLives <= 0 && !isGameOver)
        {
            GameOver();
        }
    }
    
    void GameOver()
    {
        isGameOver = true;
        resultUIPanel.SetActive(true); // (수정) 결과 패널을 보여줍니다.
        resultText.text = "패배!"; // (수정) 패배 메시지를 설정합니다.
    }

    void WinGame()
    {
        isGameOver = true;
        resultUIPanel.SetActive(true); // (수정) 결과 패널을 보여줍니다.
        resultText.text = "승리!"; // (수정) 승리 메시지를 설정합니다.
    }

    // "다시 시작" 버튼에 연결될 함수입니다.
    public void RestartGame()
    {
        // 현재 활성화된 씬을 다시 로드합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        enemiesAlive++;
        GameObject enemyGO = Instantiate(enemyPrefab, waypointHolder.GetChild(0).position, Quaternion.identity);
        enemyGO.GetComponent<EnemyMovement>().SendMessage("SetWaypointHolder", waypointHolder);
    }
}
