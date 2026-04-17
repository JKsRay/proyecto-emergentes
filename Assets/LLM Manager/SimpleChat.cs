using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LLMUnity;   

public class ChatController : MonoBehaviour
{
    [Header("UI References")]
    public Button sendButton;            // Drag your Button here
    public TMP_InputField playerInput;   // Drag your Input Field here
    public TextMeshProUGUI aiResponse;   // Drag your Character's Text bubble here

    [Header("LLM Setup")]
    public LLMAgent characterAgent;      // Drag the NPC's LLMAgent here

    void Start()
    {
        // Link the button click to the function in code
        // This is an alternative to using the "On Click()" list in the Inspector
        sendButton.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        string message = playerInput.text;

        if (!string.IsNullOrEmpty(message))
        {
            // 1. Clear the UI for the new message
            aiResponse.text = "...";

            // 2. Disable button so player can't spam while AI thinks
            sendButton.interactable = false;

            // 3. Send to LLM
            // HandleReply: updates text word-by-word
            // DoneReplying: re-enables the button
            _ = characterAgent.Chat(message, HandleReply, DoneReplying);

            // 4. Clear the input field for the next question
            playerInput.text = "";
        }
    }

    void HandleReply(string replySoFar)
    {
        // Update the text on screen in real-time
        aiResponse.text = replySoFar;
    }

    void DoneReplying()
    {
        // Re-enable the button when the AI is finished talking
        sendButton.interactable = true;
    }
}