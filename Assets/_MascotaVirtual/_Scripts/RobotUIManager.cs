using UnityEngine;
using UnityEngine.UI;

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
            Debug.LogError("[RobotUIManager] Falta asignar 'stateManager' en el Inspector.");
            return;
        }

        if (btnRecargar != null)
        {
            btnRecargar.onClick.AddListener(stateManager.BotonRecargarBateria);
        }
        else
        {
            Debug.LogError("[RobotUIManager] Falta asignar 'btnRecargar' en el Inspector.");
        }

        if (btnMantenimiento != null)
        {
            btnMantenimiento.onClick.AddListener(stateManager.BotonMantenimiento);
        }
        else
        {
            Debug.LogError("[RobotUIManager] Falta asignar 'btnMantenimiento' en el Inspector.");
        }

        if (btnJugar != null)
        {
            btnJugar.onClick.AddListener(stateManager.BotonJugar);
        }
        else
        {
            Debug.LogError("[RobotUIManager] Falta asignar 'btnJugar' en el Inspector.");
        }
    }
}
