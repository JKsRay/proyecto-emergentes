# Documentación de la Máquina de Estados Finitos (FSM) y Lógica de Negocio

Este documento detalla la estructura, funcionamiento y variables matemáticas de la Máquina de Estados Finitos (FSM) central del robot (`RobotStateManager.cs`), así como su integración con otros módulos del sistema.

## 1. Arquitectura de la FSM

La FSM del robot gestiona tres estadísticas principales: Energía, Mantenimiento y Felicidad (con rangos limitados de 0 a 100). Dependiendo de la jerarquía de estos valores, el robot asume uno de cinco estados, lo que influye directamente en los contextos (System Prompts) que se inyectan en el modelo de lenguaje (LLM).

### Estados y Umbrales (`RobotState`)

| Prioridad | Estado | Umbral | Descripción del Prompt (Personalidad) |
|---|---|---|---|
| 1 | **`BateriaCritica`** | Energía <= 30 | Estás exhausto, niegas interactuar y exiges un cargador. |
| 2 | **`Descalibrado`** | Mantenimiento <= 30 | Tus sistemas están descalibrados. Adoptas una personalidad mañosa y extremadamente sarcástica. Te quejas de vibraciones en servomotores y exiges mantenimiento. |
| 3 | **`Aburrido`** | Felicidad <= 30 | Estás aburrido, das respuestas cortantes o irónicas pidiendo atención. |
| 4 | **`Euforico`** | Felicidad >= 80 | Funcionamiento óptimo, de excelente humor dentro de tu sarcasmo habitual. |
| Por Defecto | **`Normal`** | - | Estado óptimo. Respondes con tu sarcasmo robótico habitual. |

*Nota del Historial: El estado "Descalibrado" reemplazó al antiguo estado "SucioGrunon" para proveer una personalidad mecánica mucho más rica y técnica al LLM.*

---

## 2. Lógica de Desgaste Pasivo

### Optimización de Rendimiento: Corrutinas vs `Update()`
En versiones anteriores, el desgaste de las estadísticas se calculaba cada frame multiplicando factores por `Time.deltaTime` dentro del método `Update()`. En dispositivos móviles, especialmente corriendo Realidad Aumentada, procesar matemáticas cada frame es un desperdicio de ciclos de CPU.

La arquitectura actual utiliza **Corrutinas (`WaitForSeconds`)**. Las corrutinas envían el hilo a dormir y lo despiertan exactamente en los intervalos requeridos, reduciendo a cero el costo computacional entre actualizaciones de estado.

#### Tasas de Degradación:
- **Energía**: Disminuye `2` puntos cada `20` segundos de tiempo real.
- **Felicidad**: Disminuye `2` puntos cada `10` segundos de tiempo real.
- **Mantenimiento**: **Sin desgaste pasivo**. Se degrada exclusivamente por la actividad física del robot.

---

## 3. Interacciones y Costos Matemáticos

Las acciones del usuario sobre la mascota alteran directamente las estadísticas internas. Estos son los costos matemáticos actuales de la lógica de negocio:

### Botón UI: "Jugar"
- **Felicidad:** +30
- **Energía:** -15
- **Mantenimiento:** -30

### Minijuego AR (`AplicarResultadoVictoria`)
- **Felicidad:** +50
- **Energía:** -40
- **Mantenimiento:** -40

#### Separación de Responsabilidades (Arquitectura Orientada a Eventos)
Para evitar el código espagueti, el módulo de Realidad Aumentada y la FSM están completamente desacoplados mediante eventos C# estáticos definidos en `MinigameManager`. El `RobotStateManager` se suscribe a tres de estos eventos en `OnEnable()` y se desuscribe en `OnDisable()` (evitando memory leaks si el prefab es destruido):

| Evento | Método en FSM | Efecto |
|---|---|---|
| `OnGameStarted` | `HandleGameStarted()` | Activa `isGameActive = true`, pausa el desgaste pasivo |
| `OnGameWon` | `AplicarResultadoVictoria()` | Aplica +50 Felicidad, −40 Energía, −40 Mantenimiento e inyecta prompt de victoria al LLM |
| `OnGameEnded` | `HandleGameEnded()` | Desactiva `isGameActive`; si no hubo victoria, ejecuta `AplicarResultadoCancelado()` |

* **El módulo AR:** Se encarga de la detección espacial, raycasting (vía `TouchCatcher`), navegación del robot (vía `RobotARNavigator`) y emisión de los eventos `OnRobotCaught` y `OnGameWon`.
* **El módulo FSM:** Escucha los eventos en silencio y se encarga de pausar/reanudar el desgaste, aplicar la matemática de recompensa (+50/−40/−40) y comunicar el resultado al LLM.

---
> **📌 POLÍTICA DE DOCUMENTACIÓN VIVA**
> Cualquier futuro cambio en los umbrales de la FSM, la estructura del enum `RobotState` o los costos matemáticos de las acciones debe ser registrado en este mismo documento para mantener un historial centralizado de la lógica de negocio.
