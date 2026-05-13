# Documentación Arquitectónica: Minijuego AR

Este documento centraliza las decisiones de arquitectura, desarrollo y comportamiento (*Game Design*) del módulo de Minijuego de Realidad Aumentada (AR). Esta documentación es incremental y funciona como bitácora técnica de las actualizaciones y refactorizaciones del código.

## 1. Patrón Arquitectónico del Módulo

El minijuego opera bajo un **Modelo Orientado a Eventos** (*Event-Driven Architecture*) utilizando el patrón **Observer**. Toda la lógica transversal es coordinada por `MinigameManager.cs` actuando como orquestador, mientras que los demás scripts se suscriben a sus `Action` estáticas para acoplarse de manera débil y encapsulada.

### Ciclo de Eventos:
*   `OnGameStarted`: Transiciona el estado del módulo. Oculta el panel general, muestra el panel del minijuego e inicializa las mecánicas de AR (`isGameActive = true`).
*   `OnRobotCaught`: Invocado por los inputs del jugador. Incrementa los contadores internos e interrumpe las máquinas de estado del robot forzando una animación de impacto/evasión.
*   `OnGameEnded`: Disparado al alcanzar la condición de victoria (o forzado externamente), reiniciando o destruyendo los estados de movimiento y regresando el control a la UI general.

---

## 2. Refactorizaciones y Arreglos Críticos Recientes

### 2.1. Resolución del Bug de *Input Overlap* ("Fuego Amigo")
**Síntoma:** Al instanciar el Prefab de la mascota sobre un plano usando AR, el componente encargado de los toques interactivos procesaba el mismo input (pantalla) e invocaba erráticamente el atrapado del robot, causando deslizamientos sin presionar "Jugar".

**Corrección Arquitectónica:**
*   Se introdujo control de estado condicional (`isGameActive`) en **`TouchCatcher.cs`** y en el componente de navegación de la mascota.
*   En el momento en que se efectúa `OnEnable()`, ambos módulos se suscriben rigurosamente a `MinigameManager.OnGameStarted` y `MinigameManager.OnGameEnded`.
*   El ciclo estricto interviene el bloque superior del `Update()` del input oponiendo una guarda condicional (`if (!isGameActive) return;`), inhibiendo cualquier intercepción de Raycast hasta que el flujo de UI decida explícitamente arrancar el juego. Asimismo, `RobotARNavigator.cs` previene un falso-positivo en `HandleRobotCaught`.

### 2.2. Diseño Sistémico: "Telegraphing"
**Síntoma:** Falta total de anticipación de cara al jugador. Al invocar `OnGameStarted`, la corrutina `NavigateRandomly` se disparaba instantáneamente y alterando el transform del objeto.

**Corrección Arquitectónica:**
*   Se implementó la fase de 'Telegraphing'. En **`RobotARNavigator.cs`**, el evento del manejador invoca a la nueva subcorrutina intermedia `StartGameWithDelay()`.
*   Esto añade de forma estructurada un `yield return new WaitForSeconds(4f);`, desacoplando temporalmente el flujo de ejecución para la UI futura (Feedback Visual / Textos como "*¡Listo!*", "*¡Salva al Robot!*").  

### 2.3. Migración a Dependencia Discreta (*Hide and Seek*)
**Síntoma:** El script del robot dependía de un bucle perenne dependiente del tiempo estricto (`while (true)` con *timers* de `WaitForSeconds` aleatorios). Era una máquina de estado infinita donde escapaba sin control explícito del jugador.

**Corrección Arquitectónica:**
1.  **Destrucción del bucle cronométrico:**  Se purgó el método sobrecargado `NavigateRandomly()`.
2.  **Modelo Discreto:** Se introdujo `MoveToSinglePosition()`. La función ubica un único `ARPlane`, interpola (Lerp y Slerp) el `Transform` hacia ese objetivo, finaliza ejecución natural y el proceso **muere**. El robot pasa a un estado de inactividad (`Idle`) incondicional.
3.  **Encadenamiento de Estados:** Cuando interviene el jugador (`HandleRobotCaught`), el componente procesa la captura e inicializa los festejos (`RecoverAndFlee`). Esta corrutina invierte 2 segundos inerte pre-evaluados, e invoca internamente el re-rutado discreto y singular de `MoveToSinglePosition()`.

### 2.4. Integridad del Controlador de Animación (Prefab) y Prevención de World Drift
*   **Conflictos Físicos de AR:** Se desactivaron explícitamente los scripts `Rob11Ctrl` y `RoboLift` heredados en el Prefab base. Estos scripts contaban con emuladores de controles de teclado y alteradores de físicas que interferían y creaban conflictos con el sistema de rastreo de AR Foundation.
*   **Optimizaciones de Animator:** En la raíz del `Animator` de *Rob11*, se ratificó la desactivación formal de la casilla de `Apply Root Motion`. El desplazamiento por translación real es íntegramente manejado algorítmicamente mediante Vector Math (`Vector3.Lerp` suave), lo que previene cruce de vectores con el propio sistema de gravedad dinámico en sub-estados y desalineamientos sobre el `ARPlane`.

### 2.5. Balance de Dificultad (Escape en AR)
*   Para hacer el minijuego más dinámico, se ajustó la variable de generación de los puntos destino. En la lógica de escaneo de `ARPlane`s funcionales, el radio aleatorio para calcular la posición de escape se amplió explícitamente a **3.0 metros**. Esto fuerza un traslado de mayor envergadura sobre el entorno físico del usuario.

### 2.6. Condición de Victoria (Mocking)
*   **Contador de Atrapes:** Se integró un mockup en `MinigameManager.cs` rastreando un contador interno (`catchCount`). Tras lograr la meta definida (`catchesToWin = 3`), el orquestador dispara automáticamente el evento de cierre `OnGameEnded`. Esta lógica sella los "juegos sueltos" e instaura formalmente la noción de ganar.

---

> *Nota: Este archivo es parte de la memoria del flujo de trabajo del proyecto. Ante futuras intervenciones estructurales y de iteración, deberá ser expandido automáticamente conservando este formato para su trazabilidad y mantenimiento técnico.*