using UnityEngine;

// 이 스크립트는 Enemy 오브젝트에 부착되어 설정된 Waypoint들을 따라 이동시키는 역할을 합니다.
public class EnemyMovement : MonoBehaviour
{
    // [SerializeField]는 private 변수를 Inspector 창에 노출시켜 값을 쉽게 수정할 수 있게 해줍니다.
    // 적의 이동 속도입니다.
    [SerializeField]
    private float moveSpeed = 2f;

    // Hierarchy 뷰에 있는 Waypoints 오브젝트를 할당할 변수입니다.
    // 이 변수를 통해 모든 경유지(자식 오브젝트들)의 위치 정보에 접근할 수 있습니다.
    [SerializeField]
    private Transform waypointHolder;

    // waypointHolder에 있는 모든 경유지(Transform)들을 담아둘 배열입니다.
    private Transform[] waypoints;

    // 현재 목표로 하는 경유지의 인덱스(순번)입니다. 0부터 시작합니다.
    private int currentWaypointIndex = 0;

    // Start 함수는 게임이 시작될 때 한 번만 호출됩니다.
    // 초기 설정을 하기에 좋은 곳입니다.
    
    // void Start()
    // {
    //     // waypointHolder의 자식 오브젝트 개수만큼 배열 크기를 설정합니다.
    //     waypoints = new Transform[waypointHolder.childCount];
    //     // for문을 이용해 waypointHolder의 모든 자식 오브젝트(WP_0, WP_1...)를 waypoints 배열에 순서대로 담습니다.
    //     for (int i = 0; i < waypoints.Length; i++)
    //     {
    //         waypoints[i] = waypointHolder.GetChild(i);
    //     }

    //     // 게임이 시작되면 적의 위치를 첫 번째 경유지(WP_0)의 위치로 설정합니다.
    //     transform.position = waypoints[currentWaypointIndex].position;
    // }

    // Update 함수는 매 프레임마다 호출됩니다.
    void Update()
    {
        // 모든 경유지를 다 지났는지 확인합니다.
        if (currentWaypointIndex >= waypoints.Length)
        {
            // 다 지났다면 도착한 것이므로 적 오브젝트를 파괴합니다.
            Destroy(gameObject);
            return; // 아래 코드를 더 이상 실행하지 않고 함수를 종료합니다.
        }

        // 현재 목표 경유지의 위치를 가져옵니다.
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;

        // 현재 위치에서 목표 위치까지 프레임당 moveSpeed 만큼 이동합니다.
        // Time.deltaTime은 프레임 속도에 상관없이 일정한 속도로 움직이게 해주는 보정값입니다.
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 적이 목표 경유지에 거의 도착했는지 확인합니다. (컴퓨터는 소수점 오차가 있을 수 있어 정확히 일치하는지 비교하기 어렵습니다)
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            // 도착했다면 다음 경유지를 목표로 설정하기 위해 인덱스를 1 증가시킵니다.
            currentWaypointIndex++;
        }
    }
    // GameManager로부터 WaypointHolder 정보를 받기 위한 함수입니다.
    public void SetWaypointHolder(Transform _waypointHolder)
    {
        waypointHolder = _waypointHolder;
        // Start() 함수에 있던 초기화 코드를 여기로 옮겨와 다시 실행합니다.
        waypoints = new Transform[waypointHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = waypointHolder.GetChild(i);
        }
        transform.position = waypoints[currentWaypointIndex].position;
    }
}
