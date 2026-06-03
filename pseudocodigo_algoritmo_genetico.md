# Pseudocódigo del Algoritmo Genético Manual para TSP

Este documento presenta el pseudocódigo formal y académico del algoritmo genético desarrollado manualmente en el proyecto Unity. El pseudocódigo está diseñado siguiendo convenciones académicas estándar y puede integrarse directamente en una tesis de ingeniería. Se utiliza notación de alto nivel para claridad y generalización.

---

## Notación y convenciones

- **Variables en mayúsculas**: parámetros de configuración (e.g., `Generaciones`, `Individuos`, `Ciudades`).
- **Vectores/listas**: notación `[]` sub-indexada desde 0.
- **Operadores**: `←` asignación, `←R` asignación aleatoria, `∈` pertenencia.
- **Funciones**: `f()` en formato matemático cuando corresponda.
- **Comentarios**: líneas precedidas por `//` en tipografía diferenciada.

---

## 1. Pseudocódigo del Algoritmo Genético Completo

```
ALGORITMO ALGORITMO_GENÉTICO_TSP

ENTRADA:
    Generaciones ∈ ℕ+           // Número de iteraciones del AG
    Individuos ∈ ℕ+             // Tamaño de la población
    Ciudades ∈ ℕ+               // Número de nodos en el TSP
    TazaMuta ∈ [0, 1]          // Probabilidad de mutación
    MetSelec ∈ {0, 1, 2}       // Método de selección (ruleta, torneo, ranking)
    MetCruza ∈ {0, 1, 2}       // Método de cruza (PMX, OX, CX)

SALIDA:
    mejorCromosoma             // Mejor solución encontrada

VARIABLES:
    poblacion[Individuos]      // Población actual
    ciudades[Ciudades]         // Coordenadas de ciudades
    generacionActual ← 0
    mejorActual, mejorGlobal
    
INICIO

    // ========== FASE DE INICIALIZACIÓN ==========
    poblacion ← INICIALIZAR_POBLACIÓN(Individuos, Ciudades)
    ciudades ← GENERAR_CIUDADES_ALEATORIAS(Ciudades)
    
    // Evaluación inicial
    PARA cada cromosoma ∈ poblacion HACER
        cromosoma.aptitud ← CALCULAR_APTITUD(cromosoma, ciudades)
    FIN PARA
    
    mejorGlobal ← MEJOR(poblacion)
    
    // ========== BUCLE EVOLUTIVO ==========
    PARA generacionActual ← 0 HASTA Generaciones - 1 HACER
    
        // 1. SELECCIÓN
        indicesSeleccionados ← SELECCIONAR(poblacion, MetSelec, Individuos)
        
        // 2. CRUZA (RECOMBINACIÓN)
        nuevosIndividuos ← REPRODUCIR(poblacion, indicesSeleccionados, MetCruza)
        
        // 3. MUTACIÓN
        MUTAR_POBLACIÓN(nuevosIndividuos, TazaMuta)
        
        // 4. EVALUACIÓN DE NUEVOS INDIVIDUOS
        PARA cada cromosoma ∈ nuevosIndividuos HACER
            cromosoma.aptitud ← CALCULAR_APTITUD(cromosoma, ciudades)
        FIN PARA
        
        // 5. REEMPLAZO CON ELITISMO
        poblacion ← REEMPLAZAR_POBLACIÓN(poblacion, nuevosIndividuos, Individuos)
        
        // 6. SEGUIMIENTO
        mejorActual ← MEJOR(poblacion)
        SI mejorActual.aptitud < mejorGlobal.aptitud ENTONCES
            mejorGlobal ← mejorActual
            REGISTRAR_MEJORA(generacionActual, mejorGlobal.aptitud)
        FIN SI
        
        COMUNICAR_VISUALIZACIÓN(mejorGlobal)  // Para renderizado en hilo principal
        
    FIN PARA
    
    RETORNAR mejorGlobal

FIN ALGORITMO
```

---

## 2. Inicialización de la población

