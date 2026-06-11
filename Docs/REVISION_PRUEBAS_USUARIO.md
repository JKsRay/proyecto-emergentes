# Revisión de Pruebas de Usuario — Hallazgos Pendientes

> Fecha: 10 de junio 2026
> Archivo revisado: `Docs/user_studies/PRUEBAS_USUARIO.md`
> Contrastado contra: código real (`RobotStateManager.cs`, `MinigameManager.cs`, `RobotARNavigator.cs`, `PlaceObjectOnPlane.cs`), enunciado Entrega 3, forms reales de Pre/Post-Test, e `INFO_IMPORTANTE_PREANDPOST.md`.

---

## 🔴 Problemas de coherencia con la aplicación real

### 1. Escenario 2 — Mantenimiento NO tiene desgaste pasivo (Error factual)

**Ubicación:** `PRUEBAS_USUARIO.md`, línea 33.

**Texto actual:**
> "Observar cómo decaen pasivamente las estadísticas de **Energía** o **Mantenimiento** del robot a lo largo del tiempo."

**Realidad del código (`RobotStateManager.cs`):**
- Energía: −2 puntos cada 20s ✅
- Felicidad: −2 puntos cada 10s ✅
- Mantenimiento: **sin desgaste pasivo**. Solo se degrada por acciones (Botón Jugar: −30, Ganar minijuego: −30).

**Impacto:** El moderador y el usuario van a esperar un decaimiento que nunca ocurre. El escenario debe referirse solo a Energía y Felicidad.

---

### 2. Escenario 2 — Imposible observar cambio de estado en la ventana de prueba

**Ubicación:** `PRUEBAS_USUARIO.md`, Escenario 2 completo.

**Realidad:** Para que un usuario vea un cambio de estado orgánico:
- Batería Crítica (≤30): partiendo de 100, se necesitan 70 puntos / 2 = 35 ticks × 20s = **~12 minutos** solo de decaimiento de energía.
- Descalibrado (≤30): el Mantenimiento no decae pasivamente. Requiere **3-4 partidas del minijuego** (−30 por partida) para bajar de 100 a ≤30.
- Aburrido (≤30): partiendo de 100, 35 ticks × 10s = **~6 minutos**, pero el minijuego otorga +50 al ganar, revirtiendo el progreso.

**Impacto:** En una sesión de 10-15 min totales, el usuario jamás verá un cambio de estado sin intervención del moderador (forzar stats bajas de antemano o pedirle que juegue reiteradamente).

---

### 3. Escenario 3 — No menciona la cuenta regresiva ni el telegraphing

**Ubicación:** `PRUEBAS_USUARIO.md`, líneas 42-53.

**Realidad del código (`MinigameManager.cs`, `RobotARNavigator.cs`):**
- Al presionar "Jugar", hay una cuenta regresiva de 5 segundos: "4, 3, 2, 1, ¡YA!"
- Luego el robot espera 4s adicionales de telegraphing (`yield return new WaitForSeconds(4f)`) antes de moverse.
- Total: **~9 segundos** entre que el usuario presiona el botón y el robot empieza a huir.

**Impacto:** Si el escenario no lo menciona, el usuario puede pensar que la app no funciona o que se colgó. Debe advertirse como comportamiento esperado.

---

### 4. RF-12 (Reubicar Mascota) no está cubierto por ningún escenario

**Fuente:** Paper del proyecto (`Proyecto_Emergentes.md`), requisitos funcionales RF-01 a RF-12.

**Realidad del código:** Existe el método `UnlockPlacement()` en `PlaceObjectOnPlane.cs`, pero **no está conectado a ningún botón de la UI**. El usuario no tiene forma de reubicar al robot después de colocado.

**Impacto:** De los 12 RF comprometidos, los escenarios cubren 11. El RF-12 queda sin validar. Opciones: (a) implementar el botón de reubicación antes de las pruebas, o (b) documentarlo como fuera de alcance y eliminar el RF-12 de los compromisos.

---

### 5. El orden de escenarios anula el propósito del Escenario 2

**Ubicación:** Flujo secuencial E1 → E2 → E3 definido en 5.5, línea 141.

**Problema:** El Escenario 3 (minijuego) otorga +50 Felicidad y −15 Energía al ganar. Si se ejecuta después del Escenario 2:
- La Felicidad que decayó en E2 se anula por completo.
- El usuario nunca experimenta el estado Aburrido.
- Las stats quedan artificialmente altas al final.

**Sugerencia:** Invertir el orden a E1 → E3 → E2, para que la fatiga post-minijuego sea visible al revisar las necesidades, o advertir al moderador que fuerce stats bajas para el Escenario 2.

---

### 6. El juego bloquea el inicio si el robot está en BateriaCritica o Descalibrado — no se prueba

**Realidad del código (`MinigameManager.cs`, líneas 77-85):**
```csharp
if (estado == RobotStateManager.RobotState.BateriaCritica || 
    estado == RobotStateManager.RobotState.Descalibrado)
{
    return; // Se cancela el inicio del juego visual de AR.
}
```

**Impacto:** Hay un flujo completo no cubierto: "intentar jugar → ser rechazado → atender al robot → volver a intentar". Este es uno de los bucles de juego más interesantes y ningún escenario lo prueba. Sería un cuarto escenario o una variante del Escenario 3.

---

