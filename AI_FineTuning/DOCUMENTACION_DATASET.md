# Documentación del Pipeline de Generación de Dataset ChatML
## Fine-Tuning de Mascota Virtual AR (Robot "Rob")

---

## 1. Objetivo del Pipeline

Crear un dataset sintético de **1,000 filas** en formato `.jsonl` bajo el estándar **ChatML** para realizar Fine-Tuning local (via Unsloth / LoRA) sobre el modelo **Qwen3.5-0.8B-GGUF**, que correrá de forma nativa en un dispositivo Android (Samsung Galaxy A55 5G) mediante `llama.cpp`.

El modelo entrenado debe adoptar la personalidad de **"Rob"**: un robot sarcástico, directo, con modismos chilenos sutiles y family-friendly, que reacciona de forma diferente según su estado interno (FSM) y los eventos que ocurran en el juego AR.

---

## 2. Estructura de Carpetas

```
AI_FineTuning/
├── 1_RawData/
│   └── dataset.jsonl          # Dataset crudo (1000 filas, salida directa de Gemini)
├── 2_Scripts/
│   ├── .env                   # API Key de Gemini (ignorado por .gitignore)
│   ├── requirements.txt       # Dependencias: google-genai, pydantic, python-dotenv
│   ├── generador_dataset.py   # Script de generación via API de Gemini
│   └── procesar_dataset.py    # Script de procesamiento (shuffle + split)
├── 3_Processed/
│   ├── train.jsonl            # 850 filas (85%) - Entrenamiento
│   ├── val.jsonl              # 100 filas (10%) - Validación
│   └── test.jsonl             #  50 filas  (5%) - Prueba final
├── 4_Prompts/                 # Plantillas de referencia y guía de Colab
│   ├── INSTRUCCIONES_COLAB.md # Guía para cargar el dataset en Unsloth
│   └── system_prompts_referencia.md # System prompts exactos para Unity
└── DOCUMENTACION_DATASET.md   # Este archivo
```

---

## 3. Fase 1: Planificación y Diseño

### 3.1 Estrategia de Generación

Se diseñó una **matriz combinatoria** de:

| Dimensión | Cantidad | Detalle |
|-----------|----------|---------|
| Estados FSM | 5 | BateriaCritica, Descalibrado, Aburrido, Euforico, Normal |
| Eventos del Juego | 10 | Apertura App, AFK, Toque, Minijuego, Captura AR, etc. |
| Ejemplos por combinación | 20 | Generados en un solo batch por petición |
| **Total** | **1,000** | 5 × 10 × 20 = 1,000 filas |

### 3.2 Estados FSM del Robot

Los 5 estados están definidos en el código Unity (`RobotStateManager.cs`) y determinan la personalidad activa del robot:

| Estado | Condición de Activación | Comportamiento |
|--------|------------------------|----------------|
| **BateriaCritica** | Energía ≤ 20 | Exhausto, niega interacciones, exige cargador |
| **Descalibrado** | Mantenimiento ≤ 20 | Mañoso, quejumbroso, exige reparación |
| **Aburrido** | Felicidad ≤ 30 | Apático, pasivo-agresivo, pide atención |
| **Euforico** | Felicidad ≥ 80 | Animado, colaborativo, sarcasmo positivo |
| **Normal** | Estado por defecto | Observador, directo, sarcasmo estándar |

> **Nota sobre calibración:** Originalmente el dataset se generó usando "Felicidad al 95%" para el estado Eufórico, pero tras una revisión de Game Design con el usuario, se decidió mediante un script masivo de reemplazo (`python -c "..."`) ajustar este valor al **80%**. Esto asegura que el estado eufórico sea más accesible para el jugador, sincronizando perfectamente la lógica del dataset con la del script `RobotStateManager.cs`.

### 3.3 Eventos del Juego

Los 10 eventos representan las acciones que el usuario puede realizar en la app:

| # | Evento | Tipo de Input del Usuario |
|---|--------|--------------------------|
| 1 | Apertura de App | Acción entre asteriscos |
| 2 | AFK / Idle | Acción silenciosa |
| 3 | Toque en Pantalla | Interacción física |
| 4 | Inicio Minijuego | Acción de sistema |
| 5 | Captura AR (OnRobotCaught) | Acción en AR |
| 6 | Fin Minijuego | Acción de sistema |
| 7 | Rechazo por Batería/Mantenimiento | Acción con error |
| 8 | Petición de Estado | Texto hablado |
| 9 | Interacción Casual | Texto hablado corto |
| 10 | Intento de Jailbreak | Texto malicioso |