### Función: INICIALIZAR_POBLACIÓN

```
FUNCIÓN INICIALIZAR_POBLACIÓN(Individuos : ℕ+, Ciudades : ℕ+) → poblacion[]

VARIABLES:
    poblacion[Individuos]
    cromosoma
    recorrido_temp[]
    
INICIO
    
    PARA i ← 0 HASTA Individuos - 1 HACER
        cromosoma ← NUEVO Cromosoma()
        recorrido_temp ← []
        
        // Generar permutación única de ciudades
        MIENTRAS recorrido_temp.tamaño < Ciudades HACER
            gene ←R ALEATORIO_INT(0, Ciudades - 1)
            SI gene ∉ recorrido_temp ENTONCES
                recorrido_temp.AGREGAR(gene)
            FIN SI
        FIN MIENTRAS
        
        cromosoma.recorrido ← recorrido_temp
        cromosoma.aptitud ← 0  // Será calculada después
        poblacion[i] ← cromosoma
        
    FIN PARA
    
    RETORNAR poblacion

FIN FUNCIÓN
```

**Complejidad**: O(Individuos × Ciudades²) en peor caso por verificación de unicidad.

---

### Función: GENERAR_CIUDADES_ALEATORIAS

```
FUNCIÓN GENERAR_CIUDADES_ALEATORIAS(Ciudades : ℕ+) → ciudades[]

VARIABLES:
    ciudades[Ciudades]
    ciudad
    Xmin, Xmax ← 8, 17        // Rango de coordenada X
    Zmin, Zmax ← -5, 5        // Rango de coordenada Z
    Y ← 1.35                  // Altura fija
    
INICIO
    
    PARA i ← 0 HASTA Ciudades - 1 HACER
        ciudad ← NUEVA Ciudad()
        ciudad.NumCity ← "#" + CADENA(i)
        ciudad.Ubicacion ← Vector3(
            ALEATORIO_REAL(Xmin, Xmax),
            Y,
            ALEATORIO_REAL(Zmin, Zmax)
        )
        ciudades[i] ← ciudad
    FIN PARA
    
    RETORNAR ciudades

FIN FUNCIÓN
```

**Formulación**: Las posiciones se distribuyen uniformemente en la región rectangular $[X_{min}, X_{max}] \times \{Y\} \times [Z_{min}, Z_{max}]$.

---

## 3. Evaluación de aptitud

### Función: CALCULAR_APTITUD

```
FUNCIÓN CALCULAR_APTITUD(cromosoma : Cromosoma, ciudades[] : Ciudad[]) → aptitud

VARIABLES:
    distanciaTotal ← 0.0
    ciudadActual, ciudadSiguiente ∈ ℕ
    n ← cromosoma.recorrido.tamaño
    
INICIO
    
    // Sumar distancias entre ciudades consecutivas en el recorrido
    PARA i ← 0 HASTA n - 2 HACER
        ciudadActual ← cromosoma.recorrido[i]
        ciudadSiguiente ← cromosoma.recorrido[i + 1]
        distanciaTotal ← distanciaTotal + DISTANCIA_EUCLIDIANA(
            ciudades[ciudadActual].Ubicacion,
            ciudades[ciudadSiguiente].Ubicacion
        )
    FIN PARA
    
    // Agregar distancia de retorno (última ciudad a primera)
    SI n > 0 ENTONCES
        distanciaTotal ← distanciaTotal + DISTANCIA_EUCLIDIANA(
            ciudades[cromosoma.recorrido[n - 1]].Ubicacion,
            ciudades[cromosoma.recorrido[0]].Ubicacion
        )
    FIN SI
    
    RETORNAR distanciaTotal

FIN FUNCIÓN
```

**Donde** `DISTANCIA_EUCLIDIANA(p₁, p₂)` se define como:

$$d(p_1, p_2) = \sqrt{(x_2 - x_1)^2 + (y_2 - y_1)^2 + (z_2 - z_1)^2}$$

**Complejidad**: O(Ciudades)

