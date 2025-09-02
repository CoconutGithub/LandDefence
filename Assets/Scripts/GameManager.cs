using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // (추가) 다른 스크립트에서 웨이포인트 정보에 접근할 수 있도록 static 변수로 선언합니다.
    public static Transform WaypointHolder { get; private set; }

    void Awake()
    {
        if (instance != null) { return; }
        instance = this;

        // (추가) 이 GameManager가 관리하는 웨이포인트 홀더를 static 변수에 할당합니다.
        WaypointHolder = waypointHolder;
    }

    [Header("게임 설정")]
    [SerializeField]
    private int startLives = 20;
    [SerializeField]
    private int startGold = 100;

    [Header("UI 연결")]
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

    [Header("웨이브 설정")]
    [SerializeField]
    private Wave[] waves;
    [SerializeField]
    private Transform waypointHolder;
    [SerializeField]
    private float timeBetweenWaves = 5f;

    private int lives;
    private int gold;
    private Dictionary<TowerType, int> towerExperiences = new Dictionary<TowerType, int>();
    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool gameEnded = false;

    void Start()
    {
        lives = startLives;
        gold = startGold;
        UpdateGoldUI();
        UpdateLivesUI();
        UpdateExperienceUI();
        resultUIPanel.SetActive(false);
    }

    void Update()
    {
        if (gameEnded) return;

        if (enemiesAlive > 0)
        {
            return;
        }

        if (currentWaveIndex >= waves.Length)
        {
            if(!gameEnded) WinGame();
            return;
        }

        if (timeBetweenWaves <= 0f)
        {
            StartCoroutine(SpawnWave());
            timeBetweenWaves = 5f; 
        }
        else
        {
            timeBetweenWaves -= Time.deltaTime;
        }
    }

    System.Collections.IEnumerator SpawnWave()
    {
        Wave wave = waves[currentWaveIndex];

        foreach (Wave.EnemyGroup group in wave.enemyGroups)
        {
            enemiesAlive += group.count;
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(group.spawnInterval);
            }
            yield return new WaitForSeconds(wave.timeBetweenGroups);
        }
        currentWaveIndex++;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        GameObject enemyGO = Instantiate(enemyPrefab, waypointHolder.GetChild(0).position, Quaternion.identity);
        EnemyMovement enemy = enemyGO.GetComponent<EnemyMovement>();
        enemy.SetWaypointHolder(waypointHolder);
    }
    
    public void HealLives(int amount)
    {
        lives += amount;
        UpdateLivesUI();
        SoundManager.instance.PlayBuildSound();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateGoldUI();
            return true;
        }
        return false;
    }
    
    public void AddExperience(int amount, TowerType type)
    {
        if (type == TowerType.None || type == TowerType.Hero) return;

        if (!towerExperiences.ContainsKey(type))
        {
            towerExperiences[type] = 0;
        }
        towerExperiences[type] += amount;
        UpdateExperienceUI();
    }
    
    public void EnemyReachedEnd()
    {
        if (gameEnded) return;
        lives--;
        UpdateLivesUI();
        if (lives <= 0)
        {
            EndGame(false);
        }
    }

    public void EnemyDefeated()
    {
        enemiesAlive--;
    }

    void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        foreach (KeyValuePair<TowerType, int> entry in towerExperiences)
        {
            int savedExp = DataManager.LoadExperience(entry.Key);
            int totalExp = savedExp + entry.Value;
            DataManager.SaveExperience(entry.Key, totalExp);
        }
        
        EndGame(true);
    }

    void EndGame(bool isWin)
    {
        gameEnded = true;
        resultUIPanel.SetActive(true);
        if (isWin)
        {
            resultText.text = "승리!";
        }
        else
        {
            resultText.text = "패배!";
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void UpdateGoldUI()
    {
        goldText.text = "Gold: " + gold;
    }

    void UpdateLivesUI()
    {
        livesText.text = "Lives: " + lives;
    }



    void UpdateExperienceUI()
    {
        experienceText.text = "";
        foreach (KeyValuePair<TowerType, int> entry in towerExperiences)
        {
            experienceText.text += $"{entry.Key} EXP: {entry.Value}\n";
        }
    }
}

