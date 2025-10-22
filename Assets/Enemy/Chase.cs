using UnityEngine;

public class ChaseState : EnemyState
{
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float hitRadius = 1f;  
    [SerializeField] private float cooldownTime = 2f; 
    [SerializeField] private float knockbackForce = 5f;
    

    [Header("Visual Cue")]
    [SerializeField] private Renderer enemyRenderer;       
    [SerializeField] private Color chaseColor = Color.red;  
    private Color originalColor;

    private Transform playerTransform;
    private float cooldownTimer = 0f;

    public override void EnterState(GameObject enemy)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        cooldownTimer = 0f;

        // store and change color
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
            enemyRenderer.material.color = chaseColor;
        }
    }

    public override void ExitState(GameObject enemy)
    {
        // restore original color
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = originalColor;
        }
    }

    public override void UpdateState(GameObject enemy)
    {
        if (playerTransform == null) return;

        // Handle cooldown after a hit
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);

        // Check if close enough to count as a hit
        if (distance <= hitRadius)
        {
            // Damage
            PlayerStats.Instance.TakeDamage(0.2f);

            // Knockback
            PlayerMovementFreeLook playerMovement = playerTransform.GetComponent<PlayerMovementFreeLook>();
            if (playerMovement != null)
            {
                Vector3 knockbackDir = (playerTransform.position - enemy.transform.position).normalized;
                knockbackDir.y = 0f; // keep horizontal
                playerMovement.ApplyKnockback(knockbackDir, knockbackForce);
            }

            cooldownTimer = cooldownTime;
            return;
        }

        // Move toward the player if not in cooldown
        Vector3 direction = (playerTransform.position - enemy.transform.position).normalized;
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.MovePosition(enemy.transform.position + direction * chaseSpeed * Time.deltaTime);
        }
        // Rotation (make mouse face the direction it's moving)
        if (direction != Vector3.zero)
        {
            //rotate only on Y
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
       
    }
}
