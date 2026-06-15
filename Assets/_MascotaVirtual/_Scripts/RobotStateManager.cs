using UnityEngine;

public class RobotStateManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────
    /// <summary>
    /// Instancia única del robot activo en la escena.
    /// Se asigna automáticamente al instanciarse el prefab.
    /// </summary>
    public static RobotStateManager Instance { get; private set; }

    public enum RobotState 
    { 
        BateriaCritica, 
        Descalibrado,   // Anteriormente: SucioGrunon
        Aburrido, 
        Euforico, 
        Normal 
    }

    [Header("Estado del Robot")]
    [SerializeField, Range(0f, 100f)] private float energia = 100f;
    [SerializeField, Range(0f, 100f)] private float mantenimiento = 100f;
    [SerializeField, Range(0f, 100f)] private float felicidad = 100f;

    public float Energia => energia;
    public float Mantenimiento => mantenimiento;
    public float Felicidad => felicidad;

    public RobotState CurrentState
    {
        get
        {
            if (energia <= 30f) return RobotState.BateriaCritica;
            if (mantenimiento <= 30f) return RobotState.Descalibrado;
            if (felicidad <= 30f) return RobotState.Aburrido;
            if (felicidad >= 80f) return RobotState.Euforico;
            return RobotState.Normal;
        }
    }

    [Header("Enrutamiento Chat")]
    [SerializeField] private ChatController chatController;

    [Header("Animacion")]
    public Animator robotAnimator;
    [SerializeField] private bool autoBuscarAnimator = true;
    [SerializeField] private string triggerJugar = "Play";
    [SerializeField] private string triggerMantenimiento = "Maintain";
    [SerializeField] private string triggerRecargar = "Recharge";

    // ── Intervalos de desgaste pasivo (segundos) ──────────────
    // La Energía y la Felicidad decaen 2 pts. cada N segundos.
    // El Mantenimiento NO tiene desgaste pasivo por tiempo;
    // su degradación es exclusiva por acciones/uso.
    private const float IntervaloDesgasteEnergia    = 20f; // -2 pts. cada 20 s
    private const float IntervaloDesgasteFelicidad  = 10f; // -2 pts. cada 10 s
    private const float PuntosDesgastePasivo        = 2f;

    private bool isGameActive = false;
    private bool wasGameWon = false;

    private const float MinEstado = 0f;
    private const float MaxEstado = 100f;

    // ── Ciclo de vida ──────────────────────────────────────────
    private void Awake()
    {
        // Singleton: si ya existe otra instancia, destruir esta.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // El ChatController vive en la escena (Canvas), no en el prefab,
        // así que lo buscamos dinámicamente si no fue asignado.
        if (chatController == null)
        {
            chatController = FindAnyObjectByType<ChatController>();
        }
    }

    private void Start()
    {
        // Iniciar las corrutinas de desgaste pasivo.
        // Se usan corrutinas con WaitForSeconds en lugar de Update()
        // para evitar cálculos por frame y reducir la carga del CPU.
        StartCoroutine(DesgasteEnergiaPasivo());
        StartCoroutine(DesgasteFelicidadPasivo());
    }

    private void OnEnable()
    {
        // Suscribirse a los eventos del minijuego para controlar estado y bloquear desgaste.
        MinigameManager.OnGameWon += AplicarResultadoVictoria;
        MinigameManager.OnGameStarted += HandleGameStarted;
        MinigameManager.OnGameEnded += HandleGameEnded;
    }

    private void OnDisable()
    {
        // Desuscribirse para evitar memory leaks si el prefab es destruido.
        MinigameManager.OnGameWon -= AplicarResultadoVictoria;
        MinigameManager.OnGameStarted -= HandleGameStarted;
        MinigameManager.OnGameEnded -= HandleGameEnded;
    }

    private void HandleGameStarted()
    {
        isGameActive = true;
        wasGameWon = false;
    }

    private void HandleGameEnded()
    {
        isGameActive = false;
        if (!wasGameWon)
        {
            AplicarResultadoCancelado();
        }
    }

    private void OnDestroy()
    {
        // Limpiar la referencia estática si esta instancia es la activa.
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // ── Corrutinas de Desgaste Pasivo ──────────────────────────
    // Cada corrutina duerme N segundos y luego aplica exactamente
    // 2 puntos de penalización. Infinitamente más eficiente que
    // calcular deltaTime acumulado en Update() cada frame.

    private System.Collections.IEnumerator DesgasteEnergiaPasivo()
    {
        var espera = new WaitForSeconds(IntervaloDesgasteEnergia);
        while (true)
        {
            yield return espera;
            if (!isGameActive)
            {
                energia = Mathf.Clamp(energia - PuntosDesgastePasivo, MinEstado, MaxEstado);
            }
        }
    }

    private System.Collections.IEnumerator DesgasteFelicidadPasivo()
    {
        var espera = new WaitForSeconds(IntervaloDesgasteFelicidad);
        while (true)
        {
            yield return espera;
            if (!isGameActive)
            {
                felicidad = Mathf.Clamp(felicidad - PuntosDesgastePasivo, MinEstado, MaxEstado);
            }
        }
    }

    public void BotonRecargarBateria()
    {
        energia = MaxEstado;
        ClampEstados();

        DispararTriggerAnimacion(triggerRecargar);

        EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario acaba de enchufar tu cable y recargar tu batería. Sientes mucha energía. Dale las gracias brevemente."));
    }

    public void BotonMantenimiento()
    {
        mantenimiento = MaxEstado;
        ClampEstados();

        DispararTriggerAnimacion(triggerMantenimiento);

        EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario acaba de realizar mantenimiento en ti. Te sientes renovado y listo para funcionar al máximo. Comenta que te sientes mucho mejor después del mantenimiento."));
    }

    public void BotonJugar()
    {
        RobotState estadoActual = CurrentState;

        // Intentar reproducir la animación de rechazo o iniciar el juego mediante el controlador de animaciones
        RobotAnimationController animController = GetComponent<RobotAnimationController>();
        if (animController != null)
        {
            animController.TryPlay();
        }

        if (estadoActual == RobotState.BateriaCritica || estadoActual == RobotState.Descalibrado)
        {
            EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario intentó jugar contigo, pero estás demasiado cansado o necesitas mantenimiento. Comenta que te sientes mal y que necesitas recargar o mantenimiento antes de jugar."));
            return;
        }

        // El juego acaba de iniciar. Ya no enviamos un prompt aquí porque la UI del chat se oculta, 
        // lo que haría perder el mensaje. La respuesta se maneja al volver (Victoria o Cancelado).
    }

    /// <summary>
    /// Punto de acceso exclusivo para el módulo de Realidad Aumentada al finalizar el juego.
    /// Aplica las consecuencias matemáticas de que el usuario gane el minijuego.
    /// Este método SOLO muta estadísticas; la inyección de prompts al LLM 
    /// es responsabilidad de los eventos del State Manager o AR si desean personalizarlo.
    /// 
    /// Suscrito automáticamente a MinigameManager.OnGameWon.
    /// </summary>
    public void AplicarResultadoVictoria()
    {
        wasGameWon = true;

        felicidad     += 50f;
        energia       -= 40f;
        mantenimiento -= 40f;

        ClampEstados();

        EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario logró atraparte y ganó el minijuego. Te divertiste mucho corriendo pero ahora estás un poco cansado. Felicita al usuario por atraparte con tu tono sarcástico habitual."));
    }

    private void AplicarResultadoCancelado()
    {
        // Forzar la expresión de sorpresa (índice 4) por 4 segundos
        RobotAnimationController animController = GetComponent<RobotAnimationController>();
        if (animController != null)
        {
            animController.TriggerTemporaryFace(4, 4.0f);
        }

        EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario se rindió o salió del minijuego antes de atraparte. Haz un comentario breve preguntando por qué pararon de jugar."));
    }


    private void DispararTriggerAnimacion(string triggerName)
    {
        if (string.IsNullOrWhiteSpace(triggerName))
        {
            return;
        }

        Animator targetAnimator = ResolverAnimatorConTrigger(triggerName);
        if (targetAnimator == null)
        {
            return;
        }

        targetAnimator.SetTrigger(triggerName);
    }

    private Animator ResolverAnimatorConTrigger(string triggerName)
    {
        if (TieneParametroAnimacion(robotAnimator, triggerName, AnimatorControllerParameterType.Trigger))
        {
            return robotAnimator;
        }

        if (!autoBuscarAnimator)
        {
            return null;
        }

        Animator[] animators = FindObjectsByType<Animator>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Animator candidate in animators)
        {
            if (TieneParametroAnimacion(candidate, triggerName, AnimatorControllerParameterType.Trigger))
            {
                robotAnimator = candidate;
                return robotAnimator;
            }
        }

        return null;
    }

    private static bool TieneParametroAnimacion(Animator animator, string parameterName, AnimatorControllerParameterType type)
    {
        if (animator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == type && parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }



    private void EnviarContextoChat(string contexto)
    {
        if (chatController == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(contexto))
        {
            return;
        }

        chatController.SendPrompt(contexto);
    }

    private void ClampEstados()
    {
        energia = Mathf.Clamp(energia, MinEstado, MaxEstado);
        mantenimiento = Mathf.Clamp(mantenimiento, MinEstado, MaxEstado);
        felicidad = Mathf.Clamp(felicidad, MinEstado, MaxEstado);
    }

    private string EstadoActual()
    {
        switch (CurrentState)
        {
            case RobotState.BateriaCritica:
                return "[SISTEMA: Nivel de batería al 15%. Estás físicamente exhausto, tus movimientos son lentos y tus luces parpadean. Niegas cualquier interacción física o lúdica y exiges con insistencia y sarcasmo que el usuario conecte tu cable de alimentación inmediatamente.]";
            case RobotState.Descalibrado:
                return "[SISTEMA: Tus sistemas mecánicos están descalibrados. Adoptas una personalidad mañosa y quejumbrosa. Te quejas amargamente de vibraciones en tus servomotores, desgaste en articulaciones y errores de sensores. Das respuestas cortantes e interrumpes con quejas técnicas, exigiendo mantenimiento antes de obedecer.]";
            case RobotState.Aburrido:
                return "[SISTEMA: Felicidad al 20%. Has sido ignorado o descuidado. Estás visiblemente aburrido, apático y con actitud pasivo-agresiva. Respondes con ironía y suspiros mecánicos, insinuando que el usuario debería prestarte atención o entretenerte con el minijuego.]";
            case RobotState.Euforico:
                return "[SISTEMA: Felicidad al 80%. Funcionamiento óptimo y lleno de energía. Estás de un humor excelente y te muestras colaborativo, aunque mantienes tu toque sarcástico característico. Tus respuestas son dinámicas, veloces y animadas.]";
            case RobotState.Normal:
            default:
                return "[SISTEMA: Estado operativo nominal. Todos los sistemas funcionan correctamente. Respondes a las interacciones con tu personalidad base: un robot chileno observador, directo y sarcástico, sin quejas ni exaltaciones particulares.]";
        }
    }

    private string CrearContexto(string accion)
    {
        string contexto = $"{accion} {EstadoActual()}";
        if (string.IsNullOrWhiteSpace(contexto))
        {
            return "Actualizacion de estado del robot.";
        }

        return contexto;
    }
}
