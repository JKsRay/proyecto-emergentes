using UnityEngine;

public class RobotStateManager : MonoBehaviour
{
    [Header("Estado del Robot")]
    [Range(0f, 100f)] public float energia = 100f;
    [Range(0f, 100f)] public float mantenimiento = 100f;
    [Range(0f, 100f)] public float felicidad = 100f;

    [Header("Enrutamiento Chat")]
    [SerializeField] private ChatController chatController;

    [Header("Animacion")]
    public Animator robotAnimator;
    [SerializeField] private bool autoBuscarAnimator = true;
    [SerializeField] private string triggerJugar = "Play";
    [SerializeField] private string triggerMantenimiento = "Maintain";
    [SerializeField] private string triggerRecargar = "Recharge";

    [Header("Desgaste Pasivo")]
    [SerializeField] private float factorDesgaste = 2f;

    private const float MinEstado = 0f;
    private const float MaxEstado = 100f;

    private void Update()
    {
        
        float desgasteNormal = factorDesgaste * Time.deltaTime;
        float desgasteMantenimiento = desgasteNormal * 0.5f;

        energia -= desgasteNormal;
        felicidad -= desgasteNormal;
        mantenimiento -= desgasteMantenimiento;

        ClampEstados();
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
        if (energia <= 20f || mantenimiento <= 20f)
        {
            EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario intentó jugar contigo, pero estás demasiado cansado o necesitas mantenimiento. Comenta que te sientes mal y que necesitas recargar o mantenimiento antes de jugar."));
            return;
        }

        felicidad += 30f;
        energia -= 15f;
        mantenimiento -= 10f;

        ClampEstados();

        DispararTriggerAnimacion(triggerJugar);

        EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario acaba de jugar contigo un rato. Estás muy feliz y te divertiste mucho. Comenta sobre el juego."));
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
        return $"Energia: {energia:F1} | Mantenimiento: {mantenimiento:F1} | Felicidad: {felicidad:F1}";
    }

    private string CrearContexto(string accion)
    {
        string contexto = $"{accion}. Estado actual del robot -> {EstadoActual()}";
        if (string.IsNullOrWhiteSpace(contexto))
        {
            return "Actualizacion de estado del robot.";
        }

        return contexto;
    }
}
