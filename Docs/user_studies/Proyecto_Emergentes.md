# Proyecto Emergentes

*Documento convertido automáticamente a Markdown a partir del archivo original `Proyecto_Emergentes.pdf`.*

## --- Página 1 ---

Received:
Revised:
Accepted:
Published:
Copyright:© 2026 by the authors.
Submitted toJournal Not Specifiedfor
possible open access publication under
the terms and conditions of the
Creative Commons Attribution
(CC BY) license.
Article
Mascota virtual: "RoboPet" en Realidad Aumentada (AR)
Ulysses Barreto, Isidora Cisternas, Rodolfo Fernández and Pablo Silva
1. Objetivos 1
1.1. Objetivo General 2
Desarrollar una aplicación móvil de entretenimiento basada en Realidad Aumentada 3
(AR) y procesamiento de lenguaje natural que simule una mascota robot virtual interactiva 4
orientada al acompañamiento y entrención de público general.5
1.2. Objetivos específicos 6
• Estudiar y caracterizar el dominio de las mascotas virtuales digitales como paradigma 7
de interacción humano-agente, analizando sus necesidades típicas (alimentación, 8
cuidado, entretenimiento y estimulación), y evaluar el alcance de las tecnologías 9
seleccionadas —ARCore como motor de percepción espacial y LLM como motor 10
conversacional— en el contexto de aplicaciones móviles de Realidad Aumentada.11
• Diseñar e implementar un entorno de Realidad Aumentada en Unity que permita 12
la persistencia de la mascota virtual sobre superficies planas detectadas mediante 13
ARCore, integrando mecánicas gamificadas interactivas y algoritmos de navegación 14
autónoma con heurísticas espaciales de evasión. 15
• Integrar y adaptar (mediante Fine-Tuning) un LLM lo suficientemente ligero para una 16
implementación móvil como motor de interacción, permitiendo que la mascota genere 17
respuestas conversacionales coherentes, cuyo rol está delimitado a la interacción 18
contextual con el usuario dentro del dominio de la mascota (su estado físico, el 19
entorno AR, sus necesidades y el juego), rechazando explícitamente temáticas ajenas20
al contexto de la aplicación. Ésta posee una personalidad propia, impulsadas por una 21
Máquina de Estados Finitos (FSM) que gestiona sus necesidades internas en tiempo 22
real. 23
• Validar el prototipo mediante pruebas con 40 usuarios, recolectando datos a través 24
de los instrumentos SUS (usabilidad), AEQ-S (emociones) e IMI (motivación) para 25
comparar la experiencia de usuario. 26
• Documentar el proceso de desarrollo y los resultados obtenidos mediante informes 27
técnicos y material audiovisual que evidencien el cumplimiento de los requisitos 28
funcionales propuestos. 29
2. Metodología de Desarrollo 30
Debido a la estructura de entregas académicas que exigió un prototipo funcional desde 31
las primeras etapas y a la alta complejidad técnica que posee el sistema, se implementó 32
unEnfoque de Ciclo de Vida Iterativo-Incremental (IID). Esta decisión metodológica 33
resultó fundamental para mitigar riesgos tempranamente, permitiendo validar la compleja 34
arquitectura cliente-servidor desde el inicio mediante la construcción de un prototipo base 35
de extremo a extremo (un “esqueleto” funcional). 36
Dada la integración de múltiples capas tecnológicas avanzadas (procesamiento AR- 37
Core en dispositivos móviles, gestión de estados internos y comunicación asíncrona con el 38
Version June 10, 2026 submitted toJournal Not Specifiedhttps://doi.org/10.3390/1010000

## --- Página 2 ---

Version June 10, 2026 submitted toJournal Not Specified 2 of 14
modelo LLM local), este enfoque permite que el sistema evolucione por capas. En lugar de 39
construir módulos aislados, se añaden nuevas funcionalidades de forma progresiva sobre 40
una base estable, facilitando el refinamiento continuo y elevando la calidad general del 41
prototipo en cada ciclo sin comprometer la arquitectura central.42
Para organizar este trabajo de manera secuencial, el desarrollo se estructuró en tres 43
fases o iteraciones principales: 44
• Iteración 1 (Inception y Elaboración):Centrada en el levantamiento de un prototipo 45
funcional base que abarcó la oclusión inicial, la instanciación de la mascota en Realidad 46
Aumentada y la conexión de red exitosa con la IA local.47
• Iteración 2 (Construcción):Enfocada en la integración de las mecánicas avanzadas, 48
incluyendo la estabilización de la lógica del motor de estados, fine-tunning de la LLM, 49
la implementación de minijuegos interactivos, la optimización técnica de la latencia y 50
refactorizaciones de la arquitectura de interfaz (UI).51
• Iteración 3 (T ransición):Destinada al pulido del feedback visual (animaciones autóno- 52
mas y reactivas del modelo 3D), mejoras de inmersión y la validación psicométrica 53
final (SUS, UEQ) con la muestra de usuarios finales.54
https://doi.org/10.3390/1010000

