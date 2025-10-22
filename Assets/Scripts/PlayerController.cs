using UnityEngine;
using TMPro; // Only if using TextMeshPro

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementFreeLook : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float jumpPower = 5f;
    public float gravity = 15f;

    [Header("Animation")]
    public Animator animator;

    [Header("UI")]
    public TextMeshProUGUI gearText;

    [Header("Collectibles")]
   
    public float speedBoostAmt = 0.01f;
    [Header("Speed Boost Control")]
    public float maxSpeedBoost = 3f;   // maximum total speed increase
    public float boostDuration = 0.8f; // how long speed lines stay visible


    private int totalCollected = 0;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private bool canMove = true;

    private Vector3 knockback = Vector3.zero;
    [SerializeField] private float knockbackDecay = 5f; // how fast it wears off
    [SerializeField] private TrailRenderer speedLine1;
    [SerializeField] private TrailRenderer speedLine2;
    [SerializeField] private Light eyeLight;
    [SerializeField] private float lightIncrease = 100f;
    [SerializeField] private float maxSpotlightIntensity = 600f;
    [SerializeField] private float angleIncrease = 10f;
    [SerializeField] private float maxSpotlightAngle = 90f;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!canMove)
        {
            // Still apply gravity so the body falls
            if (!characterController.isGrounded)
            {
                moveDirection.y -= gravity * Time.deltaTime;
                characterController.Move(moveDirection * Time.deltaTime);
            }
            return;
        }
        // Input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        float inputMagnitude = input.magnitude; // raw magnitude

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentWalkSpeed = walkSpeed;
        float currentRunSpeed = runSpeed;
        
        float speed = isRunning ? currentRunSpeed : currentWalkSpeed;

        //Camera-relative movement
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;
        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0;

        Vector3 desiredMove = (camForward * v + camRight * h).normalized;

        // Rotate character toward movement direction smoothly
        if (desiredMove.sqrMagnitude > 0.001f)
            transform.forward = Vector3.Slerp(transform.forward, desiredMove, 0.15f);

        // Apply movement
        moveDirection.x = desiredMove.x * speed;
        moveDirection.z = desiredMove.z * speed;

        // Jump
        if (characterController.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                // Use physics formula for jump velocity
                moveDirection.y = Mathf.Sqrt(jumpPower * 2f * gravity);
                animator.SetBool("IsInAir", true);
                animator.SetTrigger("JumpStart");
                SoundManager.Instance.PlaySFX(SoundManager.Instance.jumpClip);
            }
            else
            {
                moveDirection.y = -0.5f; // small downward force to keep grounded
                animator.SetBool("IsInAir", false);
            }
        }
        else
        {
            // Apply gravity while in air
            moveDirection.y -= gravity * Time.deltaTime;
            animator.SetBool("IsInAir", true);
        }

        // Apply knockback if active
        if (knockback.magnitude > 0.1f)
        {
            moveDirection += knockback;
            knockback = Vector3.Lerp(knockback, Vector3.zero, knockbackDecay * Time.deltaTime);
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Update Animator for Blend Tree
        if (animator != null)
        {
            float animSpeed = 0f;

            // Deadzone for idle
            if (inputMagnitude > 0.05f)
            {
                animSpeed = isRunning ? 1f : 0.5f; // Run or Walk thresholds
            }

            animator.SetFloat("Speed", animSpeed);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        // Check if the object the player collided with has the "PickUp" tag.
        if (other.gameObject.CompareTag("PickUp"))
        {
            // Deactivate the collided object (making it disappear).
            collectGear();
            other.gameObject.SetActive(false);
        }
    }

    public void freezeMovement()
    {
        //idle player
        animator.SetFloat("Speed", 0);
        canMove = false;
        // Unlock cursor so player can navigate UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void unfreezeMovement()
    {
        canMove = true;
        // Lock cursor back for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Call this when collecting a gear
    public void collectGear()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.collectClip);
        totalCollected++;

        //Level 1: Power-up is speed
        // Increment movement speed gradually (up to max)
        if (speedLine1 != null && speedLine2 != null)
        {
            if (walkSpeed < walkSpeed + maxSpeedBoost)
            {
                walkSpeed = Mathf.Min(walkSpeed + speedBoostAmt, walkSpeed + maxSpeedBoost);
                runSpeed = Mathf.Min(runSpeed + speedBoostAmt, runSpeed + maxSpeedBoost);
            }
        }
        

        // Play trail effect briefly
        StartCoroutine(ShowSpeedLines());
        
        //Level 2: Power-up is light
        // Increase spotlight intensity
        if (eyeLight != null )
        {
            eyeLight.intensity = Mathf.Min(eyeLight.intensity + lightIncrease, maxSpotlightIntensity);
            // Increase field of view (spotlight cone)
            eyeLight.spotAngle = Mathf.Min(eyeLight.spotAngle + angleIncrease, maxSpotlightAngle);
        }
    }

    private System.Collections.IEnumerator ShowSpeedLines()
    {
        if (speedLine1 != null) speedLine1.emitting = true;
        if (speedLine2 != null) speedLine2.emitting = true;

        yield return new WaitForSeconds(boostDuration);

        if (speedLine1 != null) speedLine1.emitting = false;
        if (speedLine2 != null) speedLine2.emitting = false;
    }



    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0f; // keep horizontal
        knockback = direction.normalized * force;
    }


}