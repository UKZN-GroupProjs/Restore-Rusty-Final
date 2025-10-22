using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;

public class BookManager : MonoBehaviour
{
    [Header("Canvas Root")]
    public Canvas bookCanvas;

    [Header("UI Elements")]
    public GameObject frontCover;
    public GameObject backCover;
    public GameObject bookBackOpen;

    //RawImage for video display
    public RawImage leftPageRawImage;
    public Image rightPageImage;

    [Header("Audio Clips")]
    public AudioClip frontCoverClip;
    public AudioClip pageTurnClip;
    public AudioClip bookCloseClip;

    [Header("Book Pages")]
    public List<VideoClip> leftPageVideos = new List<VideoClip>();
    public List<Sprite> rightPages = new List<Sprite>();

    [Header("Video Player Reference")]
    public VideoPlayer videoPlayer;

    [Header("Buttons to disable")]
    public List<Button> buttons = new List<Button>();

    private int currentPageIndex = 0;

    void Awake()
    {
        // Auto-assign Canvas if not set
        if (bookCanvas == null)
            bookCanvas = GetComponent<Canvas>();

        if (leftPageRawImage == null)
            leftPageRawImage = bookCanvas.transform.Find("BookBack-Open/LeftPage")?.GetComponent<RawImage>();

        if (rightPageImage == null)
            rightPageImage = bookCanvas.transform.Find("BookBack-Open/RightPage")?.GetComponent<Image>();

        SetCanvasActive(false);
    }

    public void OpenBook()
    {
        SetCanvasActive(true);
        SetButtonsInteractable(false);
        currentPageIndex = 0;
        StartCoroutine(ShowFrontCoverNextFrame());
    }

    private IEnumerator ShowFrontCoverNextFrame()
    {
        yield return null;
        ShowFrontCover();
    }

    public void CloseBook()
    {
        SetCanvasActive(false);
        SetButtonsInteractable(true);
    }

    public void ShowBookPages()
    {
        frontCover?.SetActive(false);
        backCover?.SetActive(false);
        bookBackOpen?.SetActive(true);

        UpdatePageContent();

        if (pageTurnClip != null)
            SoundManager.Instance?.PlaySFX(pageTurnClip);
    }

    public void OnLeftPageClick()
    {
        if (currentPageIndex - 1 < 0)
            ShowFrontCover();
        else
        {
            currentPageIndex--;
            ShowBookPages();
        }
    }

    public void OnRightPageClick()
    {
        if (currentPageIndex + 1 >= Mathf.Max(leftPageVideos.Count, rightPages.Count))
            ShowBackCover();
        else
        {
            currentPageIndex++;
            ShowBookPages();
        }
    }

    private void ShowFrontCover()
    {
        frontCover?.SetActive(true);
        backCover?.SetActive(false);
        bookBackOpen?.SetActive(false);

        if (frontCoverClip != null)
            SoundManager.Instance?.PlaySFX(frontCoverClip);
    }

    private void ShowBackCover()
    {
        frontCover?.SetActive(false);
        backCover?.SetActive(true);
        bookBackOpen?.SetActive(false);

        if (bookCloseClip != null)
            SoundManager.Instance?.PlaySFX(bookCloseClip);
    }

    private void UpdatePageContent()
    {
        //Left page video
        if (videoPlayer != null)
        {
            if (currentPageIndex < leftPageVideos.Count && leftPageVideos[currentPageIndex] != null)
            {
                videoPlayer.clip = leftPageVideos[currentPageIndex];
                videoPlayer.Play();
            }
            else
            {
                videoPlayer.Stop();
                leftPageRawImage.texture = null;
            }
        }

        //Right page image
        rightPageImage.sprite = (currentPageIndex < rightPages.Count)
            ? rightPages[currentPageIndex]
            : null;
    }

    private void SetCanvasActive(bool value)
    {
        if (bookCanvas != null)
            bookCanvas.gameObject.SetActive(value);
    }

    private void SetButtonsInteractable(bool value)
    {
        foreach(Button btn in buttons)
        {
            if (btn != null)
                btn.interactable = value;
        }
    }
}


