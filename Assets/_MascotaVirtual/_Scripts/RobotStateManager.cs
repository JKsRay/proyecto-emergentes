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
            if (energia <= 20f) return RobotState.BateriaCritica;
            if (mantenimiento <= 20f) return RobotState.Descalibrado;
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
        // Suscribirse al evento de captura del minijuego de la compañera.
        // Si MinigameManager cambia de nombre o estructura, solo esta línea cambia.
        MinigameManager.OnRobotCaught += AplicarResultadoCaptura;
    }

    private void OnDisable()
    {
        // Desuscribirse para evitar memory leaks si el prefab es destruido.
        MinigameManager.OnRobotCaught -= AplicarResultadoCaptura;
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
            energia = Mathf.Clamp(energia - PuntosDesgastePasivo, MinEstado, MaxEstado);
        }
    }

    private System.Collections.IEnumerator DesgasteFelicidadPasivo()
    {
        var espera = new WaitForSeconds(IntervaloDesgasteFelicidad);
        while (true)
        {
            yield return espera;
            felicidad = Mathf.Clamp(felicidad - PuntosDesgastePasivo, MinEstado, MaxEstado);
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
        if (estadoActual == RobotState.BateriaCritica || estadoActual == RobotState.Descalibrado)
        {
            EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario intentó jugar contigo, pero estás demasiado cansado o necesitas mantenimiento. Comenta que te sientes mal y que necesitas recargar o mantenimiento antes de jugar."));
            return;
        }

        // Costo real de actividad física del botón Jugar.
        felicidad     += 30f;
        energia       -= 15f;
        mantenimiento -= 30f;

        ClampEstados();

        EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario acaba de jugar contigo un rato. Estás muy feliz y te divertiste mucho. Comenta sobre el juego."));
    }

    /// <summary>
    /// Punto de acceso exclusivo para el módulo de Realidad Aumentada.
    /// Aplica las consecuencias matemáticas de que el usuario atrape al robot
    /// durante el minijuego. Este método SOLO muta estadísticas; la inyección
    /// de prompts al LLM es responsabilidad del script de AR que lo invoque.
    /// 
    /// Suscrito automáticamente a MinigameManager.OnRobotCaught.
    /// </summary>
    public void AplicarResultadoCaptura()
    {
        felicidad     += 50f;
        energia       -= 15f;
        mantenimiento -= 30f;

        ClampEstados();
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
                return "[SISTEMA: Batería crítica. Estás exhausto, niegas interactuar y exiges un cargador.]";
            case RobotState.Descalibrado:
                return "[SISTEMA: Tus sistemas mecánicos están descalibrados y con fallas. Adoptas una personalidad mañosa y extremadamente sarcástica. Te quejas amargamente de vibraciones en tus servomotores, desgaste en tus articulaciones y errores en tus sensores. Das respuestas cortantes, interrumpes con quejas técnicas y exiges mantenimiento inmediato antes de hacer cualquier cosa.]";
            case RobotState.Aburrido:
                return "[SISTEMA: Felicidad baja. Estás aburrido, das respuestas cortantes o irónicas pidiendo atención.]";
            case RobotState.Euforico:
                return "[SISTEMA: Felicidad alta. Funcionamiento óptimo, estás de excelente humor dentro de tu sarcasmo habitual.]";
            case RobotState.Normal:
            default:
                return "[SISTEMA: Estado óptimo. Respondes con tu sarcasmo robótico habitual.]";
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
