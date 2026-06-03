# Algoritmo Genético para TSP — TesisFinal

Breve descripción
-----------------
Implementación en Unity de un Algoritmo Genético para resolver el Traveling Salesman Problem (TSP). El proyecto incluye el motor genético (selección, cruce, mutación, elitismo), control por joystick (Windows/Android), visualización en escena (prefab `CityPrefab` + `LineRenderer`) y salida por `TextMeshPro`.

Características Principales
--------------------------
- Inicialización de población y generación de ciudades aleatorias en 3D.
- Evaluación de aptitud por distancia euclidiana (matriz de distancias cacheada).
- Tres métodos de selección: Ruleta, Torneo, Ranking.
- Tres operadores de cruce: PMX, OX, CX (con validaciones y fallback).
- Tres tipos de mutación: Inserción, Intercambio, Inversión.
- Elitismo (top ~10% preservado).
- Evolución en hilo separado para no bloquear la UI.
- UI que muestra generación, distancia y recorrido; trazado visual de la ruta.
- Controles joystick mapeados para Windows y Android.
- Validaciones defensivas y logging detallado.

Estado actual y limitaciones
----------------------------
- Código principal funcional y documentado; `GeneticMain.cs` contiene código mayormente comentado (auxiliar).
- No existe `LICENSE` en la raíz; `Assets/GoogleVR/LICENSE` contiene licencias para esos assets.
- Uso de `Thread.Abort()` detectado: práctica insegura. Recomendado migrar a cancelación cooperativa.
- Pequeñas discrepancias entre documentación y código (rangos de generación de coordenadas y límites de mutación).

Tecnologías Utilizadas
----------------------
| Tecnología | Versión | Propósito |
| ---------- | ------- | --------- |
| Unity Engine | N/D | Entorno de ejecución |
| C# | N/D (scripts Unity) | Lógica del algoritmo |
| .NET Framework | 4.7.1 (workspace) | Contexto de compatibilidad |
| TextMeshPro | N/D | UI de texto |
| Google VR assets | MIT / Google ToS | Assets incluidos |
| System.Threading | N/D | Threading / ejecución en segundo plano |

Estructura del Proyecto (resumida)
----------------------------------
Assets/
- `Assets/Scripts/Propio/AlgoritmoGenetico.cs` — Motor principal.
- `Assets/Scripts/Propio/ControlAGPropio.cs` — Interfaz / mapeo joystick.
- `Assets/Scripts/Propio/Cromosoma.cs` — Individuo y mutaciones.
- `Assets/Scripts/Propio/Ciudad.cs` — Representación de ciudad.
- `Assets/Scripts/Propio/GeneticMain.cs` — Auxiliar / comentado.
- `Assets/Scripts/Propio/VarsGlob.cs` — Variables globales.
- `Assets/GoogleVR/` — Assets de terceros con licencias.

Requisitos Previos
------------------
- Unity Editor compatible con los assets del proyecto.
- TextMeshPro importado.
- Opcional: Android Build Support para builds Android.
- Gamepad/Joystick para control en tiempo de ejecución.
- Visual Studio 2022 recomendado.

Instalación
-----------
1. Clonar el repositorio:
2. Abrir Unity Hub → `Add` → seleccionar `TesisFinal`.
3. Abrir el proyecto en Unity Editor.
4. Importar `TextMeshPro` si falta: __Window > Package Manager__ → instalar.
5. En la escena:
   - Añadir un GameObject con `AlgoritmoGenetico` y asignar `CityPrefab` y `recorrido` (`TMP_Text`).
   - Añadir un GameObject con `ControlAGPropio` y vincular la referencia a `AlgoritmoGenetico`.
6. Ejecutar la escena y usar joystick o las APIs públicas.

Configuración
-------------
Propiedades importantes:
- `Individuos` (int) — clamped entre 25 y 1000.
- `Ciudades` (int) — clamped entre 5 y 35.
- `Generaciones` (int) — clamped entre 50 y 2000.
- `TazaMuta` (float) — clamped entre 0.10f y 0.45f (10%–45%).
- `MetCruza` (int) — 0=PMX,1=OX,2=CX.
- `MetSelec` (int) — 0=Ruleta,1=Torneo,2=Ranking.
- `CityPrefab` (GameObject) y `recorrido` (`TMP_Text`).

Ejecución
---------
- Controles joystick (implementados en `ControlAGPropio`):
  - Windows: Button 0 = INICIA, Button 1 = DETIENE, Button 3 = REINICIA, Button 4 = siguiente parámetro, Button 6 = disminuir, Button 7 = aumentar.
  - Android: mapeo similar (botones 0..5).

Desarrollo (compilar, probar, depurar)
--------------------------------------
- Scripts compilados automáticamente en Unity al guardarlos.
- Depuración: Attach Visual Studio al proceso Unity.
- Pruebas recomendadas: escenario rápido (50 gen, 5 ciudades) para validar flujo.
- No se detectaron tests unitarios automatizados: recomendado añadir.

Scripts Disponibles
-------------------
- `AlgoritmoGenetico.inicia()`, `stop()`, `reStart()`, `best()`, `makePopulation()`, `makeCities()`, `drawCities()`, `deleteCities()`.
- `ControlAGPropio` — mapeo y control de parámetros.

API / Base de Datos
-------------------
- No hay API HTTP ni base de datos. Persistencia no implementada (recomendado).

Despliegue
----------
- Exportar build desde Unity: __File > Build Settings__ → seleccionar plataforma.
- Incluir las escenas y prefabs necesarios.
- Para Android: habilitar `Build Support` en Unity Hub.

Seguridad y Consideraciones
---------------------------
- Reemplazar `Thread.Abort()` por cancelación cooperativa (Join/flags o Task + CancellationToken).
- Validaciones defensivas implementadas (`EsCromosomaValido`, checks de índices, fallbacks).
- Verificar y respetar licencias de `Assets/GoogleVR`.

Roadmap (prioritario)
---------------------
1. Eliminar `Thread.Abort()` y usar cancelación cooperativa.
2. Añadir persistencia (guardar mejor solución).
3. Crear tests unitarios para operadores genéticos.
4. Unificar documentación con los valores reales del código.
5. Añadir `LICENSE` raíz y `CONTRIBUTING.md`, `.editorconfig`.

Documentación Relacionada
-------------------------
- `QUICK_START.md`, `INDICE_DOCUMENTACION.md`, `ANALISIS_SISTEMA_AG.md`, `GUIA_USO_AG.md`, `ANALISIS_TECNICO_ALGORITMOS.md`, `RESUMEN_IMPLEMENTACION.md`, `REGISTRO_CAMBIOS.md`, `Assets/GoogleVR/LICENSE`.

Licencia
--------
- No se encontró `LICENSE` en raíz. Añadir licencia (ej. MIT) antes de publicar.

Créditos
--------
- Repositorio: `https://github.com/Giovapolis/AgenteViajeroTesis` — autor identificado: `Giovapolis`.
- Terceros: GoogleVR, Unity, TextMeshPro.

Contacto / Contribuciones
-------------------------
- Abrir issues / PRs en el repositorio remoto.
- Use `REGISTRO_CAMBIOS.md` para documentar cambios importantes en PRs.