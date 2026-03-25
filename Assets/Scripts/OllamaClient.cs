using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles asynchronous HTTP POST requests to a local Ollama API endpoint.
/// Sends user text to the LLM and returns the generated response.
/// </summary>
public class OllamaClient : MonoBehaviour
{
    [Header("Ollama API Settings")]
    [Tooltip("Base URL of the local Ollama server.")]
    [SerializeField] private string apiUrl = "http://localhost:11434/api/generate";

    [Tooltip("Name of the Ollama model to use.")]
    [SerializeField] private string modelName = "llama3.2";

    /// <summary>
    /// Sends a text prompt to the Ollama LLM and returns the response text.
    /// </summary>
    /// <param name="userPrompt">The text prompt to send to the model.</param>
    /// <returns>The LLM's response string, or null on failure.</returns>
    public async Task<string> SendPromptAsync(string userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            Debug.LogWarning("[OllamaClient] Prompt is empty or null.");
            return null;
        }

        string jsonBody = BuildRequestJson(userPrompt);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        try
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                return ParseResponse(responseJson);
            }
            else
            {
                Debug.LogError($"[OllamaClient] Request failed: {request.error}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
            return null;
        }
    }

    /// <summary>
    /// Constructs the JSON request body for the Ollama API.
    /// </summary>
    private string BuildRequestJson(string prompt)
    {
        OllamaRequest payload = new OllamaRequest
        {
            model = modelName,
            prompt = prompt,
            stream = false
        };
        return JsonUtility.ToJson(payload);
    }

    /// <summary>
    /// Parses the JSON response from the Ollama API and extracts the response text.
    /// </summary>
    private string ParseResponse(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[OllamaClient] Received empty response.");
            return null;
        }

        OllamaResponse parsed = JsonUtility.FromJson<OllamaResponse>(json);
        return parsed?.response;
    }

    // -------------------------------------------------------------------------
    // Data transfer objects for Ollama API JSON serialization
    // -------------------------------------------------------------------------

    [Serializable]
    private class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;
    }

    [Serializable]
    private class OllamaResponse
    {
        public string model;
        public string response;
        public bool done;
    }
}
