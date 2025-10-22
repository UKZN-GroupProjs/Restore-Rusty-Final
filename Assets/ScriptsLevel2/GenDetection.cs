using UnityEngine;
public class GeneratorDetector : MonoBehaviour
{
    [SerializeField] private GameObject promptUI;
    [SerializeField] private Renderer generatorRenderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;

    private bool playerNearby = false;
    private bool isOn = false;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        if (playerNearby && !isOn && Input.GetKeyDown(KeyCode.F))
        {
            ActivateGenerator();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOn)
        {
            playerNearby = true;
            if (promptUI != null)
                promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    private void ActivateGenerator()
    {
        isOn = true;
        if (promptUI != null)
            promptUI.SetActive(false);

        if (audioSource != null && activationSound != null)
            audioSource.PlayOneShot(activationSound);

        GeneratorManager.Instance.GeneratorActivated(generatorRenderer);
    }
}