using UnityEngine;

// 자식 RectTransform들을 원형으로 배치하는 커스텀 레이아웃 그룹입니다.
// [ExecuteInEditMode] 속성은 에디터 모드에서도 스크립트가 작동하여 실시간으로 배치를 확인할 수 있게 해줍니다.
[ExecuteInEditMode]
public class CircularLayoutGroup : MonoBehaviour
{
    [Header("레이아웃 설정")]
    [SerializeField]
    private float radius = 80f; // 버튼들이 배치될 원의 반지름입니다.
    [SerializeField]
    private float startAngle = 90f; // 첫 번째 버튼이 배치될 시작 각도입니다. (90 = 위쪽)

    // 이 컴포넌트가 활성화될 때마다 레이아웃을 업데이트합니다.
    private void OnEnable()
    {
        UpdateLayout();
    }

    // 자식 오브젝트의 수나 순서가 변경될 때마다 자동으로 레이아웃을 업데이트합니다.
    private void OnTransformChildrenChanged()
    {
        UpdateLayout();
    }

    // Inspector에서 값이 변경될 때마다 실시간으로 레이아웃을 업데이트합니다.
    private void OnValidate()
    {
        UpdateLayout();
    }

    // 실제로 자식들의 위치를 계산하고 배치하는 함수입니다.
    public void UpdateLayout()
    {
        int childCount = transform.childCount;
        // 자식이 없으면 아무것도 하지 않습니다.
        if (childCount == 0) return;

        // 360도를 자식의 수로 나누어 각 버튼 사이의 각도를 계산합니다.
        float angleStep = 360f / childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            RectTransform rectChild = child as RectTransform;

            if (rectChild != null)
            {
                // 현재 버튼의 각도를 계산합니다. (시계 방향으로 배치하기 위해 -를 붙입니다)
                float angle = (startAngle - i * angleStep) * Mathf.Deg2Rad; // 각도를 라디안으로 변환
                
                // 삼각함수를 이용해 원 위의 좌표를 계산합니다.
                Vector2 newPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                
                // 계산된 위치를 버튼의 anchoredPosition에 적용합니다.
                rectChild.anchoredPosition = newPos;
            }
        }
    }
}