## --- Página 3 ---

Version June 10, 2026 submitted toJournal Not Specified 3 of 14
3. Plan y Programa de T rabajo 55
3.1. Estructura de desglose de trabajo (EDT) 56
A continuación, la Figura 1 presenta nuestro Diagrama de Estructura de Desglose de 57
Trabajo. 58
Link EDT 59
Figure 1.Diagrama EDT del sistema.
https://doi.org/10.3390/1010000

## --- Página 4 ---

Version June 10, 2026 submitted toJournal Not Specified 4 of 14
3.2. Programa 60
T able 1.Matriz de Actividades y RRHH con Entregables Completos (Actualizada)
ID (EDT) Actividad Principal agru-
pada
Entregable Principal RRHH Involu-
crado
1.1 Fase de Inception
1.1.1 a 1.1.4 Benchmarking, Factibili-
dad (Unity 6/URP), Vali-
dación Stack AR y Config-
uración IA.
Requerimientos prin-
cipales y entorno base
validado.
U. Barreto, I. Cister-
nas, P . Silva
1.1.5 Diseño Matriz de Estados
y Eventos para LLM.
Documento de diseño
lógico de FSM y compor-
tamiento.
R. Fernández
1.2 Fase de Elaboración
1.2.1, 1.2.2,
1.2.6
Escaneo (ARCore), An-
claje de Modelo 3D y
Oclusión de Entorno.
Módulo de detección espa-
cial, anclaje y física AR.
I. Cisternas, R. Fer-
nández
1.2.3, 1.2.7 Prototipado de Interfaz
Base (UI) y Animación.
Prototipo gráfico y mod-
elo 3D con animaciones.
I. Cisternas, P . Silva
1.2.4, 1.2.5 Integración IA (Chat) y
Programación de Estados.
Comunicación Cliente-
LLM y motor FSM
estático.
U. Barreto
1.2.8 Documentación de Arqui-
tectura.
Informe de primera en-
trega.
Todo el equipo
1.3 Fase de Construcción
1.3.1, 1.3.2 Refactorización UI me-
diante Prefabs y Opti-
mización Latencia.
Interfaz optimizada e it-
eración de tiempos del
LLM.
P . Silva, U. Barreto
1.3.3.1 a
1.3.3.3
Generación de Dataset,
Procesamiento y Fine-
Tuning LLM.
Modelo LLM entrenado
(GGUF) con distintos es-
tados en base a nivel de
necesidades .
R. Fernández
1.3.4.1,
1.3.4.2
Corrección UI
Passthrough y Desar-
rollo de HUD.
HUD interactivo sin coli-
siones en raycast AR.
U. Barreto, I. Cister-
nas
1.3.5.1 a
1.3.5.4
Pruebas: Escenarios,
SUS/AEQ, Cuestionarios
y Piloto.
Informe con las estruc-
turas de Pruebas de
Usuario.
P . Silva
1.3.6 a
1.3.8.1
Refinamiento Lógica FSM
y Desarrollo Minijuego
AR.
Minijuego evasivo
acoplado a la Máquina de
Estados.
I. Cisternas, R. Fer-
nández
1.4 Fase de T ransición
1.4.1 a 1.4.3 Integración GGUF móvil,
Sincronización y Feedback
UX.
Release Candidate (RC)
estabilizado en Android.
R. Fernández, I. Cis-
ternas, U. Barreto
1.4.4 Pruebas de Usuario. Pruebas presenciales
de validación objetiva
(N=40).
Equipo completo
1.4.5 a 1.4.7 Procesamiento de datos y
Material Audiovisual.
Tabulación de métricas, in-
forme y video final.
R. Fernández, I. Cis-
ternas, P . Silva
https://doi.org/10.3390/1010000

