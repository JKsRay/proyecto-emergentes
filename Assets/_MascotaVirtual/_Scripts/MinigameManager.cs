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

    /// <summary>
    /// Inicia el minijuego, oculta el chat y dispara el evento de inicio.
    /// </summary>
    public void StartGame()
    {
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
