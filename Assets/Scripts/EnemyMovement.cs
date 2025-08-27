using UnityEngine;

// 이 스크립트는 Enemy 오브젝트에 부착되어 설정된 Waypoint들을 따라 이동시키는 역할을 합니다.
public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 2f;

    // Hierarchy 뷰에 있는 Waypoints 오브젝트를 할당할 변수입니다.
    private Transform waypointHolder;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;

    // Update is called once per frame
    void Update()
    {
        // waypoints가 아직 설정되지 않았다면 아무것도 하지 않습니다. (GameManager가 설정해줄 때까지 대기)
        if (waypoints == null)
            return;

        // 모든 경유지를 다 지났는지 확인합니다.
        if (currentWaypointIndex >= waypoints.Length)
        {
            // (수정) 길 끝에 도달했으므로, GameManager에 알리고 자신을 파괴합니다.
            GameManager.instance.EnemyReachedEnd();
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            currentWaypointIndex++;
        }
    }
    
    // GameManager로부터 WaypointHolder 정보를 받기 위한 함수입니다.
    public void SetWaypointHolder(Transform _waypointHolder)
    {
        waypointHolder = _waypointHolder;
        waypoints = new Transform[waypointHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = waypointHolder.GetChild(i);
        }
        transform.position = waypoints[currentWaypointIndex].position;
    }
}