## --- Página 5 ---

Version June 10, 2026 submitted toJournal Not Specified 5 of 14
3.3. Carta Gantt 61
Link Carta Gantt
Figure 2.Carta Gantt del proyecto.
62
La planificación actualizada de nuestro proyecto, reflejada en la Carta Gantt de la 63
Figura 2, evidencia la aplicación de nuestro modelo de ciclo de vida iterativo-incremental. 64
La parte incremental de nuestro proyecto se observa en la adición escalonada de nuevos 65
bloques de valor a lo largo de las fases, tales como la incorporación del pipeline de in- 66
teligencia artificial y la implementación de las mecánicas del minijuego AR sobre la base del 67
prototipo inicial. Por su parte, la dimensión iterativa se manifiesta claramente a través de 68
las fases de "refactorización" y "refinamiento" (por ejemplo, la corrección de UI Passthrough 69
y el perfeccionamiento de la lógica del motor de estados); procesos en los cuales nuestro 70
equipo vuelve sobre módulos ya estructurados para optimizar su rendimiento, corregir 71
errores y añadir más valor a las funciones. 72
Asimismo, en la etapa de preparación para la validación con usuarios, la planificación 73
muestra la ejecución en paralelo de las tareas correspondientes al diseño de escenarios, 74
instrumentos psicométricos, estructuración de cuestionarios y planificación de la prueba pi- 75
loto. Esta concurrencia se debe a la metodología iterativa: al ser componentes dependientes 76
entre sí, se van diseñando y refinando de manera simultánea, permitiendo que cualquier 77
ajuste en las condiciones del piloto retroalimente y mejore inmediatamente el diseño de las 78
encuestas y métricas a evaluar, garantizando así la coherencia de los instrumentos antes de 79
su aplicación masiva. 80
En lo que respecta al estado de ejecución del proyecto, el cronograma registra un 81
progreso sustancial conforme a la planificación metodológica establecida, habiéndose 82
completado de manera íntegra tres de las cuatro fases principales estipuladas en la Carta 83
Gantt. La culminación de estos tres bloques secuenciales no solo consolida el despliegue de 84
los componentes iterativos e incrementales descritos sino que también sitúa al equipo de 85
desarrollo en posición para iniciar la cuarta y última fase del ciclo de vida, orientada a la 86
validación final y el cierre del proyecto. 87
4. Diseño del prototipo 88
4.1. Esquema general 89
El esquema de la Figura 16 presenta la arquitectura de integración de la mascota 90
virtual. El motor Unity 6 actúa como el núcleo central, recibiendo los activos gráficos desde 91
https://doi.org/10.3390/1010000

## --- Página 6 ---

