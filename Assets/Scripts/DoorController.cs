using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator doorAnimator; // assign in inspector

    public void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("DoorOpen");
        }
    }
}
