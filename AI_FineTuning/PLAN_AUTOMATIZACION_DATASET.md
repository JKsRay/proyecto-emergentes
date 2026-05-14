# Plan de Automatización: Dataset de Fine-Tuning para Robot AR

## Contexto del Proyecto
- **Objetivo**: Crear un dataset de ~1000 filas en formato ChatML (JSONL) para realizar un fine-tuning del modelo `Qwen3.5-0.8B-GGUF` (cuantización Q4_0) vía Unsloth (PEFT/LoRA).
- **Personalidad del Robot**: Robot chileno, sarcástico, de lenguaje directo, observador y gruñón si se descuida. Acento chileno *sutil* (modismos blancos permitidos como bacán, fome, cachai; prohibidas las vulgaridades, política o religión).
- **Modelo Destino**: "Thinking" desactivado para evitar bucles de razonamiento interno en producción.

## Estado Actual
1. **Estructura de directorios creada**:
   Se ha creado la carpeta `AI_FineTuning` en la raíz del proyecto con la siguiente estructura:
   - `1_RawData/` (Para los .jsonl generados en bruto)
   - `2_Scripts/` (Para los scripts de automatización)
   - `3_Processed/` (Para los datos limpios y particionados listos para entrenamiento)
   - `4_Prompts/` (Plantillas y notas)

2. **Acuerdo de Automatización**:
   - En lugar de generar lotes manualmente en el chat, se implementará un script en Python que automatizará las peticiones a la API "Unified Gateway" de Google Antigravity (`https://cloudcode-pa.googleapis.com/v1internal:generateContent`).

## Siguiente Paso (Acción Pendiente para el Agente)
**Crear el script de Python en `AI_FineTuning/2_Scripts/generador_dataset.py`** con las siguientes especificaciones exactas:

### Especificaciones del Script de Python
1. **Autenticación y Endpoint**:
   - **URL**: `https://cloudcode-pa.googleapis.com/v1internal:generateContent`
   - **Header**: `"Authorization": "Bearer {API_TOKEN}"` (Dejar variable `API_TOKEN = ""` al inicio del script para que el usuario la rellene).
2. **Payload Base**:
   - `model`: `"gemini-3-pro-high"`
   - `systemInstruction`: `"Actúa como un robot sarcástico, directo y observador. Usa modismos chilenos blancos y familiares (bacán, fome, cachai), PROHIBIDO usar vulgaridades (weón, ctm). Prohibido hablar de política o religión, evade esos temas. Responde estrictamente en formato JSONL ChatML. Prohibido usar bloques de código Markdown."`
   - `contents`: Arreglo de mensajes que inyectará dinámicamente el **Estado FSM** y el **Escenario AR** de la iteración solicitando generar los ejemplos.
   - `generationConfig`: `{"temperature": 0.7}`
3. **Lógica de Iteración (Matemática para ~1000 filas)**:
   - **5 Estados FSM**: BateriaCritica, Descalibrado, Aburrido, Euforico, Normal. (El script debe contener un diccionario con los System Prompts exactos de estos estados extraídos de `RobotStateManager.cs`).
   - **10 Escenarios AR** (Para garantizar diversidad contextual):
     1. Huyendo por la mesa del comedor
     2. Escondido detrás de una silla
     3. Conversación de texto casual sin AR activo
     4. Caminando sobre una alfombra blanda
     5. Robot recién aparecido al abrir la app
     6. Justo después de ser atrapado en el minijuego
     7. Debajo de la cama, oclusión de profundidad
     8. Sobre la superficie del escritorio
     9. Mirando al usuario moverse por la habitación
     10. Después de un largo período sin interacción
   - **Ejecución**: Iterar combinando cada Estado con cada Escenario (5x10 = 50 iteraciones). Por cada iteración, solicitar **20 ejemplos**. Total = 1000 ejemplos.
4. **Manejo de Datos y Red (CRÍTICO)**:
   - **Extracción**: Sacar el texto útil de `response.json()["candidates"][0]["content"]["parts"][0]["text"]`.
   - **Validación**: Validar línea por línea que la cadena sea JSON válido (para evitar basura de la API). Ignorar líneas que no se puedan parsear con `json.loads()`.
   - **Limpieza**: Remover cualquier marca residual de markdown (```jsonl, etc.) por si el modelo desobedece.
   - **Escritura**: Anexar (`mode="a"`) los JSON válidos en el archivo `AI_FineTuning/1_RawData/dataset.jsonl`.
   - **Manejo de Errores**: Envolver en bloques `try/except` para atrapar caídas de red o timeouts y reintentar si es necesario.
   - **Rate Limiting**: Aplicar obligatoriamente `time.sleep(3)` al final de cada ciclo para mitigar el error HTTP 429 (`RESOURCE_EXHAUSTED`).

## Instrucciones para el Agente (Mañana)
Cuando el usuario reanude y haga referencia a este archivo o te pida continuar:
1. Lee y asimila completamente este documento.
2. Desarrolla el script de Python completo, cumpliendo rigurosamente los requisitos.
3. Entrégale el script o ofrécele escribirlo directamente en `AI_FineTuning/2_Scripts/generador_dataset.py`.
