using UnityEngine;

public class TrainOnPolyline : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("Waypoints the train will move along (in order).")]
    public Transform[] waypoints;

    [Header("Movement")]
    [Tooltip("Speed of the train (units per second).")]
    public float speed = 5f;

    [Tooltip("Smoothness of rotation (higher = snappier).")]
    public float turnSpeed = 5f;

    [Tooltip("If true, the train restarts at the first waypoint when reaching the last.")]
    public bool loop = true;

    [Header("Debug")]
    [Tooltip("Draw gizmos to visualize the path in the editor.")]
    public bool showPath = true;

    private int currentIndex = 0;

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogError("[TrainOnPolyline] Please assign at least two waypoints.");
            enabled = false;
            return;
        }

        // Start train at first waypoint
        transform.position = waypoints[0].position;
        currentIndex = 1;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        Transform target = waypoints[currentIndex];

        // Move towards the next waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // Face the direction of travel
        Vector3 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(0, -90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentIndex++;

            if (currentIndex >= waypoints.Length)
            {
                if (loop)
                    currentIndex = 0; // restart
                else
                    enabled = false; // stop moving
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showPath || waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        if (loop && waypoints[0] != null && waypoints[^1] != null)
            Gizmos.DrawLine(waypoints[^1].position, waypoints[0].position);
    }
}