---

## 4. Selección de parejas

### Función: SELECCIONAR (Rutador)

```
FUNCIÓN SELECCIONAR(poblacion[] : Cromosoma[], MetSelec : {0, 1, 2}, 
                    N : ℕ+) → indicesSeleccionados[]

INICIO
    
    SEGÚN MetSelec HACER
        CASO 0:
            indicesSeleccionados ← SELECCIÓN_POR_RULETA(poblacion, N)
        CASO 1:
            indicesSeleccionados ← SELECCIÓN_POR_TORNEO(poblacion, N)
        CASO 2:
            indicesSeleccionados ← SELECCIÓN_POR_RANKING(poblacion, N)
        POR_DEFECTO:
            indicesSeleccionados ← SELECCIÓN_POR_RULETA(poblacion, N)
    FIN SEGÚN
    
    RETORNAR indicesSeleccionados

FIN FUNCIÓN
```

---

### Función: SELECCIÓN_POR_RULETA

```
FUNCIÓN SELECCIÓN_POR_RULETA(poblacion[] : Cromosoma[], N : ℕ+) → indicesSel[]

VARIABLES:
    aptitudesInvertidas[poblacion.tamaño]
    maxAptitud, aptitudTotal, probabilidad, acumulado
    indicesSel[] ← VECTOR_VACÍO
    
INICIO
    
    // Invertir aptitudes (conversión minimización → maximización)
    maxAptitud ← MÁXIMO(poblacion.aptitud[])
    
    PARA i ← 0 HASTA poblacion.tamaño - 1 HACER
        aptitudesInvertidas[i] ← maxAptitud - poblacion[i].aptitud
    FIN PARA
    
    aptitudTotal ← SUMAR(aptitudesInvertidas[])
    
    // Seleccionar N individuos con probabilidad proporcional
    PARA i ← 0 HASTA N - 1 HACER
        probabilidad ←R ALEATORIO_REAL(0, aptitudTotal)
        acumulado ← 0
        
        PARA j ← 0 HASTA poblacion.tamaño - 1 HACER
            acumulado ← acumulado + aptitudesInvertidas[j]
            SI acumulado ≥ probabilidad ENTONCES
                indicesSel.AGREGAR(j)
                SALIR_BUCLE_INTERNO
            FIN SI
        FIN PARA
        
    FIN PARA
    
    RETORNAR indicesSel

FIN FUNCIÓN
```

**Principio**: Cada individuo con aptitud $a_i$ tiene probabilidad $P(i) = \frac{\max(a) - a_i}{\sum_j (\max(a) - a_j)}$ de ser seleccionado.

**Complejidad**: O(N × poblacion.tamaño)

---

### Función: SELECCIÓN_POR_TORNEO

```
FUNCIÓN SELECCIÓN_POR_TORNEO(poblacion[] : Cromosoma[], N : ℕ+) → indicesSel[]

VARIABLES:
    indicesSel[] ← VECTOR_VACÍO
    tamanoTorneo ← MÁXIMO(2, N / 4)
    mejorIdx, candidatoIdx
    
INICIO
    
    PARA i ← 0 HASTA N - 1 HACER
        mejorIdx ←R ALEATORIO_INT(0, poblacion.tamaño - 1)
        
        // Comparar con candidatos aleatorios del torneo
        PARA j ← 1 HASTA tamanoTorneo - 1 HACER
            candidatoIdx ←R ALEATORIO_INT(0, poblacion.tamaño - 1)
            SI poblacion[candidatoIdx].aptitud < poblacion[mejorIdx].aptitud ENTONCES
                mejorIdx ← candidatoIdx
            FIN SI
        FIN PARA
        
        indicesSel.AGREGAR(mejorIdx)
        
    FIN PARA
    
    RETORNAR indicesSel

FIN FUNCIÓN
```

**Complejidad**: O(N × tamanoTorneo) ≈ O(N²/4)

---

### Función: SELECCIÓN_POR_RANKING