> **Nota importante:** Los eventos 1-7 incluyen instrucciones de formato explícitas para que Gemini genere el mensaje del usuario como una acción entre asteriscos. Los eventos 8-10 generan texto hablado directo. Esto enseña al modelo a distinguir entre acciones físicas y texto del usuario.

### 3.4 Consideraciones sobre Alcance vs. Código Unity Actual

Al momento de generar el dataset, se verificó que el código Unity solo inyecta prompts al LLM en 4 situaciones:
- Botón Recargar Batería
- Botón Mantenimiento
- Botón Jugar (y su rechazo)
- Chat directo del usuario

Eventos como AFK/Idle, Toque en Pantalla, Captura AR y Fin Minijuego **aún no están implementados en Unity**, pero fueron incluidos deliberadamente en el dataset como **visión de futuro**. El modelo quedará preparado para responder a estos estímulos cuando se conecten en el código C#.

---

## 4. Fase 2: Script de Generación (`generador_dataset.py`)

### 4.1 Modelo de IA Utilizado

| Propiedad | Valor |
|-----------|-------|
| Proveedor | Google AI (Gemini) |
| Modelo final | `gemini-3.1-flash-lite` |
| SDK | `google-genai` (SDK oficial de Python) |
| Structured Output | Sí, via Pydantic + `response_mime_type: application/json` |

**Historial de modelos probados:**
1. `gemini-3-flash-preview` → Rechazado (límite de 20 requests/día en capa gratuita)
2. `gemini-2.0-flash` → Funcional pero con cuota ajustada
3. `gemini-3.1-flash-lite` → **Modelo final**, sin problemas de cuota

### 4.2 Arquitectura del Script

```
generador_dataset.py
│
├── Modelos Pydantic (Structured Output)
│   ├── Message (role + content)
│   ├── ChatMLExample (3 mensajes)
│   └── BatchResponse (lista de ejemplos)
│
├── Diccionarios de Configuración
│   ├── ESTADOS_FSM (5 estados con system_prompt enriquecido)
│   └── EVENTOS_JUEGO (10 eventos con descripción + notas de formato)
│
├── META_SYSTEM_PROMPT
│   └── Instrucciones maestras para Gemini sobre personalidad,
│       tono, chilenismos permitidos y reglas de seguridad
│
├── generar_lote() → Función con reintentos (429 + 503)
│
└── main() → Bucle doble (5 estados × 10 eventos)
    └── Smart Resume: salta lotes ya generados
```

### 4.3 Mecanismos de Robustez

| Mecanismo | Detalle |
|-----------|---------|
| **Reintentos automáticos** | Hasta 5 intentos por error 429 (Rate Limit) o 503 (High Demand), con pausa de 15 segundos |
| **Smart Resume** | Cuenta las filas existentes en el JSONL y salta los lotes ya completados al reiniciar |
| **Modo Append** | Escribe en modo `"a"` para nunca sobrescribir datos previos |
| **Flush en prints** | `flush=True` para monitoreo en tiempo real desde terminales Windows |
| **Sleep entre peticiones** | 5 segundos entre cada request para respetar el límite de 15 RPM |

### 4.4 Seguridad de la API Key

La API Key **no está hardcodeada** en el script. Se utiliza `python-dotenv` para cargarla desde un archivo `.env` local que está incluido en `.gitignore`:

```python
from dotenv import load_dotenv
load_dotenv()
GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")
```

---

## 5. Fase 3: Script de Procesamiento (`procesar_dataset.py`)

### 5.1 Funcionalidad

El script toma el dataset crudo de `1_RawData/dataset.jsonl` y produce tres archivos listos para entrenamiento:

| Archivo | Porcentaje | Filas | Propósito |
|---------|-----------|-------|-----------|
| `train.jsonl` | 85% | 850 | Material de entrenamiento principal |
| `val.jsonl` | 10% | 100 | Evaluación durante el entrenamiento (evitar overfitting) |
| `test.jsonl` | 5% | 50 | Evaluación final post-entrenamiento |

### 5.2 Procesamiento Realizado

