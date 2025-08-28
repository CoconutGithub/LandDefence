using UnityEngine;

public class ExperienceController : MonoBehaviour
{
    private int experienceValue;
    private TowerType towerType;

    // Setup 함수로 경험치 값과 종류를 설정합니다.
    public void Setup(int value, TowerType type)
    {
        experienceValue = value;
        towerType = type;
    }

    // (수정) OnMouseDown을 Collect 함수로 변경하여 영웅이 직접 호출하도록 합니다.
    public void Collect()
    {
        // GameManager에 경험치를 추가할 때, 종류도 함께 알려줍니다.
        GameManager.instance.AddExperience(experienceValue, towerType);
        Destroy(gameObject);
    }
}