```
FUNCIÓN SELECCIÓN_POR_RANKING(poblacion[] : Cromosoma[], N : ℕ+) → indicesSel[]

VARIABLES:
    poblacionOrdenada[] ← ORDENAR_POR_APTITUD(poblacion)  // Ascendente
    rankings[]
    rangoTotal ← (poblacion.tamaño × (poblacion.tamaño + 1)) / 2
    indicesSel[] ← VECTOR_VACÍO
    probabilidad, acumulado
    
INICIO
    
    // Asignar ranking lineal (mejor individuo = rank n)
    PARA i ← 0 HASTA poblacion.tamaño - 1 HACER
        rankings[i] ← (i + 1) / rangoTotal
    FIN PARA
    
    // Seleccionar N individuos según su rango
    PARA k ← 0 HASTA N - 1 HACER
        probabilidad ←R ALEATORIO_REAL(0, 1)
        acumulado ← 0
        
        PARA j ← 0 HASTA poblacionOrdenada.tamaño - 1 HACER
            acumulado ← acumulado + rankings[j]
            SI acumulado ≥ probabilidad ENTONCES
                indicesSel.AGREGAR(OBTENER_ÍNDICE_ORIGINAL(poblacionOrdenada[j]))
                SALIR_BUCLE_INTERNO
            FIN SI
        FIN PARA
        
    FIN PARA
    
    RETORNAR indicesSel

FIN FUNCIÓN
```

**Complejidad**: O(n log n) por ordenamiento + O(N × n) por selección = O(n log n + Nn)

---

## 5. Recombinación (Cruza)

### Función: REPRODUCIR

```
FUNCIÓN REPRODUCIR(poblacion[] : Cromosoma[], indicesSel[] : ℕ[], 
                   MetCruza : {0, 1, 2}) → hijos[]

VARIABLES:
    hijos[] ← VECTOR_VACÍO
    
INICIO
    
    // Cruzar parejas consecutivas de índices seleccionados
    PARA i ← 0 HASTA indicesSel.tamaño - 1 PASO 2 HACER
        
        SI i + 1 < indicesSel.tamaño ENTONCES
            padre1 ← poblacion[indicesSel[i]]
            padre2 ← poblacion[indicesSel[i + 1]]
            
            // Verificación de permutaciones válidas
            SI padre1.recorrido.tamaño ≠ padre2.recorrido.tamaño ENTONCES
                REGISTRAR_ERROR("Tamaños de recorrido inconsistentes")
                CONTINUAR
            FIN SI
            
            // Aplicar método de cruza
            genesHijo ← CRUZAR(padre1.recorrido, padre2.recorrido, MetCruza)
            
            hijo ← NUEVO Cromosoma()
            hijo.recorrido ← genesHijo
            hijos.AGREGAR(hijo)
            
        SINO
            // Caso impar: clonar último padre
            hijo ← NUEVO Cromosoma()
            hijo.recorrido ← COPIAR(poblacion[indicesSel[i]].recorrido)
            hijos.AGREGAR(hijo)
            
        FIN SI
        
    FIN PARA
    
    // Garantizar tamaño exacto de población
    MIENTRAS hijos.tamaño < poblacion.tamaño HACER
        idxAleatorio ←R ALEATORIO_INT(0, indicesSel.tamaño - 1)
        hijoClonado ← NUEVO Cromosoma()
        hijoClonado.recorrido ← COPIAR(poblacion[indicesSel[idxAleatorio]].recorrido)
        hijos.AGREGAR(hijoClonado)
    FIN MIENTRAS
    
    RETORNAR hijos

FIN FUNCIÓN
```

---

### Función: CRUZAR (Rutador)

```
FUNCIÓN CRUZAR(padre1[] : ℕ, padre2[] : ℕ, MetCruza : {0, 1, 2}) → hijo[]

INICIO

    SEGÚN MetCruza HACER
        CASO 0:
            hijo ← CRUZA_PMX(padre1, padre2)
        CASO 1:
            hijo ← CRUZA_OX(padre1, padre2)
        CASO 2:
            hijo ← CRUZA_CX(padre1, padre2)
        POR_DEFECTO:
            hijo ← CRUZA_PMX(padre1, padre2)
    FIN SEGÚN
    
    RETORNAR hijo

FIN FUNCIÓN
```

