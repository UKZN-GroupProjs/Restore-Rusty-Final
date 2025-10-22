using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GoToEnd : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float delayBeforeLoad = 2f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(LoadEndingAfterDelay());
        }
    }

    private IEnumerator LoadEndingAfterDelay()
    {
        BackgroundSoundManager.Instance?.StopMusic();
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene("Ending");
    }
}