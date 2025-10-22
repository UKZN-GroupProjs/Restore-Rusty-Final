using UnityEngine;

public class EnemyFSMController : MonoBehaviour
{
    [SerializeField] private float chaseRadius = 5f;

    private EnemyState currentState;
    private PatrolState patrolState;
    private ChaseState chaseState;
    private Transform playerTransform;
    private float offTableCooldown = 1.5f;
    private float offTableTimer = 0f;

    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Grab state scripts from children
        patrolState = GetComponentInChildren<PatrolState>();
        chaseState = GetComponentInChildren<ChaseState>();

        SwitchState(patrolState);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float verticalDiff = Mathf.Abs(playerTransform.position.y - transform.position.y);//Player must be on the table before chasing
        // countdown off-table timer
        if (offTableTimer > 0f)
            offTableTimer -= Time.deltaTime;

        bool playerOnSameHeight = verticalDiff < 0.5f;

        // if player is too far vertically, start cooldown
        if (!playerOnSameHeight)
            offTableTimer = offTableCooldown;

        if (distance <= chaseRadius && playerOnSameHeight && offTableTimer <= 0f && currentState != chaseState)
        {
            SwitchState(chaseState);
        }
        else if (distance > chaseRadius && currentState != patrolState)
        {
            SwitchState(patrolState);
        }

        currentState?.UpdateState(gameObject);
    }

   public void SwitchState(EnemyState newState)
    {
        currentState?.ExitState(gameObject);
        currentState = newState;
        currentState?.EnterState(gameObject);
    }

   
}
