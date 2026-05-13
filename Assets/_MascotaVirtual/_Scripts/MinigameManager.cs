using UnityEngine;
using System;

public class MinigameManager : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    public GameObject panelChat;
    public GameObject panelJuego;

    [Header("Eventos del Minijuego")]
    public static Action OnGameStarted;
    public static Action OnGameEnded;
    public static Action OnRobotCaught;

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
        if (catchCount >= catchesToWin)
        {
            EndGame();
        }
    }

    /// <summary>
    /// Inicia el minijuego, oculta el chat y dispara el evento de inicio.
    /// </summary>
    public void StartGame()
    {
        catchCount = 0;
        if (panelChat != null) panelChat.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(true);

        OnGameStarted?.Invoke();
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
