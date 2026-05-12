# Documentación de Arquitectura y Refactorización
**Proyecto:** Mascota Virtual AR
**Módulo:** Core Architecture, FSM, AR Minigames
**Fecha:** Mayo 2026

---

## 1. Refactorización de la Máquina de Estados Finitos (FSM)

### El Problema Anterior
En iteraciones previas del desarrollo, la comunicación del estado interno del robot (Energía, Mantenimiento, Felicidad) al modelo de lenguaje (LLM) se realizaba transmitiendo variables flotantes crudas. Este enfoque presentaba múltiples deficiencias:
- **Ineficiencia en la Inferencia:** El modelo requería procesar y evaluar contextualmente números continuos (ej. `felicidad = 34.5f`, `energia = 12.0f`), lo que incrementaba la carga cognitiva y propiciaba respuestas inconsistentes o alucinaciones.
- **Acoplamiento Débil:** No existía una definición formal del comportamiento de la mascota según sus métricas, requiriendo que la lógica del sistema dependiera de condiciones improvisadas.

### La Solución: FSM Jerárquica y Discreta
Se implementó una Máquina de Estados Finitos (FSM) encapsulada en la propiedad `CurrentState` de `RobotStateManager`. En lugar de evaluar rangos continuos constantemente, el sistema clasifica el estado global de la mascota en 5 estados discretos y jerárquicos, priorizados según la criticidad de la métrica:

```csharp
public enum RobotState 
{ 
    BateriaCritica, // Prioridad 1: Energía <= 20
    SucioGrunon,    // Prioridad 2: Mantenimiento <= 20
    Aburrido,       // Prioridad 3: Felicidad <= 30
    Euforico,       // Prioridad 4: Felicidad >= 80
    Normal          // Default
}
```

### Inyección de Contexto (Llama 3.2 Local)
El `RobotStateManager` actúa como un orquestador semántico. Al detectar una interacción o cambio de estado, traduce la evaluación discreta de la FSM en un **System Prompt** estructurado que inyecta contexto en el modelo local (Llama 3.2). El usuario interactúa de forma natural, pero en segundo plano (y de manera invisible) se anteponen directrices contextuales mediante el método `CrearContexto`:

```text
[SISTEMA: Batería crítica. Estás exhausto, niegas interactuar y exiges un cargador.]
```
Esto fuerza al modelo a interpretar un "rol" estricto antes de generar la respuesta, resultando en comportamientos deterministas, coherentes con las mecánicas del juego y de alto rendimiento al evitar ambigüedades.

---

## 2. Integración del Minijuego AR (Módulo Externo)

La reciente integración fusionó el módulo desarrollado por el equipo secundario (referido conceptualmente como las mecánicas de *Hide and Seek AR*), consolidado en los scripts `MinigameManager`, `RobotARNavigator` y `TouchCatcher`.

### Mecánica Exacta Implementada
1. **Navegación Táctica (Evasión):** El script `RobotARNavigator` evalúa los planos AR detectados (`ARPlane`) y calcula posiciones aleatorias sobre la superficie física (`PlaneAlignment.HorizontalUp`). El robot se desplaza interpolando su posición y rotando permanentemente (Slerp) hacia la cámara del usuario para generar engagement visual.
2. **Sistema de Captura (Raycasting):** A través del script `TouchCatcher`, el input del usuario (Touch) lanza un rayo desde la cámara origin en espacio de mundo. Si el rayo colisiona con el Tag del robot o detecta el componente `RobotStateManager` en su jerarquía a una distancia umbral (`catchDistance`), la evasión se detiene.

### Contrato de Interfaz y Variables Encapsuladas
Para asegurar la cohesión de los sistemas sin romper la FSM, la arquitectura de los estados expone propiedades de solo lectura:
```csharp
public float Energia { get; private set; }
public float Felicidad { get; private set; }
```
- **Validación Cíclica:** El minijuego lee estos getters para validar si el robot está en condiciones operativas (no iniciará si el estado es `BateriaCritica`).
- **Escucha de Eventos (Observer):** La integración es asíncrona y reactiva. El gestor de estados escucha las notificaciones estáticas (`Action`) disparadas por `MinigameManager.OnRobotCaught`. Al consumarse la captura, el estado interno se muta, incrementando la Felicidad y deduciendo la Energía de forma centralizada sin requerir que los scripts de AR conozcan la lógica interna de la mascota.

> [!WARNING]
> **Disclaimer:** La implementación actual de la mecánica del minijuego AR constituye una "Versión 1" (MVP). Está sujeta a refinamiento iterativo, particularmente en la optimización del Raycasting (implementación de LayerMasks) y suavizado de los algoritmos de evasión (`NavMeshSurface` para AR).

---

## 3. Refactorización Arquitectónica y Buenas Prácticas en Unity

### Desacoplamiento del Canvas
Originalmente, `RobotStateManager` y `RobotUIManager` residían en el Canvas. Arquitectónicamente, un gestor de estados que define la lógica de negocio de un NPC no debe depender del árbol de renderizado de la UI.
- **La Acción:** El `RobotStateManager` fue migrado a la raíz del Prefab dinámico del robot (`Rob11`).
- **El Beneficio:** Se elimina el anti-patrón de referenciar GameObjects de mundo desde la interfaz. La UI actúa únicamente como capa de presentación, respetando el principio de Responsabilidad Única (SRP).

### Patrón Singleton en Entornos Dinámicos (AR)
El robot es instanciado en tiempo de ejecución por `PlaceObjectOnPlane` cuando los sensores espaciales de AR Foundation detectan una superficie válida. Al instanciarse, el script inicializa su propia referencia global:

```csharp
public static RobotStateManager Instance { get; private set; }
```
Dado que por diseño del sistema AR solo existe un clon activo de la mascota a la vez, el patrón Singleton facilita que cualquier sistema del juego (Minijuego, UI, Eventos) descubra al robot en _Runtime_ con un costo de CPU de `O(1)`, obviando la necesidad de costosas llamadas como `FindObjectOfType`.

### Preservación de Eventos y Trabajo Colaborativo (Git)
Uno de los mayores focos de conflictos (Merge Conflicts) en repositorios de Unity son las colisiones de serialización de `.prefab` o `.unity` en los arrays de eventos de interfaz gráfica (UnityEvents). 

Para evitar sobreescribir la integración en el Inspector hecha por la compañera (quien asignó `StartGame()` en el botón del Inspector):
- Las referencias de interfaz (`onClick.AddListener()`) para el `RobotUIManager` se inyectan estricta y explícitamente mediante código C# (runtime).
- **El Beneficio:** Unity permite la coexistencia de UnityEvents (Inspector) y Listeners C# delegados simultáneamente. Esto garantiza una fusión de Git completamente limpia entre programadores sin destruir el flujo de trabajo del diseñador.

### Higiene del Proyecto y Metadatos
El archivo principal de escena temporal ("Cubo") fue renombrado directamente desde el explorador del Editor de Unity hacia su nomenclatura definitiva de producción (ej. `AR_MainScene`).
Este protocolo de refactorización "In-Editor" preserva la integridad del archivo `AR_MainScene.unity.meta`. Mantener sincronizados los GUIDs subyacentes es crítico en Unity, ya que previene la corrupción masiva de dependencias, evitando que Build Settings o prefabs referenciados reporten "Missing Reference Exceptions".
