# Comparación de Algoritmos Genéticos para el TSP en Unity

Este documento compara las dos implementaciones del algoritmo genético (AG) presentes en el proyecto: la basada en la biblioteca **GeneticSharp** y la desarrollada **manualmente** desde cero. El análisis se centra en aspectos técnicos, arquitectónicos y prácticos, con el fin de proporcionar una base para la evaluación académica en el contexto de una tesis de ingeniería.

---

## 1. Diferencias arquitectónicas

### Implementación con GeneticSharp

La arquitectura se apoya en la biblioteca externa GeneticSharp, que proporciona una estructura modular y extensible:

- **Componentes principales**: `GeneticAlgorithm` como núcleo, con interfaces como `IFitness`, `IChromosome`, `ICrossover`, `IMutation`, `ISelection`.
- **Separación de responsabilidades**: el problema (TSP) se modela en clases específicas (`TspFitness`, `TspChromosome`), mientras que el algoritmo general se configura en `GAController`.
- **Eventos y delegados**: el ciclo evolutivo se maneja mediante eventos (`GenerationRan`), permitiendo hooks para visualización o logging.
- **Multihilo**: utiliza `ParallelTaskExecutor` dentro de GeneticSharp para paralelizar evaluaciones.

### Implementación manual

La arquitectura es monolítica y autocontenida, sin dependencias externas para el AG:

- **Clase central**: `AlgoritmoGenetico` encapsula todo el ciclo evolutivo (selección, cruza, mutación, evaluación) en métodos privados.
- **Modelado simple**: `Cromosoma` y `Ciudad` son clases básicas sin interfaces abstractas; la lógica de operadores se implementa directamente.
- **Hilos manuales**: se utiliza `System.Threading.Thread` para ejecutar el bucle evolutivo en un hilo separado, con sincronización manual para visualización.
- **Integración directa**: la evaluación de aptitud y visualización se entrelazan en la misma clase, facilitando el acceso a datos de Unity.

**Diferencia clave**: GeneticSharp impone una arquitectura basada en interfaces y composición, mientras que la manual es procedural y orientada a objetos simple, con menos abstracciones.

---

## 2. Diferencias en operadores genéticos

### Operadores en GeneticSharp

- **Selección**: limitada a operadores predefinidos como `RouletteWheelSelection`. No se implementan torneo o ranking en el código actual.
- **Cruza**: utiliza `OrderedCrossover` (equivalente a OX), que preserva el orden relativo de genes.
- **Mutación**: `ReverseSequenceMutation`, que invierte un segmento aleatorio de la secuencia.
- **Fitness**: cálculo personalizado en `TspFitness`, con penalización por ciudades repetidas y normalización del fitness (1.0 - distancia / maxDistancia).

### Operadores en la implementación manual

- **Selección**: tres métodos implementados: ruleta (con inversión de aptitudes), torneo (con tamaño configurable) y ranking (lineal).
- **Cruza**: tres operadores: PMX (mapeo parcial), OX (orden) y CX (ciclo), todos implementados desde cero.
- **Mutación**: tres tipos: inserción, intercambio e inversión, aplicados con probabilidad configurable.
- **Fitness**: distancia euclidiana directa, sin normalización ni penalizaciones adicionales; minimización pura.

**Diferencia clave**: la manual ofrece mayor variedad y control fino sobre operadores, permitiendo experimentación con variantes no disponibles en GeneticSharp sin modificar la biblioteca.

---

## 3. Ventajas y desventajas

### Ventajas de GeneticSharp

- **Rapidez de desarrollo**: configuración rápida mediante componentes reutilizables; no requiere implementar operadores básicos.
- **Robustez**: operadores probados y optimizados; manejo automático de paralelismo y eventos.
- **Extensibilidad**: fácil agregar nuevos operadores heredando de interfaces; comunidad activa para soporte.
- **Separación clara**: facilita testing unitario y modularidad.

### Desventajas de GeneticSharp

- **Dependencia externa**: requiere incluir la biblioteca completa, aumentando el tamaño del proyecto.
- **Menos control**: operadores fijos limitan la experimentación; cambios requieren subclases o forks.
- **Curva de aprendizaje**: familiaridad con la API de GeneticSharp necesaria.
- **Overhead**: paralelismo interno puede no optimizarse para escenarios específicos de Unity.

### Ventajas de la implementación manual

- **Transparencia total**: cada línea de código es visible y modificable; ideal para educación y debugging.
- **Flexibilidad máxima**: operadores personalizables sin límites; fácil añadir variantes o métricas.
- **Independencia**: no depende de bibliotecas externas; portable y autocontenido.
- **Integración nativa**: sincronización con Unity (hilos, visualización) se maneja directamente.

### Desventajas de la implementación manual

- **Mayor complejidad de desarrollo**: implementar operadores desde cero aumenta el riesgo de errores.
- **Mantenimiento**: cambios en algoritmos requieren editar código manualmente; no hay reutilización automática.
- **Rendimiento potencial**: paralelismo limitado; optimizaciones requieren esfuerzo adicional.
- **Escalabilidad**: para problemas más complejos, el código puede volverse difícil de mantener.

---

## 4. Flexibilidad de cada enfoque

### Flexibilidad en GeneticSharp

