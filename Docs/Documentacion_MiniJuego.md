# Documentación Arquitectónica: Minijuego AR

Este documento centraliza las decisiones de arquitectura, desarrollo y comportamiento (*Game Design*) del módulo de Minijuego de Realidad Aumentada (AR). Esta documentación es incremental y funciona como bitácora técnica de las actualizaciones y refactorizaciones del código.

## 1. Patrón Arquitectónico del Módulo

El minijuego opera bajo un **Modelo Orientado a Eventos** (*Event-Driven Architecture*) utilizando el patrón **Observer**. Toda la lógica transversal es coordinada por `MinigameManager.cs` actuando como orquestador, mientras que los demás scripts se suscriben a sus `Action` estáticas para acoplarse de manera débil y encapsulada.

### Ciclo de Eventos:
*   `OnGameStarted`: Invocado al iniciar la partida. Valida primero el estado del sistema a través de `RobotStateManager` (bloqueando el juego si la batería es crítica o está descalibrado). Si es válido, oculta el panel general, muestra la UI de AR (cuenta regresiva y marcador) e inicializa las mecánicas (`isGameActive = true`).
*   `OnRobotCaught`: Invocado por los inputs físicos del jugador al tocar al robot. Incrementa el tracking visual del marcador en pantalla e interrumpe al robot forzando una animación evasiva y reubicación.
*   `OnGameWon`: Evento de clímax disparado por el orquestador cuando se alcanza la meta de victorias (3 toques). Transfiere las penalizaciones/premios de métricas finales al robot y comunica el éxito al LLM.
*   `OnGameEnded`: Disparado al concluir orgánicamente la partida o cerrarla forzosamente, regresando el control a la UI principal. Si ocurre sin que se evalúe victoria, se reporta una "cancelación" al modelo de IA.

---

## 2. Refactorizaciones y Arreglos Críticos Recientes

### 2.1. Resolución del Bug de *Input Overlap* ("Fuego Amigo") y "UI Passthrough"
**Síntoma:** Al interactuar con la interfaz del Canvas (botones como "Jugar" o "Recargar"), el Raycast de AR detectaba el toque tras la pantalla y movía/instanciaba erróneamente al robot.

**Corrección Arquitectónica:**
*   Se reescribió `IsTouchOverUI(touch)` en `PlaceObjectOnPlane.cs` para utilizar `PointerEventData` y evaluar todas las proyecciones mediante `EventSystem.current.RaycastAll()`. Esto blinda completamente la interacción protegiendo toda injerencia fantasma a través de la UI.
*   Se introdujo control estricto en el input general del juego suscribiéndolo a los ciclos `OnGameStarted` y `OnGameEnded`.

### 2.2. Diseño Sistémico: Cuenta Regresiva ("Telegraphing") y Limpieza de Animator
**Síntoma:** Arranque brusco y choques de animación en el Frame 1. `RobotStateManager` ordenaba bailar por el botar "Jugar" al mismo tiempo que `RobotARNavigator` esperaba estaticidad, quebrando el comportamiento.

**Corrección Arquitectónica (`RobotARNavigator.cs` y `MinigameManager.cs`):**
*   Ahora la rutina principal incluye una espera de un frame (`yield return null;`) seguida por una directiva `animator.Rebind()`. Esto asfixia el trigger de baile en la misma milésima e impone obligatoriamente al robot mantenerse estático y en pose base (Idle).
*   Se ligó dinámicamente un text mesh de visualización asíncrona ("4, 3, 2, 1, ¡YA!"), forzando que todo movimiento algorítmico del robot quede suspendido durante el transcurso exacto del Delay.

### 2.3. Balance de Dificultad (Anti-Paredes y Escape en Interiores AR)
**Síntoma:** El robot cruzaba paredes reales frecuentemente porque los cálculos geométricos proyectaban destinos aleatorios ilógicos y la distancia máxima de escape era muy agresiva (3.0 metros).

