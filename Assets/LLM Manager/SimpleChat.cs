using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LLMUnity;   

public class ChatController : MonoBehaviour
{
    private struct PendingRequest
    {
        public string Message;
        public bool ClearInputField;

        public PendingRequest(string message, bool clearInputField)
        {
            Message = message;
            ClearInputField = clearInputField;
        }
    }

    [Header("UI References")]
    public Button sendButton;            // Drag your Button here
    public TMP_InputField playerInput;   // Drag your Input Field here
    public TextMeshProUGUI aiResponse;   // Drag your Character's Text bubble here

    [Header("LLM Setup")]
    public LLMAgent characterAgent;      // Drag the NPC's LLMAgent here

    [Header("Animacion")]
    public Animator robotAnimator;
    [SerializeField] private bool autoBuscarAnimator = true;

    public bool IsRequestInFlight => isRequestInFlight;
    public event Action<bool> RequestInFlightChanged;

    private bool isRequestInFlight;
    private readonly Queue<PendingRequest> pendingRequests = new Queue<PendingRequest>();

    void Start()
    {
        if (sendButton != null)
        {
            // Link the button click to the function in code
            // This is an alternative to using the "On Click()" list in the Inspector
            sendButton.onClick.RemoveListener(OnButtonClick);
            sendButton.onClick.AddListener(OnButtonClick);
        }
    }

    void OnDestroy()
    {
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnButtonClick);
        }
    }

    public void OnButtonClick()
    {
        if (playerInput == null)
        {
            return;
        }

        SendTextToLLM(playerInput.text, clearInputField: true);
    }

    public void SendPrompt(string message)
    {
        SendTextToLLM(message, clearInputField: false);
    }

    private void SetRequestInFlight(bool inFlight)
    {
        if (isRequestInFlight == inFlight)
        {
            return;
        }

        isRequestInFlight = inFlight;
        RequestInFlightChanged?.Invoke(inFlight);
    }

    private void SendTextToLLM(string message, bool clearInputField)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (isRequestInFlight)
        {
            pendingRequests.Enqueue(new PendingRequest(message, clearInputField));
            return;
        }

        if (characterAgent == null)
        {
            return;
        }

        // 1. Clear the UI for the new message
        if (aiResponse != null)
        {
            aiResponse.text = "...";
        }

        // 2. Disable button so player can't spam while AI thinks
        if (sendButton != null)
        {
            sendButton.interactable = false;
        }

        SetRequestInFlight(true);

        try
        {
            // 3. Send to LLM
            // HandleReply: updates text word-by-word
            // DoneReplying: re-enables the button
            _ = characterAgent.Chat(message, HandleReply, DoneReplying);
        }
        catch (System.Exception)
        {
            SetRobotTalking(false);
            SetRequestInFlight(false);
            if (sendButton != null)
            {
                sendButton.interactable = true;
            }

            return;
        }

        // 4. Clear the input field only for manual prompts
        if (clearInputField && playerInput != null)
        {
            playerInput.text = "";
        }
    }

    void HandleReply(string replySoFar)
    {
        SetRobotTalking(true);

        // Update the text on screen in real-time
        if (aiResponse != null)
        {
            aiResponse.text = replySoFar;
        }
    }

    void DoneReplying()
    {
        SetRobotTalking(false);

        // Re-enable the button when the AI is finished talking
        SetRequestInFlight(false);
        if (sendButton != null)
        {
            sendButton.interactable = true;
        }

        ProcesarSiguienteMensajeEnCola();
    }

    private void ProcesarSiguienteMensajeEnCola()
    {
        if (isRequestInFlight || pendingRequests.Count == 0)
        {
            return;
        }

        PendingRequest nextRequest = pendingRequests.Dequeue();
        SendTextToLLM(nextRequest.Message, nextRequest.ClearInputField);
    }

    private void SetRobotTalking(bool isTalking)
    {
        Animator targetAnimator = ResolverAnimatorTalk();
        if (targetAnimator == null)
            return;

        targetAnimator.SetBool("Talk", isTalking);
    }

    private Animator ResolverAnimatorTalk()
    {
        if (robotAnimator != null)
            return robotAnimator;

        if (!autoBuscarAnimator)
            return null;

        Animator[] animators = FindObjectsByType<Animator>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Animator candidate in animators)
        {
            if (TieneParametroAnimacion(candidate, "Talk", AnimatorControllerParameterType.Bool))
            {
                robotAnimator = candidate;
                return robotAnimator;
            }
        }

        return null;
    }

    private static bool TieneParametroAnimacion(Animator animator, string parameterName, AnimatorControllerParameterType type)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == type && parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
}