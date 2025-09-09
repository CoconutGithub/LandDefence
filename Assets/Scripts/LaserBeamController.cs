using UnityEngine;

// (수정) 스프라이트 기반 레이저의 시각적 요소를 제어하는 새로운 방식의 스크립트입니다.
public class LaserBeamController : MonoBehaviour
{
    [Header("레이저 파츠 연결")]
    [SerializeField]
    private SpriteRenderer startCap;
    [SerializeField]
    private SpriteRenderer middleBeam;
    [SerializeField]
    private SpriteRenderer endCap;

    // TowerController에서 호출하여 레이저의 시작점, 끝점, 색상을 업데이트합니다.
    public void UpdateLaser(Vector3 startPoint, Vector3 endPoint, Color color)
    {
        // 1. 레이저의 시작 위치를 설정합니다.
        transform.position = startPoint;

        // 2. 끝점을 향하도록 레이저 전체를 회전시킵니다.
        Vector3 direction = endPoint - startPoint;
        transform.right = direction;

        // 3. 레이저의 길이를 계산합니다.
        float beamLength = direction.magnitude;

        // 4. 시작 파츠의 색상을 설정합니다. (위치는 항상 (0,0,0))
        if (startCap != null)
        {
            startCap.color = color;
        }

        // 5. 끝 파츠를 레이저의 길이에 맞춰 끝점에 배치하고 색상을 설정합니다.
        if (endCap != null)
        {
            endCap.transform.localPosition = new Vector3(beamLength, 0, 0);
            endCap.color = color;
        }
        
        // 6. 중간 파츠의 길이를 조절하고 색상을 설정합니다.
        if (middleBeam != null)
        {
            // Sprite Renderer의 Draw Mode가 'Tiled'로 설정되어 있어야 합니다.
            // size.x 값을 조절하면 스프라이트가 길이에 맞게 반복해서 그려집니다.
            middleBeam.size = new Vector2(beamLength, middleBeam.size.y);
            middleBeam.color = color;
        }
    }
}

