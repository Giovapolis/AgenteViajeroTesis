# Análisis Técnico del Proyecto TSP en Realidad Virtual

Este documento presenta un análisis técnico detallado del proyecto Unity desarrollado para resolver el Problema del Agente Viajero (TSP) mediante algoritmos genéticos. El enfoque es académico y está orientado a tesis de ingeniería; se incluyen implementaciones propias y basadas en la librería **GeneticSharp**, así como elementos de visualización aprovechables en entornos de realidad virtual.

---

## 1. Arquitectura general del sistema

El sistema se construye sobre el motor Unity3D siguiendo la arquitectura estándar basada en la composición de **GameObjects** y **Componentes** (scripts). La estructura incluye:

1. **Carpeta Assets**: contendrá todo el contenido que será empaquetado en el build, dividida en subdirectorios como `Scripts`, `Prefabs`, `Scenes`, `GoogleVR`, `GeneticSharp` y otros.
2. **Scripts**: código en C# organizado en subdirectorios (`Propio`, `Genetic`, `Utils`) que implementa la lógica del algoritmo, la interfaz y utilidades varias.
3. **Prefabs**: plantillas reutilizables (p.ej. la ciudad móvil) que se instancian en tiempo de ejecución.
4. **Scenes**: distintas escenas (`Inicio.unity`, `Propio.unity`, `Genetic.unity`, `Escenario`, `Pruebas`) que representan estados o variantes de la aplicación.
5. **Plugins / Bibliotecas**: incluye la carpeta `GeneticSharp` con la biblioteca de algoritmos genéticos y `GoogleVR` para soporte de realidad virtual.

La comunicación entre capas se hace mediante referencias serializadas en el inspector de Unity y llamadas a métodos públicos desde scripts que controlan el flujo de ejecución.

---

## 2. Descripción de cada carpeta relevante

| Carpeta | Contenido y propósito |
|---|---|
| `Assets` | Raíz de recursos; alberga todos los subelementos descritos a continuación. |
| `Assets/Scripts/Propio` | Implementación manual del AG (clases `AlgoritmoGenetico`, `Cromosoma`, etc.). |
| `Assets/Scripts/Genetic` | Implementación que utiliza GeneticSharp (`GAController`, `TspChromosome`, etc.). |
| `Assets/Scripts/Utils` | Utilidades generales (gestión de escenas, control de vistas, rotación de texto, etc.). |
| `Assets/Prefabs` | Objetos preconfigurados como el prefab de ciudad que se instancia dinámicamente. |
| `Assets/Scenes` | Colección de archivos `.unity` para distintas configuraciones de la aplicación. |
| `Assets/GeneticSharp` | Código fuente y dependencias de la librería de algoritmos genéticos. |
| `Assets/GoogleVR` | SDK de Google para habilitar realidad virtual (UI, controladores, etc.). |
| `Plugins` | Bibliotecas externas adicionales (no siempre usadas directamente por el usuario). |
| `Library`, `Packages`, `Temp` | Carpetas de Unity para gestión interna; no modificadas por el desarrollador. |

---

## 3. Descripción de cada script

Se ofrece una visión sintética de los scripts principales.

### Propio

* **Ciudad.cs**: clase simple que encapsula un identificador (`NumCity`) y la posición (`Vector3`). Usada por el algoritmo propio para generar puntos.
* **Cromosoma.cs**: modelo de individuo; almacena un `List<int>` de índices de ciudades y una aptitud. Implementa tres operadores de mutación (inserción, intercambio, inversión) y un `ToString()` para debugging.
* **AlgoritmoGenetico.cs**: núcleo del AG propio. Gestiona población, generación de ciudades, evaluación de aptitud, selección (ruleta, torneo, ranking), cruza (PMX, OX, CX), mutación, la evolución iterativa en un hilo separado, y la visualización mediante `LineRenderer`.
* **ControlAGPropio.cs**: controlador de interfaz para la implementación propia. Escucha entradas de joystick/teclado, actualiza parámetros de texto (`TMP_Text`) mostrando el parámetro seleccionado y transmite valores a `AlgoritmoGenetico` (núm. de ciudades, generaciones, etc.).
* **GeneticMain.cs**: clase auxiliar con código preliminar/comentado para arranque; actualmente se limita a mostrar el mejor cromosoma obtenido por `AlgoritmoGenetico`.

### Genetic (GeneticSharp)

