# Optimización de Rendimiento en Módulos de Realidad Aumentada (AR)

Este documento detalla los cambios de optimización realizados en los scripts `PlaceObjectOnPlane.cs` y `RobotARNavigator.cs`, explicando las razones técnicas de estas modificaciones y cómo ayudan a mejorar la fluidez y rendimiento del juego en dispositivos móviles Android.

---

## 1. Contexto de Optimización Móvil

Al ejecutar un juego de realidad aumentada (AR) junto con un modelo de lenguaje (LLM) local en el mismo procesador del teléfono móvil, la CPU experimenta una carga de trabajo muy alta. Para evitar tirones en la pantalla (micro-stutters), sobrecalentamiento y un consumo acelerado de batería, es fundamental eliminar cualquier procesamiento redundante en el ciclo principal de actualización (`Update()`) de Unity y reducir el uso de APIs pesadas del motor.

---

## 2. Optimización en `PlaceObjectOnPlane.cs`

### El Cambio Realizado:
*   Se añadieron las variables miembro `_cachedTotalHorizontalArea` y `_cachedHorizontalPlanesCount` para almacenar en caché la información del entorno.
*   Se eliminó el bucle `foreach` que recorría todos los planos detectados en cada frame dentro de `UpdateOnboardingUI()`.
*   Se creó el método `RecalcularAreaPlanos()`, el cual se ejecuta únicamente cuando se dispara el evento `OnPlanesChanged` de AR Foundation (es decir, solo cuando se agrega, actualiza o remueve un plano en el entorno físico).
*   Los métodos `IsScanStable()` y `UpdateOnboardingUI()` ahora consultan directamente las variables en caché.

### ¿Por qué se hizo este cambio?
*   **Eliminación de Basura en Memoria (Garbage Collection):** Recorrer colecciones dinámicas de Unity como `_planeManager.trackables` usando un bucle `foreach` en el método `Update()` (aproximadamente 60 veces por segundo) genera pequeñas asignaciones de memoria temporales en el *Heap*. Con el paso de los segundos, estas asignaciones acumulan basura en memoria, lo que obliga al recolector de basura (*Garbage Collector*) de Unity a pausar brevemente el juego para limpiar, provocando congelamientos de décimas de segundo (tirones de FPS).
*   **Reducción de Uso de CPU:** Sumar las dimensiones de los planos es una matemática simple, pero realizarla en cada cuadro de renderizado para 10 o 20 planos detectados es un desperdicio de ciclos de CPU. Al calcularlo de forma reactiva (solo cuando hay cambios reales), el costo de CPU de este cálculo pasa a ser casi cero durante el flujo regular del juego.

---

## 3. Optimización en `RobotARNavigator.cs`

### El Cambio Realizado:
*   Se añadió un campo privado `planeManager` que almacena la referencia al componente `ARPlaneManager` de la escena.
*   Se inicializó esta referencia en el método `Awake()` mediante `Object.FindFirstObjectByType<ARPlaneManager>()` (y como un mecanismo de seguridad en `GetRandomPositionOnPlanes()` si llegase a ser nulo).
*   Se modificó el método `GetRandomPositionOnPlanes()` para que busque los planos iterando directamente sobre la lista en memoria `planeManager.trackables`, eliminando la línea:
    `ARPlane[] arPlanes = Object.FindObjectsByType<ARPlane>(FindObjectsSortMode.None);`

### ¿Por qué se hizo este cambio?
*   **Evitar la Búsqueda en el Árbol de Escena (Scene Graph Traversal):** La función `Object.FindObjectsByType<T>()` de Unity es una de las operaciones más costosas del motor. Obliga a Unity a recorrer la jerarquía de todos los GameObjects activos en la escena buscando componentes del tipo solicitado. Aunque este método se ejecutaba solo al calcular una nueva dirección de huida, en escenas con jerarquías complejas representa un pico de uso de CPU que puede congelar la pantalla en el momento en que el robot empieza a correr.
*   **Acceso Directo en Memoria O(1):** Dado que AR Foundation ya mantiene una lista interna optimizada de todos los planos activos dentro del componente `ARPlaneManager`, es infinitamente más eficiente consultar esa lista directamente en memoria en lugar de pedirle a Unity que busque los planos en toda la escena.

---

## 4. Resumen de Beneficios
*   **Tasa de Cuadros por Segundo (FPS) más Estable:** Menos micro-stutters durante la fase de escaneo y colocación del robot en AR.
*   **Menor Carga de CPU:** Liberamos ciclos de procesamiento que ahora pueden ser utilizados de manera limpia por la inferencia en tiempo real del LLM.
*   **Mayor Eficiencia Energética:** Menos recalentamiento del dispositivo y mayor duración de batería durante las pruebas de usuario en la universidad.
