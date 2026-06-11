# 5. Diseño de Pruebas de Usuario

Esta sección detalla el plan de evaluación con usuarios para el prototipo **RoboPet**. Las pruebas de usuario contarán con una muestra de **40 individuos** e incluirán tres etapas secuenciales: un **pre-test**, la **prueba de interacción** con el prototipo, y un **post-test** para evaluar las métricas seleccionadas. Se utilizará la técnica **"Thinking Aloud" (Pensar en voz alta)** para indicar a los usuarios que verbalicen sus pensamientos sobre el producto mientras lo utilizan activamente, sin que el moderador intervenga salvo en casos de bloqueo crítico.

---

## 5.1. Escenarios de Prueba

Se han definido **3 escenarios principales** que buscan cubrir el ciclo completo de interacción con el prototipo, abordando sus tres subsistemas centrales: el entorno de Realidad Aumentada (AR), la Máquina de Estados Finitos (FSM) y el motor conversacional (LLM).

---

### Escenario 1: Onboarding y Descubrimiento (AR + LLM)

**Objetivo:** Evaluar la facilidad con la que un usuario nuevo logra posicionar al robot en el mundo real y completar su primera interacción conversacional con el sistema.

**Tarea del usuario:**
1. Abrir la aplicación móvil.
2. Seguir las instrucciones del flujo de inducción ("Onboarding") que aparecen en pantalla.
3. Escanear el entorno físico con la cámara del dispositivo hasta detectar una superficie plana (AR Plane).
4. Colocar el modelo 3D del robot sobre dicha superficie plana anclándolo al punto deseado.
5. Enviar un mensaje inicial de saludo a la mascota a través de la interfaz de chat (LLM) y observar la respuesta generada.

**Subsistemas evaluados:** ARCore (escaneo y anclaje), LLMUnity/llama.cpp (generación de respuesta), UI de Onboarding.

---

### Escenario 2: Gestión de Necesidades (FSM)

**Objetivo:** Evaluar la comprensión intuitiva del sistema de desgaste pasivo de estadísticas y la efectividad de la interfaz de botones de cuidado para modificar el estado interno del robot.

**Tarea del usuario:**
1. Observar cómo decaen pasivamente las estadísticas de **Energía** o **Mantenimiento** del robot a lo largo del tiempo.
2. Interactuar con los botones de **"Recargar Batería"** y **"Realizar Mantenimiento"** disponibles en la interfaz de control (HUD).
3. Observar y describir cómo cambia la actitud del robot en el chat cuando se encuentra en los estados **"Descalibrado"** (Mantenimiento ≤ 30) o **"Batería Crítica"** (Energía ≤ 30).
4. Aplicar el cuidado correspondiente y observar cómo reacciona la mascota tras ser atendida (transición al estado **Normal** o **Euforico**).

**Subsistemas evaluados:** Máquina de Estados Finitos (FSM), HUD de estadísticas, lógica de desgaste pasivo, inyección de contexto al LLM.

---

### Escenario 3: Interacción Activa (Minijuego AR)

**Objetivo:** Probar el sistema de navegación autónoma del robot en el entorno AR, la mecánica de captura mediante toque en pantalla y el bucle de juego completo.

**Tarea del usuario:**
1. Presionar el botón **"Jugar"** en la interfaz principal para iniciar el minijuego.
2. Perseguir al modelo 3D del robot en el espacio físico detectado mientras este intenta "huir" mediante su algoritmo de evasión en zigzag (heurística de divergencia de vectores).
3. Interactuar físicamente con el robot tocando la pantalla en el momento en que el Raycast intersecta el hitbox del modelo.
4. Conseguir las **3 capturas necesarias** para finalizar el juego exitosamente.
5. Leer la reacción de la mascota en el chat una vez terminado el minijuego.

**Subsistemas evaluados:** ARNavigator (navegación y evasión), TouchCatcher (detección de toque), GameManager (lógica del bucle de juego), FSM (actualización de estadísticas al finalizar), LLM (respuesta de reacción post-juego).

---

## 5.2. Métricas a Medir

Se medirán las siguientes variables utilizando modelos y cuestionarios estandarizados y validados. La selección de instrumentos responde a la naturaleza multidimensional del prototipo, que combina interfaces táctiles, entorno de Realidad Aumentada y un agente conversacional.

### 5.2.1. Usabilidad — System Usability Scale (SUS)

El **SUS** es un instrumento estandarizado que proporciona una métrica global de usabilidad en una escala de **0 a 100 puntos**. Dado que el prototipo RoboPet integra simultáneamente tres tipos de interfaz —UI táctil (HUD), chat conversacional (LLM) y entorno AR—, es fundamental medir la percepción de complejidad global del sistema para identificar fricciones en la curva de aprendizaje.

- **Número de ítems:** 10 preguntas fijas e indivisibles (paquete estándar SUS). No se puede seleccionar un subconjunto de estas preguntas.
- **Escala de respuesta:** Likert de 5 puntos (Totalmente en desacuerdo → Totalmente de acuerdo).
- **Resultado:** Puntuación global de usabilidad (0–100) e indicador de intención de uso.

