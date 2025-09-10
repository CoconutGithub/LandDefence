using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

// (추가) 타워 타입에 따른 경험치 구슬 스프라이트를 관리하기 위한 클래스
[System.Serializable]
public class ExperienceOrbSprite
{
    public TowerType towerType;
    public Sprite sprite;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Transform WaypointHolder { get; private set; }

    void Awake()
    {
        if (instance != null) { return; }
        instance = this;
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

    // (추가) 인스펙터에서 설정할 경험치 구슬 스프라이트 목록
    [Header("경험치 구슬 스프라이트")]
    public List<ExperienceOrbSprite> experienceOrbSprites;

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

    // (추가) 타워 타입에 맞는 경험치 구슬 스프라이트를 찾아 반환하는 함수
    public Sprite GetExperienceOrbSprite(TowerType type)
    {
        // 목록에서 일치하는 타워 타입을 찾습니다.
        foreach (var orbSprite in experienceOrbSprites)
        {
            if (orbSprite.towerType == type)
            {
                return orbSprite.sprite; // 찾았으면 해당 스프라이트를 반환합니다.
            }
        }
        return null; // 맞는 스프라이트가 없으면 null을 반환합니다.
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

