using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controlador de animaciones y expresiones faciales para la mascota virtual (Robot).
/// Administra de forma centralizada la comunicación con el Animator y los offsets de texturas faciales.
/// </summary>
public class RobotAnimationController : MonoBehaviour
{
    [Header("Referencias de Animación")]
    [Tooltip("Referencia al componente Animator. Se buscará en Awake si queda vacía.")]
    [SerializeField] private Animator animator;

    [Header("Referencias de Expresión Facial")]
    [Tooltip("Renderer de los ojos (Rob11_Eyes). Se buscará automáticamente si queda vacío.")]
    [SerializeField] private Renderer eyesRenderer;
    [Tooltip("Renderer de la boca (Rob11_Mouth). Se buscará automáticamente si queda vacío.")]
    [SerializeField] private Renderer mouthRenderer;

    [Header("Métricas de la Mascota")]
    [Tooltip("Nivel de energía actual (0 a 100). Sincronizado automáticamente si RobotStateManager está presente.")]
    public float currentEnergy = 100f;
    [Tooltip("Nivel de felicidad actual (0 a 100). Sincronizado automáticamente si RobotStateManager está presente.")]
    public float currentHappiness = 100f;
    [Tooltip("Nivel de mantenimiento actual (0 a 100). Sincronizado automáticamente si RobotStateManager está presente.")]
    public float currentMaintenance = 100f;

    [Header("Nombres de Parámetros del Animator")]
    [SerializeField] private string triggerHello = "Tr_Hello";
    [SerializeField] private string triggerRefuseNo = "Tr_RefuseNo";

    [Header("Eventos de UI")]
    [Tooltip("Evento que se dispara cuando el robot acepta jugar (todas las necesidades >= 20).")]
    public UnityEvent OnPlaySuccess;

    // Propiedades de Shader en caché para optimización de rendimiento
    private static readonly int BaseMapProp = Shader.PropertyToID("_BaseMap");
    private static readonly int EmissionMapProp = Shader.PropertyToID("_EmissionMap");

    // Variables de control de expresiones temporales (Overrides)
    private float faceOverrideTimer = 0f;
    private int overrideFaceIndex = -1;

    // Variables de simulación de habla
    private bool isSpeaking = false;
    private float speakMouthTimer = 0f;
    private bool speakMouthOpen = false;

    private void Awake()
    {
        // Guardar la referencia al Animator en el Awake()
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        // Buscar automáticamente los renderers de ojos y boca en la jerarquía
        if (eyesRenderer == null)
        {
            eyesRenderer = FindInChildrenByName("Rob11_Eyes")?.GetComponent<Renderer>();
        }
        if (mouthRenderer == null)
        {
            mouthRenderer = FindInChildrenByName("Rob11_Mouth")?.GetComponent<Renderer>();
        }

    }

    private void OnEnable()
    {
        // Saludo físico
        if (animator != null)
        {
            animator.SetTrigger(triggerHello);
        }

        // Saludo facial: Forzar la cara de sonrisa grande (índice 1) durante 4 segundos al aparecer
        TriggerTemporaryFace(1, 4.0f);
    }

    private void Update()
    {
        // Sincronizar automáticamente con el RobotStateManager si está activo en la escena
        if (RobotStateManager.Instance != null)
        {
            currentEnergy = RobotStateManager.Instance.Energia;
            currentHappiness = RobotStateManager.Instance.Felicidad;
            currentMaintenance = RobotStateManager.Instance.Mantenimiento;
        }

        // ── Simulación Facial de Habla (Boca Abriéndose/Cerrándose) ───
        if (isSpeaking)
        {
            speakMouthTimer += Time.deltaTime;
            if (speakMouthTimer >= 0.12f) // Alternar boca abierta/cerrada cada 120 ms
            {
                speakMouthOpen = !speakMouthOpen;
                speakMouthTimer = 0f;
            }
        }
        else
        {
            speakMouthOpen = false;
            speakMouthTimer = 0f;
        }

        // ── Manejo de Expresiones ─────────────────────────────────────
        int faceIndexToUse;

        if (faceOverrideTimer > 0f)
        {
            // Hay una expresión temporal activa (como saludo o sorpresa)
            faceOverrideTimer -= Time.deltaTime;
            faceIndexToUse = overrideFaceIndex;
        }
        else
        {
            // Determinar la expresión natural según las necesidades actuales
            faceIndexToUse = GetFaceIndexFromNeeds();
        }

        // Aplicar los offsets de textura a los ojos
        SetEyesExpression(faceIndexToUse);

        // Aplicar los offsets a la boca (alternando si habla para simular fonemas)
        if (isSpeaking && speakMouthOpen)
        {
            // Usar boca de cara 5 (índice 4 - sorprendido con boca de "O" neutra)
            // de modo que no sonría de golpe si está triste/afligido
            SetMouthExpression(4);
        }
        else
        {
            SetMouthExpression(faceIndexToUse);
        }
    }

