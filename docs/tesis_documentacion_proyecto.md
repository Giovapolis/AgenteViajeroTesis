# Documentación Técnica del Proyecto

## 1. Introducción

El proyecto desarrollado en el entorno de Unity constituye un sistema interactivo de optimización de rutas basado en algoritmos genéticos. Su propósito es resolver instancias del Problema del Viajante de Comercio (TSP) mediante métodos evolutivos, aprovechando la capacidad de Unity para ofrecer simulaciones visuales y, opcionalmente, interfaces en realidad virtual (VR). Este documento presenta una descripción detallada del sistema, su arquitectura, la implementación del AG y la forma en que se integra la visualización y el soporte para VR.

## 2. Descripción general del sistema

A alto nivel, el sistema genera un conjunto de ciudades distribuidas aleatoriamente en el espacio 3D. Una población de soluciones candidatas (cromosomas) representa permutaciones de las ciudades. Un hilo de ejecución independientes aplica ciclos evolutivos que consisten en evaluar la aptitud de cada cromosoma, seleccionar padres, cruzarlos y mutarlos para crear generaciones sucesivas. Paralelamente, el hilo principal de Unity dibuja las ciudades y la mejor ruta encontrada en cada generación, lo que permite al usuario observar la evolución del algoritmo en tiempo real.

## 3. Arquitectura del proyecto

La estructura del proyecto sigue la convención de Unity con carpetas diferenciadas para scripts, prefabs, escenas y recursos externos. Los scripts se agrupan en subdirectorios (`Propio`, `Genetic`, `Utils`) según su función. El módulo central es `AlgoritmoGenetico`, responsable de la lógica evolutiva. `ControlAGPropio` actúa como interfaz entre la entrada del usuario y el algoritmo. Otros componentes, como `Cromosoma` y `Ciudad`, modelan datos básicos. Las interacciones entre scripts se realizan mediante referencias directas y llamadas a métodos; la comunicación con el subsistema de visualización se realiza con un `LineRenderer` en un objeto dedicado.

## 4. Implementación del algoritmo genético

### Representación de individuos
Cada individuo se define como una instancia de la clase `Cromosoma`, que contiene una lista de índices correspondientes a ciudades. Esta permutación lineal representa el orden de visita.

### Población
La población inicial se construye en `makePopulation()` generando cromosomas con recorridos aleatorios que cubren todas las ciudades sin repetición.

### Fitness
La aptitud de un cromosoma se calcula sumando las distancias euclidianas entre ciudades consecutivas en el recorrido, incluyendo el retorno al primer nodo. Valores menores de aptitud indican soluciones mejores.

### Selección
Se proporcionan tres métodos de selección: por ruleta, por torneo y por ranking. Cada uno devuelve índices de individuos escogidos en función de sus aptitudes. La ruleta invierte las aptitudes para favorecer valores menores; el torneo compara subconjuntos aleatorios; el ranking asigna probabilidades basadas en el orden.

### Cruza
Para generar descendencia se implementan tres operadores de cruza adecuados para permutaciones: PMX, OX y CX. El método `reproducir()` itera sobre pares de padres seleccionados, aplicando el operador configurado y corrigiendo el tamaño de la población mediante clonación si es necesario.

### Mutación
Tras la cruza, cada nuevo individuo puede sufrir mutación según una tasa configurable. Los operadores disponibles son inserción, intercambio e inversión, cada uno alterando la permutación de manera distinta.

### Evolución de generaciones
El ciclo evolutivo de `evolve()` combina los pasos anteriores con evaluación de aptitud y reemplazo elitista. Se mantiene una élite de los mejores individuos y se sustituyen los demás con los nuevos generados. El proceso se repite hasta agotar el número de generaciones configurado.

## 5. Parámetros configurables del algoritmo

El sistema expone varios parámetros que el usuario puede ajustar antes de iniciar la ejecución:

- **Número de generaciones**: determinante del número de iteraciones evolutivas.
- **Número de ciudades**: tamaño del grafo de TSP.
- **Tamaño de la población** (individuos): cantidad de cromosomas simultáneos.
- **Tasa de mutación**: probabilidad de que un cromosoma mutado se aplique.
- **Métodos de selección**: ruleta, torneo o ranking (codificados como valores enteros).
- **Métodos de cruza**: PMX, OX o CX.

Estos parámetros se configuran mediante la clase `ControlAGPropio` que lee la entrada del usuario desde un gamepad o dispositivos VR y actualiza los campos públicos del objeto `AlgoritmoGenetico`.

## 6. Sistema de visualización en Unity

### Ciudades
Las ciudades se representan como instancias de un prefab (`CiudadMobil`) colocadas en posiciones aleatorias dentro de un rango definido. Cada instancia incluye un componente de texto para identificar el número de ciudad.

### Rutas
Un objeto `LineRenderer` dibuja la ruta definida por un cromosoma. Se establece un degradado de color y un ancho constante. El renderer se actualiza cada vez que el hilo de evolución señala una nueva mejor solución, copiando los datos de forma thread-safe.

### Evolución de generaciones
Durante la ejecución del AG, cada generación genera una actualización visual del mejor cromosoma. El hilo segundo comunica a través de una bandera y un bloqueo la información necesaria; el hilo principal ejecuta `drawRoute()` en el `Update()` para reflejar los cambios.

## 7. Integración con realidad virtual

El proyecto utiliza el paquete Google VR para proporcionar soporte VR en Android. El control de parámetros y el inicio/detención del algoritmo se mapea a botones del controlador VR. La escena se renderiza en modo estéreo cuando se ejecuta en un dispositivo compatible, permitiendo al usuario moverse y observar la representación espacial de ciudades y rutas dentro de un entorno inmersivo.

## 8. Flujo completo de ejecución

1. Se carga la escena principal (`Propio.unity`) y se inicializa `ControlAGPropio`.
2. El usuario ajusta parámetros mediante interfaz (gamepad/VR).
3. Al pulsar el botón de inicio, `ControlAGPropio` asigna valores al objeto `AlgoritmoGenetico` y llama a `inicia()`.
4. `inicia()` valida parámetros, genera la población inicial y las ciudades, elimina instancias previas y dibuja el estado inicial.
5. Se lanza un hilo de fondo (`evolve()`) que ejecuta el ciclo evolutivo.
6. Cada generación realiza selección, cruza, mutación y reemplazo; también evalúa aptitud y envía datos de la mejor solución al hilo principal.
7. El hilo principal actualiza la visualización mediante `drawRoute()` en `Update()`.
8. Cuando se alcanza el número de generaciones, el hilo finaliza y el sistema queda inactivo, mostrando la mejor ruta encontrada.

## 9. Conclusiones técnicas

El sistema integra de manera eficiente un algoritmo genético con capacidades de visualización interactiva en Unity. La separación entre el cálculo evolutivo y la visualización permite un rendimiento fluido incluso en plataformas móviles y VR. La modularidad de la arquitectura facilita la extensión y experimentación con diferentes operadores y parámetros. Entre las ventajas del enfoque se encuentran la claridad conceptual para la enseñanza y la capacidad de observar en tiempo real la convergencia del algoritmo. Las principales limitaciones están relacionadas con la escalabilidad a muy grandes instancias del TSP y la dependencia de parámetros finamente ajustados. Futuras mejoras podrían incluir paralelización más profunda, algoritmos híbridos o análisis estadístico de resultados.
