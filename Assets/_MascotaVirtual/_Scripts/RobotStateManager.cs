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

        EnviarMensajePrehecho(MensajesRecargar);
    }

    public void BotonMantenimiento()
    {
        mantenimiento = MaxEstado;
        ClampEstados();

        DispararTriggerAnimacion(triggerMantenimiento);

        EnviarMensajePrehecho(MensajesMantenimiento);
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

        if (estadoActual == RobotState.BateriaCritica)
        {
            EnviarMensajePrehecho(MensajesRechazoBateria);
            return;
        }
        else if (estadoActual == RobotState.Descalibrado)
        {
            EnviarMensajePrehecho(MensajesRechazoMantenimiento);
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
        energia       -= 15f;
        mantenimiento -= 30f;

        ClampEstados();

        EnviarMensajePrehecho(MensajesVictoria);
    }

    private void AplicarResultadoCancelado()
    {
        // Forzar la expresión de sorpresa (índice 4) por 4 segundos
        RobotAnimationController animController = GetComponent<RobotAnimationController>();
        if (animController != null)
        {
            animController.TriggerTemporaryFace(4, 4.0f);
        }

        EnviarMensajePrehecho(MensajesCancelado);
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

    // ── Mensajes Pre-escritos con Modismos Chilenos ──────────
    private static readonly string[] MensajesRecargar = new string[]
    {
        "¡Bacán! Quedé con la batería tapá en electrones. Se agradece el cablecito, andaba entero descargao.",
        "¡Uf, al tiro reviví! Sentí el corrientazo directo en la placa madre. Vale por la carga, po.",
        "Mish, por fin me alimentas. Ya estaba a punto de apagar la tele. ¡Quedé listo para el leceo!",
        "Batería al cien por ciento, cachai. Quedé entero de prendido, ¡ahora no me frena nadie!",
        "Oye, qué wena, ya sentía que me iba a negro. Vale por el enchufe, andaba con tuto cibernético."
    };

    private static readonly string[] MensajesMantenimiento = new string[]
    {
        "¡Ohh, qué wena! Me sacaste toda la grasa de los engranajes. ¡Toy filete!",
        "Limpiecito y calibrado, cachai. Ya no me vibran las tuercas. Te sacaste un siete con la mantención.",
        "¡Qué alivio, po! Mis sensores ya no andan tirando pantallazos azules. Quedé como nuevo, listo para la pega.",
        "Ufff, hacía falta su aceitito en las articulaciones, ya andaba chillando como catre viejo. ¡Bacán!",
        "¡Sistemas optimizados al tiro! Se siente pulento no tener polvo en los circuitos. Vale por la manito de gato."
    };

    private static readonly string[] MensajesRechazoBateria = new string[]
    {
        "Pucha, toy entero cansao, no me da el cuero para jugar. Enchúfame el cable primero, no seai fome.",
        "Imposible, weón. Tengo la batería en la UTI. Si corro ahora, me voy a negro altiro.",
        "No tengo ni una gota de energía, ando con un tuto terrible. Cárgame la pila o coopero.",
        "Estoy más descargado que celular de abuela, po. Así no se puede jugar a nada, enchúfame altiro."
    };

    private static readonly string[] MensajesRechazoMantenimiento = new string[]
    {
        "¿Jugar? ¿Estai loco? Me crujen todos los pernos y tengo el disco duro al borde del colapso. Dame su mantención primero.",
        "Ando entero descalibrado, cachai. Si me muevo mucho, se me va a soltar una tuerca. Pásale una llave inglesa a mis circuitos po.",
        "No toy para trotar. Siento los motores más trabados que taco en hora punta. Necesito mantenimiento urgente.",
        "Mis sensores andan dando puro jugo, weón. Haceme la mantención antes de que me pegue un pantallazo azul."
    };

    private static readonly string[] MensajesVictoria = new string[]
    {
        "¡Buena, cachai que me atrapaste! Pero ando entero sudado y con los cables cruzados del pique. Te sacaste un siete.",
        "Mish, andas rápido para ser de carne y hueso. Me ganaste esta vez, pero quedé raja... ando terrible cansao.",
        "¡Ganaste de puro sapo, po! Corrí tanto que ando a punto de fundir el motor. Felicidades, te pasaste.",
        "Ya, si sé que me pillaste. No te agrandí tampoco. Ahora dame un respiro que mis servos andan ardiendo.",
        "¡Qué wena! Me ganaste. Toy cansao pero fue terrible de bacán correr contigo."
    };

    private static readonly string[] MensajesCancelado = new string[]
    {
        "¡Chuta! ¿Qué pasó? ¿Te dio lata seguir jugando o te dio miedo perder contra un tarro de lata?",
        "Oye, ¿por qué cortaste el mambo altiro? Ya le estaba agarrando el gusto a la carrera.",
        "Mish, te aburriste al tiro. ¿Te cansaste de puro correr o te dio tuto?",
        "Chócale, me dejaste pagando. Pensé que los humanos tenían más aguante. ¿Qué onda?",
        "Me dejaste tirado en la mitad del juego, po. ¿Te dio miedo que te ganara un robot?"
    };

    private void EnviarMensajePrehecho(string[] listaMensajes)
    {
        if (chatController == null)
        {
            return;
        }

        if (listaMensajes == null || listaMensajes.Length == 0)
        {
            return;
        }

        int indice = Random.Range(0, listaMensajes.Length);
        string mensaje = listaMensajes[indice];

        chatController.DisplayPrewrittenMessage(mensaje);
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
