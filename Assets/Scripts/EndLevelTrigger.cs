using UnityEngine;

public class DetectionAreaTrigger : MonoBehaviour
{
    [Header("UI Prompt")]
    [SerializeField] private GameObject interactPromptCanvas; // "Press F" canvas

    [Header("Game Manager")]
    public GameManagerScript gameManager;

    private bool playerInRange = false;

    void Start()
    {
        if (interactPromptCanvas != null)
            interactPromptCanvas.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (gameManager != null)
            {
                gameManager.levelComplete();

                // Hide the prompt after interaction
                if (interactPromptCanvas != null)
                    interactPromptCanvas.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

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
