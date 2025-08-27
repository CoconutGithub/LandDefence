using UnityEngine;

// 경험치 구슬의 동작을 제어하는 스크립트입니다.
public class ExperienceController : MonoBehaviour
{
    [SerializeField]
    private int experienceValue = 5; // 이 구슬이 가진 경험치의 양입니다.

    // OnMouseDown 함수는 이 오브젝트의 Collider가 마우스로 클릭되었을 때 호출됩니다.
    private void OnMouseDown()
    {
        // GameManager에 경험치를 추가하라고 알립니다.
        GameManager.instance.AddExperience(experienceValue);

        // 경험치를 전달했으므로 이 구슬 오브젝트는 파괴합니다.
        Destroy(gameObject);
    }
}