    /// <summary>
    /// Devuelve el índice de expresión correcto basado en las necesidades actuales.
    /// </summary>
    private int GetFaceIndexFromNeeds()
    {
        // 1. Necesidades críticas (Energía o Mantenimiento <= 5%) -> Cara de muerto (índice 5)
        if (currentEnergy <= 5f || currentMaintenance <= 5f)
        {
            return 5;
        }

        // 2. Necesidades bajas (Energía o Mantenimiento < 30%) -> Cara de afligido / preocupado (índice 6)
        if (currentEnergy < 30f || currentMaintenance < 30f)
        {
            return 6;
        }

        // 3. Estado Normal -> Cara sonriente sutil (índice 0)
        return 0;
    }

    /// <summary>
    /// Cambia el offset de textura en el material de los ojos para seleccionar una expresión.
    /// </summary>
    private void SetEyesExpression(int index)
    {
        float offsetX = index * 0.1f;
        Vector2 offset = new Vector2(offsetX, 0f);

        if (eyesRenderer != null && eyesRenderer.material != null)
        {
            eyesRenderer.material.SetTextureOffset(BaseMapProp, offset);
            eyesRenderer.material.SetTextureOffset(EmissionMapProp, offset);
        }
    }

    /// <summary>
    /// Cambia el offset de textura en el material de la boca para seleccionar una expresión.
    /// </summary>
    private void SetMouthExpression(int index)
    {
        float offsetX = index * 0.1f;
        Vector2 offset = new Vector2(offsetX, 0f);

        if (mouthRenderer != null && mouthRenderer.material != null)
        {
            mouthRenderer.material.SetTextureOffset(BaseMapProp, offset);
            mouthRenderer.material.SetTextureOffset(EmissionMapProp, offset);
        }
    }

    /// <summary>
    /// Fuerza una expresión facial temporal durante una cantidad determinada de segundos.
    /// </summary>
    public void TriggerTemporaryFace(int index, float duration)
    {
        overrideFaceIndex = index;
        faceOverrideTimer = duration;
        
        // Aplicar inmediatamente para evitar delay de 1 frame
        SetEyesExpression(index);
        SetMouthExpression(index);
    }

    /// <summary>
    /// Activa o desactiva la simulación de habla.
    /// Es llamado por el ChatController al enviar respuestas.
    /// </summary>
    public void SetSpeaking(bool speaking)
    {
        isSpeaking = speaking;
        if (!speaking)
        {
            speakMouthOpen = false;
            speakMouthTimer = 0f;
        }
    }

    /// <summary>
    /// Busca un objeto hijo en toda la jerarquía de este prefab usando una coincidencia de nombre.
    /// </summary>
    private GameObject FindInChildrenByName(string targetName)
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (var child in allChildren)
        {
            if (child.name == targetName)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Comprueba si el Animator contiene un parámetro específico.
    /// </summary>
    private static bool TieneParametroAnimacion(Animator anim, string parameterName, AnimatorControllerParameterType type)
    {
        if (anim == null) return false;
        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.type == type && parameter.name == parameterName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Verifica las necesidades del robot.
    /// Si la energía o el mantenimiento son menores a 30f,
    /// lanza el Trigger Tr_RefuseNo. De lo contrario, inicia el juego.
    /// </summary>
    public void TryPlay()
    {
        if (currentEnergy < 30f || currentMaintenance < 30f)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerRefuseNo);
            }
        }
        else
        {
            // Si el evento tiene listeners asignados (por ejemplo, en un objeto de la escena)
            if (OnPlaySuccess != null && OnPlaySuccess.GetPersistentEventCount() > 0)
            {
                OnPlaySuccess.Invoke();
            }
            else
            {
                // Fallback automático en tiempo de ejecución: buscar el MinigameManager en la escena
                MinigameManager minigame = FindAnyObjectByType<MinigameManager>();
                if (minigame != null)
                {
                    minigame.StartGame();
                }
                else
                {
                    Debug.LogWarning("[RobotAnimationController] No se encontró MinigameManager en la escena ni listeners en OnPlaySuccess.");
                }
            }
        }
    }
}
