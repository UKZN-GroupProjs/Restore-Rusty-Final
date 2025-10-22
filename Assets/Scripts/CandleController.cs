using UnityEngine;
using System.Collections;

public class CandleController : MonoBehaviour
{
    [Header("Physics Settings")]
    public Rigidbody physicsRb;
    public Vector3 nudgeDirection = new Vector3(-1, 0, 0);
    public float forceStrength = 0.5f;
    public float nudgeDelay = 0.1f;

    [Header("QTE Settings")]
    [SerializeField] private int requiredPresses = 5;
    private int currentPresses = 0;
    private bool hasDropped = false;
    private bool playerInRange = false;

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactPromptCanvas;

    [Header("Sound")]
    [SerializeField] private AudioClip pushSound;

    void Start()
    {
        if (interactPromptCanvas != null)
            interactPromptCanvas.SetActive(false);

        // Freeze candle at start
        if (physicsRb != null)
        {
            physicsRb.isKinematic = true;
            physicsRb.linearVelocity = Vector3.zero;
            physicsRb.angularVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        if (playerInRange && !hasDropped && Input.GetKeyDown(KeyCode.R))
        {
            currentPresses++;
            Debug.Log($"Pressed R: {currentPresses}/{requiredPresses}");

            if (currentPresses >= requiredPresses)
            {
                Debug.Log("QTE Success - Pushing Candle");

                SoundManager.Instance?.PlaySFX(pushSound);

                StartCoroutine(DropAndNudge());
                hasDropped = true;

                if (interactPromptCanvas != null)
                    interactPromptCanvas.SetActive(false);
            }
        }
    }

    private IEnumerator DropAndNudge()
    {
        yield return new WaitForSeconds(nudgeDelay);

        if (physicsRb != null)
        {
            physicsRb.isKinematic = false;
            physicsRb.linearVelocity = Vector3.zero;
            physicsRb.angularVelocity = Vector3.zero;

            physicsRb.Sleep();
            physicsRb.WakeUp();

            physicsRb.AddForce(nudgeDirection.normalized * forceStrength, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasDropped && other.CompareTag("Player"))
        {
            playerInRange = true;
            currentPresses = 0;
            if (interactPromptCanvas != null)
                interactPromptCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptCanvas != null)
                interactPromptCanvas.SetActive(false);
        }
    }
}
