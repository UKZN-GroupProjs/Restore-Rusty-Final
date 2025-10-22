using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DialogueEditor;

public class GameManagerScript : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverUI;
    public GameObject levelCompleteUI;
    public GameObject pauseMenuUI;

    [Header("Player Reference")]
    public PlayerMovementFreeLook playerMovement;

    [Header("Animator Transition")]
    public Animator sceneTransitionAnimator;

    [Header("Audio SFX")]
    public AudioClip pauseClip;
    public AudioClip resumeClip;
    public AudioClip buttonClickClip;
    public AudioClip pauseFailClip;

    [Header("Audio Music")]
    public AudioClip mainMenuMusic;
    public AudioClip levelMusic;
    public AudioClip gameOverMusic;
    public AudioClip levelCompleteMusic;

    private bool isPaused = false;
    private bool isGameOver = false;
    private bool isLevelComplete = false;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;

        BackgroundSoundManager.Instance?.SetVolume(0.1f);
        BackgroundSoundManager.Instance?.PlayMusic(levelMusic);

        StartCoroutine(FadeInOnStart());
    }

    void Update()
    {
        HandlePauseInput();
        UpdateCursorState();
    }

    private IEnumerator FadeInOnStart()
    {
        yield return StartCoroutine(WaitForAnimation("Start"));
    }

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) &&
            (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive))
        {
            SoundManager.Instance?.PlaySFX(pauseFailClip);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver && !isLevelComplete)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void UpdateCursorState()
    {
        bool showCursor =
            gameOverUI?.activeInHierarchy == true ||
            levelCompleteUI?.activeInHierarchy == true ||
            pauseMenuUI?.activeInHierarchy == true;

        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void gameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        playerMovement?.freezeMovement();
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(3f);
        BackgroundSoundManager.Instance?.CrossfadeTo(gameOverMusic);
        Time.timeScale = 0f;
        gameOverUI?.SetActive(true);
    }

    public void levelComplete()
    {
        if (isLevelComplete) return;
        isLevelComplete = true;

        playerMovement?.freezeMovement();
        StartCoroutine(LevelCompleteSequence());
    }

    private IEnumerator LevelCompleteSequence()
    {
        yield return new WaitForSeconds(1f);
        BackgroundSoundManager.Instance?.CrossfadeTo(levelCompleteMusic);
        Time.timeScale = 0f;
        levelCompleteUI?.SetActive(true);
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        pauseMenuUI?.SetActive(true);
        Time.timeScale = 0f;

        playerMovement?.freezeMovement();

        BackgroundSoundManager.Instance?.SetVolume(0.01f);
        SoundManager.Instance?.PlaySFX(pauseClip);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        pauseMenuUI?.SetActive(false);
        Time.timeScale = 1f;

        playerMovement?.unfreezeMovement();

        BackgroundSoundManager.Instance?.SetVolume(0.1f);
        SoundManager.Instance?.PlaySFX(resumeClip);
    }

    public void Restart()
    {
        SoundManager.Instance?.PlaySFX(buttonClickClip);
        StartCoroutine(DelayedSceneLoad(SceneManager.GetActiveScene().name, 0.2f));
    }

    public void ToNextLevel()
    {
        SoundManager.Instance?.PlaySFX(buttonClickClip);
        StartCoroutine(DelayedSceneLoad("Bedroom", 0.2f));
    }

    public void Quit()
    {
        SoundManager.Instance?.PlaySFX(buttonClickClip);
        StartCoroutine(DelayedSceneLoad("MainMenu",  0.2f));
    }

    private IEnumerator DelayedSceneLoad(string sceneName,  float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator WaitForAnimation(string triggerName)
    {
        sceneTransitionAnimator?.SetTrigger(triggerName);
        yield return null;

        string stateName = triggerName == "Start" ? "StartFade" : "EndFade";

        while (sceneTransitionAnimator != null && !sceneTransitionAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }

        if (sceneTransitionAnimator != null)
        {
            AnimatorStateInfo stateInfo = sceneTransitionAnimator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSecondsRealtime(stateInfo.length);
        }
    }
}
