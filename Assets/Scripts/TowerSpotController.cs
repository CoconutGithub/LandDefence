using UnityEngine;
using UnityEngine.EventSystems; // (수정) Unity의 이벤트 시스템을 다시 사용합니다.

// (수정) IPointerClickHandler 인터페이스를 다시 상속받습니다.
public class TowerSpotController : MonoBehaviour, IPointerClickHandler
{
    private GameObject currentTower; // 현재 이 부지에 건설된 타워를 저장하는 변수입니다.

    // (수정) OnMouseDown 대신 OnPointerClick 함수를 사용합니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        // --- 디버깅을 위한 로그 ---
        // 이 메시지가 콘솔에 나타나는지 확인하는 것이 가장 중요합니다.
        // Debug.Log(gameObject.name + " 클릭 감지!");

        // 현재 부지에 타워가 있다면 아무것도 하지 않습니다.
        if (currentTower != null)
        {
            Debug.Log("이미 타워가 건설되어 있습니다.");
            // 여기에 나중에 타워 업그레이드 UI를 여는 코드를 추가할 수 있습니다.
            return;
        }
        
        // 타워 건설 UI를 엽니다.
        TowerBuildUI.instance.Show(this);
    }

    // TowerBuildUI로부터 건설할 타워 설계도를 받아와 타워를 건설합니다.
    public void BuildTower(TowerBlueprint blueprint)
    {
        // 골드가 충분한지 확인하고 소모합니다.
        if (GameManager.instance.SpendGold(blueprint.cost))
        {
            // 타워 프리팹을 현재 타워 부지의 위치에 생성합니다.
            GameObject towerGO = Instantiate(blueprint.prefab, transform.position, Quaternion.identity);
            currentTower = towerGO;
        }
    }
}
