using UnityEngine;
using UnityEngine.SceneManagement;

// 메인 메뉴의 전반적인 흐름과 UI 패널 전환을 관리하는 스크립트입니다.
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField]
    private GameObject stageSelectPanel; // 스테이지 선택 패널
    [SerializeField]
    private GameObject techTreePanel;    // 테크 트리 패널

    void Start()
    {
        // 게임 시작 시 모든 패널을 숨깁니다.
        stageSelectPanel.SetActive(false);
        techTreePanel.SetActive(false);
    }

    // "스테이지 선택" 버튼에 연결될 함수입니다.
    public void ShowStageSelectPanel()
    {
        stageSelectPanel.SetActive(true);
        techTreePanel.SetActive(false);
    }

    // "업그레이드" 버튼에 연결될 함수입니다.
    public void ShowTechTreePanel()
    {
        stageSelectPanel.SetActive(false);
        techTreePanel.SetActive(true);
    }

    // "뒤로가기" 버튼에 연결되어 모든 패널을 숨기는 새로운 함수입니다.
    public void HideAllPanels()
    {
        stageSelectPanel.SetActive(false);
        techTreePanel.SetActive(false);
    }

    // "게임 시작" 버튼 (StageSelectPanel 안에 있음)에 연결될 함수입니다.
    public void StartGame()
    {
        // 게임 플레이 씬(SampleScene)을 로드합니다.
        SceneManager.LoadScene("SampleScene");
    }
}