## 🟡 Inconsistencia de instrumentos de medición

### 7. PRUEBAS_USUARIO.md dice SUS + UEQ, pero los forms reales miden 5 dimensiones híbridas

**Ubicación:** Secciones 5.2, 5.3 y 5.4 completas.

| Lo que dice el documento | Lo que miden los forms reales (`Pre_test.md`, `Post_test.md`) |
|---|---|
| Parte 1: SUS (10 ítems) | Usabilidad (10 ítems tipo SUS, Likert 1-5) |
| Parte 2: UEQ (6 dimensiones) | Intención de uso y valor percibido (5 ítems, Likert 1-7) |
| Parte 3: Percepción LLM | Competencia percibida (5 ítems, Likert 1-7) |
| — | Autonomía e interacción (5 ítems, Likert 1-7) |
| — | Inmersión e involucramiento (5 ítems, Likert 1-7) |
| — | Percepción de la LLM (ítems cuantitativos + Parte 4 cualitativa) |

**Nota metodológica:** El docente ya advirtió que SUS+UEQ juntos son redundantes (`INFO_IMPORTANTE`, línea 43). Las 5 dimensiones implementadas (Usabilidad, Intención de uso, Competencia, Autonomía, Inmersión) mezclan ítems de TAM, IMI y HMSAM, lo cual es correcto y está alineado con la recomendación docente. Solo falta reflejarlo en el documento.

**Secciones que necesitan reescritura:** 5.2 (Métricas a Medir), 5.3 (Cuestionario Pre-Prueba, punto 3), y 5.4 (Cuestionario Post-Prueba, Partes 1-3).

---

## 🔴 Faltantes respecto al Enunciado Entrega 3

### 8. Punto 6: no se mencionan "propuestas de mejora"

**Ubicación:** Sección 5.8 (Análisis Estadístico de Resultados).

**El enunciado exige:**
> "Realizar un análisis de los resultados (ej., comparación de medias entre efectos) **y propuestas de mejora**."

**Estado actual:** La sección 5.8 describe medias, desviaciones, comparación Pre vs Post y análisis cualitativo, pero **no menciona derivar propuestas de mejora concretas** a partir de los resultados.

**Sugerencia:** Agregar un punto explícito en 5.8: "A partir de los hallazgos cuantitativos y cualitativos, se redactarán propuestas de mejora priorizadas para cada dimensión evaluada, orientadas a guiar futuras iteraciones del prototipo."

---

### 9. Punto 8: no se mencionan "datos de interacción" en el RAW DATA

**Ubicación:** Sección 5.8, primer bullet.

**El enunciado exige:**
> "Los datos del excel DEBEN ser detallados (RAW DATA), especificando por sujeto cada respuesta a cada ítem de los cuestionarios, **datos de interacción** y el identificador de cada sujeto (ej. correo)."

**Estado actual:** Solo se mencionan "respuestas individuales a las preguntas Pre y Post" y "datos demográficos". No se mencionan datos de interacción como:
- Número de capturas por partida
- Tiempo para completar el minijuego
- Cantidad de mensajes enviados al chat
- Acciones de cuidado ejecutadas (recargas, mantenimientos)
- Estados FSM alcanzados durante la sesión

**Nota:** Esto requiere logging en el código de Unity para capturar estas métricas durante la sesión. Si no está implementado, hay que hacerlo antes de las pruebas o documentar la limitación.

---

## ✅ Lo que SÍ cumple correctamente

| Requisito | Estado |
|---|---|
| 40 participantes mínimo | ✅ |
| Máximo 50% compañeros de asignatura (≤20) | ✅ |
| Identificación por correo electrónico | ✅ |
| Pre-test + Post-test con tiempos verbales correctos | ✅ |
| Thinking Aloud | ✅ |
| Consentimiento informado | ✅ |
| Evidencia visual (fotos/video de perfil o espaldas) | ✅ |
| 2 pruebas piloto previas | ✅ |
| Duración estimada realista (20-25 min) | ✅ |
| 5 dimensiones × 5 ítems mínimo (30 ítems total con SUS-like) | ✅ |
| Preguntas abiertas: 1 por dimensión + 1 extra | ✅ |
| Aleatorización en Google Forms sin títulos de dimensión | ✅ |
| Escala Likert apropiada por sección | ✅ |

---

## Resumen de prioridades

| # | Hallazgo | Severidad | Tipo |
|---|---|---|---|
| 1 | Mantenimiento sin desgaste pasivo | 🔴 Crítico | Error factual |
| 7 | Instrumentos SUS+UEQ vs 5 dimensiones reales | 🔴 Crítico | Documentación desactualizada |
| 8 | Faltan "propuestas de mejora" | 🔴 Crítico | Falta enunciado |
| 2 | Imposible ver cambio de estado en ventana de prueba | 🟠 Alto | Diseño de escenario |
| 5 | Orden E1→E2→E3 anula propósito de E2 | 🟠 Alto | Diseño de escenario |
| 9 | Faltan "datos de interacción" en RAW DATA | 🟠 Alto | Falta enunciado + código |
| 4 | RF-12 sin escenario ni UI | 🟡 Medio | Alcance |
| 6 | Rechazo de juego por estado crítico no se prueba | 🟡 Medio | Escenario faltante |
| 3 | Cuenta regresiva no documentada en E3 | 🟡 Medio | Documentación incompleta |
