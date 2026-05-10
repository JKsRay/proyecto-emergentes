using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la interacción de los botones de la UI con el estado del robot.
/// Utiliza RobotStateManager.Instance (Singleton) para comunicarse con el
/// robot instanciado dinámicamente, sin requerir referencia por inspector.
///
/// IMPORTANTE: Los listeners se añaden con AddListener (runtime).
/// Esto NO interfiere con los eventos persistentes configurados en el
/// Inspector (ej. MinigameManager.StartGame/EndGame en el botón Jugar).
/// Unity ejecuta ambos tipos de suscripción simultáneamente.
/// </summary>
public class RobotUIManager : MonoBehaviour
{
    [SerializeField] private ChatController chatController;

    [Header("Botones de Acción")]
    [SerializeField] private Button btnRecargar;
    [SerializeField] private Button btnMantenimiento;
    [SerializeField] private Button btnJugar;

    private void Start()
    {
        if (chatController == null)
        {
            chatController = FindAnyObjectByType<ChatController>();
        }

        // Conectar los botones a los métodos wrapper.
        // AddListener añade listeners de runtime que conviven con los
        // listeners persistentes del Inspector (ej. MinigameManager).
        ConectarBoton(btnRecargar, OnClickRecargar);
        ConectarBoton(btnMantenimiento, OnClickMantenimiento);
        ConectarBoton(btnJugar, OnClickJugar);

        if (chatController != null)
        {
            chatController.RequestInFlightChanged += OnRequestInFlightChanged;
            OnRequestInFlightChanged(chatController.IsRequestInFlight);
        }
    }

    private void OnDestroy()
    {
        DesconectarBoton(btnRecargar, OnClickRecargar);
        DesconectarBoton(btnMantenimiento, OnClickMantenimiento);
        DesconectarBoton(btnJugar, OnClickJugar);

        if (chatController != null)
        {
            chatController.RequestInFlightChanged -= OnRequestInFlightChanged;
        }

        SetBotonesInteractables(true);
    }

    // ── Wrappers que acceden al Singleton ──────────────────────
    // Verifican que el robot exista antes de ejecutar la acción.
    // Si el robot aún no ha sido colocado en la escena, el botón
    // simplemente no hará nada (falla silenciosa controlada).

    private void OnClickRecargar()
    {
        if (RobotStateManager.Instance != null)
            RobotStateManager.Instance.BotonRecargarBateria();
    }

    private void OnClickMantenimiento()
    {
        if (RobotStateManager.Instance != null)
            RobotStateManager.Instance.BotonMantenimiento();
    }

    private void OnClickJugar()
    {
        if (RobotStateManager.Instance != null)
            RobotStateManager.Instance.BotonJugar();
    }

    // ── Estado de botones ──────────────────────────────────────

    private void OnRequestInFlightChanged(bool inFlight)
    {
        SetBotonesInteractables(!inFlight);
    }

    private void SetBotonesInteractables(bool interactable)
    {
        SetBotonInteractable(btnRecargar, interactable);
        SetBotonInteractable(btnMantenimiento, interactable);
        SetBotonInteractable(btnJugar, interactable);
    }

    private static void SetBotonInteractable(Button boton, bool interactable)
    {
        if (boton != null)
        {
            boton.interactable = interactable;
        }
    }

    // ── Helpers de conexión ────────────────────────────────────

    private void ConectarBoton(Button boton, UnityEngine.Events.UnityAction accion)
    {
        if (boton == null)
        {
            return;
        }

        // RemoveListener solo elimina la instancia runtime exacta,
        // NUNCA los listeners persistentes configurados en el Inspector.
        boton.onClick.RemoveListener(accion);
        boton.onClick.AddListener(accion);
    }

    private void DesconectarBoton(Button boton, UnityEngine.Events.UnityAction accion)
    {
        if (boton != null)
        {
            boton.onClick.RemoveListener(accion);
        }
    }
}
