using System.Collections;
using System.Collections.Generic; // (추가) Dictionary를 사용하기 위해 추가합니다.
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
    private TextMeshProUGUI experienceText; // 이 텍스트는 이제 모든 경험치를 요약해서 보여줍니다.
    [SerializeField]
    private TextMeshProUGUI livesText;
    [SerializeField]
    private GameObject resultUIPanel;
    [SerializeField]
    private TextMeshProUGUI resultText;

    [Header("Player Stats")]
    [SerializeField]
    private int startGold = 100;
    [SerializeField]
    private int startLives = 20;
    private int currentGold;
    private int currentLives;

    // (수정) 인게임에서 얻은 경험치를 타워 종류별로 저장하는 Dictionary입니다.
    private Dictionary<TowerType, int> inGameExperience;

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
        
        // (수정) 인게임 경험치 Dictionary를 초기화합니다.
        inGameExperience = new Dictionary<TowerType, int>();

        UpdateGoldText();
        UpdateExperienceText();
        UpdateLivesText();
        resultUIPanel.SetActive(false);
    }
    
    void WinGame()
    {
        isGameOver = true;
        resultUIPanel.SetActive(true);
        resultText.text = "승리!";

        // (수정) 게임 승리 시, 이번 판에서 얻은 모든 종류의 경험치를 영구 저장합니다.
        foreach (var expPair in inGameExperience)
        {
            TowerType type = expPair.Key;
            int gainedExp = expPair.Value;

            int totalExp = DataManager.LoadExperience(type);
            totalExp += gainedExp;
            DataManager.SaveExperience(type, totalExp);
        }
    }

    // AddExperience 함수가 타워 종류를 받아서 처리합니다.
    public void AddExperience(int amount, TowerType type)
    {
        // (수정) Dictionary에 경험치를 누적합니다.
        if (!inGameExperience.ContainsKey(type))
        {
            inGameExperience[type] = 0;
        }
        inGameExperience[type] += amount;

        UpdateExperienceText();
    }

    // UpdateExperienceText 함수가 모든 경험치를 요약해서 보여주도록 변경합니다.
    void UpdateExperienceText()
    {
        string expString = "";
        foreach (var expPair in inGameExperience)
        {
            expString += $"{expPair.Key}: {expPair.Value} EXP\n";
        }
        experienceText.text = expString;
    }
    
    // ... (이하 다른 함수들은 기존과 동일) ...
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        resultUIPanel.SetActive(true);
        resultText.text = "패배!";
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
    void UpdateGoldText()
    {
        goldText.text = "Gold: " + currentGold;
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
