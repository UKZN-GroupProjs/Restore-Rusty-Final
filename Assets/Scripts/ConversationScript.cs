using UnityEngine;
using DialogueEditor;

public class DetectionAreaConversation : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private NPCConversation myConversation;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private string talkAnimationTrigger = "jump";

    [Header("UI Settings")]
    [SerializeField] private GameObject talkPromptCanvas;
    [SerializeField] private GameObject conversationControlCanvas;

    [Header("Player Reference")]
    public PlayerMovementFreeLook playerMovement;

    private bool playerInRange = false;
    private bool wasInConversation = false;
    private float continueCooldown = 0.2f;
    private float lastContinueTime = 0f;
    private bool controlCanvasVisible = true;

    // Track which NPC owns the current conversation
    private static DetectionAreaConversation activeConversationNPC = null;

    void Start()
    {
        if (talkPromptCanvas != null)
            talkPromptCanvas.SetActive(false);

        if (conversationControlCanvas != null)
            conversationControlCanvas.SetActive(false);
    }

    void Update()
    {
        // Start conversation
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            StartConversation();
        }

        // Only the active NPC should handle controls
        if (activeConversationNPC == this)
        {
            HandleConversationControls();

            bool isActive = ConversationManager.Instance.IsConversationActive;

            
            if (conversationControlCanvas != null)
            {
                if (isActive && !wasInConversation)
                {
                    conversationControlCanvas.SetActive(true);
                    controlCanvasVisible = true;
                }
                else if (!isActive && wasInConversation)
                {
                    conversationControlCanvas.SetActive(false);
                }
            }

            // Detect when conversation ends → unfreeze player
            if (wasInConversation && !isActive)
            {
                if (playerMovement != null)
                    playerMovement.unfreezeMovement();

                activeConversationNPC = null; // release ownership
            }

            wasInConversation = isActive;
        }
    }

    private void StartConversation()
    {
        if (playerMovement != null)
            playerMovement.freezeMovement();

        if (ConversationManager.Instance != null && myConversation != null)
        {
            if (talkPromptCanvas != null)
                talkPromptCanvas.SetActive(false);

            ConversationManager.Instance.StartConversation(myConversation);

            // Play NPC animation
            if (npcAnimator != null && !string.IsNullOrEmpty(talkAnimationTrigger))
                npcAnimator.SetTrigger(talkAnimationTrigger);

            activeConversationNPC = this; // mark this NPC as the one in charge
        }
    }

    private void HandleConversationControls()
    {
        if (!ConversationManager.Instance.IsConversationActive)
            return;

        
        if (Input.GetKeyDown(KeyCode.C) && conversationControlCanvas != null)
        {
            controlCanvasVisible = !controlCanvasVisible;
            conversationControlCanvas.SetActive(controlCanvasVisible);
        }

        if (Input.GetKeyDown(KeyCode.W))
            ConversationManager.Instance.SelectPreviousOption();
        else if (Input.GetKeyDown(KeyCode.S))
            ConversationManager.Instance.SelectNextOption();
        else if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastContinueTime > continueCooldown)
        {
            ConversationManager.Instance.PressSelectedOption();

            // Play NPC animation
            if (npcAnimator != null && !string.IsNullOrEmpty(talkAnimationTrigger))
                npcAnimator.SetTrigger(talkAnimationTrigger);

            lastContinueTime = Time.time;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (talkPromptCanvas != null)
                talkPromptCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (talkPromptCanvas != null)
                talkPromptCanvas.SetActive(false);
        }
    }
}
