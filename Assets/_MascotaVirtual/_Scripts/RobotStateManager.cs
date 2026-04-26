using UnityEngine;

public class RobotStateManager : MonoBehaviour
{
    [Header("Estado del Robot")]
    [Range(0f, 100f)] public float energia = 100f;
    [Range(0f, 100f)] public float mantenimiento = 100f;
    [Range(0f, 100f)] public float felicidad = 100f;

    [Header("Enrutamiento Chat")]
    [SerializeField] private ChatController chatController;

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

        EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario acaba de enchufar tu cable y recargar tu batería. Sientes mucha energía. Dale las gracias brevemente."));
    }

    public void BotonMantenimiento()
    {
        mantenimiento = MaxEstado;
        ClampEstados();

        EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario acaba de hacerte mantenimiento y reparar tus piezas. Te sientes como nuevo. Agradécele."));
    }

    public void BotonJugar()
    {
        if (energia <= 20f || mantenimiento <= 20f)
        {
            EnviarContextoChat(CrearContexto("Intento de juego rechazado por energia/mantenimiento insuficiente"));
            return;
        }

        felicidad += 30f;
        energia -= 15f;
        mantenimiento -= 10f;

        ClampEstados();

        EnviarContextoChat(CrearContexto("[SISTEMA]: El usuario acaba de jugar contigo un rato. Estás muy feliz y te divertiste mucho. Comenta sobre el juego."));
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
