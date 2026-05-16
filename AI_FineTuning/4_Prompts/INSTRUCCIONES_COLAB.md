# Guía de Fine-Tuning con Unsloth (Google Colab)

Esta guía detalla los pasos exactos para tomar nuestros archivos procesados e iniciar el entrenamiento del modelo `Qwen3.5-0.8B`.

## 1. Preparación de los Archivos
Debes tener a mano los archivos generados en la carpeta `3_Processed`:
- `train.jsonl`
- `val.jsonl`

*(El archivo `test.jsonl` no se sube a Colab, se guarda para probar el modelo después).*

## 2. Configurar el Notebook de Unsloth
1. Entra a Google Colab y abre el notebook oficial de Unsloth para ChatML (suele llamarse "Unsloth ChatML Notebook").
2. Asegúrate de estar usando un entorno con **GPU (T4 o superior)**. (Arriba a la derecha: Entorno de ejecución > Cambiar tipo de entorno > T4 GPU).

## 3. Subir el Dataset a Colab
1. En la barra lateral izquierda de Google Colab, haz clic en el icono de **Carpeta (Archivos)**.
2. Arrastra y suelta los archivos `train.jsonl` y `val.jsonl` en ese panel.
3. Espera a que el círculo de carga termine.

## 4. Modificar el Código de Carga (Paso Crítico)
En el bloque de código del Notebook donde Unsloth carga el dataset (usualmente usa `load_dataset`), debes modificarlo para que lea nuestros archivos locales.

Busca la celda similar a esta:
```python
from datasets import load_dataset
dataset = load_dataset("philschmid/guanaco-sharegpt-style", split = "train")
```

Y cámbiala por esto:
```python
from datasets import load_dataset

# Cargar nuestros archivos locales
dataset = load_dataset("json", data_files={"train": "train.jsonl", "test": "val.jsonl"})

# Obtener los splits
train_dataset = dataset["train"]
val_dataset = dataset["test"]
```

## 5. Aplicar la Plantilla ChatML
En la celda donde Unsloth aplica la plantilla, asegúrate de que esté configurado para `chatml`:

```python
from unsloth.chat_templates import get_chat_template

tokenizer = get_chat_template(
    tokenizer,
    chat_template = "chatml", # <--- IMPORTANTE
    mapping = {"role" : "role", "content" : "content", "user" : "user", "assistant" : "assistant"},
)

def formatting_prompts_func(examples):
    convos = examples["messages"]
    texts = [tokenizer.apply_chat_template(convo, tokenize = False, add_generation_prompt = False) for convo in convos]
    return { "text" : texts }

# Aplicamos el mapeo al dataset
train_dataset = train_dataset.map(formatting_prompts_func, batched = True)
val_dataset = val_dataset.map(formatting_prompts_func, batched = True)
```

## 6. Configurar el Entrenador (SFTTrainer)
Asegúrate de pasar `train_dataset` y `val_dataset` al entrenador.

```python
from trl import SFTTrainer
from transformers import TrainingArguments

trainer = SFTTrainer(
    model = model,
    tokenizer = tokenizer,
    train_dataset = train_dataset,
    eval_dataset = val_dataset,      # <--- Agregamos validación
    dataset_text_field = "text",
    max_seq_length = max_seq_length,
    dataset_num_proc = 2,
    packing = False, # Puede acelerar el entrenamiento si es True
    args = TrainingArguments(
        per_device_train_batch_size = 2,
        gradient_accumulation_steps = 4,
        evaluation_strategy = "steps", # Evaluar periódicamente
        eval_steps = 50,
        warmup_steps = 5,
        max_steps = 60, # Ajusta según necesidad
        learning_rate = 2e-4,
        fp16 = not is_bfloat16_supported(),
        bf16 = is_bfloat16_supported(),
        logging_steps = 1,
        optim = "adamw_8bit",
        weight_decay = 0.01,
        lr_scheduler_type = "linear",
        seed = 3407,
        output_dir = "outputs",
    ),
)
```

## 7. Exportación
Al final del Notebook, asegúrate de usar la opción de Unsloth para guardar el modelo en formato GGUF (cuantización `q4_k_m` o `q4_0`), ya que es el formato exacto que necesita `llama.cpp` en Android.