- **Alta en configuración**: fácil cambiar operadores existentes (p.ej. `ICrossover` a `PartiallyMappedCrossover`).
- **Media en extensión**: agregar nuevos operadores requiere implementar interfaces, pero es estructurado.
- **Baja en modificación interna**: operadores predefinidos no se alteran fácilmente sin modificar la biblioteca.
- **Ejemplo**: cambiar selección a torneo implica instanciar `TournamentSelection`, pero no personalizar el tamaño del torneo sin subclase.

### Flexibilidad en la implementación manual

- **Máxima**: cualquier aspecto (probabilidades, tamaños, lógica de operadores) se modifica editando código directo.
- **Alta en experimentación**: añadir un nuevo operador de mutación es tan simple como crear un método nuevo y llamarlo.
- **Baja en reutilización**: flexibilidad viene a costa de no poder "plug and play" componentes de otras bibliotecas.
- **Ejemplo**: ajustar el tamaño del torneo en selección requiere cambiar una línea de código; añadir un operador híbrido es directo.

**Conclusión**: la manual es más flexible para investigación pura, mientras que GeneticSharp es flexible para prototipado rápido con componentes estándar.

---

## 5. Rendimiento esperado

### Rendimiento de GeneticSharp

- **Ventaja en paralelismo**: `ParallelTaskExecutor` puede acelerar evaluaciones en CPUs multi-core.
- **Eficiencia en operadores**: algoritmos optimizados reducen overhead.
- **Escalabilidad**: maneja poblaciones grandes mejor debido a la madurez de la biblioteca.
- **Limitación**: overhead de eventos y composición puede afectar en escenarios pequeños o móviles.

### Rendimiento de la implementación manual

- **Eficiencia en escenarios simples**: menos overhead; adecuado para poblaciones medianas (50-500 individuos).
- **Paralelismo limitado**: solo el hilo del AG; evaluaciones secuenciales.
- **Optimización manual**: potencial para mejoras específicas (p.ej. memoización en distancias), pero requiere esfuerzo.
- **Limitación**: en problemas grandes, el bucle manual puede ser más lento debido a implementaciones no optimizadas.

**Estimación cuantitativa**: para un TSP con 50 ciudades y 100 generaciones, GeneticSharp podría ser 20-30% más rápido en hardware multi-core debido al paralelismo; la manual es comparable en single-thread pero más predecible en dispositivos móviles.

---

## 6. Mantenibilidad del código

### Mantenibilidad en GeneticSharp

- **Alta**: código modular facilita cambios; actualizaciones de la biblioteca pueden mejorar sin tocar el proyecto.
- **Baja complejidad local**: el código del proyecto es corto; bugs se aíslan en componentes de la biblioteca.
- **Desventaja**: dependencias externas pueden introducir incompatibilidades en futuras versiones de Unity o .NET.

### Mantenibilidad en la implementación manual

- **Media**: código autocontenido es fácil de entender, pero cambios en algoritmos requieren editar múltiples métodos.
- **Alta visibilidad**: bugs son evidentes y corregibles directamente; no hay "caja negra".
- **Desventaja**: refactorización (p.ej. añadir paralelismo) requiere reescribir secciones enteras.

**Conclusión**: GeneticSharp es más mantenible a largo plazo para proyectos estables; la manual lo es para prototipos educativos donde la transparencia es clave.

---

## 7. Integración con Unity

### Integración de GeneticSharp

- **Buena**: `GAController` se integra bien con GameObjects y componentes; eventos permiten actualizar UI/visualización.
- **Hilos**: usa `Thread` para el AG, similar a la manual; sincronización manual para `LineRenderer`.
- **Dependencias**: requiere incluir `GeneticSharp.dll` o fuentes; compatible con Unity's .NET.
- **Limitación**: paralelismo interno puede interferir con el hilo principal de Unity en builds móviles.

### Integración de la implementación manual

- **Excelente**: diseñada específicamente para Unity; acceso directo a `Vector3`, `GameObject`, `TMP_Text`.
- **Sincronización**: `visualizationPending` y `lock` aseguran thread-safety para renderizado.
- **Simplicidad**: no hay capas adicionales; cambios en visualización se hacen editando `drawRoute`.
- **Limitación**: requiere más código para manejar hilos, pero es más controlable.

**Conclusión**: ambas integran bien, pero la manual es más "nativa" a Unity, facilitando personalizaciones visuales; GeneticSharp es mejor para separación de concerns.

---

### Resumen comparativo

| Aspecto | GeneticSharp | Implementación Manual |
|---------|--------------|-----------------------|
| Arquitectura | Modular, basada en interfaces | Monolítica, procedural |
| Operadores | Limitados pero probados | Amplios y personalizables |
| Ventajas | Rapidez, robustez | Transparencia, flexibilidad |
| Desventajas | Dependencia, menos control | Complejidad, mantenimiento |
| Flexibilidad | Media-alta en configuración | Máxima en modificación |
| Rendimiento | Mejor en paralelo | Eficiente en simple |
| Mantenibilidad | Alta a largo plazo | Media para prototipos |
| Integración Unity | Buena, con eventos | Excelente, nativa |

Esta comparación resalta que GeneticSharp es ideal para aplicaciones prácticas y rápidas, mientras que la implementación manual es superior para investigación académica y educación. En el contexto de una tesis, la coexistencia de ambas permite demostrar trade-offs y validar resultados empíricamente.</content>
<parameter name="filePath">c:\Users\Giovapolis\Documents\TesisFinal\comparacion_algoritmos_tsp.md