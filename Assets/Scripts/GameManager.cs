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
    private GameObject resultUIPanel;
    [SerializeField]
    private TextMeshProUGUI resultText;

    [Header("Player Stats")]
    [SerializeField]
    private int startGold = 100;
    [SerializeField]
    private int startLives = 20;
    private int currentGold;
    private int currentExperience = 0;
    private int currentLives;

    // (수정) 누락되었던 archerExperience 변수를 여기에 선언합니다.
    private int archerExperience = 0;

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
        archerExperience = 0; 
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

        int totalArcherExp = DataManager.LoadArcherExperience();
        totalArcherExp += archerExperience;
        DataManager.SaveArcherExperience(totalArcherExp);
    }
    
    void GameOver()
    {
        isGameOver = true;
        resultUIPanel.SetActive(true);
        resultText.text = "패배!";
    }

    // "다시 시작" 버튼에 연결될 함수입니다.
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // "메인 메뉴로" 버튼에 연결될 새로운 함수입니다.
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void AddExperience(int amount, TowerType type)
    {
        switch (type)
        {
            case TowerType.Archer:
                archerExperience += amount;
                break;
            case TowerType.Mage:
                break;
            case TowerType.Barracks:
                break;
        }
        UpdateExperienceText();
    }

    void UpdateExperienceText()
    {
        experienceText.text = "궁수 EXP: " + archerExperience;
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
