using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class RobotUIManager : MonoBehaviour
{
    [SerializeField] private RobotStateManager stateManager;

    [SerializeField] private Button btnRecargar;
    [SerializeField] private Button btnMantenimiento;
    [SerializeField] private Button btnJugar;

    private void Start()
    {
        if (stateManager == null)
        {
            return;
        }

        ConectarBoton(btnRecargar, stateManager.BotonRecargarBateria);
        ConectarBoton(btnMantenimiento, stateManager.BotonMantenimiento);
        ConectarBoton(btnJugar, stateManager.BotonJugar);
    }

    private void OnDestroy()
    {
        if (stateManager == null)
        {
            return;
        }

        DesconectarBoton(btnRecargar, stateManager.BotonRecargarBateria);
        DesconectarBoton(btnMantenimiento, stateManager.BotonMantenimiento);
        DesconectarBoton(btnJugar, stateManager.BotonJugar);
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
