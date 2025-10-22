using UnityEngine;

public class PatrolState : EnemyState
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float arrivalThreshold = 0.5f;
   

    public enum PatrolMode { Loop, PingPong }
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Loop;

    private int currentPatrolIndex = 0;
    private int direction = 1; // only used in PingPong mode
    private float waitTimer = 0f;

    public override void EnterState(GameObject enemy)
    {
        waitTimer = 0f;
    }

    public override void UpdateState(GameObject enemy)
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPatrolIndex];
        Vector3 toTarget = target.position - enemy.transform.position;
        float distance = toTarget.magnitude;

        if (distance <= arrivalThreshold)
        {
            // Wait before switching to next point
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                AdvancePatrolIndex();
                waitTimer = 0f;
            }
            return;
        }

        // Move towards current patrol point
        Vector3 directionVec = toTarget.normalized;
        float step = patrolSpeed * Time.deltaTime;

        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.MovePosition(enemy.transform.position + directionVec * step);
        }

        // Rotation (make mouse face the direction it's moving)
        if (directionVec != Vector3.zero)
        {
            //rotate only on Y
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionVec.x, 0, directionVec.z));
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, 10f * Time.deltaTime);
        }

        
    }

    private void AdvancePatrolIndex()
    {
        if (patrolMode == PatrolMode.Loop)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        else if (patrolMode == PatrolMode.PingPong)
        {
            currentPatrolIndex += direction;
            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = patrolPoints.Length - 2;
                direction = -1;
            }
            else if (currentPatrolIndex < 0)
            {
                currentPatrolIndex = 1;
                direction = 1;
            }
        }

        Debug.Log($"Patrol switched to point {currentPatrolIndex}: {patrolPoints[currentPatrolIndex].name}");
    }

    private void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.yellow;

        // Draw spheres at patrol points
        foreach (Transform point in patrolPoints)
        {
            if (point != null)
                Gizmos.DrawSphere(point.position, 0.2f);
        }

        // Draw connecting lines
        for (int i = 0; i < patrolPoints.Length - 1; i++)
        {
            if (patrolPoints[i] != null && patrolPoints[i + 1] != null)
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
        }

        // Close loop if Loop mode
        if (patrolMode == PatrolMode.Loop && patrolPoints.Length > 1)
        {
            Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
        }

        // Arrival radius
        if (currentPatrolIndex < patrolPoints.Length && patrolPoints[currentPatrolIndex] != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(patrolPoints[currentPatrolIndex].position, arrivalThreshold);
        }
    }
}
