# AR Virtual Pet — Proyecto Emergentes

An Augmented Reality mobile virtual pet application where players interact with a living AR companion through natural language. The pet responds intelligently via a locally-hosted Large Language Model (Llama 3.2 via Ollama), and its emotional/physical state evolves based on how well the player cares for it.

---

## Tech Stack

| Layer | Technology |
|---|---|
| **Frontend / Game Engine** | Unity 2022 LTS (C#) |
| **AR Framework** | Google ARCore (via AR Foundation) |
| **LLM Backend** | [Llama 3.2](https://ollama.com/library/llama3.2) running locally through [Ollama](https://ollama.com/) |
| **API Communication** | REST (HTTP POST, JSON) via `UnityWebRequest` |
| **Target Platform** | Android (ARCore-supported devices) |

---

## Project Structure

```
proyecto-emergentes/
├── Assets/
│   ├── Animations/        # Animation clips and controllers for the AR pet
│   ├── Materials/         # Materials and shaders used by 3D models
│   ├── Prefabs/           # Reusable AR pet prefabs and UI prefabs
│   ├── Scenes/            # Unity scene files (.unity)
│   ├── Scripts/
│   │   ├── OllamaClient.cs      # Async HTTP client for the Ollama REST API
│   │   └── PetStateManager.cs   # Pet needs management (Hunger, Hygiene, Happiness)
│   └── Textures/          # Texture assets for the pet and environment
├── Packages/              # Unity Package Manager manifest
├── ProjectSettings/       # Unity project settings (build, physics, etc.)
├── .gitignore
└── README.md
```

---

## Core Scripts

### `OllamaClient.cs`
Handles asynchronous HTTP POST requests to the local Ollama server. Sends a user text prompt and returns the LLM's response string.

**Key method:**
```csharp
public async Task<string> SendPromptAsync(string userPrompt)
```

**Configurable fields (Inspector):**
- `apiUrl` — URL of the Ollama endpoint (default: `http://localhost:11434/api/generate`)
- `modelName` — Ollama model to use (default: `llama3.2`)

---

### `PetStateManager.cs`
Manages the virtual pet's internal needs. All values are clamped in the range **[0, 100]**.

| Stat | Description |
|---|---|
| **Hunger** | 0 = full, 100 = starving |
| **Hygiene** | 0 = dirty, 100 = spotless |
| **Happiness** | 0 = miserable, 100 = ecstatic |

**Key methods:**
```csharp
void IncreaseHunger(float amount)
void DecreaseHunger(float amount)
void IncreaseHygiene(float amount)
void DecreaseHygiene(float amount)
void IncreaseHappiness(float amount)
void DecreaseHappiness(float amount)
PetState GetCurrentState()
void ResetToDefaults()
```

Fires `OnStateChanged` (a `Action<PetState>` event) whenever any stat changes.

---

## Prerequisites

- **Unity 2022 LTS** or later with **AR Foundation** and **ARCore XR Plugin** packages installed.
- An **ARCore-compatible Android device** for on-device testing.
- **[Ollama](https://ollama.com/)** installed and running on your local machine or network.
- **Llama 3.2** model pulled into Ollama.

---

## Local API Setup

1. **Install Ollama**
   Follow the official instructions at [https://ollama.com/download](https://ollama.com/download).

2. **Pull the Llama 3.2 model**
   ```bash
   ollama pull llama3.2
   ```

3. **Start the Ollama server**
   ```bash
   ollama serve
   ```
   The server listens on `http://localhost:11434` by default.

4. **Verify the API is reachable**
   ```bash
   curl http://localhost:11434/api/generate \
     -d '{"model":"llama3.2","prompt":"Hello!","stream":false}'
   ```
   You should receive a JSON response containing a `"response"` field.

5. **Configure Unity**
   - Open the Unity project.
   - In the scene, select the GameObject that holds `OllamaClient`.
   - In the Inspector, set `Api Url` to match your Ollama server address (e.g., `http://192.168.x.x:11434/api/generate` if running on a separate machine on your LAN).

---

## Getting Started

1. Clone this repository:
   ```bash
   git clone https://github.com/JKsRay/proyecto-emergentes.git
   ```
2. Open the project in **Unity Hub** → **Open Project** (select the cloned folder).
3. Install the required packages via **Window → Package Manager**:
   - `AR Foundation`
   - `ARCore XR Plugin`
4. Follow the **Local API Setup** steps above.
5. Build and run on an ARCore-compatible Android device via **File → Build Settings → Android → Build and Run**.

---

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

---

## License

This project is open-source. See `LICENSE` for details.
