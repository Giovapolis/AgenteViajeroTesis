# Análisis Arquitectónico del Proyecto

## Introducción

El sistema desarrollado en este repositorio utiliza el motor Unity como plataforma de simulación y visualización para estudiar la aplicación de algoritmos genéticos en la resolución de problemas de optimización de rutas. El propósito general es ofrecer una herramienta interactiva que permita configurar parámetros de un algoritmo evolutivo, ejecutar el proceso y observar en tiempo real cómo la población de soluciones converge hacia la ruta óptima. El contexto de desarrollo es académico, orientado a tesis de ingeniería, educación y demostraciones de algoritmos metaheurísticos. Se contemplan aplicaciones en escenarios de realidad virtual para enriquecer la experiencia de interacción.

## Estructura general del proyecto

El proyecto mantiene la estructura habitual de una aplicación Unity, con carpetas diferenciadas que almacenan código de comportamiento (`Scripts`), activos reutilizables (`Prefabs`), escenas (`Scenes`), recursos adicionales (`Resources`) y componentes de terceros (`Plugins`, `GoogleVR`, `GeneticSharp`). 

Cada carpeta cumple un propósito particular:

* **Assets**: contenedor raíz para todos los recursos del juego, subdividido internamente.
* **Scripts**: ubicado dentro de Assets, agrupa en subdirectorios el código fuente escrito en C#.
* **Prefabs**: objetos preconfigurados que se instancian en la escena (ciudades, jugador, etc.).
* **Scenes**: colecciones de objetos y configuraciones de Unity para diferentes estados de la aplicación (inicio, prueba, entorno genético). 
* **Plugins** y **GoogleVR**: bibliotecas externas que aportan funcionalidades de terceros, como soporte VR o el marco GeneticSharp.
* **Resources**, **Packages**, **Library**: estructura propia de Unity para almacenamiento y gestión de dependencias.

La interacción entre estos módulos se efectúa a través de componentes adjuntos a GameObjects y llamadas a métodos públicos, respetando la arquitectura de Unity basada en la composición.

## Análisis de carpetas del proyecto

### Assets

Contiene todos los recursos que se incluirán en la compilación. Se subdivide en numerosas carpetas pero las más importantes para la lógica son `Scripts`, `Prefabs`, `Scenes`, `GoogleVR` y `GeneticSharp`.

### Scripts

Agrupa el código en tres grandes subdirectorios:

* **Propio**: implementaciones propias del algoritmo genético y control de la interfaz. Incluye clases como `AlgoritmoGenetico`, `Cromosoma`, `Ciudad`, `ControlAGPropio` y `GeneticMain`.
* **Genetic**: implementación alternativa basada en la biblioteca GeneticSharp. Contiene controladores específicos (`GAController`, `CityController`, `ControlParametros`) y clases que modelan el TSP (`TspChromosome`, `TspFitness`, `TspCity`).
* **Utils**: servicios genéricos como manejo de texto en UI, cambio de escenas y controles.

### Prefabs

Incluyen plantillas visuales para la instanciación en tiempo de ejecución. El prefab `CiudadMobil` representa visualmente cada ciudad y se utiliza tanto en la implementación propia como en la basada en GeneticSharp.

### Scenes

Varias escenas precargadas permiten ejecutar pruebas y mostrar la funcionalidad: `Inicio.unity` para la pantalla inicial, `Propio.unity` para la versión desarrollada manualmente y `Genetic.unity` para la implementación con GeneticSharp. Escenas adicionales (`Escenario`, `Pruebas`) pueden contener configuraciones experimentales.

### Plugins

Contienen bibliotecas externas. La carpeta `GeneticSharp` almacena la implementación de código fuente de la biblioteca de algoritmos genéticos, mientras que `GoogleVR` incluye los SDK necesarios para habilitar VR en plataformas compatibles.

### Resources y otros

Carpetas generadas automáticamente por Unity para gestionar assets en tiempo de ejecución y dependencias; no se manipulan directamente en el proyecto.

## Implementaciones del algoritmo genético

El proyecto incorpora dos implementaciones diferenciadas del algoritmo genético, permitiendo comparar un desarrollo a medida con el uso de una biblioteca especializada.

### Implementación basada en GeneticSharp

GeneticSharp es una biblioteca de código abierto para algoritmos genéticos en .NET que ofrece componentes reutilizables para fitness, poblaciones, cruces, mutaciones y estrategias de selección. Se integra en el proyecto a través de la carpeta `Assets/GeneticSharp` y mediante scripts específicos en `Assets/Scripts/Genetic`.

La clase `GAController` actúa como puente entre Unity y la biblioteca. Configura la instancia `GeneticAlgorithm` usando:

* **TspFitness**: implementación de la función de aptitud para el TSP, calculando distancias y generando ciudades con posiciones aleatorias dentro de un área delimitada.
* **TspChromosome**: representa un individuo como una permutación única de índices de ciudad. Extiende `ChromosomeBase` e incorpora un campo `Distance` para almacenar la distancia total recorrida.
* **Operadores integrados**: `OrderedCrossover` para cruza, `ReverseSequenceMutation` para mutación y `RouletteWheelSelection` para selección.
* **Población**: parametrizada con tamaños mínimos y máximos (por ejemplo, 50–100). La terminación se define por número de generaciones.

El controlador inicializa el `LineRenderer`, genera las ciudades mediante el componente `CityPrefab`, y arranca el algoritmo en un hilo separado (`m_gaThread`). La actualización visual se realiza en `Update()` mediante `DrawRouteGenetic()`, que obtiene el mejor cromosoma de la generación actual y dibuja la ruta correspondiente.

