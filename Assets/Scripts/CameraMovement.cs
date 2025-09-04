//CameraMovement.cs
using UnityEngine;

// 이 스크립트는 메인 카메라에 부착되어 마우스 드래그로 카메라를 이동시키는 역할을 합니다.
public class CameraMovement : MonoBehaviour
{
    // 카메라 이동 속도를 조절하기 위한 변수입니다. Inspector 창에서 값을 바꿀 수 있습니다.
    [SerializeField]
    private float moveSpeed = 0.1f;

    // 마우스 드래그가 시작된 위치를 저장하기 위한 변수입니다.
    private Vector3 dragOrigin;

    // Update 함수는 매 프레임마다 호출됩니다. 게임의 상태를 계속 확인하고 업데이트합니다.
    void Update()
    {
        // 마우스 오른쪽 버튼을 클릭했을 때의 로직
        // GetMouseButtonDown(1)은 마우스 오른쪽 버튼을 '누르는 순간'을 감지합니다.
        if (Input.GetMouseButtonDown(1))
        {
            // 마우스 커서의 현재 위치(화면 좌표)를 월드 좌표로 변환하여 dragOrigin에 저장합니다.
            // 이것이 드래그 시작점이 됩니다.
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        // 마우스 오른쪽 버튼을 누르고 있는 동안의 로직
        // GetMouseButton(1)은 마우스 오른쪽 버튼을 '누르고 있는 동안' 계속 감지합니다.
        if (Input.GetMouseButton(1))
        {
            // 현재 마우스 위치와 드래그 시작점의 차이를 계산합니다.
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 카메라의 현재 위치에 계산된 차이값을 더해 새로운 위치로 이동시킵니다.
            // 이렇게 하면 마우스를 드래그하는 방향의 반대로 카메라가 움직여 자연스러운 이동 효과를 줍니다.
            transform.position += difference;
        }
    }
}
