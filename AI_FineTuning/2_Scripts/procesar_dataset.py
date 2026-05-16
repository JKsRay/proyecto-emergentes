import json
import os
import random
from collections import Counter

# ==============================================================================
# PROCESADOR DE DATASET PARA FINE-TUNING (UNSLOTH / HUGGINGFACE)
# ==============================================================================
# Este script toma el dataset.jsonl crudo, lo mezcla de forma determinista
# (para reproducibilidad) y lo divide en Train, Val y Test.
#
# - Semilla fija (42) para garantizar splits idénticos entre ejecuciones.
# - Reporte de distribución de estados FSM en cada split.
# - Validación estructural de cada fila (3 mensajes: system, user, assistant).
# ==============================================================================

# Rutas de carpetas
BASE_DIR = os.path.dirname(__file__)
RAW_FILE = os.path.join(BASE_DIR, "..", "1_RawData", "dataset.jsonl")
PROCESSED_DIR = os.path.join(BASE_DIR, "..", "3_Processed")

# Archivos de salida
TRAIN_FILE = os.path.join(PROCESSED_DIR, "train.jsonl")
VAL_FILE = os.path.join(PROCESSED_DIR, "val.jsonl")
TEST_FILE = os.path.join(PROCESSED_DIR, "test.jsonl")

# Configuracion de los porcentajes de division (Split)
TRAIN_RATIO = 0.85
VAL_RATIO = 0.10
TEST_RATIO = 0.05


def _extraer_estado_fsm(fila: dict) -> str:
    """Intenta clasificar la fila por estado FSM segun el contenido del system prompt."""
    try:
        system_content = fila["messages"][0]["content"].lower()
    except (KeyError, IndexError):
        return "desconocido"

    if any(kw in system_content for kw in ["bateria critica", "batería crítica", "energia critica", "energía crítica", "exhausto"]):
        return "BateriaCritica"
    elif any(kw in system_content for kw in ["descalibrad", "manoso", "vibraciones", "servomotores"]):
        return "Descalibrado"
    elif any(kw in system_content for kw in ["aburrido", "ignorado", "apatico", "apático", "pasivo-agresivo"]):
        return "Aburrido"
    elif any(kw in system_content for kw in ["euforico", "eufórico", "felicidad al 95", "felicidad alta", "excelente humor"]):
        return "Euforico"
    elif any(kw in system_content for kw in ["nominal", "optimo", "óptimo", "personalidad base", "normal"]):
        return "Normal"
    else:
        return "otro"


def _reportar_distribucion(data: list, etiqueta: str):
    """Imprime la distribucion de estados FSM dentro de un split."""
    conteo = Counter(_extraer_estado_fsm(fila) for fila in data)
    print(f"         Distribucion de estados en {etiqueta}:")
    for estado, cantidad in sorted(conteo.items()):
        print(f"           {estado:<18}: {cantidad:>4} filas")


def procesar_dataset():
    if not os.path.exists(RAW_FILE):
        print(f"[ERROR] No se encontro el archivo crudo en {RAW_FILE}")
        return

    # --- Advertencia de sobreescritura ---
    archivos_existentes = [f for f in [TRAIN_FILE, VAL_FILE, TEST_FILE] if os.path.exists(f)]
    if archivos_existentes:
        nombres = ", ".join(os.path.basename(f) for f in archivos_existentes)
        print(f"[WARN] Los siguientes archivos ya existen y seran sobreescritos: {nombres}")

    # 1. Leer el archivo crudo
    print("[INFO] Leyendo dataset crudo...")
    dataset = []
    errores = 0
    with open(RAW_FILE, "r", encoding="utf-8") as f:
        for i, linea in enumerate(f, 1):
            if not linea.strip():
                continue
            try:
                obj = json.loads(linea)
                # Validacion estructural: debe tener 3 mensajes con roles correctos
                msgs = obj.get("messages", [])
                if len(msgs) != 3:
                    print(f"[WARN] Linea {i}: Se esperaban 3 mensajes, tiene {len(msgs)}. Ignorada.")
                    errores += 1
                    continue
                roles = [m.get("role") for m in msgs]
                if roles != ["system", "user", "assistant"]:
                    print(f"[WARN] Linea {i}: Roles incorrectos {roles}. Ignorada.")
                    errores += 1
                    continue
                dataset.append(obj)
            except json.JSONDecodeError:
                print(f"[WARN] Linea {i}: JSON corrupto. Ignorada.")
                errores += 1

    total_filas = len(dataset)
    print(f"[OK] Se leyeron {total_filas} filas validas ({errores} descartadas).")

    if total_filas == 0:
        print("[ERROR] No hay datos para procesar.")
        return

    # 2. Mezclar aleatoriamente (Shuffle)
    # Usamos una semilla fija para que si ejecutamos el script 2 veces,
    # el orden siga siendo exactamente el mismo (reproducibilidad).
    random.seed(42)
    random.shuffle(dataset)
    print("[INFO] Dataset mezclado correctamente (seed=42).")

    # 3. Calcular indices de corte usando los ratios definidos
    train_end = int(total_filas * TRAIN_RATIO)
    val_end = train_end + int(total_filas * VAL_RATIO)
    # TEST_RATIO se usa implicitamente: test_data recibe el resto,
    # que deberia ser aprox. total_filas * TEST_RATIO filas.

    # 4. Dividir el dataset
    train_data = dataset[:train_end]
    val_data = dataset[train_end:val_end]
    test_data = dataset[val_end:]

    # Asegurar que el directorio de salida existe
    os.makedirs(PROCESSED_DIR, exist_ok=True)

    # 5. Guardar en disco
    print("[INFO] Guardando archivos particionados...")
    _guardar_jsonl(train_data, TRAIN_FILE, "Entrenamiento (Train)")
    _guardar_jsonl(val_data, VAL_FILE, "Validacion (Val)")
    _guardar_jsonl(test_data, TEST_FILE, "Prueba (Test)")

    # 6. Reporte de distribucion de estados FSM por split
    print("\n[INFO] Reporte de distribucion por estado FSM:")
    _reportar_distribucion(train_data, "Train")
    _reportar_distribucion(val_data, "Val")
    _reportar_distribucion(test_data, "Test")

    # 7. Resumen final
    print(f"\n[OK] Procesamiento completado con exito!")
    print(f"     Carpeta de destino: {os.path.abspath(PROCESSED_DIR)}")
    print(f"     Total filas procesadas: {total_filas}")
    print(f"     Split: Train={len(train_data)} | Val={len(val_data)} | Test={len(test_data)}")


def _guardar_jsonl(data, path, etiqueta):
    with open(path, "w", encoding="utf-8") as f:
        for obj in data:
            f.write(json.dumps(obj, ensure_ascii=False) + "\n")
    print(f"   - {etiqueta:<22}: {len(data):>4} filas -> {os.path.basename(path)}")


if __name__ == "__main__":
    procesar_dataset()