### 5.2.2. Experiencia de Usuario — User Experience Questionnaire (UEQ)

El **UEQ** es un instrumento que mide la experiencia de usuario en **6 dimensiones** distribuidas en 26 ítems de escala diferencial semántica. A diferencia del SUS, el UEQ permite segmentarse para seleccionar únicamente las dimensiones relevantes para el proyecto.

Las dimensiones de interés para RoboPet son:

| Dimensión | Relevancia para el Proyecto |
|---|---|
| **Atracción** | Impresión general positiva o negativa de la aplicación |
| **Eficiencia** | Percepción de fluidez y rapidez para completar tareas |
| **Claridad** | Facilidad para aprender a usar el sistema |
| **Control** | Sensación de dominio sobre las interacciones con la mascota |
| **Novedad / Originalidad** | Percepción del carácter innovador de la experiencia AR + LLM |

> **Nota metodológica:** Usar SUS y UEQ en conjunto puede resultar redundante, ya que ambos instrumentos abordan parcialmente la dimensión de usabilidad. Se evaluará la conveniencia de mantener ambos o priorizar uno según el alcance final del cuestionario post-prueba.

---

## 5.3. Cuestionario Pre-Prueba

El cuestionario pre-prueba tiene como objetivo recopilar datos demográficos básicos del participante y medir sus **expectativas previas** respecto al sistema, antes de cualquier interacción con el prototipo.

**Estructura:**

1. **Datos de identificación:** Correo electrónico (institucional o personal), edad, género y carrera/ocupación.
2. **Experiencia previa:** Preguntas sobre el nivel de familiaridad del usuario con aplicaciones de Realidad Aumentada, mascotas virtuales y chatbots/asistentes de IA.
3. **Expectativas (Pre-Test cuantitativo):** Las mismas dimensiones medidas en el post-test (SUS/UEQ), formuladas en **tiempo verbal futuro** para capturar la expectativa del usuario antes de usar el sistema (ej. "Creo que la aplicación *será* fácil de usar", "Pienso que la experiencia *será* entretenida").

> El cuestionario Pre-Prueba completo se puede visualizar en el siguiente enlace: **[Link Pre-Test]**

---

## 5.4. Cuestionario Post-Prueba

El cuestionario post-prueba se aplica **inmediatamente** después de que el usuario concluye los 3 escenarios de interacción. Se divide en tres partes:

**Parte 1 — SUS (Usabilidad):** 10 preguntas fijas que miden en una escala de 0 a 100 la usabilidad global del sistema, formuladas en **tiempo verbal pasado** (ej. "Sentí que el sistema *fue* fácil de usar").

**Parte 2 — UEQ (Experiencia de Usuario):** Ítems seleccionados de las dimensiones de Atracción, Eficiencia, Claridad, Control y Novedad, formulados en tiempo pasado para reflejar la experiencia vivida (ej. "La experiencia *fue* entretenida / aburrida").

**Parte 3 — Percepción de la LLM:** Preguntas cualitativas y cuantitativas enfocadas específicamente en evaluar la calidad percibida de las respuestas generadas por el modelo de lenguaje, incluyendo: coherencia de la personalidad sarcástica del robot, adecuación contextual de las respuestas al estado de la FSM, y fluidez conversacional percibida.

**Parte 4 — Preguntas Abiertas (Cualitativas):** Una pregunta abierta por cada dimensión, formuladas para explicar el *por qué* detrás de los datos cuantitativos. Se incluye una pregunta adicional para la dimensión de Inmersión (a decidir cuál queda final).

| # | Dimensión | Pregunta Abierta |
|---|---|---|
| 1 | Usabilidad | Si tuvieras que explicarle a un amigo cómo usar RoboPet en una sola frase, ¿qué le dirías? ¿Qué parte te costaría más explicar? |
| 2 | Intención de uso y valor percibido | ¿Sentís que interactuar con un robot en Realidad Aumentada te dio algo distinto a simplemente chatear con una IA en una app tradicional? Explicame la diferencia. |
| 3 | Competencia percibida | Si tuvieras que volver a usar RoboPet mañana, ¿qué harías distinto? ¿Hay algo que la primera vez no entendiste y ahora sí? |
| 4 | Autonomía e interacción | ¿Hubo algo que quisieras hacer con el robot y no encontraste cómo? ¿Qué habrías querido probar? |
| 5 | Inmersión e involucramiento | *(Opción A)* El robot tiene una personalidad sarcástica y responde distinto según su estado (contento, aburrido, sin batería). ¿Eso te motivó a querer provocar ciertas reacciones? ¿Intentaste algo para ver cómo respondía? |
| 6 | Inmersión e involucramiento | *(Opción B)* ¿Te generó curiosidad descubrir cómo reaccionaba el robot en situaciones no previstas? ¿Probaste alguna interacción fuera de los escenarios indicados? |

> El cuestionario Post-Prueba completo se puede visualizar en el siguiente enlace: **[Link Post-Test]**

