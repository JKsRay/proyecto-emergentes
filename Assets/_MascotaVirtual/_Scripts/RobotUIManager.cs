using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class RobotUIManager : MonoBehaviour
{
    [SerializeField] private RobotStateManager stateManager;
    [SerializeField] private ChatController chatController;

    [SerializeField] private Button btnRecargar;
    [SerializeField] private Button btnMantenimiento;
    [SerializeField] private Button btnJugar;

    private void Start()
    {
        if (chatController == null)
        {
            chatController = FindAnyObjectByType<ChatController>();
        }

        if (stateManager != null)
        {
            ConectarBoton(btnRecargar, stateManager.BotonRecargarBateria);
            ConectarBoton(btnMantenimiento, stateManager.BotonMantenimiento);
            ConectarBoton(btnJugar, stateManager.BotonJugar);
        }

        if (chatController != null)
        {
            chatController.RequestInFlightChanged += OnRequestInFlightChanged;
            OnRequestInFlightChanged(chatController.IsRequestInFlight);
        }
    }

    private void OnDestroy()
    {
        if (stateManager != null)
        {
            DesconectarBoton(btnRecargar, stateManager.BotonRecargarBateria);
            DesconectarBoton(btnMantenimiento, stateManager.BotonMantenimiento);
            DesconectarBoton(btnJugar, stateManager.BotonJugar);
        }

        if (chatController != null)
        {
            chatController.RequestInFlightChanged -= OnRequestInFlightChanged;
        }

        SetBotonesInteractables(true);
    }

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

    private void ConectarBoton(Button boton, UnityAction accion)
    {
        if (boton == null)
        {
            return;
        }

        // Evita registros duplicados si el ciclo de vida recompone componentes.
        boton.onClick.RemoveListener(accion);
        boton.onClick.AddListener(accion);
    }

    private void DesconectarBoton(Button boton, UnityAction accion)
    {
        if (boton != null)
        {
            boton.onClick.RemoveListener(accion);
        }
    }
}
