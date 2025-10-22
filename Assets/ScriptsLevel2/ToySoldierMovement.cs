using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ToySoldierMovement : MonoBehaviour
{
    [Header("Movement")]
    public float runSpeed = 5f;
    public float rotationSmooth = 10f;

    [Header("Animation")]
    public Animator animator;

    private CharacterController cc;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Move character controller
        cc.Move(moveDirection * Time.deltaTime);

        // Rotate toward movement direction
        RotateTowardsMovement(new Vector3(moveDirection.x, 0, moveDirection.z));

        // Update animator
        UpdateAnimator();
    }

    
    public void MoveByDirection(Vector3 worldDir)
    {
        worldDir.y = 0;
        moveDirection.x = worldDir.x * runSpeed;
        moveDirection.z = worldDir.z * runSpeed;
    }

    public void Idle()
    {
        moveDirection.x = 0f;
        moveDirection.z = 0f;
    }

    
    private void UpdateAnimator()
    {
        if (animator == null) return;
        float horizontalSpeed = new Vector3(moveDirection.x, 0f, moveDirection.z).magnitude;
        animator.SetFloat("Speed", horizontalSpeed > 0.01f ? 1f : 0f); // 0 = idle, 1 = run
    }

    private void RotateTowardsMovement(Vector3 dir)
    {
        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();
            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * rotationSmooth);
        }
    }
}