Version June 10, 2026 submitted toJournal Not Specified 6 of 14
Figure 3.Esquema general de la arquitectura del sistema.
la Asset Store y los datos espaciales físicos mediante ARCore. Para el procesamiento de 92
lenguaje natural, la lógica del juego se comunica internamente con el paquete LLMUnity, el 93
cual actúa como cliente para gestionar peticiones de red HTTP (JSON) hacia un servidor 94
externo llama.cpp. Este servidor aloja y ejecuta el modelo local Qwen3.5-0.8B-GGUF 95
(seleccionado en reemplazo de Llama 3.2 por su eficiencia en dispositivos móviles), el cual 96
ha sido adaptado mediantefine-tuningpara incorporar una personalidad sarcástica. El 97
modelo procesa el contexto dinámico inyectado por la Máquina de Estados Finitos (FSM) 98
(que controla la Energía, Mantenimiento y Felicidad de la mascota) y devuelve respuestas 99
que determinan el comportamiento del agente. Finalmente, el sistema se compila en una 100
aplicación Android que provee la interfaz de realidad aumentada al usuario.101
4.2. Requisitos Funcionales y No Funcionales 102
A continuación se realiza un listado de los requisitos funcionales y no funcionales del 103
sistema a desarrollar. 104
4.2.1. Requisitos Funcionales (RF) 105
• RF-01:La aplicación debe escanear el entorno físico a través de la cámara del disposi- 106
tivo móvil y detectar planos horizontales continuos (suelos o mesas).107
• RF-02:El sistema debe permitir al usuario colocar el modelo 3D de la mascota virtual 108
anclado a un punto específico del plano detectado.109
• RF-03:El sistema debe implementar una interfaz de control de eventos que permita al 110
usuario ejecutar acciones de cuidado explícitas sobre la mascota —incluyendo recargar 111
batería, realizar mantenimiento mecánico e iniciar sesiones de entretenimiento— cada 112
una con efectos diferenciados y cuantificados sobre las variables internas del sistema 113
(Energía, Mantenimiento y Felicidad). 114
• RF-04:El sistema debe calcular y actualizar en tiempo real tres variables internas de 115
la mascota: Energía, Mantenimiento y Felicidad, en una escala de 0 a 100 según las 116
acciones realizadas por el usuario. 117
• RF-05:El sistema debe incluir una interfaz de chat que permita al usuario ingresar 118
texto libre y seleccionar frases predeterminadas para interactuar con la mascota.119
• RF-06:El sistema debe evaluar el estado actual de las necesidades de la mascota 120
(Energía, Mantenimiento, Felicidad) junto con las entradas del usuario, para adaptar 121
el comportamiento y las respuestas de la mascota a dicha situación.122
• RF-07:El sistema debe procesar las entradas de texto del usuario para generar respues- 123
tas conversacionales coherentes y autónomas en tiempo real.124
• RF-08:La mascota virtual debe reflejar su estado de ánimo y el tono de la conversación 125
mediante la ejecución autónoma de animaciones 3D observables por el usuario.126
• RF-09:El sistema debe incluir un módulo de minijuego interactivo ("Atrapar a la 127
mascota") que se ejecute en el entorno de Realidad Aumentada, requiriendo que el 128
usuario intercepte físicamente al modelo 3D un número determinado de veces.129
https://doi.org/10.3390/1010000

## --- Página 7 ---

Version June 10, 2026 submitted toJournal Not Specified 7 of 14
• RF-10:El sistema debe suspender temporalmente el desgaste pasivo de las estadísticas 130
internas (Energía, Felicidad, Mantenimiento) de la mascota durante la ejecución del 131
minijuego para evitar penalizaciones injustas al usuario.132
• RF-11:El sistema debe proveer un flujo de inducción ("Onboarding") mediante men- 133
sajes de texto dinámicos en la pantalla, guiando al usuario paso a paso durante el 134
escaneo del entorno físico y la instanciación inicial.135
• RF-12:La aplicación debe proporcionar una función para "Reubicar Mascota", permi- 136
tiendo al usuario trasladar el modelo 3D a un nuevo punto del plano detectado sin 137
necesidad de reiniciar la sesión. 138
4.2.2. Requisitos No Funcionales (RNF) 139
• RNF-01:El tiempo de latencia entre el envío del mensaje en la app y la recepción de 140
la respuesta del servidor local no debe superar los 3 segundos, para mantener una 141
experiencia conversacional fluida. 142
• RNF-02:La aplicación móvil cliente debe ser compatible con el sistema operativo 143
Android. 144
• RNF-03:La aplicación en Unity debe renderizar la experiencia de Realidad Aumentada 145
a un mínimo sostenido de 30 FPS en el dispositivo móvil para evitar fatiga visual 146
durante las pruebas de usuario. 147
• RNF-04:La interfaz de usuario (HUD y chat) debe ser intuitiva y contar con retroali- 148
mentación visual inmediata (ej. indicadores de carga mientras la IA procesa).149
• RNF-05:El servidor de procesamiento debe ser capaz de procesar las peticiones del 150
LLM de manera continua y estable durante los bloques de pruebas con los usuarios.151
• RNF-06:La representación gráfica de las mallas de los planos físicos detectados por 152
ARCore debe mantenerse invisible para el usuario final, garantizando el realismo y la 153
inmersión de la experiencia de Realidad Aumentada.154
• RNF-07:El algoritmo de navegación del modelo 3D debe integrar una heurística 155
defensiva de escape (divergencia de vectores) que obligue a la mascota a realizar 156
trayectorias en zigzag, minimizando la probabilidad de que atraviese paredes físicas 157
reales. 158
• RNF-08:La interfaz de usuario superpuesta (Canvas UI) debe aislar estrictamente 159
sus eventos táctiles del entorno tridimensional, previniendo que los toques en los 160
botones disparen interacciones fantasma o reubicaciones accidentales en la Realidad 161
Aumentada (prevención de UI Passthrough). 162
4.3. Diagrama de casos de uso 163
En esta sección se detalla la funcionalidad de nuestro sistema "RobotPet" mediante 164
diagramas de casos de uso. Se presenta primero un diagrama general y luego diagramas 165
detallados para los subsistemas con mayor complejidad lógica y técnica.166
https://doi.org/10.3390/1010000

## --- Página 8 ---

Version June 10, 2026 submitted toJournal Not Specified 8 of 14
Figure 4.Diagrama de Caso de Uso a Alto Nivel (General). Muestra los límites del sistema "Mascota
Virtual" y las funcionalidades principales accesibles para el Usuario.
Figure 5.Diagrama de Caso de Uso Detallado: Alterar Estado Mascota FSM. Muestra las formas
específicas de cuidado (Recargar energía, Realizar mantenimiento) a las que accede el Actor Usuario
de forma directa.
Figure 6.Diagrama de Caso de Uso Detallado: Instanciar con Mascota. Ilustra el escaneo del espacio,
la instanciación del robot dentro de la escena y la reubicación de la mascota dentro del espacio.
https://doi.org/10.3390/1010000

## --- Página 9 ---

Version June 10, 2026 submitted toJournal Not Specified 9 of 14
Figure 7.Diagrama de Caso de Uso Detallado: Jugar Con Mascota. Ilustra el flujo de las transiciones
y las distintas interacciones para el usuario.
4.4. Maquetas (mockups) de vistas principales 167
A continuacion se muestran los mockups de los estados principales del sistema.168
Figure 8.Mockup Estado Feliz
Figure 9.Mockup Estado sin batería
https://doi.org/10.3390/1010000

## --- Página 10 ---

Version June 10, 2026 submitted toJournal Not Specified 10 of 14
Figure 10.Mockup Estado Error
4.5. Vista funcional 169
Figure 11.Diagrama de secuencia de la funcionalidad "conversar con mascota".
https://doi.org/10.3390/1010000

## --- Página 11 ---

Version June 10, 2026 submitted toJournal Not Specified 11 of 14
Figure 12.Diagrama de secuencia de la funcionalidad Instanciar mascota".
Figure 13.Diagrama de secuencia de la funcionalidad "jugar con mascota".
https://doi.org/10.3390/1010000

## --- Página 12 ---

Version June 10, 2026 submitted toJournal Not Specified 12 of 14
Figure 14.Diagrama de secuencia de la funcionalidad "Gestionar Estados Mascota FSM".
4.6. Vista estática 170
Cabe notar que la aplicación no utiliza una gran cantidad de datos que almacenar, así 171
que no se consideró un modelo de bases de datos más complejo.172
Figure 15.Diagrama ER del sistema.
https://doi.org/10.3390/1010000

## --- Página 13 ---

Version June 10, 2026 submitted toJournal Not Specified 13 of 14
4.7. Vista dinámica 173
Figure 16.Diagrama Estado del robot según gestión FSM.
5. Diseño de Pruebas de Usuario 174
Esta sección detalla el plan de evaluación con usuarios para el prototipo. Las prueba 175
de usuarios contarán con una muestra de 40 individuos y contendrán un pre-test, la prueba 176
como tal, y un post-test para evaluar las métricas elegidas. Se utilizará la técnica "Thinking 177
Out Loud" para indicar a los usuarios que digan lo que piensan sobre el producto mientras 178
lo están probando. 179
5.1. Escenarios de Prueba 180
Se han definido 3 escenarios principales que buscan cubrir el ciclo completo de inter- 181
acción con el prototipo: 182
•Escenario 1: Onboarding y Descubrimiento (AR + LLM)183
Objetivo: Evaluar la facilidad para posicionar al robot en el mundo real y tener la 184
primera interacción con el chat. 185
T area del usuario: Abrir la aplicación, escanear el entorno, colocar al robot sobre una 186
superficie plana (AR Plane) y enviarle un mensaje inicial de saludo a través del chat 187
(LLM) para ver su respuesta. 188
•Escenario 2: Gestión de Necesidades (FSM) 189
Objetivo: Evaluar la comprensión del sistema de desgaste pasivo y la interacción a 190
través de botones de UI. 191
T area del usuario: Observar cómo decae la energía o mantenimiento. Interactuar 192
con los botones de "Recargar" y "Mantenimiento". Observar cómo cambia la actitud 193
del robot en el chat cuando está en estado "Descalibrado" o "Batería Crítica", y cómo 194
reacciona tras ser atendido. 195
•Escenario 3: Interacción Activa (Minijuego AR) 196
Objetivo: Probar el sistema de navegación AR del robot y el bucle de juego.197
T area del usuario: Presionar el botón "Jugar" para iniciar el minijuego. Perseguir e 198
interactuar físicamente (tocando la pantalla) con el robot en el espacio físico mientras 199
https://doi.org/10.3390/1010000

## --- Página 14 ---

Version June 10, 2026 submitted toJournal Not Specified 14 of 14
este intenta "huir". Conseguir las 3 capturas necesarias para terminar el juego y leer la 200
reacción de la mascota en el chat. 201
5.2. Métricas a medir 202
Se medirán las siguientes variables utilizando modelos y cuestionarios estandarizados: 203
1. Usabilidad (System Usability Scale - SUS):Al ser un prototipo con interfaces (UI), 204
Chat y AR, es vital medir la percepción de complejidad global. El SUS proporciona 205
una métrica estándar (0-100) para evaluar usabilidad e intención de uso.206
2. Atracción y Estimulación (User Experience Questionnaire - UEQ):El UEQ mide 6 207
dimensiones, Atracción, Eficiencia, Claridad, Control y Novedad. Así que tiene las 208
métricas que nos interesan para evaluar la aplicación.209
5.3. Cuestionario Pre Prueba 210
Consistirá en preguntas básicas como datos identificadores, y un conjunto de preguntas 211
para conocer la experiencia previa del usuario. El Cuestionario se puede visualizar Link 212
Pre-Test. 213
5.4. Cuestionario Post Prueba 214
Se divide en tres partes: El cuestionario SUS, el cual medirá en una escala del 1-100 la 215
usabilidad del sistema. Y la segunda parte consiste en el UEQ, que medirá la experiencia 216
general del usuario respecto al sistema. Finalmente se encuentra una tercera parte enfocada 217
en la percepcion del usuario sobre las respuestas que otrogan la LLM. El Cuestionario Post 218
Prueba se puede visualizar Link Post-Test. 219
5.5. Procedimiento de aplicación de las pruebas 220
Las pruebas se harán de forma presencial en algún entorno seguro, acorde a la disponi- 221
bilidad que tengamos, de forma que se pueda probar la aplicación de forma amena. Tiene 222
una duración Aproximada de 20 a 25 minutos. 223
El orden sería el siguiente: 224
1. Bienvenida y Pre-test (5 min): Explicación del propósito de la prueba (sin revelar 225
detalles técnicos que sesguen), firma de consentimiento y llenado del Cuestionario 226
Pre-prueba. 227
2. Interacción con el prototipo (10 min): El facilitador entrega el dispositivo móvil 228
(Android) con la app. El usuario ejecuta los 3 escenarios. Se utilizará la técnica de 229
"Thinking Aloud" (Pensar en voz alta), pidiendo al usuario que relate lo que piensa y 230
hace sin que el moderador intervenga (salvo bloqueos críticos).231
3. Cuestionario Post-prueba (5 min): Aplicación de los instrumentos de medición (SUS, 232
UEQ) inmediatamente después del uso.s 233
5.6. Ejecución de Pruebas Piloto 234
Se realizarán 2 pruebas piloto para verificar que los tiempos y los escenarios sean 235
acordes a lo mencionado anteriormente, de esta forma se refinarán los detalles necesarios y 236
se dará a cabo el plan de pruebas de usuario. 237
238
https://doi.org/10.3390/1010000

