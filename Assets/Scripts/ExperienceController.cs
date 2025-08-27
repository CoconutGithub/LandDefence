using UnityEngine;

public class ExperienceController : MonoBehaviour
{
    private int experienceValue;
    private TowerType towerType; // (수정) 이 경험치의 종류

    // (수정) Setup 함수로 경험치 값과 종류를 설정합니다.
    public void Setup(int value, TowerType type)
    {
        experienceValue = value;
        towerType = type;
    }

    private void OnMouseDown()
    {
        // (수정) GameManager에 경험치를 추가할 때, 종류도 함께 알려줍니다.
        GameManager.instance.AddExperience(experienceValue, towerType);
        Destroy(gameObject);
    }
}