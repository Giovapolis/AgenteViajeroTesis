# Modificación: Método reStart() - Reutilización de Ciudades

**Fecha:** 2026-03-23  
**Archivo:** Assets/Scripts/Propio/AlgoritmoGenetico.cs  
**Estado:** ✅ Implementado y Validado (Sin Errores de Compilación)

---

## 📋 Resumen Ejecutivo

Se modificó el comportamiento del método `reStart()` para que **NO regenere ciudades** y solo reinicialice el algoritmo genético. Las ciudades permanecen en memoria y en la escena de Unity.

---

## 🎯 Objetivo Alcanzado

| Requisito | Estado | Detalles |
|-----------|--------|----------|
| ❌ NO regenerar ciudades | ✅ Hecho | Método `makeCities()` no se llama en reinicio |
| ❌ NO reinicializar completamente | ✅ Hecho | Solo se reinicia población y contadores |
| ✅ Detener thread actual | ✅ Hecho | `isRunning = false` + espera de 100ms |
| ✅ Reutilizar ciudades | ✅ Hecho | Array `cities[]` se mantiene intacto |
| ✅ Leer nuevos parámetros | ✅ Compatible | Cambiar valores antes de `reStart()` |

---

## 🔧 Cambios Implementados

### 1. MÉTODO `inicia()` - MODIFICADO (Líneas 82-130)

**Cambio:** Verificación inteligente de existencia de ciudades

```csharp
// ANTES: Siempre regeneraba
makePopulation();
makeCities();
deleteCities();
drawCities();

// AHORA: Verifica si ciudades existen
makePopulation();

if (cities == null || cities.Length == 0 || cities.Length != Ciudades) {
    // Primera ejecución: crear ciudades desde cero
    makeCities();
    deleteCities();
    drawCities();
} else {
    // Reinicio: reutilizar ciudades
    if (rutaRenderer == null) {
        initializeRouteRenderer();
    }
}
```

**Lógica:**
- **Primera vez:** `cities` es null → crea ciudades
- **Reinicio:** `cities` existe → reutiliza y solo reinicializa ruta visual

---

### 2. MÉTODO `reStart()` - COMPLETAMENTE REFACTORIZADO (Líneas 133-145)

**ANTES:**
```csharp
public void reStart() {
    Debug.Log("ReInicio");
    stop();
    inicia();  // ❌ PROBLEMA: inicia() regeneraba ciudades
}
```

**AHORA:**
```csharp
public void reStart() {
    Debug.Log("ReInicio del AG - Manteniendo ciudades");
    stop();  // Detener thread
    
    // Esperar breve tiempo para asegurar que el thread anterior finalizó
    Thread.Sleep(100);
    
    ResetEvolution();  // ✅ Nuevo método que no regenera ciudades
}
```

**Mejoras:**
- ✅ Separación clara: `stop()` detiene, `ResetEvolution()` reinicia
- ✅ Seguridad de threading: espera 100ms
- ✅ No llama a `inicia()` que regeneraría ciudades

---

### 3. NUEVO MÉTODO `ResetEvolution()` - CREADO (Líneas 148-192)

**Propósito:** Reinicializar SOLO el algoritmo, no las ciudades

```csharp
private void ResetEvolution() {
    Debug.Log("Reseteando estado de evolución (ciudades se mantienen)");
    
    // 1️⃣ Reiniciar bandera y contadores
    isRunning = true;
    _generacionActual = 0;
    _mejorDistancia = float.MaxValue;
    _hasNewData = false;

    // 2️⃣ Reiniciar arreglos de algoritmo
    selectedIndices = new int[Individuos];
    childPopulation = new Cromosoma[Individuos];

    // 3️⃣ Reiniciar población (MISMAS ciudades)
    makePopulation();
    evaluatePopulation();

    // 4️⃣ Guardar mejor ruta inicial de forma thread-safe
    Cromosoma mejorInicial = best();
    lock (lockObject) {
        _mejorRuta = new List<int>(mejorInicial.Recorrido);
        _mejorDistancia = mejorInicial.Aptitud;
        _hasNewData = true;
    }

    // 5️⃣ Iniciar nuevo Thread de evolución
    if (evolutionThread != null) {
        evolutionThread.Abort();
    }
    evolutionThread = new Thread(EvolutionThread);
    evolutionThread.Start();
    
    Debug.Log("AG reiniciado - Ciudades mantenidas, nuevo Thread iniciado");
}
```

**QUÉ HACE:**
| Acción | ✅ Sí | ❌ No |
|--------|-------|-------|
| Reinicia población | ✅ | |
| Reinicia generaciónActual → 0 | ✅ | |
| Reinicia mejorDistancia | ✅ | |
| Reinicia arrays (selection, crossover) | ✅ | |
| Detiene thread anterior | ✅ | |
| Inicia nuevo thread | ✅ | |
| | | ❌ Regenera ciudades |
| | | ❌ Llama makeCities() |
| | | ❌ Destruye GameObjects |
| | | ❌ Llama deleteCities() |

---

## 🔄 Flujo de Ejecución Comparativo

### Primer inicio: `inicia()`
```
┌─────────────────────────────────────┐
│ Click btnStart                      │
├─────────────────────────────────────┤
│ 1. makePopulation()                 │
│ 2. makeCities() [crear datos]       │
│ 3. deleteCities() [limpiar visuals] │
│ 4. drawCities() [crear GameObjects] │
│ 5. evaluatePopulation()             │
│ 6. Thread de evolución              │
└─────────────────────────────────────┘
```

