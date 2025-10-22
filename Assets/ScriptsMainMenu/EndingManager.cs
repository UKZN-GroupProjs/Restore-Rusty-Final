using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer endingPlayer;

    void Start()
    {
        if (endingPlayer != null)
        {
            endingPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("EndingManager: No VideoPlayer assigned!");
        }
    }

    
    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene("MainMenu");
    }

    void OnDestroy()
    {
        if (endingPlayer != null)
        {
            endingPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}
