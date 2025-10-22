using UnityEngine;

public class CandleLandingTrigger : MonoBehaviour
{
    public Animator handleAnimator;
    public Animator doorAnimator;
    public GameObject endLevel;

    [Header("Sound")]
    [SerializeField] private AudioClip doorOpenSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Candle"))
        {
            Debug.Log("Candle hits handle");

            SoundManager.Instance?.PlaySFX(doorOpenSound);

            handleAnimator?.SetTrigger("HandlePulled");
            doorAnimator?.SetTrigger("DoorOpen");

            endLevel?.SetActive(true);
        }
    }
}
