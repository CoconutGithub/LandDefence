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
    private TextMeshProUGUI livesText; // (수정) 생명력을 표시할 UI 텍스트입니다.

    [Header("Player Stats")]
    [SerializeField]
    private int startGold = 100;
    [SerializeField]
    private int startLives = 20; // (수정) 게임 시작 시 주어지는 생명력입니다.
    private int currentGold;
    private int currentExperience = 0;
    private int currentLives; // (수정) 현재 생명력을 저장하는 변수입니다.

    private int waveIndex = 0;
    private float countdown = 2f;
    private bool isGameOver = false; // (수정) 게임오버 상태를 확인하는 변수입니다.

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        isGameOver = false;
        currentGold = startGold;
        currentLives = startLives; // (수정) 생명력 초기화
        UpdateGoldText();
        UpdateExperienceText();
        UpdateLivesText(); // (수정) 생명력 UI 업데이트
    }

    void Update()
    {
        // (수정) 게임오버 상태이면 아무것도 실행하지 않습니다.
        if (isGameOver)
            return;

        if (waveIndex >= waves.Length) { return; }

        if (countdown <= 0f)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }
        countdown -= Time.deltaTime;
    }

    // (수정) 적이 길 끝에 도달했을 때 EnemyMovement에서 호출하는 함수입니다.
    public void EnemyReachedEnd()
    {
        currentLives--;
        UpdateLivesText();

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    // (수정) 게임오버 처리 함수입니다.
    void GameOver()
    {
        isGameOver = true;
        Debug.Log("게임 오버!");
        // Time.timeScale = 0f; // (선택) 게임 시간을 멈춰 모든 움직임을 정지시킵니다.
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

    // (수정) 생명력 UI 텍스트를 업데이트하는 함수입니다.
    void UpdateLivesText()
    {
        livesText.text = "Lives: " + currentLives;
    }

    IEnumerator SpawnWave()
    {
        if (waveIndex >= waves.Length)
        {
            Debug.Log("모든 웨이브를 클리어했습니다!");
            this.enabled = false;
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
        GameObject enemyGO = Instantiate(enemyPrefab, waypointHolder.GetChild(0).position, Quaternion.identity);
        enemyGO.GetComponent<EnemyMovement>().SendMessage("SetWaypointHolder", waypointHolder);
    }
}
