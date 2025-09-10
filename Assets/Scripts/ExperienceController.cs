using UnityEngine;

public class ExperienceController : MonoBehaviour
{
    private int experienceValue;
    private TowerType towerType;
    private SpriteRenderer spriteRenderer; // (추가) 스프라이트 렌더러 참조

    // (추가) Awake에서 SpriteRenderer 컴포넌트를 미리 찾아둡니다.
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("ExperienceController: SpriteRenderer 컴포넌트를 찾을 수 없습니다! ExperienceOrb 프리팹에 추가해주세요.");
        }
    }

    // (수정) Setup 함수에서 스프라이트를 설정하는 로직을 추가합니다.
    public void Setup(int value, TowerType type)
    {
        experienceValue = value;
        towerType = type;

        // GameManager에 해당 타워 타입의 스프라이트를 요청합니다.
        Sprite orbSprite = GameManager.instance.GetExperienceOrbSprite(type);

        // 찾은 스프라이트가 있고, 렌더러도 있다면
        if (orbSprite != null && spriteRenderer != null)
        {
            // 찾은 스프라이트로 이미지를 변경합니다.
            spriteRenderer.sprite = orbSprite;
        }
    }

    public void Collect()
    {
        GameManager.instance.AddExperience(experienceValue, towerType);
        Destroy(gameObject);
    }
}

