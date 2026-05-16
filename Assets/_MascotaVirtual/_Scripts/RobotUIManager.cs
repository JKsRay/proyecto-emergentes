using TMPro;
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

    [Header("HUD de Estadísticas (Opcional)")]
    [Tooltip("Renderiza la energía actual. Ej: '85%'")]
    [SerializeField] private TextMeshProUGUI textoHUD_Energia;
    
    [Tooltip("Renderiza el mantenimiento actual. Ej: '85%'")]
    [SerializeField] private TextMeshProUGUI textoHUD_Mantenimiento;
    
    [Tooltip("Renderiza la felicidad actual. Ej: '85%'")]
    [SerializeField] private TextMeshProUGUI textoHUD_Felicidad;

    private void Start()
    {
        if (chatController == null)
        {
            chatController = FindAnyObjectByType<ChatController>();
        }

        // Suscribirse al evento Update UI global de Unity para asegurar refresco rápido 
        // sin depender del ciclo de Update por cada frame ni crear rutinas nuevas.
        // Es más eficiente ya que solo renderizamos si el singleton existe.
        Application.onBeforeRender += RefrescarHUD;

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
        Application.onBeforeRender -= RefrescarHUD;

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

    private void RefrescarHUD()
    {
        // Solo actualizar si el Singleton del robot está activo en la escena
        if (RobotStateManager.Instance == null) return;

        // Obtener los valores exactos, redondearlos a enteros (truncando decimales) y añadir %
        if (textoHUD_Energia != null)
        {
            int valEnergia = Mathf.RoundToInt(RobotStateManager.Instance.Energia);
            textoHUD_Energia.text = $"{valEnergia}%";
        }
        
        if (textoHUD_Mantenimiento != null)
        {
            int valMantenimiento = Mathf.RoundToInt(RobotStateManager.Instance.Mantenimiento);
            textoHUD_Mantenimiento.text = $"{valMantenimiento}%";
        }

        if (textoHUD_Felicidad != null)
        {
            int valFelicidad = Mathf.RoundToInt(RobotStateManager.Instance.Felicidad);
            textoHUD_Felicidad.text = $"{valFelicidad}%";
        }
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
