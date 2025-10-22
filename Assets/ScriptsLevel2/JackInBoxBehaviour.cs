using UnityEngine;

public class JackInTheBoxEnemy : MonoBehaviour
{
    public AudioClip JackPop;
    [SerializeField] private float hitRadius = 1.5f;    // trigger distance
    [SerializeField] private float damage = 0.2f;       // how much damage to apply
    [SerializeField] private float knockbackForce = 8f; // knockback strength
    [SerializeField] private Animator animator;         // assign in inspector

    private Transform playerTransform;
    private bool hasPopped = false;

    private void Start()
    {
        // Find player once at start
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void Update()
    {
        if (hasPopped || playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Trigger pop once
        if (distance <= hitRadius)
        {
            PopAndAttack();
        }
    }

    private void PopAndAttack()
    {
        hasPopped = true;

        // Play popping animation if assigned
        if (animator != null)
        {
            animator.SetTrigger("Pop");
        }
        SoundManager.Instance?.PlaySFX(JackPop);

        // Deal damage
        PlayerStats.Instance.TakeDamage(damage);

        // Knockback
        PlayerMovementFreeLook playerMovement = playerTransform.GetComponent<PlayerMovementFreeLook>();
        if (playerMovement != null)
        {
            Vector3 knockbackDir = (playerTransform.position - transform.position).normalized;
            knockbackDir.y = 0f; // horizontal only
            playerMovement.ApplyKnockback(knockbackDir, knockbackForce);
        }

        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}

