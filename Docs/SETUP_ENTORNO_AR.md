# 🚀 Guía de Inicio Rápido: Proyecto Mascota AR

Toda la configuración técnica de ARCore, inputs y gráficos ya está sincronizada en el repositorio. Para ejecutar el proyecto en tu máquina y compilar en Android, sigue estos pasos exactos:

## 1. Instalar Módulos en Unity Hub
Al clonar el repo y abrirlo en Unity Hub, asegúrate de tener instalada exactamente la misma versión de Unity 6 que estamos usando.
* **Obligatorio:** Debes tener instalado el módulo **`Android Build Support`** (junto con OpenJDK y Android SDK & NDK Tools). Si no lo tienes, Unity no podrá compilar la app.

## 2. El Paso Crítico: "Switch Platform"
Por defecto, cuando clonas un proyecto de Unity en un PC nuevo, Unity abre el editor en modo "Windows/PC Standalone". Si intentas compilar ahí, la Realidad Aumentada se romperá.
1. Abre el proyecto en Unity.
2. Ve a `File > Build Profiles` (o Build Settings).
3. En la lista de plataformas de la izquierda, selecciona **`Android`**.
4. Haz clic en el botón que dice **`Switch Platform`** o **`Make Active`**.
5. *Nota: Unity se quedará pensando unos minutos mientras re-importa todos los gráficos al formato de celular. Esto es normal.*

## 3. La Primera Compilación (Paciencia)
1. Conecta tu celular Android por USB (con la Depuración USB activada).
2. Abre la escena `RayCast` (es la escena principal).
3. En la ventana de `Build Profiles`, dale a **`Build And Run`**.
4. **Advertencia de tiempo:** La primera vez que compiles, el proceso de "IL2CPP" tardará entre 5 y 15 minutos en traducir todo el código C# a C++ para tu celular. No canceles el proceso. Las compilaciones futuras tardarán solo un par de minutos.