using UnityEngine;

public class RobotStateManager : MonoBehaviour
{
    [Header("Estado del Robot")]
    [Range(0f, 100f)] public float energia = 100f;
    [Range(0f, 100f)] public float mantenimiento = 100f;
    [Range(0f, 100f)] public float felicidad = 100f;

    [Header("Desgaste Pasivo")]
    [SerializeField] private float factorDesgaste = 2f;

    private const float MinEstado = 0f;
    private const float MaxEstado = 100f;

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

        Debug.Log($"[RobotStateManager] Bateria recargada. {EstadoActual()}");
    }

    public void BotonMantenimiento()
    {
        mantenimiento = MaxEstado;
        ClampEstados();

        Debug.Log($"[RobotStateManager] Mantenimiento restaurado. {EstadoActual()}");
    }

    public void BotonJugar()
    {
        // Solo permite jugar si hay suficiente energia y mantenimiento.
        if (energia <= 20f || mantenimiento <= 20f)
        {
            Debug.Log($"[RobotStateManager] No se pudo jugar (energia/mantenimiento insuficiente). {EstadoActual()}");
            return;
        }

        felicidad += 30f;
        energia -= 15f;
        mantenimiento -= 10f;

        ClampEstados();

        Debug.Log($"[RobotStateManager] Robot jugo correctamente. {EstadoActual()}");
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
