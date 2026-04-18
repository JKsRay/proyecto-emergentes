using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LLMUnity;
using System;
using System.Threading.Tasks;

public class ChatController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private TextMeshProUGUI aiResponse;

    [Header("LLM Setup")]
    [SerializeField] private LLMAgent characterAgent;

    [Header("Optional Robot Sync")]
    [SerializeField] private RobotStateManager robotStateManager;

    private bool isRequestInFlight;
    private bool loggedCurrentReply;

    private void Awake()
    {
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnButtonClick);
            sendButton.onClick.AddListener(OnButtonClick);
        }

        if (robotStateManager == null)
        {
            robotStateManager = FindObjectOfType<RobotStateManager>();
        }
    }

    public void OnButtonClick()
    {
        if (playerInput == null)
        {
            Debug.LogError("[ChatController] Falta asignar playerInput en el Inspector.");
            return;
        }

        SendFromInputField(playerInput.text);
    }

    public void SendFromInputField(string message)
    {
        _ = SendTextToLLM(message, true);
    }

    public void SendPrompt(string prompt)
    {
        _ = SendTextToLLM(prompt, false);
    }

    public async Task SendTextToLLM(string message, bool clearInputField)
    {
        Debug.Log("Chat: Intentando enviar texto...");

        if (isRequestInFlight)
        {
            Debug.LogWarning("[ChatController] Ya hay una solicitud en curso.");
            return;
        }

        if (characterAgent == null)
        {
            Debug.LogError("Chat: Error en el LLM... LLMAgent no asignado.");
            return;
        }

        if (aiResponse == null)
        {
            Debug.LogError("[ChatController] Falta asignar aiResponse en el Inspector.");
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning("[ChatController] El mensaje esta vacio.");
            return;
        }

        isRequestInFlight = true;
        loggedCurrentReply = false;
        aiResponse.text = "...";

        if (sendButton != null)
        {
            sendButton.interactable = false;
        }

        if (clearInputField && playerInput != null)
        {
            playerInput.text = "";
        }

        try
        {
            string fullReply = await characterAgent.Chat(message, HandleReply, DoneReplying);

            if (!loggedCurrentReply)
            {
                Debug.Log("Chat: Respuesta recibida del LLM...");
                loggedCurrentReply = true;
            }

            if (!string.IsNullOrEmpty(fullReply))
            {
                aiResponse.text = fullReply;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Chat: Error en el LLM... {ex.Message}");
            aiResponse.text = "No pude generar respuesta ahora.";
            DoneReplying();
        }
    }

    public void HandleReply(string replySoFar)
    {
        if (!loggedCurrentReply)
        {
            Debug.Log("Chat: Respuesta recibida del LLM...");
            loggedCurrentReply = true;
        }

        aiResponse.text = replySoFar;
        robotStateManager?.OnLLMReplyProgress(replySoFar);
    }

    public void DoneReplying()
    {
        isRequestInFlight = false;
        if (sendButton != null)
        {
            sendButton.interactable = true;
        }

        robotStateManager?.OnLLMReplyComplete();
    }
}