---

### Función: CRUZA_PMX (Partially Mapped Crossover)

```
FUNCIÓN CRUZA_PMX(padre1[] : ℕ, padre2[] : ℕ) → hijo[]

VARIABLES:
    n ← padre1.tamaño
    hijo[n] INICIALIZADO CON 0
    mapeo : DICCIONARIO
    startPos, endPos
    
INICIO
    
    SI n ≤ 1 ENTONCES
        RETORNAR COPIAR(padre1)
    FIN SI
    
    // Seleccionar un segmento aleatorio
    startPos ←R ALEATORIO_INT(0, n - 2)
    endPos ←R ALEATORIO_INT(startPos + 1, n - 1)
    
    // Copiar segmento del padre 1 y crear mapeo
    PARA i ← startPos HASTA endPos HACER
        hijo[i] ← padre1[i]
        mapeo[padre2[i]] ← padre1[i]
    FIN PARA
    
    // Llenar posiciones restantes
    PARA i ← 0 HASTA hijo.tamaño - 1 HACER
        
        SI i < startPos O i > endPos ENTONCES
            valor ← padre2[i]
            
            // Resolver mapeo iterativamente
            MIENTRAS valor ∈ mapeo HACER
                valor ← mapeo[valor]
            FIN MIENTRAS
            
            hijo[i] ← valor
            
        FIN SI
        
    FIN PARA
    
    RETORNAR hijo

FIN FUNCIÓN
```

---

### Función: CRUZA_OX (Order Crossover)

```
FUNCIÓN CRUZA_OX(padre1[] : ℕ, padre2[] : ℕ) → hijo[]

VARIABLES:
    hijo[] ← VECTOR_VACÍO
    n ← padre1.tamaño
    c1, c2
    
INICIO
    
    // Seleccionar dos puntos de corte
    c1 ←R ALEATORIO_INT(0, n - 2)
    c2 ←R ALEATORIO_INT(c1 + 1, n - 1)
    
    // Copiar subsecuencia del padre 1
    PARA i ← c1 HASTA c2 HACER
        hijo.AGREGAR(padre1[i])
    FIN PARA
    
    // Completar con padre 2 en orden
    indice ← 0
    MIENTRAS hijo.tamaño < n HACER
        elemento ← padre2[indice]
        SI elemento ∉ hijo ENTONCES
            hijo.AGREGAR(elemento)
        FIN SI
        indice ← (indice + 1) MOD n
    FIN MIENTRAS
    
    RETORNAR hijo

FIN FUNCIÓN
```

---

### Función: CRUZA_CX (Cycle Crossover)

```
FUNCIÓN CRUZA_CX(padre1[] : ℕ, padre2[] : ℕ) → hijo[]

VARIABLES:
    n ← padre1.tamaño
    hijo[n] INICIALIZADO CON 0
    visitado[n] INICIALIZADO CON FALSO
    i ← 0
    
INICIO
    
    // Construir ciclos
    REPETIR
        hijo[i] ← padre1[i]
        visitado[i] ← VERDADERO
        
        indiceProx ← POSICIÓN_DE(padre2, padre1[i])
        SI NO visitado[indiceProx] ENTONCES
            i ← indiceProx
        SINO
            // Encontrar siguiente no visitado
            ENCONTRADO ← FALSO
            PARA j ← 0 HASTA n - 1 HACER
                SI NO visitado[j] ENTONCES
                    i ← j
                    ENCONTRADO ← VERDADERO
                    SALIR_BUCLE
                FIN SI
            FIN PARA
            SI NO ENCONTRADO ENTONCES
                SALIR_REPETIR
            FIN SI
        FIN SI
        
    HASTA QUE NO EXISTA j NO visitado
    
    // Llenar con elementos del padre 2
    PARA i ← 0 HASTA n - 1 HACER
        SI NO visitado[i] ENTONCES
            hijo[i] ← padre2[i]
        FIN SI
    FIN PARA
    
    RETORNAR hijo

FIN FUNCIÓN
```

