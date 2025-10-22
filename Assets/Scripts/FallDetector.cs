using UnityEngine;

public class FallDetector : MonoBehaviour
{
    [Header("Fall Damage Settings")]
    [Tooltip("Minimum downward speed to take damage.")]
    public float fallSpeedThreshold = -5f;

    [Tooltip("How far below the player to check for ground contact.")]
    public float groundCheckDistance = 0.5f;

    [Tooltip("Select one or more layers considered as ground.")]
    public LayerMask groundLayers; // can now select multiple layers in Inspector

    private bool isInAir = false;
    private float lastY;
    private float maxFallVelocity = 0f;

    void Start()
    {
        lastY = transform.position.y;
    }

    void Update()
    {
        float currentY = transform.position.y;
        float deltaY = (currentY - lastY) / Time.deltaTime;
        lastY = currentY;

        bool grounded = IsGrounded();

        if (!grounded)
        {
            isInAir = true;
            maxFallVelocity = Mathf.Min(maxFallVelocity, deltaY);
        }
        else
        {
            if (isInAir)
            {
                if (maxFallVelocity < fallSpeedThreshold)
                {
                    PlayerStats.Instance.TakeDamage(1);
                    Debug.Log($"Fall damage applied! Max fall speed: {maxFallVelocity}");
                }

                maxFallVelocity = 0f;
                isInAir = false;
            }
        }
    }

    private bool IsGrounded()
    {
        // visualize raycast in Scene view
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);

        // checks against ALL selected layers
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayers);
    }
}