Esta implementación ofrece ventajas claras: se apoya en componentes probados, reduce el código requerido y permite cambiar operadores con mínima intervención. Sin embargo, sacrifica cierto control fino sobre el ciclo evolutivo y requiere familiaridad con la API de GeneticSharp.

### Implementación del algoritmo genético desarrollada en el proyecto

La versión custom se encuentra en `Assets/Scripts/Propio`. La clase principal `AlgoritmoGenetico` implementa íntegramente el algoritmo siguiendo los pasos clásicos:

* **Representación**: `Cromosoma` almacena una lista de enteros representing a permutation of cities. Incluye métodos de mutación (inserción, intercambio, inversión). `Ciudad` encapsula identificación y posición.
* **Población**: generada aleatoriamente en `makePopulation()`. Se inicializa con `Individuos` cromosomas.
* **Fitness**: `calculateAptitud()` recorre la secuencia de cada cromosoma sumando distancias euclidianas y asigna el valor al campo `Aptitud`.
* **Selección**: tres métodos opcionales (ruleta, torneo, ranking). La elección se controla con el parámetro `MetSelec`.
* **Cruza**: operadores PMX, OX y CX seleccionables mediante `MetCruza`. El método `reproducir()` gestiona la creación de la nueva generación y garantiza el tamaño de la población.
* **Mutación**: aplicada con probabilidad `TazaMuta` usando uno de tres operadores implementados en `Cromosoma`.
* **Evolución generacional**: el método `evolve()` ejecuta un bucle hasta `Generaciones`, realizando selección, cruza, mutación, reemplazo elitista y evaluación de aptitud. El proceso se ejecuta en un hilo separado y comunica la ruta del mejor individuo al hilo principal para visualización.

Esta implementación proporciona control completo sobre cada paso del algoritmo y facilita la experimentación con variantes e innovaciones. La desventaja es la mayor cantidad de código y, potencialmente, mayor propensión a errores.

## Comparación conceptual entre ambas implementaciones

| Aspecto | GeneticSharp | Implementación Propia |
|---------|--------------|-----------------------|
| Flexibilidad | Alta en operadores predefinidos, extensible mediante subclases | Máxima; cualquier operador se implementa directamente |
| Control | Gestión delegada a la biblioteca | Totalmente manual, granular |
| Facilidad de implementación | Rápida, requiere entender API | Requiere desarrollo completo desde cero |
| Personalización | Debe adaptarse a modelo de la biblioteca | Libertad total para ajustar estructuras y lógicas |
| Integración visual | Similar, se desarrolla código adicional | Similar, pero el algoritmo interno es propio |

La coexistencia de ambas versiones en el mismo proyecto permite comparar rendimientos, experimentar con distintas configuraciones y demostrar ventajas pedagógicas de cada enfoque.

## Visualización del algoritmo en Unity

El sistema representa los elementos del TSP mediante objetos 3D y componentes gráficos:

* **Ciudades**: instancias de un prefab con un sprite o malla, acompañadas de un texto con su índice. Se posicionan aleatoriamente en un área definida.
* **Rutas**: renderizadas mediante un componente `LineRenderer` adjunto a un GameObject. Cada vértice corresponde a la posición de una ciudad en el recorrido del cromosoma.
* **Evolución**: La ruta dibujada se actualiza cada generación con los datos del mejor individuo, creándose un efecto de transformación continua.
* **Mejor solución**: se visualiza mediante el trazo permanente del `LineRenderer` y un texto que muestra la secuencia de ciudades y la distancia.

Los componentes utilizados incluyen `GameObject`, `LineRenderer`, `TMP_Text` (TextMesh Pro) y prefabs personalizables. La comunicación entre lógica y visualización se realiza con métodos que construyen o destruyen instancias en la escena y ajustan posiciones.

## Flujo general de ejecución del sistema

1. **Inicio de la aplicación**: se carga la escena seleccionada (`Propio` o `Genetic`), se inician los componentes de control y se establecen referencias.
2. **Configuración de parámetros**: el usuario modifica valores en la interfaz (joystick, botones VR, sliders o textos) mediante `ControlAGPropio` o `ControlParametros`.
3. **Generación de ciudades**: al iniciar el algoritmo, se crean objetos `Ciudad` con posiciones aleatorias dentro de un área determinada.
4. **Ejecución del algoritmo genético**: dependiendo de la escena seleccionada, se arranca la implementación propia o la basada en GeneticSharp en un hilo separado.
5. **Evolución de generaciones**: el algoritmo realiza iteraciones de selección, cruza, mutación y reemplazo. En cada generación, el mejor individuo se evalúa y su ruta se comunica para visualización.
6. **Visualización de resultados**: el hilo principal recibe datos y actualiza el `LineRenderer` para mostrar la ruta óptima actual, junto con información textual sobre fitness.
7. **Finalización**: tras el número de generaciones configurado o al detener manualmente, el algoritmo cesa y el usuario puede observar la solución final.

## Conclusiones técnicas

La arquitectura del sistema destaca por su modularidad y doble implementación de algoritmos genéticos, lo que ofrece un valioso recurso para comparación académica. La integración con Unity permite una visualización clara y dinámica del proceso evolutivo, mientras que la inclusión de soporte VR añade una dimensión inmersiva. La implementación propia brinda control completo y facilidad para experimentar con variantes, mientras que la versión basada en GeneticSharp reduce el esfuerzo de desarrollo y aprovecha componentes maduros. La coexistencia de ambas fortalece el proyecto como entorno educativo y como plataforma de investigación, facilitando exploraciones metodológicas y demostraciones durante la redacción de la tesis.