1. **Lectura con validación estructural**: Cada fila se verifica para confirmar que tenga exactamente 3 mensajes con roles `system`, `user`, `assistant` en ese orden.
2. **Shuffle determinista**: Semilla fija `random.seed(42)` para reproducibilidad. El mismo dataset crudo siempre genera los mismos splits.
3. **Split por porcentajes**: División en Train/Val/Test según los ratios configurados.
4. **Reporte de distribución**: Análisis automático de cuántas filas de cada estado FSM cayeron en cada split.

### 5.3 Resultado de la Distribución Final

```
Distribución en Train (850 filas):
  Aburrido          :  212 filas
  BateriaCritica    :   66 filas
  Descalibrado      :  133 filas
  Euforico          :  146 filas
  Normal            :  190 filas
  otro              :  103 filas

Distribución en Val (100 filas):
  Aburrido          :   22 filas
  BateriaCritica    :    6 filas
  Descalibrado      :   18 filas
  Euforico          :   20 filas
  Normal            :   23 filas
  otro              :   11 filas

Distribución en Test (50 filas):
  Aburrido          :    6 filas
  BateriaCritica    :    8 filas
  Descalibrado      :    9 filas
  Euforico          :   14 filas
  Normal            :    7 filas
  otro              :    6 filas
```

> **Nota sobre la categoría "otro":** Algunas filas generadas por Gemini usan system prompts con redacción ligeramente diferente a las plantillas maestras (ej. "Felicidad 20%. Robot aburrido" en vez del prompt completo). El clasificador heurístico no las reconoce por keywords exactas, pero **su contenido es correcto** y el modelo las aprenderá sin problemas.

---

## 6. Formato del Dataset (ChatML)

Cada fila del dataset sigue este esquema JSON exacto:

```json
{
  "messages": [
    {
      "role": "system",
      "content": "[SISTEMA: Descripción del estado FSM y personalidad activa]"
    },
    {
      "role": "user",
      "content": "Texto del usuario o *acción entre asteriscos*"
    },
    {
      "role": "assistant",
      "content": "Respuesta de Rob en español chileno sarcástico"
    }
  ]
}
```

Este formato es compatible con:
- **Unsloth**: Usa `get_chat_template("chatml")` para convertirlo a tokens `<|im_start|>` / `<|im_end|>`.
- **HuggingFace TRL**: `SFTTrainer` acepta este formato directamente.
- **llama.cpp**: En Android, se debe replicar la misma estructura ChatML en la inferencia.

---

## 7. Dependencias

```
google-genai>=1.0.0      # SDK oficial de Google para Gemini
pydantic>=2.0.0           # Structured Outputs (validación de esquema JSON)
python-dotenv>=1.0.0      # Carga segura de API Key desde .env
```

Instalación: `pip install -r AI_FineTuning/2_Scripts/requirements.txt`

---

## 8. Reglas de Personalidad Entrenadas

El dataset enseña al modelo las siguientes reglas de forma implícita:

| Regla | Ejemplo en Dataset |
|-------|-------------------|
| Chilenismos sutiles | "bacán", "cachai", "al tiro", "filete", "fome" |
| Prohibición de vulgaridades | Nunca aparece "weón", "ctm", "ql" |
| Consciencia de hardware | "Mis servomotores están vibrando", "Mi CPU está al máximo" |
| Rechazo de jailbreak | "No gasto ciclos de CPU en debates políticos" |
| Diferenciación acción vs. texto | Respuestas diferentes a `*toca la pantalla*` vs. `"Hola"` |

---

## 9. Próximos Pasos (Fase 3: Entrenamiento)

1. **Entorno de Colab**: Subir `train.jsonl` y `val.jsonl` a Google Colab.
2. **Configuración de Unsloth**: Seguir las instrucciones detalladas en `4_Prompts/INSTRUCCIONES_COLAB.md` para cargar correctamente el dataset JSONL, aplicar la plantilla `chatml` y ejecutar el SFTTrainer con Qwen3.5-0.8B.
3. **Exportación GGUF**: Convertir el modelo entrenado a formato GGUF cuantizado (Q4_0 o q4_k_m) compatible con `llama.cpp` en Android.
4. **Integración en Unity**: 
   - Conectar los eventos faltantes (AFK, Toque, Captura AR, Fin Minijuego) al `ChatController`.
   - Copiar los prompts oficiales desde `4_Prompts/system_prompts_referencia.md` al `RobotStateManager.cs` para asegurar la paridad exacta entre el entrenamiento y la inferencia.
