using UnityEngine;
using LLMUnity;

public class RobotStateManager : MonoBehaviour
{
    [Header("Estado del Robot")]
    [Range(0f, 100f)] public float energia = 100f;
    [Range(0f, 100f)] public float mantenimiento = 100f;
    [Range(0f, 100f)] public float felicidad = 100f;

    [Header("Integracion")]
    [SerializeField] private LLMAgent llmAgent;

    [Header("Desgaste Pasivo")]
    [SerializeField] private float factorDesgaste = 2f;

    private const float MinEstado = 0f;
    private const float MaxEstado = 100f;

    private const string TriggerDance = "Dance0";
    private const string TriggerWin = "Win";
    private const string TriggerThumb = "Thumb";
    private const string TriggerTalk = "Talk";

    private Animator robotAnimator;
    private bool llmRespondiendo;

    private void Update()
    {
        // Simula el paso del tiempo: energia y felicidad bajan normal,
        // mantenimiento baja a la mitad de esa velocidad.
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

        DispararTrigger(TriggerWin);
        EnviarContextoALLM("El usuario recargo tu bateria. Te sientes listo y agradecido. Responde brevemente sobre esto.");

        Debug.Log($"[RobotStateManager] Bateria recargada. {EstadoActual()}");
    }

    public void BotonMantenimiento()
    {
        mantenimiento = MaxEstado;
        ClampEstados();

        DispararTrigger(TriggerThumb);
        EnviarContextoALLM("El usuario realizo tu mantenimiento. Te sientes estable y seguro. Responde brevemente sobre esto.");

        Debug.Log($"[RobotStateManager] Mantenimiento restaurado. {EstadoActual()}");
    }

    public void BotonJugar()
    {
        // Solo permite jugar si hay suficiente energia y mantenimiento.
        if (energia <= 20f || mantenimiento <= 20f)
        {
            EnviarContextoALLM("El usuario quiso jugar contigo, pero tenias poca energia o mantenimiento. Responde brevemente sobre tu estado.");
            Debug.Log($"[RobotStateManager] No se pudo jugar (energia/mantenimiento insuficiente). {EstadoActual()}");
            return;
        }

        felicidad += 30f;
        energia -= 15f;
        mantenimiento -= 10f;

        ClampEstados();

        DispararTrigger(TriggerDance);
        EnviarContextoALLM("El usuario ha jugado contigo. Estas feliz. Responde brevemente sobre esto.");

        Debug.Log($"[RobotStateManager] Robot jugo correctamente. {EstadoActual()}");
    }

    public void RegisterRobot(Animator targetAnimator)
    {
        robotAnimator = targetAnimator;

        if (robotAnimator == null)
        {
            Debug.LogWarning("[RobotStateManager] RegisterRobot recibio un Animator nulo.");
            return;
        }

        Debug.Log($"[RobotStateManager] Animator del robot registrado: {robotAnimator.name}");
    }

    public void OnLLMReplyProgress(string _)
    {
        if (llmRespondiendo)
        {
            return;
        }

        llmRespondiendo = true;
        DispararTrigger(TriggerTalk);
    }

    public void OnLLMReplyComplete()
    {
        llmRespondiendo = false;
    }

    private void EnviarContextoALLM(string contexto)
    {
        if (llmAgent == null)
        {
            Debug.LogWarning("[RobotStateManager] No hay LLMAgent asignado en el Inspector.");
            return;
        }

        llmRespondiendo = false;
        _ = llmAgent.Chat(contexto, OnLLMReplyProgress, OnLLMReplyComplete);
    }

    private void DispararTrigger(string triggerName)
    {
        if (robotAnimator == null)
        {
            return;
        }

        robotAnimator.SetTrigger(triggerName);
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
}