---

## 6. Mutación

### Función: MUTAR_POBLACIÓN

```
FUNCIÓN MUTAR_POBLACIÓN(poblacion[] : Cromosoma[], TazaMuta : [0, 1])

INICIO
    
    PARA cada cromosoma ∈ poblacion HACER
        SI ALEATORIO_REAL(0, 1) < TazaMuta ENTONCES
            tipoMutacion ←R ALEATORIO_INT(0, 2)
            
            SEGÚN tipoMutacion HACER
                CASO 0:
                    MUTAR_INSERCIÓN(cromosoma)
                CASO 1:
                    MUTAR_INTERCAMBIO(cromosoma)
                CASO 2:
                    MUTAR_INVERSIÓN(cromosoma)
            FIN SEGÚN
            
        FIN SI
    FIN PARA

FIN FUNCIÓN
```

---

### Función: MUTAR_INSERCIÓN

```
FUNCIÓN MUTAR_INSERCIÓN(cromosoma : Cromosoma)

VARIABLES:
    indice1, indice2
    elemento
    n ← cromosoma.recorrido.tamaño
    
INICIO
    
    indice1 ←R ALEATORIO_INT(0, n - 1)
    indice2 ←R ALEATORIO_INT(0, n - 1)
    
    // Asegurar que son diferentes
    MIENTRAS indice1 = indice2 HACER
        indice2 ←R ALEATORIO_INT(0, n - 1)
    FIN MIENTRAS
    
    elemento ← cromosoma.recorrido[indice1]
    
    SI indice1 < indice2 ENTONCES
        cromosoma.recorrido.REMOVER_EN(indice1)
        cromosoma.recorrido.INSERTAR_EN(indice2 - 1, elemento)
    SINO
        cromosoma.recorrido.REMOVER_EN(indice1)
        cromosoma.recorrido.INSERTAR_EN(indice2, elemento)
    FIN SI

FIN FUNCIÓN
```

---

### Función: MUTAR_INTERCAMBIO

```
FUNCIÓN MUTAR_INTERCAMBIO(cromosoma : Cromosoma)

VARIABLES:
    indice1, indice2
    temporal
    n ← cromosoma.recorrido.tamaño
    
INICIO
    
    indice1 ←R ALEATORIO_INT(0, n - 1)
    indice2 ←R ALEATORIO_INT(0, n - 1)
    
    // Asegurar que son diferentes
    MIENTRAS indice1 = indice2 HACER
        indice2 ←R ALEATORIO_INT(0, n - 1)
    FIN MIENTRAS
    
    // Intercambiar elementos
    temporal ← cromosoma.recorrido[indice1]
    cromosoma.recorrido[indice1] ← cromosoma.recorrido[indice2]
    cromosoma.recorrido[indice2] ← temporal

FIN FUNCIÓN
```

---

### Función: MUTAR_INVERSIÓN

```
FUNCIÓN MUTAR_INVERSIÓN(cromosoma : Cromosoma)

VARIABLES:
    puntoInicio, puntoFin
    n ← cromosoma.recorrido.tamaño
    
INICIO
    
    puntoInicio ←R ALEATORIO_INT(0, n - 1)
    puntoFin ←R ALEATORIO_INT(puntoInicio, n - 1)
    
    // Invertir el rango [puntoInicio, puntoFin]
    cromosoma.recorrido.INVERTIR(puntoInicio, puntoFin - puntoInicio + 1)

FIN FUNCIÓN
```

---

## 7. Reemplazo generacional con elitismo

### Función: REEMPLAZAR_POBLACIÓN

