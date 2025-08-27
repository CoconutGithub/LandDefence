using UnityEngine;

// 이 스크립트는 Enemy 오브젝트에 부착되어 설정된 Waypoint들을 따라 이동시키는 역할을 합니다.
public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 2f;

    private Transform waypointHolder;
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints == null)
            return;

        if (currentWaypointIndex >= waypoints.Length)
        {
            // (수정) 적이 도착했음을 GameManager에 알리는 동시에, 적이 사라졌음도 알립니다.
            GameManager.instance.EnemyReachedEnd();
            GameManager.instance.EnemyDefeated(); // 도착한 적도 처치된 것으로 간주
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
