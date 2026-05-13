using UnityEngine;

public class TouchCatcher : MonoBehaviour
{
    [Header("Configuración de Captura")]
    [Tooltip("El Tag que debe tener el collider del robot para ser detectado.")]
    [SerializeField] private string robotTag = "Player";
    [Tooltip("Distancia máxima (en unidades de Unity) para poder atrapar al robot.")]
    [SerializeField] private float catchDistance = 1.5f;
    
    [Header("Cooldown")]
    [Tooltip("Tiempo en segundos antes de poder volver a intentar tocar al robot.")]
    [SerializeField] private float cooldownTime = 1.0f;

    private Camera mainCamera;
    private bool canCatch = true;
    private float cooldownTimer = 0f;
    private bool isGameActive = false;

    private void OnEnable()
    {
        MinigameManager.OnGameStarted += SetGameActive;
        MinigameManager.OnGameEnded += SetGameInactive;
    }

    private void OnDisable()
    {
        MinigameManager.OnGameStarted -= SetGameActive;
        MinigameManager.OnGameEnded -= SetGameInactive;
    }

    private void SetGameActive() => isGameActive = true;
    private void SetGameInactive() => isGameActive = false;

    private void Start()
    {
        // En un rig de XR Origin, la cámara principal suele tener el tag "MainCamera".
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[TouchCatcher] No se encontró Camera.main. Asegúrate de que la cámara de tu XR Origin tenga el tag 'MainCamera'.");
        }
    }

    private void Update()
    {
        if (!isGameActive) return;

        // Manejar el cooldown
        if (!canCatch)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canCatch = true;
            }
        }

        // Si estamos en cooldown, no procesar inputs
        if (!canCatch) return;

        // Detectar input táctil en móvil
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Solo evaluar cuando el dedo acaba de tocar la pantalla
            if (touch.phase == TouchPhase.Began)
            {
                ProcessTouch(touch.position);
            }
        }
        // Soporte adicional para pruebas en el Editor de Unity usando el ratón
        else if (Input.GetMouseButtonDown(0))
        {
            ProcessTouch(Input.mousePosition);
        }
    }

    private void ProcessTouch(Vector2 screenPosition)
    {
        if (mainCamera == null) return;

        // Lanzar rayo desde la cámara originado en el toque
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        
        // Ejecutar Physics.Raycast. (Opcional: puedes agregar un LayerMask si requieres optimizar el raycast).
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Validar que hemos impactado al robot
            // Nota: Se valida por Tag, pero también hacemos un fallback buscando el RobotStateManager
            // por si el Tag no está configurado exactamente.
            bool isRobot = hit.collider.CompareTag(robotTag) || hit.collider.GetComponentInParent<RobotStateManager>() != null;

            if (isRobot)
            {
                // Verificar que no esté demasiado lejos
                if (hit.distance <= catchDistance)
                {
                    CatchRobot();
                }
            }
        }
    }

    private void CatchRobot()
    {
        // Aplicar cooldown
        canCatch = false;
        cooldownTimer = cooldownTime;

        // Disparar el evento estático
        MinigameManager.OnRobotCaught?.Invoke();
        
        Debug.Log("[TouchCatcher] ¡Robot impactado! Evento OnRobotCaught disparado.");
    }
}