```
FUNCIÓN REEMPLAZAR_POBLACIÓN(poblacionActual[] : Cromosoma[], 
                              nuevosIndividuos[] : Cromosoma[],
                              TamañoPoblacion : ℕ+) → poblacionNueva[]

VARIABLES:
    eliteSize ← MÁXIMO(1, TamañoPoblacion / 10)
    poblacionActual_Ord[] ← COPIAR(poblacionActual)
    poblacionNueva[] ← []
    
INICIO
    
    // Ordenar población actual por aptitud (mejor primero)
    ORDENAR_ASCENDENTE_POR(poblacionActual_Ord, aptitud)
    
    // Mantener élite
    PARA i ← 0 HASTA eliteSize - 1 HACER
        poblacionNueva.AGREGAR(poblacionActual_Ord[i])
    FIN PARA
    
    // Agregar nuevos individuos hasta completa tamaño
    PARA i ← 0 HASTA nuevosIndividuos.tamaño - 1 HACER
        SI poblacionNueva.tamaño < TamañoPoblacion ENTONCES
            poblacionNueva.AGREGAR(nuevosIndividuos[i])
        FIN SI
    FIN PARA
    
    // Rellenar si falta (clonar aleatorios de élite)
    MIENTRAS poblacionNueva.tamaño < TamañoPoblacion HACER
        idxAleatorio ←R ALEATORIO_INT(0, eliteSize - 1)
        clon ← COPIAR(poblacionActual_Ord[idxAleatorio])
        poblacionNueva.AGREGAR(clon)
    FIN MIENTRAS
    
    RETORNAR poblacionNueva

FIN FUNCIÓN
```

**Principio de elitismo**: La estrategia preserva los mejores individuos de la generación anterior, asegurando monotonicidad no decreciente en la aptitud máxima.

---

## 8. Función auxiliar: MEJOR

```
FUNCIÓN MEJOR(poblacion[] : Cromosoma[]) → mejorCromosoma

VARIABLES:
    mejorIdx ← 0
    
INICIO
    
    PARA i ← 1 HASTA poblacion.tamaño - 1 HACER
        SI poblacion[i].aptitud < poblacion[mejorIdx].aptitud ENTONCES
            mejorIdx ← i
        FIN SI
    FIN PARA
    
    RETORNAR poblacion[mejorIdx]

FIN FUNCIÓN
```

**Nota**: La función busca el cromosoma con **menor** aptitud (distancia), validando que el AG sea un problema de **minimización**.

---

## 9. Complejidad computacional global

La complejidad del algoritmo por generación se categoriza como:

| Fase | Complejidad |
|------|------------|
| Selección | O(n²) (ruleta) o O(n log n) (ranking) |
| Cruza | O(n × m) (m = tamaño cromosoma) |
| Mutación | O(n × m) |
| Evaluación | O(n × m) |
| Reemplazo | O(n log n) |
| **Total por generación** | **O(G × (n² + n × m))** |
| **Total (G generaciones)** | **O(G × (n² + n × m))** |

Donde $n$ = tamaño población, $m$ = número de ciudades, $G$ = generaciones.

---

## 10. Notas académicas

Este pseudocódigo refleja fielmente la implementación en C# del proyecto. Las siguientes observaciones se aplican a la correcta comprensión:

1. **Permutaciones**: El AG mantiene permutaciones válidas de ciudades en todo momento, esencial para problemas de TSP.
2. **Minimización vs. Maximización**: El algoritmo busca minimizar distancia; inversiones de aptitud en ruleta y ranking convierten a maximización implícita.
3. **Elitismo**: La preservación de mejores individuos garantiza convergencia monótona, un estándar en AG prácticos.
4. **Sincronización de hilos**: En la implementación, `COMUNICAR_VISUALIZACIÓN()` usa `locks` para thread-safety.
5. **Parámetros adaptativos**: Aunque no incluido aquí, futuras mejoras podrían incluir ajuste dinámico de `TazaMuta` y tamaño de torneo.

---

Este pseudocódigo puede integrarse directamente en una sección de una tesis bajo encabezados como "Algoritmo propuesto" o "Descripción formal del AG manual".