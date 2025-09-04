//TowerSpotController.cs
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
            currentTower.GetComponent<IPointerClickHandler>().OnPointerClick(eventData);
            return;
        }
        
        TowerBuildUI.instance.Show(this);
    }

    public void BuildTower(TowerBlueprint blueprint)
    {
        if (GameManager.instance.SpendGold(blueprint.cost))
        {
            SoundManager.instance.PlayBuildSound();
            GameObject towerGO = Instantiate(blueprint.prefab, transform.position, Quaternion.identity);
            currentTower = towerGO;

            // (수정) 생성된 타워의 종류를 확인하고, 각각에 맞는 방식으로 부모 정보를 설정합니다.
            TowerController towerController = currentTower.GetComponent<TowerController>();
            if (towerController != null)
            {
                towerController.SetParentSpot(this);
            }

            BarracksController barracksController = currentTower.GetComponent<BarracksController>();
            if (barracksController != null)
            {
                barracksController.SetParentSpot(this);
            }
        }
    }

    // 업그레이드 시 현재 타워 정보를 갱신하기 위한 함수
    public void SetCurrentTower(GameObject tower)
    {
        currentTower = tower;
    }
}
