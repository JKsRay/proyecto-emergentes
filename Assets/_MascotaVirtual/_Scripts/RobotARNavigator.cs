using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(Animator))]
public class RobotARNavigator : MonoBehaviour
{
    private Animator animator;
    private Camera mainCamera;
    
    // Caché del trigger encontrado en Rob11.controller
    private readonly int triggerJump = Animator.StringToHash("Jump");

    private Coroutine movementCoroutine;
    private bool isGameActive = false;

    // Memoria del movimiento previo para heurística de rebote ("anti-paredes")
    private Vector3 lastMoveDirection = Vector3.zero;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        // Suscribirse a los eventos del minijuego
        MinigameManager.OnGameStarted += HandleGameStarted;
        MinigameManager.OnRobotCaught += HandleRobotCaught;
        MinigameManager.OnGameEnded += HandleGameEnded;
    }

    private void OnDisable()
    {
        // Desuscribirse para evitar colisiones en memoria
        MinigameManager.OnGameStarted -= HandleGameStarted;
        MinigameManager.OnRobotCaught -= HandleRobotCaught;
        MinigameManager.OnGameEnded -= HandleGameEnded;
    }

    private void HandleGameEnded()
    {
        isGameActive = false;

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        if (animator != null)
        {
            animator.Play("Motions");
            animator.Rebind();
            animator.Update(0f);
        }
    }

    private void HandleGameStarted()
    {
        isGameActive = true;

        // Iniciar el comportamiento del robot con un delay inicial ("Telegraphing")
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(StartGameWithDelay());
    }

    private IEnumerator StartGameWithDelay()
    {
        // Para evitar interferencia con el RobotStateManager (que dispara la animación de baile al mismo tiempo
        // desde el botón Jugar), limpiamos cualquier trigger residual esperando 1 frame para que se procesen los eventos paralelos.
        yield return null;
        
        if (animator != null)
        {
            animator.Rebind(); // Forzar el Animator nuevamente a su flujo por defecto (Idle)
            animator.Update(0f);
        }

        // 4 Segundos de Telegraphing antes de que el robot se empiece a mover (cuenta regresiva UI)
        yield return new WaitForSeconds(4f);
        movementCoroutine = StartCoroutine(MoveToSinglePosition());
    }

    private void HandleRobotCaught()
    {
        if (!isGameActive) return;

        // Disparar la animación de salto
        animator.SetTrigger(triggerJump);
        
        // Detener la navegación actual y empezar la recuperación
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(RecoverAndFlee());
    }

    private IEnumerator RecoverAndFlee()
    {
        // El salto y festejo del robot (aprox 2 segundos)
        yield return new WaitForSeconds(2f);
        // Cuando termine el salto, huye y se posiciona en una nueva ubicación hasta que vuelvan a tocarlo
        movementCoroutine = StartCoroutine(MoveToSinglePosition());
    }

    private IEnumerator MoveToSinglePosition()
    {
        Vector3 targetPosition = GetRandomPositionOnPlanes();
        
        // Si el punto devuelto es la misma posición (ej. no hay planos), no hace nada hasta el próximo trigger o simplemente se queda ahí
        if (Vector3.Distance(targetPosition, transform.position) > 0.1f)
        {
            float duration = 2.0f; // Tiempo que toma desplazarse al punto
            float elapsed = 0f;
            Vector3 startPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);

                // Mover al robot suavemente con Lerp
                transform.position = Vector3.Lerp(startPos, targetPosition, t);

                // Rotar para mirar a la cámara permanentemente durante su movimiento
                if (mainCamera != null)
                {
                    Vector3 lookPos = mainCamera.transform.position;
                    lookPos.y = transform.position.y; // Mantenerlo recto, ignorando diferencia de altura
                    Vector3 direction = (lookPos - transform.position).normalized;
                    
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        // Interpolación esférica de la rotación para suavizado
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
                    }
                }

                yield return null;
            }
        }
        
        // Una vez alcanzó el punto, la corrutina termina y el robot NO se moverá de nuevo automáticamente.
        // Se queda en Idle y mirando hacia donde quedó hasta que lo toquen de nuevo.
    }

    /// <summary>
    /// Calcula un punto aleatorio asegurándose de referenciar la altura 
    /// de un plano AR Horizontal detectado, optimizado para interiores pequeños.
    /// Utiliza una heurística de rebote para evitar continuar en la misma dirección.
    /// </summary>
    private Vector3 GetRandomPositionOnPlanes()
    {
        // Buscar todos los planos AR en la jerarquía
        ARPlane[] arPlanes = Object.FindObjectsByType<ARPlane>(FindObjectsSortMode.None);

        if (arPlanes == null || arPlanes.Length == 0) 
            return transform.position;

        Vector2 randomDir = Vector2.zero;
        
        // --- Heurística Anti-Paredes ---
        // Intentar hasta 10 veces encontrar un vector que diverja significativamente del último movimiento.
        // Queremos un producto punto <= 0.2f (es decir, el robot debe girar bruscamente al menos ~78 grados).
        float targetDot = 0.2f;
        float bestDot = float.MaxValue;
        
        for (int i = 0; i < 10; i++)
        {
            Vector2 candidateDir = Random.insideUnitCircle.normalized;
            
            // Si es el primer movimiento del juego, aceptarlo libremente
            if (lastMoveDirection == Vector3.zero)
            {
                randomDir = candidateDir;
                break;
            }

            Vector3 candidate3D = new Vector3(candidateDir.x, 0, candidateDir.y);
            float dot = Vector3.Dot(candidate3D, lastMoveDirection);

            // Si el ángulo de escape es suficientemente distinto al de la corrida anterior
            if (dot <= targetDot)
            {
                randomDir = candidateDir;
                break;
            }

            // Guardar el "menos malo" por si se agotan los 10 intentos
            if (dot < bestDot)
            {
                bestDot = dot;
                randomDir = candidateDir;
            }
        }

        // Limitar distancia de huida asegurando que el robot se mueva al menos 1 metro, máximo 1.5 metros, adaptado a habitaciones pequeñas
        float randomDistance = Random.Range(1.0f, 1.5f);
        Vector2 escapeVector = randomDir * randomDistance;
        
        Vector3 candidatePos = transform.position + new Vector3(escapeVector.x, 0, escapeVector.y);

        // Actualizar la memoria direccional para la próxima vez que huya
        lastMoveDirection = new Vector3(escapeVector.x, 0, escapeVector.y).normalized;

        ARPlane bestPlane = null;
        float closestDistance = float.MaxValue;

        // Filtrar y buscar el plano de suelo más cercano al candidato
        foreach (var plane in arPlanes)
        {
            if (plane.alignment != PlaneAlignment.HorizontalUp) 
                continue;

            float dist = Vector2.Distance(
                new Vector2(candidatePos.x, candidatePos.z), 
                new Vector2(plane.transform.position.x, plane.transform.position.y)
            );

            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestPlane = plane;
            }
        }

        if (bestPlane != null)
        {
            // Ajustar la altura al Y detectado en el plano físico
            candidatePos.y = bestPlane.transform.position.y;
            return candidatePos;
        }

        return transform.position;
    }
}
