using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour
{
    [Header("Canvases")]
    public Canvas welcomeCanvas;
    public Canvas menuCanvas;
    public Canvas controlsCanvas;
    public Canvas creditsCanvas;
    public Canvas aboutCanvas;

    [Header("Buttons to Disable When Controls Are Open")]
    public List<Button> buttonsToDisable = new List<Button>();

    [Header("Animator Transition")]
    public Animator sceneTransitionAnimator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickClip;
    public AudioClip mainMenuMusic;
    public AudioClip diningRoomMusic;

    [Header("Credits player")]
    public VideoPlayer creditsPlayer;

    private bool isControlsActive = false;
    private bool isCreditsActive = false;
    private bool isAboutActive = false;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (controlsCanvas != null)
            controlsCanvas.gameObject.SetActive(false);
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);
        if (creditsCanvas != null)
            creditsCanvas.gameObject.SetActive(false);
        if (aboutCanvas != null)
            aboutCanvas.gameObject.SetActive(false);
        if (welcomeCanvas != null)
            welcomeCanvas.gameObject.SetActive(true);

        BackgroundSoundManager.Instance?.PlayMusic(mainMenuMusic);
        BackgroundSoundManager.Instance?.SetVolume(0.1f);

        if (creditsPlayer != null)
            creditsPlayer.loopPointReached += onVideoEnd;
    }

    void Update()
    {
        if (isControlsActive && Input.GetKeyDown(KeyCode.Escape))
            ToggleControlsCanvas();

        if (isCreditsActive && Input.GetKeyDown(KeyCode.Escape))
            HideCredits();

        if (isAboutActive && Input.GetKeyDown(KeyCode.Escape))
            HideAbout();
    }

    public void ShowMainMenu()
    {
        audioSource?.PlayOneShot(clickClip);

        if (welcomeCanvas != null)
            welcomeCanvas.gameObject.SetActive(false);
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowCredits()
    {
        BackgroundSoundManager.Instance?.SetVolume(0.0f);
        audioSource?.PlayOneShot(clickClip);

        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);
        if (creditsCanvas != null)
            creditsCanvas.gameObject.SetActive(true);

        isCreditsActive = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void onVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video finished!");
        HideCredits();
    }

    public void HideCredits()
    {
        BackgroundSoundManager.Instance?.SetVolume(0.1f);
        if (creditsCanvas != null)
            creditsCanvas.gameObject.SetActive(false);
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(true);

        isCreditsActive = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowAbout()
    {
        audioSource?.PlayOneShot(clickClip);

        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);
        if (aboutCanvas != null)
            aboutCanvas.gameObject.SetActive(true);

        isAboutActive = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideAbout()
    {
        if (aboutCanvas != null)
            aboutCanvas.gameObject.SetActive(false);
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(true);

        isAboutActive = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlayGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(TransitionToScene("DiningRoom"));
    }

    public void QuitGame()
    {
        StartCoroutine(TransitionToQuit());
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        audioSource?.PlayOneShot(clickClip);

        yield return StartCoroutine(WaitForAnimation("End"));
        SceneManager.LoadScene(sceneName);
        BackgroundSoundManager.Instance?.CrossfadeTo(diningRoomMusic);
    }

    private IEnumerator TransitionToQuit()
    {
        audioSource?.PlayOneShot(clickClip);

        yield return StartCoroutine(WaitForAnimation("End"));
        Application.Quit();
    }

    private IEnumerator WaitForAnimation(string triggerName)
    {
        sceneTransitionAnimator?.SetTrigger(triggerName);
        yield return null;

        string stateName = triggerName == "Start" ? "StartFade" : "EndFade";

        while (sceneTransitionAnimator != null && !sceneTransitionAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        if (sceneTransitionAnimator != null)
        {
            AnimatorStateInfo stateInfo = sceneTransitionAnimator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
        }
    }

    public void ToggleControlsCanvas()
    {
        if (controlsCanvas == null) return;

        isControlsActive = !isControlsActive;
        controlsCanvas.gameObject.SetActive(isControlsActive);
        SetButtonsInteractable(!isControlsActive);
    }

    public void ShowControls()
    {
        if (controlsCanvas == null) return;

        audioSource?.PlayOneShot(clickClip);

        controlsCanvas.gameObject.SetActive(true);
        SetButtonsInteractable(false);
        isControlsActive = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideControls()
    {
        if (controlsCanvas == null) return;

        controlsCanvas.gameObject.SetActive(false);
        SetButtonsInteractable(true);
        isControlsActive = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void SetButtonsInteractable(bool value)
    {
        foreach (Button btn in buttonsToDisable)
            if (btn != null)
                btn.interactable = value;
    }
}
