using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlgoritmoGenetico : MonoBehaviour {

    [SerializeField]
    public TMP_Text recorrido;
    public GameObject CityPrefab;

    private System.Random rnd = new System.Random();
    private bool isRunning = false;
    private Thread evolutionThread = null;

    private Cromosoma[] poblacion;
    private Ciudad[] cities;
    private List<GameObject> ciudadesInstanciadas = new List<GameObject>();
    private LineRenderer rutaRenderer;

    private int[] selectedIndices;
    private float[] aptitudesInvertidas;
    private Cromosoma[] childPopulation;

    public int GeneracionesPorFrame { get; set; } = 1;
    public int MaxCiudades { get; set; } = 35;
    public int MaxIndividuos { get; set; } = 1000;

    // ========== VARIABLES COMPARTIDAS PARA SINCRONIZACIÓN THREAD ==========
    private object lockObject = new object();
    private int _generacionActual = 0;
    private List<int> _mejorRuta = new List<int>();
    private float _mejorDistancia = float.MaxValue;
    private bool _hasNewData = false;

    // ========== CACHE DE DISTANCIAS PARA OPTIMIZACIÓN ==========
    private float[,] distancias;

    private int individuos;
    private int ciudades;
    private int generaciones;
    private float tazaMuta;
    private int generacionActual = 0;

    private int metSelec = 0;

    // ========== FUNCIÓN DE VALIDACIÓN DE CROMOSOMAS ==========
    private bool EsCromosomaValido(List<int> recorrido, int numCiudades) {
        if (recorrido == null || recorrido.Count != numCiudades) return false;
        var distinct = recorrido.Distinct();
        if (distinct.Count() != numCiudades) return false;
        foreach (int city in recorrido) {
            if (city < 0 || city >= numCiudades) return false;
        }
        return true;
    }
    private int metCruza = 0;

    public int Individuos { get => individuos; set => individuos = value; }
    public int Ciudades { get => ciudades; set => ciudades = value; }
    public int Generaciones { get => generaciones; set => generaciones = value; }
    public float TazaMuta { get => tazaMuta; set => tazaMuta = value; }
    public int MetSelec { get => metSelec; set => metSelec = value; }
    public int MetCruza { get => metCruza; set => metCruza = value; }
    public int GeneracionActual { get => _generacionActual; }

    public AlgoritmoGenetico() {
    }

    void Update() {
        // ========== ACTUALIZAR VISUALIZACIÓN CUANDO HAY NUEVOS DATOS ==========
        if (_hasNewData) {
            lock (lockObject) {
                if (_hasNewData) {
                    // Actualizar texto
                    string rutaTexto = FormatoRuta(_mejorRuta);
                    string texto = $"Generación: {_generacionActual}\nDistancia: {_mejorDistancia:F2}\nRuta: {rutaTexto}";
                    recorrido.text = texto;

                    // Actualizar visualización de ruta
                    drawRouteFromData(_mejorRuta, _mejorDistancia);

                    _hasNewData = false;
                }
            }
        }
    }

    public void inicia() {
        Debug.Log("═════════════════════════════════════════════════════════════");
        Debug.Log(">>> INICIA ALGORITMO GENÉTICO <<<");
        Debug.Log("═════════════════════════════════════════════════════════════");
        
        // ========== VALIDACIÓN EXHAUSTIVA DE PARÁMETROS ==========
        // 1. VALIDAR GENERACIONES (CRÍTICO)
        int genOriginal = Generaciones;
        Generaciones = Mathf.Clamp(Generaciones, 50, 2000);
        if (Generaciones != genOriginal) {
            Debug.LogWarning($"⚠️ GENERACIONES AJUSTADO: {genOriginal} → {Generaciones} (rango: 50-2000)");
        }

        // 2. VALIDAR INDIVIDUOS
        int indOriginal = Individuos;
        Individuos = Mathf.Clamp(Individuos, 25, 1000);
        if (Individuos != indOriginal) {
            Debug.LogWarning($"⚠️ INDIVIDUOS AJUSTADO: {indOriginal} → {Individuos} (rango: 25-1000)");
        }

        // 3. VALIDAR CIUDADES
        int civOriginal = Ciudades;
        Ciudades = Mathf.Clamp(Ciudades, 5, 35);
        if (Ciudades != civOriginal) {
            Debug.LogWarning($"⚠️ CIUDADES AJUSTADO: {civOriginal} → {Ciudades} (rango: 5-35)");
        }

        // 4. VALIDAR TASA DE MUTACIÓN
        float tazaOriginal = TazaMuta;
        TazaMuta = Mathf.Clamp(TazaMuta, 0.10f, 0.45f);
        if (TazaMuta != tazaOriginal) {
            Debug.LogWarning($"⚠️ TAZA MUTACIÓN AJUSTADA: {tazaOriginal:F3} → {TazaMuta:F3} (rango: 10%-45%)");
        }

        // 5. VALIDAR MÉTODOS
        if (MetSelec < 0 || MetSelec > 2) {
            Debug.LogWarning($"⚠️ MÉTODO SELECCIÓN INVÁLIDO: {MetSelec} → ajustado a 0 (Ruleta)");
            MetSelec = 0;
        }
        if (MetCruza < 0 || MetCruza > 2) {
            Debug.LogWarning($"⚠️ MÉTODO CRUZAMIENTO INVÁLIDO: {MetCruza} → ajustado a 0 (PMX)");
            MetCruza = 0;
        }

        // ========== LOGGING EXHAUSTIVO DE PARÁMETROS FINALES ==========
        Debug.Log("┌─ PARÁMETROS FINALES CONFIRMADOS ─────────────────────────┐");
        Debug.Log($"│ Ciudades:           {Ciudades}");
        Debug.Log($"│ Individuos:         {Individuos}");
        Debug.Log($"│ Generaciones:       {Generaciones} ← NÚMERO OBJETIVO");
        Debug.Log($"│ Tasa Mutación:      {(TazaMuta * 100):F1}% ({TazaMuta:F3})");
        Debug.Log($"│ Método Selección:   {ObtenerNombreSeleccion(MetSelec)} (opción {MetSelec})");
        Debug.Log($"│ Método Cruzamiento: {ObtenerNombreCruce(MetCruza)} (opción {MetCruza})");
        Debug.Log("└───────────────────────────────────────────────────────────┘");

        isRunning = true;
        _generacionActual = 0;
        _mejorDistancia = float.MaxValue;
        _hasNewData = false;

        selectedIndices = new int[Individuos];
        childPopulation = new Cromosoma[Individuos];

        makePopulation();
        
        // SIEMPRE eliminar y volver a crear las ciudades
        deleteCities();
        makeCities();
        drawCities(); // Instanciar ciudades visuales al inicio
        
        // Inicializar el renderer de ruta
        initializeRouteRenderer();

        evaluatePopulation();

        // Inicializar mejor ruta inicial
        Cromosoma mejorInicial = best();
        lock (lockObject) {
            _mejorRuta = new List<int>(mejorInicial.Recorrido);
            _mejorDistancia = mejorInicial.Aptitud;
            _hasNewData = true;
        }

        // Iniciar Thread para evolución
        if (evolutionThread != null) {
            evolutionThread.Abort();
        }
        evolutionThread = new Thread(EvolutionThread);
        evolutionThread.Start();
        Debug.Log("AG iniciado en Thread separado");
}
    public void stop() {
        Debug.Log("⏹️ DETENIENDO ALGORITMO GENÉTICO");
        Debug.Log($"   Generaciones ejecutadas: {_generacionActual}");
        Debug.Log($"   Mejor distancia encontrada: {_mejorDistancia:F4}");
        
        isRunning = false;
        
        if (evolutionThread != null && evolutionThread.IsAlive) {
            Debug.Log("   Esperando a que el thread finalice...");
            
            // Dar tiempo al thread para terminar naturalmente
            if (!evolutionThread.Join(5000)) {  // Esperar máximo 5 segundos
                Debug.LogWarning("   ⚠️ Thread no finalizó en 5 segundos. Abortando...");
                evolutionThread.Abort();
            }
            
            evolutionThread = null;
            Debug.Log("   ✅ Thread finalizado");
        } else {
            Debug.Log("   ℹ️ No hay thread activo");
        }
    }

    public void reStart() {
        Debug.Log("═════════════════════════════════════════════════════════════");
        Debug.Log(">>> REINICIANDO ALGORITMO GENÉTICO - MANTENIENDO CIUDADES <<<");
        Debug.Log("═════════════════════════════════════════════════════════════");

        // ========== VALIDACIÓN EXHAUSTIVA DE PARÁMETROS ==========
        // 1. VALIDAR GENERACIONES (CRÍTICO)
        int genOriginal = Generaciones;
        Generaciones = Mathf.Clamp(Generaciones, 50, 2000);
        if (Generaciones != genOriginal) {
            Debug.LogWarning($"⚠️ GENERACIONES AJUSTADO: {genOriginal} → {Generaciones} (rango: 50-2000)");
        }

        // 2. VALIDAR INDIVIDUOS
        int indOriginal = Individuos;
        Individuos = Mathf.Clamp(Individuos, 25, 1000);
        if (Individuos != indOriginal) {
            Debug.LogWarning($"⚠️ INDIVIDUOS AJUSTADO: {indOriginal} → {Individuos} (rango: 25-1000)");
        }

        // 3. VALIDAR CIUDADES (NO CAMBIA, PERO SE VALIDA)
        int civOriginal = Ciudades;
        Ciudades = Mathf.Clamp(Ciudades, 5, 35);
        if (Ciudades != civOriginal) {
            Debug.LogWarning($"⚠️ CIUDADES AJUSTADO: {civOriginal} → {Ciudades} (rango: 5-35)");
        }

        // 4. VALIDAR TASA DE MUTACIÓN
        float tazaOriginal = TazaMuta;
        TazaMuta = Mathf.Clamp(TazaMuta, 0.10f, 0.45f);
        if (TazaMuta != tazaOriginal) {
            Debug.LogWarning($"⚠️ TAZA MUTACIÓN AJUSTADA: {tazaOriginal:F3} → {TazaMuta:F3} (rango: 10%-45%)");
        }

        // 5. VALIDAR MÉTODOS
        if (MetSelec < 0 || MetSelec > 2) {
            Debug.LogWarning($"⚠️ MÉTODO SELECCIÓN INVÁLIDO: {MetSelec} → ajustado a 0 (Ruleta)");
            MetSelec = 0;
        }
        if (MetCruza < 0 || MetCruza > 2) {
            Debug.LogWarning($"⚠️ MÉTODO CRUZAMIENTO INVÁLIDO: {MetCruza} → ajustado a 0 (PMX)");
            MetCruza = 0;
        }

        // ========== LOGGING EXHAUSTIVO DE PARÁMETROS FINALES ==========
        Debug.Log("┌─ PARÁMETROS REINICIO CONFIRMADOS ───────────────────────┐");
        Debug.Log($"│ Ciudades:           {Ciudades} (MANTENIDAS)");
        Debug.Log($"│ Individuos:         {Individuos}");
        Debug.Log($"│ Generaciones:       {Generaciones} ← NÚMERO OBJETIVO");
        Debug.Log($"│ Tasa Mutación:      {(TazaMuta * 100):F1}% ({TazaMuta:F3})");
        Debug.Log($"│ Método Selección:   {ObtenerNombreSeleccion(MetSelec)} (opción {MetSelec})");
        Debug.Log($"│ Método Cruzamiento: {ObtenerNombreCruce(MetCruza)} (opción {MetCruza})");
        Debug.Log("└───────────────────────────────────────────────────────────┘");

        stop();

        // Esperar breve tiempo para asegurar que el thread anterior finalizó
        Thread.Sleep(100);

        ResetEvolution();
    }

    /// <summary>
    /// Reinicializa SOLO el algoritmo manteniendo las ciudades intactas.
    /// - NO regenera ciudades
    /// - NO destruye GameObjects de ciudades
    /// - Solo reinicializa población y parámetros de evolución
    /// - Los parámetros ya fueron validados en reStart()
    /// </summary>
    private void ResetEvolution() {
        Debug.Log("Reseteando estado de evolución (ciudades se mantienen)");
        
        // Reinicializar bandera y contadores
        isRunning = true;
        _generacionActual = 0;
        _mejorDistancia = float.MaxValue;
        _hasNewData = false;

        // Reinicializar arreglos de algoritmo
        selectedIndices = new int[Individuos];
        childPopulation = new Cromosoma[Individuos];

        // Asegurar que el renderer de ruta esté inicializado y actualizado
        initializeRouteRenderer();

        // Reinicializar población con MISMAS ciudades
        makePopulation();
        evaluatePopulation();

        // Obtener mejor ruta inicial
        Cromosoma mejorInicial = best();
        lock (lockObject) {
            _mejorRuta = new List<int>(mejorInicial.Recorrido);
            _mejorDistancia = mejorInicial.Aptitud;
            _hasNewData = true;
        }

        // Iniciar nuevo Thread para evolución
        if (evolutionThread != null) {
            evolutionThread.Abort();
        }
        evolutionThread = new Thread(EvolutionThread);
        evolutionThread.Start();
        Debug.Log("AG reiniciado - Ciudades mantenidas, nuevo Thread iniciado");
    }

    private void EvolutionThread() {
        try {
            Debug.Log($"🧬 THREAD DE EVOLUCIÓN INICIADO - Ejecutará {Generaciones} generaciones");
            
            while (isRunning && _generacionActual < Generaciones) {
                try {
                    // 1. Selección
                    SeleccionarIndices();

                    // 2. Cruza
                    Reproducir();

                    // 3. Mutación
                    MutarPoblacion(childPopulation);

                    // 4. Reemplazo de población (elitismo)
                    ReemplazarPoblacion(childPopulation);

                    // 5. Evaluar nueva población
                    evaluatePopulation();

                    // 6. Actualizar mejor ruta en variables compartidas
                    Cromosoma mejorActual = best();
                    lock (lockObject) {
                        _generacionActual++;
                        if (mejorActual.Aptitud < _mejorDistancia) {
                            _mejorRuta = new List<int>(mejorActual.Recorrido);
                            _mejorDistancia = mejorActual.Aptitud;
                        }
                        _hasNewData = true;
                    }

                    // LOG CADA 25 GENERACIONES (o menos si pocas generaciones)
                    if (Generaciones <= 50 || _generacionActual % 25 == 0 || _generacionActual == Generaciones) {
                        Debug.Log($"  Gen {_generacionActual:D4}/{Generaciones:D4} | Mejor distancia: {_mejorDistancia:F4}");
                    }

                    // Pequeña pausa para no consumir CPU al 100%
                    Thread.Sleep(1);
                } 
                catch (Exception genException) {
                    Debug.LogError(
                        "════════════════════════════════════════════════════════════\n" +
                        $"❌ ERROR EN GENERACIÓN {_generacionActual + 1}/{Generaciones}\n" +
                        $"Excepción: {genException.GetType().Name}\n" +
                        $"Mensaje: {genException.Message}\n" +
                        $"StackTrace:\n{genException.StackTrace}\n" +
                        "════════════════════════════════════════════════════════════");
                    throw;  // Re-lanzar para que sea capturada por el catch externo
                }
            }

            // Verificar si se completó correctamente o fue interrumpido
            if (_generacionActual >= Generaciones) {
                Debug.Log($"✅ AG COMPLETADO EXITOSAMENTE - {_generacionActual}/{Generaciones} generaciones ejecutadas");
                Debug.Log($"   Mejor distancia encontrada: {_mejorDistancia:F4}");
            } else {
                Debug.LogWarning($"⚠️ AG INTERRUMPIDO - Solo {_generacionActual}/{Generaciones} generaciones ejecutadas (isRunning={isRunning})");
            }
        } 
        catch (ThreadAbortException) {
            Debug.Log($"ℹ️ Thread abortado en generación {_generacionActual}/{Generaciones}");
        } 
        catch (Exception e) {
            Debug.LogError(
                "════════════════════════════════════════════════════════════\n" +
                "❌ ERROR CRÍTICO EN THREAD DE EVOLUCIÓN\n" +
                $"Generaciones completadas: {_generacionActual}/{Generaciones}\n" +
                $"Tipo de excepción: {e.GetType().Name}\n" +
                $"Mensaje: {e.Message}\n" +
                $"StackTrace:\n{e.StackTrace}\n" +
                "════════════════════════════════════════════════════════════");
        }
    }



    public Cromosoma best() {
        if (poblacion == null || poblacion.Length == 0)
            return new Cromosoma();
            
        Cromosoma win = poblacion[0];
        for (int i = 1; i < poblacion.Length; i++) {
            if (poblacion[i].Aptitud < win.Aptitud) {
                win = poblacion[i];
            }
        }
        return win;
    }



    private string FormatoRuta(List<int> recorrido) {
        if (recorrido == null || recorrido.Count == 0)
            return "N/A";

        // Construir string manualmente para evitar LINQ en bucles críticos
        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < recorrido.Count; i++) {
            builder.Append(recorrido[i]);
            if (i < recorrido.Count - 1) {
                builder.Append(" -> ");
            }
        }
        return builder.ToString();
    }

    private void drawRouteFromData(List<int> ruta, float distancia) {
        if (rutaRenderer == null || ruta == null || ruta.Count == 0) {
            return;
        }

        // Establecer número de posiciones (ruta + retorno a inicio)
        int positionCount = ruta.Count + 1;
        rutaRenderer.positionCount = positionCount;

        // Dibujar líneas entre ciudades
        for (int i = 0; i < ruta.Count; i++) {
            int ciudadIndex = ruta[i];
            if (ciudadIndex >= 0 && ciudadIndex < cities.Length) {
                rutaRenderer.SetPosition(i, cities[ciudadIndex].Ubicacion);
            }
        }

        // Cerrar la ruta (retorno a la primera ciudad)
        if (ruta.Count > 0) {
            rutaRenderer.SetPosition(ruta.Count, cities[ruta[0]].Ubicacion);
        }
    }



    // ========== EVALUACIÓN DE APTITUD ==========
    private void evaluatePopulation() {
        // VALIDACIÓN: Asegurar que distancias fue calculada
        if (distancias == null || distancias.GetLength(0) == 0) {
            Debug.LogError("❌ ERROR: Matriz de distancias no inicializada en evaluatePopulation");
            return;
        }
        
        // VALIDACIÓN: Asegurar que población existe
        if (poblacion == null || poblacion.Length == 0) {
            Debug.LogError("❌ ERROR: Población nula o vacía en evaluatePopulation");
            return;
        }

        foreach (Cromosoma cromosoma in poblacion) {
            if (cromosoma != null) {
                calculateAptitud(cromosoma);
            }
        }
    }

    private void calculateAptitud(Cromosoma cromosoma) {
        // VALIDACIÓN: Verificar que el cromosoma es válido
        if (cromosoma == null || cromosoma.Recorrido == null || cromosoma.Recorrido.Count == 0) {
            Debug.LogWarning("⚠️ WARNING: Cromosoma inválido en calculateAptitud");
            cromosoma.Aptitud = float.MaxValue;
            return;
        }

        // VALIDACIÓN: Verificar que es una permutación válida TSP
        if (!EsCromosomaValido(cromosoma.Recorrido, Ciudades)) {
            Debug.LogWarning($"⚠️ WARNING: Cromosoma no es permutación válida en calculateAptitud: {string.Join(",", cromosoma.Recorrido)}");
            cromosoma.Aptitud = float.MaxValue;
            return;
        }

        float distanciaTotal = 0f;
        
        // VALIDACIÓN: Verificar índices
        try {
            for (int i = 0; i < cromosoma.Recorrido.Count - 1; i++) {
                int ciudadActual = cromosoma.Recorrido[i];
                int ciudadSiguiente = cromosoma.Recorrido[i + 1];
                
                // Validar que los índices estén en rango
                if (ciudadActual < 0 || ciudadActual >= distancias.GetLength(0) ||
                    ciudadSiguiente < 0 || ciudadSiguiente >= distancias.GetLength(0)) {
                    Debug.LogError($"❌ ERROR: Índice fuera de rango en calculateAptitud - [{ciudadActual}, {ciudadSiguiente}]");
                    cromosoma.Aptitud = float.MaxValue;
                    return;
                }
                
                distanciaTotal += distancias[ciudadActual, ciudadSiguiente];
            }
            
            // Agregar distancia de retorno (último a primero)
            if (cromosoma.Recorrido.Count > 0) {
                int lastCity = cromosoma.Recorrido[cromosoma.Recorrido.Count - 1];
                int firstCity = cromosoma.Recorrido[0];
                
                if (lastCity >= 0 && lastCity < distancias.GetLength(0) &&
                    firstCity >= 0 && firstCity < distancias.GetLength(0)) {
                    distanciaTotal += distancias[lastCity, firstCity];
                } else {
                    Debug.LogError($"❌ ERROR: Índices fuera de rango en distancia de retorno");
                    cromosoma.Aptitud = float.MaxValue;
                    return;
                }
            }
            
            cromosoma.Aptitud = distanciaTotal;
            
            // Validación final
            if (float.IsNaN(cromosoma.Aptitud) || float.IsInfinity(cromosoma.Aptitud)) {
                Debug.LogError($"❌ ERROR: Aptitud inválida (NaN o Infinity): {cromosoma.Aptitud}");
                cromosoma.Aptitud = float.MaxValue;
            }
        }
        catch (Exception e) {
            Debug.LogError($"❌ ERROR en calculateAptitud: {e.Message}");
            cromosoma.Aptitud = float.MaxValue;
        }
    }

    // ========== MÉTODOS DE SELECCIÓN ==========
    private void SeleccionarIndices() {
        if (selectedIndices == null || selectedIndices.Length != Individuos) {
            selectedIndices = new int[Individuos];
        }

        switch (MetSelec) {
            case 0:
                SeleccionPorRuleta();
                break;
            case 1:
                SeleccionPorTorneo();
                break;
            case 2:
                SeleccionPorRanking();
                break;
            default:
                SeleccionPorRuleta();
                break;
        }
    }

    private void SeleccionPorRuleta() {
        // VALIDACIÓN: Asegurar que poblacion existe
        if (poblacion == null || poblacion.Length == 0) {
            Debug.LogError("❌ ERROR: Población nula en SeleccionPorRuleta");
            for (int i = 0; i < Individuos; i++) {
                selectedIndices[i] = 0;
            }
            return;
        }

        // Invertir aptitudes (menor distancia = mayor probabilidad)
        float maxApt = float.MinValue;
        for (int i = 0; i < poblacion.Length; i++) {
            if (poblacion[i].Aptitud > maxApt) maxApt = poblacion[i].Aptitud;
        }

        if (aptitudesInvertidas == null || aptitudesInvertidas.Length != Individuos) {
            aptitudesInvertidas = new float[Individuos];
        }

        float aptTotal = 0f;
        for (int i = 0; i < poblacion.Length; i++) {
            aptitudesInvertidas[i] = maxApt - poblacion[i].Aptitud;
            aptTotal += aptitudesInvertidas[i];
        }

        // VALIDACIÓN: Si aptTotal es 0 o negativo, usar selección aleatoria uniforme
        if (aptTotal <= 0f) {
            Debug.LogWarning("⚠️ WARNING: aptTotal <= 0 en SeleccionPorRuleta. Usando selección aleatoria uniforme.");
            for (int i = 0; i < Individuos; i++) {
                selectedIndices[i] = rnd.Next(poblacion.Length);
            }
            return;
        }

        // Selección por ruleta
        for (int i = 0; i < Individuos; i++) {
            float prob = (float)rnd.NextDouble() * aptTotal;
            float acum = 0f;
            bool found = false;

            for (int j = 0; j < poblacion.Length; j++) {
                acum += aptitudesInvertidas[j];
                if (acum >= prob) {
                    selectedIndices[i] = j;
                    found = true;
                    break;
                }
            }

            // VALIDACIÓN: Si no se encontró índice, usar uno aleatorio
            if (!found || selectedIndices[i] < 0 || selectedIndices[i] >= poblacion.Length) {
                selectedIndices[i] = rnd.Next(poblacion.Length);
            }
        }
    }

    private void SeleccionPorTorneo() {
        int tamanoTorneo = Mathf.Max(2, Individuos / 4);

        for (int i = 0; i < Individuos; i++) {
            int mejor = rnd.Next(poblacion.Length);
            for (int j = 1; j < tamanoTorneo; j++) {
                int candidato = rnd.Next(poblacion.Length);
                if (poblacion[candidato].Aptitud < poblacion[mejor].Aptitud) {
                    mejor = candidato;
                }
            }
            selectedIndices[i] = mejor;
        }
    }

    private void SeleccionPorRanking() {
        int n = poblacion.Length;
        int[] orden = new int[n];
        for (int i = 0; i < n; i++) orden[i] = i;

        // Ordenación simple por aptitud ascendente
        for (int i = 0; i < n - 1; i++) {
            for (int j = i + 1; j < n; j++) {
                if (poblacion[orden[j]].Aptitud < poblacion[orden[i]].Aptitud) {
                    int tmp = orden[i];
                    orden[i] = orden[j];
                    orden[j] = tmp;
                }
            }
        }

        float rangoTotal = (n * (n + 1)) / 2f;

        for (int i = 0; i < Individuos; i++) {
            float prob = (float)rnd.NextDouble();
            float acum = 0f;
            for (int k = 0; k < n; k++) {
                acum += (k + 1) / rangoTotal;
                if (acum >= prob) {
                    selectedIndices[i] = orden[k];
                    break;
                }
            }

            // Fix: Asegurar que siempre se asigne un índice válido
            if (selectedIndices[i] < 0 || selectedIndices[i] >= poblacion.Length) {
                selectedIndices[i] = orden[rnd.Next(orden.Length)];
            }
        }
    }

    // ========== REPRODUCCIÓN (CRUCE) ==========
    private void Reproducir() {
        int childIndex = 0;

        for (int i = 0; i + 1 < Individuos; i += 2) {
            Cromosoma padre1 = poblacion[selectedIndices[i]];
            Cromosoma padre2 = poblacion[selectedIndices[i + 1]];

            if (childPopulation[childIndex] == null) childPopulation[childIndex] = new Cromosoma();

            List<int> hijoGenes = cruzar(padre1.Recorrido, padre2.Recorrido);
            if (childPopulation[childIndex].Recorrido == null) {
                childPopulation[childIndex].Recorrido = new List<int>(hijoGenes);
            } else {
                childPopulation[childIndex].Recorrido.Clear();
                childPopulation[childIndex].Recorrido.AddRange(hijoGenes);
            }

            // Validar cromosoma después de cruce
            if (!EsCromosomaValido(childPopulation[childIndex].Recorrido, Ciudades)) {
                Debug.LogWarning($"Cromosoma inválido después de cruce en generación {_generacionActual}, regenerando...");
                childPopulation[childIndex].Recorrido = CrearRecorridoAleatorio();
            }

            childIndex++;
        }

        if (Individuos % 2 == 1) {
            int ultimoPadre = selectedIndices[Individuos - 1];
            if (childPopulation[childIndex] == null) childPopulation[childIndex] = new Cromosoma();
            if (childPopulation[childIndex].Recorrido == null) {
                childPopulation[childIndex].Recorrido = new List<int>();
            }
            childPopulation[childIndex].Recorrido.Clear();
            childPopulation[childIndex].Recorrido.AddRange(poblacion[ultimoPadre].Recorrido);
            // Validar copia del padre
            if (!EsCromosomaValido(childPopulation[childIndex].Recorrido, Ciudades)) {
                Debug.LogWarning($"Cromosoma inválido en copia de padre, regenerando...");
                childPopulation[childIndex].Recorrido = CrearRecorridoAleatorio();
            }
            childIndex++;
        }

        // Asegurar población completa
        while (childIndex < Individuos) {
            int idx = rnd.Next(Individuos);
            if (childPopulation[childIndex] == null) childPopulation[childIndex] = new Cromosoma();
            childPopulation[childIndex].Recorrido.Clear();
            childPopulation[childIndex].Recorrido.AddRange(poblacion[selectedIndices[idx]].Recorrido);
            // Validar copia
            if (!EsCromosomaValido(childPopulation[childIndex].Recorrido, Ciudades)) {
                Debug.LogWarning($"Cromosoma inválido en copia de población, regenerando...");
                childPopulation[childIndex].Recorrido = CrearRecorridoAleatorio();
            }
            childIndex++;
        }
    }

    private List<int> cruzar(List<int> padre1, List<int> padre2) {
        switch (MetCruza) {
            case 0:
                return CruzePMX(padre1, padre2);
            case 1:
                return CruceOX(padre1, padre2);
            case 2:
                return CruceCX(padre1, padre2);
            default:
                return CruzePMX(padre1, padre2);
        }
    }

    private List<int> CruzePMX(List<int> padre1, List<int> padre2) {
        // Validaciones de seguridad
        int n = padre1.Count;
        if (padre2.Count != n) {
            Debug.LogError($"PMX parents length mismatch: {n} vs {padre2.Count}. Returning new random valid chromosome.");
            return CrearRecorridoAleatorio();
        }
        if (n <= 1) {
            // si solo hay 0 o 1 ciudad, no tiene sentido cruzar
            return new List<int>(padre1);
        }

        // Verificar que los padres no tengan duplicados (ciudades únicas en TSP)
        if (padre1.Distinct().Count() != n || padre2.Distinct().Count() != n) {
            Debug.LogWarning("PMX parents contain duplicates. Returning new random valid chromosome.");
            return CrearRecorridoAleatorio();
        }

        // evitar startPos = n-1 para no causar error en Next
        int startPos = rnd.Next(0, n - 1);
        int endPos = rnd.Next(startPos + 1, n);

        List<int> hijo = new List<int>(new int[n]);
        Dictionary<int, int> mapeo = new Dictionary<int, int>();

        // Copiar y crear mapeo
        for (int i = startPos; i <= endPos; i++) {
            hijo[i] = padre1[i];
            mapeo[padre2[i]] = padre1[i];
        }

        // Llenar el resto
        for (int i = 0; i < hijo.Count; i++) {
            if (i < startPos || i > endPos) {
                int valor = padre2[i];
                // resolver mapeo hasta encontrar candidato sin conflicto, con protección contra bucles infinitos
                int maxIterations = n; // límite razonable
                int iterations = 0;
                while (mapeo.ContainsKey(valor) && iterations < maxIterations) {
                    valor = mapeo[valor];
                    iterations++;
                }
                if (iterations >= maxIterations) {
                    Debug.LogWarning("PMX mapping cycle detected, infinite loop prevented. Returning new random valid chromosome.");
                    return CrearRecorridoAleatorio();
                }
                hijo[i] = valor;
            }
        }

        return hijo;
    }

    private List<int> CruceOX(List<int> padre1, List<int> padre2) {
        int n = padre1.Count;
        if (padre2.Count != n) {
            Debug.LogError($"OX parents length mismatch: {n} vs {padre2.Count}. Returning new random valid chromosome.");
            return CrearRecorridoAleatorio();
        }
        if (n <= 1) {
            return new List<int>(padre1);
        }

        // Verificar que los padres no tengan duplicados
        if (padre1.Distinct().Count() != n || padre2.Distinct().Count() != n) {
            Debug.LogWarning("OX parents contain duplicates. Returning new random valid chromosome.");
            return CrearRecorridoAleatorio();
        }

        List<int> hijo = new List<int>();
        
        int c1 = rnd.Next(0, n - 1);
        int c2 = rnd.Next(c1 + 1, n);
        
        // Copiar subsecuencia del padre 1
        for (int i = c1; i <= c2; i++) {
            hijo.Add(padre1[i]);
        }
        
        // Completar con padre 2
        int idx = 0;
        while (hijo.Count < n) {
            int elemento = padre2[idx];
            if (!hijo.Contains(elemento)) {
                hijo.Add(elemento);
            }
            idx = (idx + 1) % n;
        }
        
        return hijo;
    }

    private List<int> CruceCX(List<int> padre1, List<int> padre2) {
        int n = padre1.Count;
        if (padre2.Count != n) {
            Debug.LogError($"CX parents length mismatch: {n} vs {padre2.Count}. Returning new random valid chromosome.");
            return CrearRecorridoAleatorio();
        }
        if (n <= 1) {
            return new List<int>(padre1);
        }

        // Verificar que los padres no tengan duplicados
        if (padre1.Distinct().Count() != n || padre2.Distinct().Count() != n) {
            Debug.LogWarning("CX parents contain duplicates. Returning new random valid chromosome.");
            return CrearRecorridoAleatorio();
        }

        List<int> hijo = new List<int>(new int[padre1.Count]);
        bool[] visitado = new bool[padre1.Count];
        
        int i = 0;
        int iterations = 0;
        int maxIterations = padre1.Count * 2; // Protección contra bucle infinito
        
        do {
            iterations++;
            hijo[i] = padre1[i];
            visitado[i] = true;
            
            int nextIdx = padre2.IndexOf(padre1[i]);
            if (!visitado[nextIdx]) {
                i = nextIdx;
            } else {
                bool found = false;
                for (int j = 0; j < padre1.Count; j++) {
                    if (!visitado[j]) {
                        i = j;
                        found = true;
                        break;
                    }
                }
                if (!found || visitado[i]) break;
            }
        } while (i != 0 && !AllTrue(visitado) && iterations < maxIterations);
        
        // Fix: Si el bucle terminó por límite de iteraciones, completar manualmente
        if (iterations >= maxIterations) {
            Debug.LogWarning("CruceCX: Límite de iteraciones alcanzado, completando manualmente");
            for (int j = 0; j < hijo.Count; j++) {
                if (!visitado[j]) {
                    hijo[j] = padre2[j];
                }
            }
        } else {
            // Llenar resto con padre 2
            for (int j = 0; j < hijo.Count; j++) {
                if (!visitado[j]) {
                    hijo[j] = padre2[j];
                }
            }
        }
        
        return hijo;
    }

    private bool AllTrue(bool[] array) {
        for (int k = 0; k < array.Length; k++) {
            if (!array[k]) return false;
        }
        return true;
    }

    // ========== MUTACIÓN ==========
    private void MutarPoblacion(Cromosoma[] poblacionAMutar) {
        for (int i = 0; i < poblacionAMutar.Length; i++) {
            if (poblacionAMutar[i] == null) continue;
            if ((float)rnd.NextDouble() < TazaMuta) {
                int tipoMutacion = rnd.Next(0, 3);

                switch (tipoMutacion) {
                    case 0:
                        poblacionAMutar[i].MutarPorInsercion();
                        break;
                    case 1:
                        poblacionAMutar[i].MutarPorIntercambio();
                        break;
                    case 2:
                        poblacionAMutar[i].MutarPorInversion();
                        break;
                }

                // Validar después de mutación
                if (!EsCromosomaValido(poblacionAMutar[i].Recorrido, Ciudades)) {
                    Debug.LogWarning($"Cromosoma inválido después de mutación en generación {_generacionActual}, regenerando...");
                    poblacionAMutar[i].Recorrido = CrearRecorridoAleatorio();
                }
            }
        }
    }

    // ========== REEMPLAZO DE POBLACIÓN ==========
    private void ReemplazarPoblacion(Cromosoma[] nuevos) {
        int eliteSize = Mathf.Max(1, Individuos / 10);
        // Ordenar índices de población por aptitud ascendente (menor es mejor)
        int[] indices = new int[Individuos];
        for (int i = 0; i < Individuos; i++) indices[i] = i;

        for (int i = 0; i < Individuos - 1; i++) {
            for (int j = i + 1; j < Individuos; j++) {
                if (poblacion[indices[j]].Aptitud < poblacion[indices[i]].Aptitud) {
                    int temp = indices[i];
                    indices[i] = indices[j];
                    indices[j] = temp;
                }
            }
        }

        Cromosoma[] poblacionFinal = new Cromosoma[Individuos];

        int writeIndex = 0;
        for (int i = 0; i < eliteSize && i < Individuos; i++) {
            poblacionFinal[writeIndex++] = poblacion[indices[i]];
        }

        for (int i = 0; i < nuevos.Length && writeIndex < Individuos; i++) {
            if (nuevos[i] != null && nuevos[i].Recorrido != null && nuevos[i].Recorrido.Count > 0) {
                poblacionFinal[writeIndex++] = nuevos[i];
            } else {
                // Fix: Usar copia del mejor cromosoma disponible si el nuevo es inválido
                Debug.LogWarning($"Cromosoma inválido en reemplazo, usando copia del mejor. Índice: {i}");
                if (poblacion.Length > 0 && poblacion[0] != null && poblacion[0].Recorrido != null) {
                    poblacionFinal[writeIndex++] = new Cromosoma {
                        Recorrido = new List<int>(poblacion[0].Recorrido),
                        Aptitud = poblacion[0].Aptitud
                    };
                } else {
                    // Fallback: crear cromosoma aleatorio
                    poblacionFinal[writeIndex++] = new Cromosoma {
                        Recorrido = CrearRecorridoAleatorio()
                    };
                }
            }
        }

        poblacion = poblacionFinal;
    }

    private List<int> CrearRecorridoAleatorio() {
        List<int> recorrido = new List<int>(Ciudades);
        for (int j = 0; j < Ciudades; j++) {
            recorrido.Add(j);
        }

        for (int j = Ciudades - 1; j > 0; j--) {
            int k = rnd.Next(j + 1);
            int temp = recorrido[j];
            recorrido[j] = recorrido[k];
            recorrido[k] = temp;
        }

        return recorrido;
    }

    public void makePopulation() {
        // ========== VALIDACIONES CRÍTICAS ==========
        if (Ciudades <= 0) {
            Debug.LogError($"❌ ERROR: Ciudades = {Ciudades} en makePopulation. No se puede crear población.");
            return;
        }
        if (Individuos <= 0) {
            Debug.LogError($"❌ ERROR: Individuos = {Individuos} en makePopulation. No se puede crear población.");
            return;
        }
        if (Ciudades < 3) {
            Debug.LogError($"❌ ERROR: Ciudades = {Ciudades} (mínimo 3 requerido)");
            return;
        }
        if (Individuos < 25) {
            Debug.LogError($"❌ ERROR: Individuos = {Individuos} (mínimo 25 requerido)");
            return;
        }

        poblacion = new Cromosoma[Individuos];

        for (int i = 0; i < Individuos; i++) {
            Cromosoma tmp = new Cromosoma();
            tmp.Recorrido = CrearRecorridoAleatorio();
            // Validar cromosoma inicial
            if (!EsCromosomaValido(tmp.Recorrido, Ciudades)) {
                Debug.LogWarning($"Cromosoma inválido generado en makePopulation, regenerando...");
                tmp.Recorrido = CrearRecorridoAleatorio();
            }
            poblacion[i] = tmp;
        }

        Debug.Log($"✅ Población creada: {Individuos} individuos con {Ciudades} ciudades cada uno");
    }

    public void makeCities() {
        // ========== VALIDACIÓN CRÍTICA ==========
        if (Ciudades <= 0) {
            Debug.LogError($"❌ ERROR: No se pueden crear ciudades. Ciudades = {Ciudades}");
            return;
        }
        if (Ciudades > MaxCiudades) {
            Debug.LogWarning($"⚠️ Ciudades ({Ciudades}) > MaxCiudades ({MaxCiudades}). Ajustando...");
            Ciudades = MaxCiudades;
        }

        cities = new Ciudad[Ciudades];
        for (int i = 0; i < Ciudades; i++) {
            Ciudad tmp = new Ciudad();
            tmp.NumCity = "#" + i;
            tmp.Ubicacion = new Vector3(
                (float)rnd.NextDouble() * (15 - 7) + 7,     // X entre 7 y 15
                1.32f,                                       // Y fijo en 1.32
                (float)rnd.NextDouble() * (3 - (-3)) + (-3)  // Z entre 3 y -3
            );
            cities[i] = tmp;
        }
        Debug.Log($"✅ Ciudades creadas: {Ciudades} puntos con ubicaciones aleatorias");

        // ========== CACHEAR DISTANCIAS PARA OPTIMIZACIÓN ==========
        if (distancias == null || distancias.GetLength(0) != Ciudades || distancias.GetLength(1) != Ciudades) {
            distancias = new float[Ciudades, Ciudades];
        }
        
        for (int i = 0; i < Ciudades; i++) {
            for (int j = 0; j < Ciudades; j++) {
                if (i == j) {
                    distancias[i, j] = 0f;
                } else {
                    distancias[i, j] = Vector3.Distance(cities[i].Ubicacion, cities[j].Ubicacion);
                }
            }
        }
        Debug.Log($"✅ Matriz de distancias cacheada: {Ciudades}×{Ciudades} = {Ciudades * Ciudades} valores");
    }

    public void drawCities() {
        // Limpiar instancias previas
        foreach (GameObject go in ciudadesInstanciadas) {
            Destroy(go);
        }
        ciudadesInstanciadas.Clear();

        // Instanciar prefabs de ciudades
        for (int i = 0; i < cities.Length; i++) {
            Ciudad city = cities[i];
            GameObject go = GameObject.Instantiate(CityPrefab, city.Ubicacion, Quaternion.identity) as GameObject;
            go.name = "Ciudad_" + i;
            
            // Asignar texto del número de ciudad
            Text textComponent = go.GetComponentInChildren<Text>();
            if (textComponent != null) {
                textComponent.text = city.NumCity;
            }
            
            ciudadesInstanciadas.Add(go);
        }
        
        Debug.Log($"Ciudades instanciadas: {ciudadesInstanciadas.Count}");
        
        // Inicializar visualización de ruta
        initializeRouteRenderer();
        
        // Dibujar ruta inicial (mejor cromosoma)
        drawRoute(best());
    }

    private void initializeRouteRenderer() {
        // Si no existe, crear
        if (rutaRenderer == null) {
            // Crear GameObject para LineRenderer
            GameObject routeObject = new GameObject("RutaVisual");
            rutaRenderer = routeObject.AddComponent<LineRenderer>();
        }
        
        // Configurar propiedades del LineRenderer (siempre actualizar)
        rutaRenderer.positionCount = 0;
        rutaRenderer.startWidth = 0.08f;  // Ancho ligeramente visible y optimizado
        rutaRenderer.endWidth = 0.08f;
        rutaRenderer.useWorldSpace = true; // Importante para coordenadas absolutas
        
        // Crear material solo si no existe
        if (rutaRenderer.material == null) {
            // Intentar con shader estándar, si no existe usar el básico
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) {
                shader = Shader.Find("Unlit/Color");
            }
            rutaRenderer.material = new Material(shader);
        }
        
        rutaRenderer.numCapVertices = 8; // Más vértices para bordes más redondeados
        
        // Forzar color verde sólido para compatibilidad móvil
        rutaRenderer.startColor = Color.green;
        rutaRenderer.endColor = Color.green;
        rutaRenderer.material.color = Color.green;
    }



    private void drawRoute(Cromosoma cromosoma) {
        if (rutaRenderer == null || cromosoma == null || cromosoma.Recorrido == null) {
            return;
        }
        
        List<int> recorrido = cromosoma.Recorrido;
        
        if (recorrido.Count == 0) {
            rutaRenderer.positionCount = 0;
            return;
        }
        
        // Establecer número de posiciones (ruta + retorno a inicio)
        int positionCount = recorrido.Count + 1;
        rutaRenderer.positionCount = positionCount;
        
        // Dibujar líneas entre ciudades
        for (int i = 0; i < recorrido.Count; i++) {
            int ciudadIndex = recorrido[i];
            if (ciudadIndex >= 0 && ciudadIndex < cities.Length) {
                rutaRenderer.SetPosition(i, cities[ciudadIndex].Ubicacion);
            }
        }
        
        // Cerrar la ruta (retorno a la primera ciudad)
        if (recorrido.Count > 0) {
            rutaRenderer.SetPosition(recorrido.Count, cities[recorrido[0]].Ubicacion);
        }
    }

    public void deleteCities() {
        // Destruir todas las instancias almacenadas
        foreach (GameObject go in ciudadesInstanciadas) {
            if (go != null) {
                Destroy(go);
            }
        }
        ciudadesInstanciadas.Clear();
        
        // Destruir LineRenderer de la ruta
        if (rutaRenderer != null) {
            Destroy(rutaRenderer.gameObject);
            rutaRenderer = null;
        }
        
        Debug.Log("Ciudades instanciadas eliminadas");
    }

    public override string ToString() {
        string features = "";
        features += "Numero de Individuos: " + Individuos + "\n";
        features += "Numero de Ciudades: " + Ciudades + "\n";
        features += "Numero de Generaciones: " + Generaciones + "\n";
        features += "Taza Mutacion: " + (TazaMuta * 100) + "%" + "\n";
        features += "Metodo de Seleccion: " + ObtenerNombreSeleccion(MetSelec) + "\n";
        features += "Metodo de Cruce: " + ObtenerNombreCruce(MetCruza) + "\n";
        features += "Generacion Actual: " + GeneracionActual;
        return features;
    }

    private string ObtenerNombreSeleccion(int metodo) {
        switch (metodo) {
            case 0: return "Ruleta";
            case 1: return "Torneo";
            case 2: return "Ranking";
            default: return "Desconocido";
        }
    }

    private string ObtenerNombreCruce(int metodo) {
        switch (metodo) {
            case 0: return "PMX";
            case 1: return "OX";
            case 2: return "CX";
            default: return "Desconocido";
        }
    }
}
