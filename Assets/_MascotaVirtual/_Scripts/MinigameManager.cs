using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class MinigameManager : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    public GameObject panelChat;
    public GameObject panelJuego;

    [Header("UI Textos")]
    public TextMeshProUGUI Texto_CuentaRegresiva;
    public TextMeshProUGUI Texto_ContadorCapturas;

    [Header("Eventos del Minijuego")]
    public static Action OnGameStarted;
    public static Action OnGameEnded;
    public static Action OnRobotCaught;
    public static Action OnGameWon;

    // Sistema temporal de 'Mocking'
    private int catchCount = 0;
    private int catchesToWin = 3;

    private void OnEnable()
    {
        OnRobotCaught += HandleRobotCaught;
    }

    private void OnDisable()
    {
        OnRobotCaught -= HandleRobotCaught;
    }

    private void HandleRobotCaught()
    {
        catchCount++;
        UpdateCatchCounterText();
        if (catchCount >= catchesToWin)
        {
            OnGameWon?.Invoke();
            EndGame();
        }
    }

    private void UpdateCatchCounterText()
    {
        if (Texto_ContadorCapturas != null)
        {
            Texto_ContadorCapturas.text = $"Atrápame {catchCount}/{catchesToWin}";
        }
    }

    /// <summary>
    /// Inicia el minijuego, oculta el chat y dispara el evento de inicio.
    /// </summary>
    public void StartGame()
    {
        // Validación: impedir iniciar juego si el estado del robot es crítico
        if (RobotStateManager.Instance != null)
        {
            var estado = RobotStateManager.Instance.CurrentState;
            if (estado == RobotStateManager.RobotState.BateriaCritica || estado == RobotStateManager.RobotState.Descalibrado)
            {
                return; // Se cancela el inicio del juego visual de AR.
            }
        }

        catchCount = 0;
        if (panelChat != null) panelChat.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(true);

        UpdateCatchCounterText();
        StartCoroutine(StartCountdown());

        OnGameStarted?.Invoke();
    }

    private IEnumerator StartCountdown()
    {
        if (Texto_CuentaRegresiva != null)
        {
            Texto_CuentaRegresiva.gameObject.SetActive(true);
            Texto_CuentaRegresiva.text = "4";
            yield return new WaitForSeconds(1f);
            Texto_CuentaRegresiva.text = "3";
            yield return new WaitForSeconds(1f);
            Texto_CuentaRegresiva.text = "2";
            yield return new WaitForSeconds(1f);
            Texto_CuentaRegresiva.text = "1";
            yield return new WaitForSeconds(1f);
            Texto_CuentaRegresiva.text = "¡YA!";
            yield return new WaitForSeconds(1f);
            Texto_CuentaRegresiva.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Termina el minijuego, oculta el panel de juego y restaura el chat.
    /// </summary>
    public void EndGame()
    {
        if (panelJuego != null) panelJuego.SetActive(false);
        if (panelChat != null) panelChat.SetActive(true);

        OnGameEnded?.Invoke();
    }
}