* **TspCity.cs**: simple POCO que encapsula la posición de la ciudad. Utilizada por `TspFitness`.
* **TspChromosome.cs**: extiende `ChromosomeBase` para representar permutaciones de ciudades; inicializa genes con índices únicos y guarda la distancia calculada.
* **TspFitness.cs**: implementación de la función de aptitud para GeneticSharp. Genera ciudades al azar dentro de un rectángulo definido y calcula la distancia total de un cromosoma. Convierte la distancia en fitness, penaliza soluciones con ciudades repetidas.
* **GAController.cs**: puente entre Unity y GeneticSharp. Configura el `GeneticAlgorithm` (selección, cruza, mutación, término), arranca el proceso en un hilo y dibuja ciudades/rutas con `LineRenderer` y `CityPrefab`. Ofrece métodos (`iniciarAG`, `detente`, `recalcular`) para controlar el ciclo.
* **ControlParametros.cs**: interfaz para modificar parámetros del AG basado en GeneticSharp (número de generaciones y ciudades). Similar a `ControlAGPropio` pero con menos opciones.
* **CityController.cs**: permite arrastrar con ratón las ciudades generadas en la escena, actualizando su posición en la lista de `TspCity`.

### Utils

* **SceneMan.cs**: simple loader de escenas por nombre.
* **ControlViews.cs**: controla la cámara / transform de un objeto para ciclar entre vistas predefinidas.
* **ControlTexto.cs**: hace que un `Transform` (panel de texto) siempre mire a la cámara principal (billboarding).

### Otros scripts de la UI y movimiento

* **Movimiento.cs**: controla el movimiento de un `CharacterController` con entradas horizontales/verticales.
* **GamepadButtonAClick.cs**, **DefaultButtonSelector.cs**: adaptan eventos de gamepad a la interfaz de usuario (selección y clic de botones). 
* **ContolInterfazGenetic.cs** y **CambioScript.cs**: gestionan entradas genéricas y habilitan/deshabilitan componentes dependiendo de la colisión con el jugador.

---

## 4. Flujo de ejecución del programa

1. **Carga de la escena**: Unity recarga `Propio` o `Genetic` según la selección del usuario desde la pantalla de inicio, ejecutando `Awake`/`Start` de los componentes presentes.
2. **Inicialización de parámetros**: componentes de control (`ControlAGPropio` o `ControlParametros`) leen el sistema operativo y configuran textos y valores iniciales.
3. **Interacción**: el usuario navega entre parámetros con botones del controlador o teclas; los controladores actualizan la interfaz visual y preparan variables.
4. **Inicio del AG**: al pulsar el botón de ejecución, el controlador correspondiente asigna parámetros y llama a `inicia()`/`iniciarAG()` de la clase genérica de algoritmo.
5. **Generación de ciudades**: se crea una lista de objetos `Ciudad` o `TspCity` con posiciones aleatorias y se instancian prefabs en el mundo.
6. **Ejecución del algoritmo**: el AG corre en un hilo separado (propio) o mediante `GeneticAlgorithm` de GeneticSharp, iterando hasta alcanzar el número de generaciones configurado.
7. **Visualización de la ruta**: tras cada generación, el mejor cromosoma se comunica al hilo principal y se llama a `drawRoute()`/`DrawRouteGenetic()` para actualizar el `LineRenderer`.
8. **Control de ciclo**: botones adicionales permiten pausar, detener o reiniciar la ejecución.
9. **Finalización**: al terminar las generaciones o al detener manualmente, el hilo se cierra y se mantiene la visualización final. El usuario puede iniciar un nuevo experimento con parámetros distintos.

---

## 5. Componentes que conectan Unity con el algoritmo

* **AlgoritmoGenetico** y **GAController** actúan como las piezas que unen la lógica del algoritmo (población, operadores, evaluaciones) con los elementos de Unity (prefabs, transformaciones, UI).
* Ambos exponen propiedades públicas para ajustar parámetros desde el inspector o desde scripts de control.
* La visualización (instanciación de ciudades, actualización de `LineRenderer`, texto de resultados) se realiza mediante métodos específicos dentro de estas clases.
* El uso de **Threads** en ambas implementaciones separa la ejecución del algoritmo del hilo principal para evitar bloqueos en la interfaz.

---

## 6. Descripción del sistema de generación de ciudades

En las dos implementaciones las ciudades se generan aleatoriamente dentro de un área rectangular definida en coordenadas 3D. Las diferencias principales:

* **Algoritmo propio** (`AlgoritmoGenetico.makeCities`) utiliza `System.Random` para escoger valores de X entre 8 y 17, Y fijo en 1.35, Z entre -5 y 5. Cada ciudad se almacena en un objeto `Ciudad` con un identificador "#i".
* **GeneticSharp** (`TspFitness` constructor) emplea el proveedor de aleatoriedad de GeneticSharp (`RandomizationProvider.Current`) y un rectángulo `Rect(-27.2f, -3.2f, 6.2f, 6.2f)` para generar posiciones. El atributo `Cities` en la clase encapsula la lista de `TspCity` resultantes.

Las posiciones se convierten en instancias del prefab `CityPrefab`, cuya altura en Y se ajusta a 1.35f para destacarlas en el escenario.

---

## 7. Explicación del sistema de visualización

La visualización del proceso evolutivo consta de tres elementos:

