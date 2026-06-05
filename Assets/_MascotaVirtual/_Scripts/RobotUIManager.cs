using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la interacción de los botones de la UI con el estado del robot.
/// Utiliza RobotStateManager.Instance (Singleton) para comunicarse con el
/// robot instanciado dinámicamente, sin requerir referencia por inspector.
/// </summary>
public class RobotUIManager : MonoBehaviour
{
    [SerializeField] private ChatController chatController;

    [Header("Botones de Acción")]
    [SerializeField] private Button btnRecargar;
    [SerializeField] private Button btnMantenimiento;
    [SerializeField] private Button btnJugar;

    [Header("HUD de Estadísticas (Textos)")]
    [SerializeField] private TextMeshProUGUI textoHUD_Energia;
    [SerializeField] private TextMeshProUGUI textoHUD_Mantenimiento;
    [SerializeField] private TextMeshProUGUI textoHUD_Felicidad;

    [Header("HUD de Estadísticas (Barras Radiales)")]
    [Tooltip("Renderiza el progreso circular de la batería (Valores de 0 a 1)")]
    [SerializeField] private Image radialHUD_Energia;
    
    [Tooltip("Renderiza el progreso circular del mantenimiento (Valores de 0 a 1)")]
    [SerializeField] private Image radialHUD_Mantenimiento;
    
    [Tooltip("Renderiza el progreso circular de la felicidad (Valores de 0 a 1)")]
    [SerializeField] private Image radialHUD_Felicidad;

    private void Start()
    {
        if (chatController == null)
        {
            chatController = FindAnyObjectByType<ChatController>();
        }

        Application.onBeforeRender += RefrescarHUD;

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
        if (RobotStateManager.Instance == null) return;

        // Actualización de Textos
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

        // Actualización de Imágenes Radiales
        if (radialHUD_Energia != null)
        {
            radialHUD_Energia.fillAmount = RobotStateManager.Instance.Energia / 100f;
        }

        if (radialHUD_Mantenimiento != null)
        {
            radialHUD_Mantenimiento.fillAmount = RobotStateManager.Instance.Mantenimiento / 100f;
        }

        if (radialHUD_Felicidad != null)
        {
            radialHUD_Felicidad.fillAmount = RobotStateManager.Instance.Felicidad / 100f;
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
        if (boton == null) return;
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