**Corrección Arquitectónica (`RobotARNavigator.cs`):**
*   **Ajuste Espacial Conservador:** La distancia de salto hacia el siguiente plano AR se acotó estrictamente a un anillo con mínimos garantizados (`1.0` a `1.5` metros).
*   **Heurística Robusta (Producto Punto):** Se implantó un algoritmo defensivo de escape. El robot archiva `lastMoveDirection` en memoria virtual y efectúa una evaluación matemática condicional por iteraciones múltiples contra los futuros vectores calculados. Evalúa vía producto escalar limitante (Dot Product <= 0.2f), lo que previene reincidencia ortogonal hacia adelante; forzando al ente a esquivar en zigzag o rebotar dinámicamente contra los techos y límites de habitaciones estrechas.

### 2.4. Integridad del Entorno AR (Instanciación Segura y Planos Invisibles)
**Síntoma:** La instanciación re-posicionaba al robot caóticamente con cada toque a la pantalla, y los planos amarillos por defecto arruinaban el nivel de inmersión en la habitación real.

**Corrección Arquitectónica (`PlaceObjectOnPlane.cs`):**
*   **Bloqueo de Mapeo (Teletransporte Cero):** Tras instanciarse existosamente en un primer plano válido, se detona la barrera lógica privada `_isPlacementLocked = true`. Expusimos públicamente `UnlockPlacement()` en API como puerto futuro por si la función requiere reposicionamiento voluntario.
*   **Sustancias Invisibles de AR:** Optimización de Planos AR (Prefabs Nativos): Para garantizar la inmersión sin sacrificar ciclos de CPU (evitando interceptar eventos planesChanged), se resolvió desde la arquitectura del motor. Se extrajo el AR Default Plane conformando un Prefab personalizado (PlanoInvisible_Prefab), al cual se le eliminaron los componentes MeshRenderer y LineRenderer de raíz. AR Foundation ahora clona nativamente moldes invisibles que solo contienen colisiones físicas, reduciendo la carga de procesamiento del dispositivo móvil.

### 2.5. Integración con Máquina de Estados (RobotStateManager) y Desacople Lógico
**Síntoma:** El robot se moría drenado en medio del juego, penalizando variables estadísticas de energía injustamente por cada salto interactivo o cobrando castigos dobles de cansancio prematuro. Adicionalmente de inyectar textos a una UI falsa (apagada).

**Corrección Arquitectónica (`RobotStateManager.cs`):**
*   **Pausa Asíncrona (Criostasis Temporal):** Al ingresar en `isGameActive`, las rutinas periódicas de gasto metabólico natural evaden sus ejecuciones matemáticas orgánicas pausando la resta continua y suspendiendo el tiempo virtual hasta culminar.
*   **Paquetes de Resultado Compacto (OnGameWon vs OnGameEnded):** El balance y compensaciones de final de partida fueron apartados de los bucles repetitivos de intercepción (`OnRobotCaught`); empaquetándose en saldos en crudo definitivos aplicados durante el cierre triunfal en formato global (Ej: `Felicidad += 50`, `Energia -= 15`), enviando el Promt y log interno en estricta coordinación tras retornar visibilidad al chat. 

### 2.6. Migración a Dependencia Discreta (*Hide and Seek*)
**Síntoma:** El script del robot dependía de un bucle perenne dependiente del tiempo estricto (`while (true)` con *timers* de `WaitForSeconds` aleatorios). Era una máquina de estado infinita donde escapaba sin control explícito del jugador.

**Corrección Arquitectónica:**
1.  **Destrucción del bucle cronométrico:**  Se purgó el método sobrecargado `NavigateRandomly()`.
2.  **Modelo Discreto:** Se introdujo `MoveToSinglePosition()`. La función ubica un único `ARPlane`, interpola (Lerp y Slerp) el `Transform` hacia ese objetivo, finaliza ejecución natural y el proceso **muere**. El robot pasa a un estado de inactividad (`Idle`) incondicional.
3.  **Encadenamiento de Estados:** Cuando interviene el jugador (`HandleRobotCaught`), el componente procesa la captura e inicializa los festejos (`RecoverAndFlee`). Esta corrutina invierte 2 segundos inerte pre-evaluados, e invoca internamente el re-rutado discreto y singular de `MoveToSinglePosition()`.

---

> *Nota: Este archivo es parte de la memoria del flujo de trabajo del proyecto. Ante futuras intervenciones estructurales y de iteración, deberá ser expandido automáticamente conservando este formato para su trazabilidad y mantenimiento técnico.*