---

## 5.5. Procedimiento de Aplicación de las Pruebas

Las pruebas se realizarán de forma **presencial** en un entorno seguro y controlado, adecuado para el uso de la aplicación AR (superficie plana disponible, iluminación suficiente para ARCore). La sesión completa tiene una **duración aproximada de 20 a 25 minutos** por participante.

El orden del procedimiento es el siguiente:

### Etapa 1: Bienvenida y Pre-Test — *5 minutos*

1. Recibimiento del participante y explicación del propósito general de la prueba, **sin revelar detalles técnicos** que puedan sesgar su percepción o expectativas.
2. Lectura y firma del **formulario de consentimiento informado**.
3. Llenado del **cuestionario pre-prueba** (datos demográficos, experiencia previa y expectativas cuantitativas).

### Etapa 2: Interacción con el Prototipo — *10 a 15 minutos*

1. El facilitador entrega el dispositivo móvil Android con la aplicación RoboPet activa y lista para usar.
2. El usuario ejecuta los **3 escenarios de prueba** de forma secuencial: Onboarding + AR (Escenario 1), Gestión de Necesidades FSM (Escenario 2) y Minijuego AR (Escenario 3).
3. Se aplica la técnica **"Thinking Aloud"**: se solicita al usuario que verbalice en voz alta todo lo que piensa, percibe y hace mientras interactúa con la aplicación. El moderador **no interviene** durante la ejecución, salvo en casos de bloqueo crítico que impida completar la tarea mínima.
4. Se registran observaciones del moderador sobre comportamientos, errores recurrentes y comentarios espontáneos del usuario.

### Etapa 3: Cuestionario Post-Prueba — *5 minutos*

1. Aplicación inmediata de los instrumentos de medición (SUS, UEQ y percepción de LLM) al finalizar la interacción con el prototipo.
2. Recopilación de evidencia visual: fotografía o captura breve (5 segundos) del usuario interactuando con la aplicación, preferentemente de perfil o de espaldas, para evidenciar la realización de la prueba.

---

## 5.6. Ejecución de Pruebas Piloto

Previo a la aplicación masiva con los 40 usuarios, se realizarán **2 pruebas piloto** con participantes externos al equipo de desarrollo. El objetivo es:

- Verificar que los **tiempos estimados** por etapa (5 + 10-15 + 5 minutos) son realistas y ajustados.
- Confirmar que la **redacción de los escenarios y cuestionarios** es clara e inequívoca para un usuario sin conocimientos técnicos del sistema.
- Detectar posibles **bloqueos o fricciones** no previstas en el flujo de la aplicación que impidan completar los escenarios correctamente.
- Refinar los detalles del protocolo (instrucciones del moderador, material de apoyo, condiciones del entorno físico) antes de su aplicación definitiva.

Los resultados y aprendizajes de las pruebas piloto retroalimentarán directamente el diseño de los escenarios, los cuestionarios y el protocolo del moderador, garantizando la coherencia y validez de los instrumentos antes de la ejecución con la muestra completa.

---

## 5.7. Requisitos de la Muestra de Usuarios

| Parámetro | Valor |
|---|---|
| **Tamaño mínimo de muestra** | 40 participantes |
| **Máximo de alumnos de la misma asignatura** | 50% (20 personas) |
| **Resto de la muestra** | Público general, familiares, estudiantes de otras carreras o universidades |
| **Método de identificación** | Correo electrónico (institucional o personal). No se utilizará RUN/RUT para resguardar la privacidad de datos personales. |
| **Modalidad** | Presencial |

> **Nota:** Se exige la entrega de **registros fotográficos y/o de video breves** (cortes de 5 segundos o capturas) que constaten la realización efectiva de las pruebas. No es necesario registrar la totalidad de las 40 sesiones de manera exhaustiva. Las capturas deben mostrar al usuario en interacción activa con la interfaz o el entorno AR (preferentemente de perfil o de espaldas).

---

## 5.8. Análisis Estadístico de Resultados

Una vez recolectados todos los datos, se realizará el siguiente análisis mínimo:

- **Datos en bruto (raw data):** Se adjuntará el archivo Excel exportado directamente desde la plataforma de encuestas (Google Forms). Las filas corresponderán a los 40 sujetos evaluados y las columnas desglosarán correos de identificación, datos demográficos y respuestas individuales a las preguntas Pre y Post.
- **Cálculo de medias y desviaciones estándar** del Pre y Post para cada una de las dimensiones evaluadas (SUS global + dimensiones UEQ seleccionadas).
- **Comparación Pre vs. Post** por dimensión para identificar el delta de percepción entre expectativas y experiencia real.
- **Análisis cualitativo** de las respuestas abiertas del Post-Test para complementar e interpretar los datos cuantitativos de cada dimensión.

Como análisis **opcionales y complementarios** que añadirán valor al informe final se contemplan: segmentaciones por género o rango etario (si los subgrupos superan los 10 participantes) y pruebas de significancia estadística (T-test para comparación Pre-Post por dimensión).