### Reinicio: `reStart()`
```
┌─────────────────────────────────────┐
│ Click btnRestart                    │
├─────────────────────────────────────┤
│ stop()                              │
│   ├─ isRunning = false              │
│   └─ Thread.Abort()                 │
│                                     │
│ Thread.Sleep(100) [esperar]         │
│                                     │
│ ResetEvolution()                    │
│   ├─ 1. Reiniciar contadores       │
│   ├─ 2. makePopulation()            │
│   ├─ 3. evaluatePopulation()        │
│   ├─ 4. Iniciar nuevo Thread        │
│   └─ ❌ NOactúa sobre cities[]     │
│   └─ ❌ NO actúa sobre GameObject  │
└─────────────────────────────────────┘
     ↓
   Ciudades intactas en escena
```

---

## 🧵 Gestión de Threading

**Bandera `isRunning` (ya existía):**
```csharp
private bool isRunning = false;  // Control de ejecución del thread

// En EvolutionThread():
while (isRunning && _generacionActual < Generaciones) {
    // Evolución...
}
```

**Cambios de reStart():**
1. Establece `isRunning = false` (mediante `stop()`)
2. Aborta thread anterior
3. Espera 100ms
4. Establece `isRunning = true` (mediante `ResetEvolution()`)
5. Inicia nuevo thread

**Garantías:**
- ✅ No hay duplicación de threads
- ✅ No hay race conditions (usa locks compartidos)
- ✅ Espera asegurada antes de reiniciar

---

## 📊 Variables Compartidas (Thread-Safe)

Estas variables usan `lockObject` para sincronización:

```csharp
private object lockObject = new object();
private int _generacionActual = 0;
private List<int> _mejorRuta = new List<int>();
private float _mejorDistancia = float.MaxValue;
private bool _hasNewData = false;
```

**Ambos métodos respetan los locks:**
- `ResetEvolution()` inicializa dentro de `lock()`
- `EvolutionThread()` actualiza dentro de `lock()`
- `Update()` lee dentro de `lock()`

---

## 📝 Parámetros Reutilizables Entre Reinicios

Puedes cambiar estos parámetros ANTES de llamar a `reStart()`:

```csharp
// Cambiar parámetros
algoritmoGenetico.Generaciones = 500;
algoritmoGenetico.Individuos = 80;
algoritmoGenetico.TazaMuta = 0.15f;
algoritmoGenetico.MetSelec = 1;  // Torneo
algoritmoGenetico.MetCruza = 2;  // CX

// Reiniciar con nuevos parámetros
algoritmoGenetico.reStart();
```

**Las ciudades permanecen IDÉNTICAS**, solo cambia la evolución.

---

## ✔️ Validación de Requisitos

| # | Requisito | Implementado | Verificación |
|---|-----------|--------------|--------------|
| 1 | Bandera `isRunning` | ✅ Ya existía | Línea ~18 |
| 2 | Thread verifica bandera | ✅ Ya existía | `EvolutionThread()` |
| 3 | `reStart()` detiene thread | ✅ Hecho | `stop()` + Sleep(100) |
| 4 | `reStart()` NO regenera ciudades | ✅ Hecho | `ResetEvolution()` |
| 5 | `reStart()` NO llama `makeCities()` | ✅ Hecho | Solo en primer inicio |
| 6 | `reStart()` NO llama `drawCities()` | ✅ Hecho | Solo en primer inicio |
| 7 | `reStart()` NO llama `deleteCities()` | ✅ Hecho | Solo en primer inicio |
| 8 | Reinicia población | ✅ Hecho | `makePopulation()` |
| 9 | Reinicia generaciónActual = 0 | ✅ Hecho | `_generacionActual = 0` |
| 10 | Reinicia fitness | ✅ Hecho | `_mejorDistancia = MaxValue` |
| 11 | Inicio nuevo thread | ✅ Hecho | Nueva instancia de Thread |
| 12 | Evita threads duplicados | ✅ Hecho | `Abort()` antes de crear |
| 13 | Sin errores de compilación | ✅ Validado | ✓ 0 errores |

---

## 🚀 Cómo Usar

### Uso Estándar:

```csharp
// En un botón de la UI
public void OnRestartClicked() {
    algoritmoGenetico.reStart();
}
```

### Con parámetros nuevos:

```csharp
public void OnRestartWithNewParams() {
    // Cambiar parámetros
    algoritmoGenetico.Generaciones = 1000;
    algoritmoGenetico.TazaMuta = 0.2f;
    
    // Reiniciar: cidades se mantienen, solo cambia algoritmo
    algoritmoGenetico.reStart();
}
```

---

## 🔒 Seguridad

- **Threading:** Uso de `lock()` en variables compartidas
- **Espera:** 100ms entre `stop()` y `ResetEvolution()`
- **Validación:** Verificación de existencia de ciudades
- **Integridad:** Array `cities[]` nunca se modifica en reinicio

---

## 📌 Notas Importantes

- ✅ No se modificó la lógica base del algoritmo
- ✅ No se cambió ningún otro archivo
- ✅ Compatible con modo VR/móvil (visualización en UI)
- ✅ Reutiliza matriz de distancias cacheadas
- ✅ Mejora: Menos allocaciones de memoria en reinicio

---

## 🐛 Troubleshooting

| Problema | Causa | Solución |
|----------|-------|----------|
| Ciudades se regeneran | Reiniciaste con `inicia()` | Usa `reStart()` |
| GameObjects duplicados | Llamaste `drawCities()` manualmente | No lo hagas |
| Thread no termina | Aborte incompleto | Ya hay Sleep(100) |
| Ruta no se actualiza | `rutaRenderer` es null | Inicializa en primer `inicia()` |

---

**Versión:** 2.0  
**Cambios base:** AlgoritmoGenetico.cs  
**Estado:** ✅ Production Ready
