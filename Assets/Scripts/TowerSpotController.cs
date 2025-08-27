using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSpotController : MonoBehaviour, IPointerClickHandler
{
    private GameObject currentTower;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentTower != null)
        {
            // 이미 타워가 지어져 있다면, 타워의 클릭 이벤트를 대신 처리하도록 합니다.
            // 이렇게 하면 타워와 타워 부지가 겹쳐있을 때 타워가 우선적으로 클릭됩니다.
            currentTower.GetComponent<IPointerClickHandler>().OnPointerClick(eventData);
            return;
        }
        
        TowerBuildUI.instance.Show(this);
    }

    public void BuildTower(TowerBlueprint blueprint)
    {
        if (GameManager.instance.SpendGold(blueprint.cost))
        {
            GameObject towerGO = Instantiate(blueprint.prefab, transform.position, Quaternion.identity);
            currentTower = towerGO;

            // 생성된 타워에게 이 타워 부지 정보를 넘겨줍니다.
            TowerController towerController = currentTower.GetComponent<TowerController>();
            if (towerController != null)
            {
                towerController.SetParentSpot(this);
            }
        }
    }

    // 업그레이드 시 현재 타워 정보를 갱신하기 위한 함수
    public void SetCurrentTower(GameObject tower)
    {
        currentTower = tower;
    }
}
