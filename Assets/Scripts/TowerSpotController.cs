using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSpotController : MonoBehaviour, IPointerClickHandler
{
    private GameObject currentTower;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentTower != null)
        {
            currentTower.GetComponent<IPointerClickHandler>().OnPointerClick(eventData);
            return;
        }
        
        TowerBuildUI.instance.Show(this);
    }

    public void BuildTower(TowerBlueprint blueprint)
    {
        if (GameManager.instance.SpendGold(blueprint.cost))
        {
            // (수정) 타워 건설 시 사운드를 재생합니다.
            SoundManager.instance.PlayBuildSound();

            GameObject towerGO = Instantiate(blueprint.prefab, transform.position, Quaternion.identity);
            currentTower = towerGO;

            TowerController towerController = currentTower.GetComponent<TowerController>();
            if (towerController != null)
            {
                towerController.SetParentSpot(this);
            }
        }
    }

    public void SetCurrentTower(GameObject tower)
    {
        currentTower = tower;
    }
}
