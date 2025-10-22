using UnityEngine;

public class PushObject : MonoBehaviour
{
    [SerializeField] private float forceMagnitude = 5f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rigidbody = hit.collider.attachedRigidbody;

        if (rigidbody != null)
        {
            // Use player movement direction for pushing
            Vector3 forceDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            if (forceDirection.magnitude > 0.01f)
            {
                forceDirection.Normalize();
                rigidbody.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
            }
        }
    }
}