1. **Ciudades**: instanciadas como `GameObjects` a partir de un prefab; cada instancia muestra un texto con su número. El script `CityController` (en la versión Genetic) permite arrastrarlas.
2. **Ruta**: representada mediante un componente `LineRenderer`. Se configura con un degradado de color (verde → cian → azul) y anchura constante. Las posiciones se actualizan con el recorrido del cromosoma.
3. **Texto de resultados**: objetos `TMP_Text` que muestran parámetros actuales (número de generaciones, ciudades, método de cruce/selección, porcentaje de mutación) y la descripción del mejor cromosoma (`ToString()` o serialización de genes).

El `LineRenderer` se inicializa la primera vez que se dibujan las ciudades (`initializeRouteRenderer`) y se vuelve a dibujar en cada actualización. El código asegura que el primer y último punto se conecten para cerrar el ciclo.

La actualización en el hilo principal se maneja mediante una bandera `visualizationPending` y un bloqueo para evitar condiciones de carrera.

---

## 8. Explicación del controlador de generaciones

El `controlador de generaciones` corresponde a los scripts `ControlAGPropio` y `ControlParametros`. Su función es:

* **Leer entradas** del usuario (joystick/teclado) y determinar qué parámetro se está modificando.
* **Mantener un contador** (`contadorParms`) que indica qué variable está actualmente resaltada en la UI.
* **Incrementar/disminuir valores** acorde al botón de navegación, con límites definidos (p.ej. `numGeneraciones` entre 50 y 5000, `numCiudades` entre 5 y 50, etc.).
* **Actualizar la UI** de texto (`TMP_Text`) para reflejar cambios y resaltar el parámetro seleccionado usando etiquetas `<u>` y `<uppercase>`.
* **Transmisión de parámetros** a los componentes de algoritmo cuando el usuario presiona el botón de inicio.

Estos controladores detectan además el sistema operativo para ajustar la lógica de `JoystickButton` (distintas asignaciones en Android y Windows).

---

## 9. Explicación del renderizado de rutas

El renderizado de rutas se realiza con un objeto `LineRenderer` creado dinámicamente. Las características técnicas clave:

* **Creación**: un `GameObject` vacío llamado "RutaVisual" se utiliza exclusivamente para albergar el componente.
* **Configuración**: se establece `startWidth` y `endWidth` en 0.2f, se asigna un material (o el predeterminado `Sprites/Default`) y se define un `Gradient` para el color.
* **Actualización de posiciones**: el método `drawRoute` (o `DrawRouteGenetic`) ajusta `positionCount` y llama a `SetPosition(i, vector)` para cada ciudad en el recorrido. Se añade un punto extra para cerrar el ciclo al retornar a la ciudad de origen.
* **Optimización**: sólo se reconstruye cuando `visualizationPending` está activo, evitando trabajo innecesario en el hilo principal.

En la implementación con GeneticSharp, el renderizado se encuentra dentro de `DrawRouteGenetic`, que extrae genes del mejor cromosoma y utiliza las posiciones almacenadas en el `TspFitness`.

---

## 10. Descripción del uso de GeneticSharp

**GeneticSharp** es integrado como paquete fuente dentro de `Assets/GeneticSharp`. El uso dentro del proyecto implica:

* **Referencias**: se emplean los namespaces `GeneticSharp.Domain.*` en los scripts del subdirectorio `Genetic`.
* **Modelado del problema**: el diseño de `TspChromosome` y `TspFitness` adapta la API de la biblioteca al problema del TSP.
* **Configuración de la AG**: en `GAController.algoritmoGenetic()` se instancian componentes estándares: `OrderedCrossover`, `ReverseSequenceMutation` y `RouletteWheelSelection`. La población inicial se construye mediante `new Population(min, max, chromosome)`.
* **Control del ciclo**: se define un terminador `new GenerationNumberTermination(N_generaciones)` y un `ParallelTaskExecutor` para permitir ejecución multihilo dentro de GeneticSharp.
* **Evento de generación**: `m_ga.GenerationRan += delegate { ... }` se usa para capturar métricas de cada generación (p.ej. distancia de la mejor solución).
* **Ejecución en hilos**: el algoritmo se arranca explícitamente en un `Thread` para separar el procesamiento del `Update()` de Unity.

El resultado es una implementación robusta con operadores probados que facilita la experimentación con distintas configuraciones y reduce la complejidad de la lógica interna del algoritmo.

---

### Notas finales

La combinación de desarrollo propio y utilización de una biblioteca externa crea un entorno ideal para el análisis comparativo, permitiendo al investigador evaluar el impacto de distintas estrategias de selección, cruza y mutación tanto en términos de código como de rendimiento. La integración con VR, aunque no abordada en detalle aquí, se apoya en las carpetas `GoogleVR` y en componentes de interacción específicos para ofrecer una experiencia inmersiva.

Este documento puede servirse como base para la redacción de capítulos técnicos en la tesis, proporcionando una descripción rigurosa de la arquitectura, las implementaciones y los flujos de datos dentro del proyecto.
