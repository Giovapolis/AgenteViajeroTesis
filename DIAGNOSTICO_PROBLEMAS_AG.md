# 🔍 ANÁLISIS DE DIAGNÓSTICO - Problemas de Congelamiento y Parada Prematura

**Fecha:** 2026-03-23  
**Objetivo:** Detectar por qué Unity se congela y por qué el AG se detiene antes de completar

---

## 🏴 PROBLEMAS IDENTIFICADOS

### ⚠️ PROBLEMA 1: Validación insuficiente de Generaciones (CRÍTICO)

**Ubicación:** `AlgoritmoGenetico.cs` - Método `inicia()`

**Código actual:**
```csharp
if (Generaciones <= 0) {
    Debug.LogWarning($"Generaciones ajustadas a 1 desde {Generaciones}");
    Generaciones = 1;
}
```

**ISSUE:** 
- Si `Generaciones` nunca fue confgurado, queda en su valor default
- No hay validación que confirme explícitamente el valor ANTES de iniciar el thread
- No hay log mostrando el valor FINAL de Generaciones

**Impacto:** 
- ❌ El algoritmo podría ejecutar solo 1 generación sin saberlo
- ❌ Sin log, no hay forma de saber cuál fue el valor configurado

---

### ⚠️ PROBLEMA 2: Condición de Loop crítica (CRITICAL)

**Ubicación:** `EvolutionThread()` - Línea 195

**Código actual:**
```csharp
while (isRunning && _generacionActual < Generaciones) {
    // Operaciones...
    
    lock (lockObject) {
        _generacionActual++;  // ← Se incrementa HERE
        // ...
    }
    
    Thread.Sleep(1);
}
```

**ISSUE:**
- La condición `_generacionActual < Generaciones` se evalúa ANTES de incrementar
- Después de incrementar, vuelve a evaluar la condición
- Si `Generaciones = 100` y `_generacionActual = 99`, entra al loop (99 < 100 = true)
- Incrementa a 100, vuelve a evaluar (100 < 100 = false), SALE DEL LOOP
- ✅ Esto es CORRECTO matemáticamente de 0 a 99 = 100 iteraciones
- ⚠️ PERO: Si hay excepciones dentro del loop, sale sin completar esa iteración

---

### ⚠️ PROBLEMA 3: Manejo deficiente de excepciones (SEVERO)

**Ubicación:** `EvolutionThread()` - try-catch

**Código actual:**
```csharp
try {
    while (isRunning && _generacionActual < Generaciones) {
        // ... TODO EL CÓDIGO DEL ALGORITMO
    }
    Debug.Log($"AG COMPLETADO. Generaciones procesadas: {_generacionActual}");
} 
catch (ThreadAbortException) {
    Debug.Log("Thread abortado");
} 
catch (Exception e) {
    Debug.LogError($"Error en EvolutionThread: {e.Message}");
}
```

**ISSUE:**
- ✅ Captura general de excepciones
- ❌ **NO marca de dónde vino el error** (qué generación falló)
- ❌ **NO stacktrace completo** (solo mensaje)
- ❌ **Si ocurre excepción, el algoritmo se detiene silenciosamente**
- ❌ **No hay indicación de en qué generación ocurrió**

**Ejemplo problemático:**
```
Generación 45: División por cero en evaluatePopulation()
└─ Excepción capturada: "Attempted to divide by zero"
└─ Debug.LogError mostraría SOLO "Attempted to divide by zero"
└─ Usuario nunca sabrá que falló en gen 45
└─ Parecería que el AG simplemente "se detiene"
```

---

### ⚠️ PROBLEMA 4: Logs insuficientes para debugging (IMPORTANTE)

**Ubicación:** `EvolutionThread()` - no hay logs por generación

**Código actual:**
```csharp
while (isRunning && _generacionActual < Generaciones) {
    // 1. Selección
    SeleccionarIndices();      // ← Sin log

    // 2. Cruza
    Reproducir();              // ← Sin log

    // 3. Mutación
    MutarPoblacion();           // ← Sin log

    // 4. Reemplazo...
    // 5. Evaluación...
    
    // Solo al final:
    Debug.Log($"AG COMPLETADO. Generaciones procesadas: {_generacionActual}");
}
```

**ISSUE:**
- ❌ **No hay forma de ver el progreso** del algoritmo
- ❌ **Si falla en gen 50 de 100, usuario no sabe**
- ❌ **Sin logs por generación = imposible debuggear**

---

### ⚠️ PROBLEMA 5: Sin validación de parámetros secundarios (IMPORTANTE)

**Ubicación:** `inicia()` - validación incompleta

**Código actual:**
```csharp
Individuos = Mathf.Clamp(Individuos, 2, MaxIndividuos);
if (Individuos != individuos) Debug.LogWarning($"Individuos ajustados a {Individuos}");

Ciudades = Mathf.Clamp(Ciudades, 2, MaxCiudades);
if (Ciudades != ciudades) Debug.LogWarning($"Ciudades ajustadas a {Ciudades}");
```

**ISSUE:**
- ✅ Clamp está bien
- ❌ **Generaciones NO está en un Clamp, solo en una simple validación**
- ❌ **TazaMuta puede venir negativa o > 1.0, sin validación**
- ❌ **MetSelec y MetCruza sin validación de rango**

---

### ⚠️ PROBLEMA 6: Sin contador de intentos infinitos (IMPORTANTE)

**Ubicación:** `makePopulation()`, `SeleccionPorRuleta()`, etc.

**Riesgo:**
- Si `poblacion` es null, el loop infinito en `SeleccionPorRuleta()` puede colgar
- Si `distancias` es null, división por cero en `calculateAptitud()`
- Sin validaciones = posible congelamiento

---

### ⚠️ PROBLEMA 7: Sin verificación de inicialización (IMPORTANTE)

**Ubicación:** `inicia()` al inicio

**Código actual:**
```csharp
public void inicia() {
    Debug.Log("Inicia el AG");
    
    // Validación de parámetros críticos
    if (Generaciones <= 0) {
        Debug.LogWarning($"Generaciones ajustadas a 1 desde {Generaciones}");
        Generaciones = 1;
    }
    // ... resto
}
```

**ISSUE:**
- ❌ **No valida si `Generaciones` nunca fue seteado**
- ❌ **No muestra logs de TODOS los parámetros al inicio**
- ❌ **No confirma que `cities[]` fue generado correctamente**

---

## 📊 Resumen de Problemas

| # | Problema | Severidad | Síntoma |
|---|----------|-----------|---------|
| 1 | Validación insuficiente Generaciones | 🔴 Alta | Ejecuta 1 gen sin saberlo |
| 2 | Condición de loop potencialmente incorrecta | 🟡 Media | Podría saltar generación final |
| 3 | Excepciones sin contexto | 🔴 Alta | AG se detiene sin info útil |
| 4 | Sin logs por generación | 🟡 Media | Imposible debuggear problemas |
| 5 | Sin validación de parámetros secundarios | 🟡 Media | Valores inválidos podrían colapsar |
| 6 | Sin protección contra loops infinitos | 🔴 Alta | Posible congelamiento |
| 7 | Sin confirmación de inicialización | 🟡 Media | Estado desconocido al iniciar |

---

## ✅ SOLUCIONES A IMPLEMENTAR

### 1. Validación completa de parámetros
```csharp
// Validar TODOS los parámetros con logs
if (Generaciones <= 0) Generaciones = 1;
if (Ciudades < 2) Ciudades = 2;
if (Individuos < 2) Individuos = 2;
if (TazaMuta < 0) TazaMuta = 0.1f;
if (TazaMuta > 1) TazaMuta = 0.5f;
if (MetSelec < 0 || MetSelec > 2) MetSelec = 0;
if (MetCruza < 0 || MetCruza > 2) MetCruza = 0;
```

### 2. Logging exhaustivo al iniciar
```csharp
Debug.Log("════════════════════════════════════");
Debug.Log("INICIANDO ALGORITMO GENÉTICO");
Debug.Log($"  Ciudades: {Ciudades}");
Debug.Log($"  Individuos: {Individuos}");
Debug.Log($"  Generaciones: {Generaciones} ← CRÍTICO");
Debug.Log($"  Tasa Mutación: {TazaMuta:P2}");
Debug.Log($"  Selección: {MetSelec} (0=Ruleta, 1=Torneo, 2=Ranking)");
Debug.Log($"  Cruzamiento: {MetCruza} (0=PMX, 1=OX, 2=CX)");
Debug.Log("════════════════════════════════════");
```

### 3. Logs por generación
```csharp
lock (lockObject) {
    _generacionActual++;
    
    // LOG CRÍTICO
    if (_generacionActual % 10 == 0) {  // Cada 10 gen para no saturar
        Debug.Log($"Generación {_generacionActual}/{Generaciones} " +
                  $"- Mejor distancia: {_mejorDistancia:F2}");
    }
}
```

### 4. Excepción detallada
```csharp
catch (Exception e) {
    Debug.LogError(
        "════════════════════════════════════\n" +
        $"ERROR EN GENERACIÓN {_generacionActual}/{Generaciones}\n" +
        $"Mensaje: {e.Message}\n" +
        $"StackTrace:\n{e.StackTrace}\n" +
        "════════════════════════════════════");
}
```

### 5. Validación en `makePopulation()`
```csharp
public void makePopulation() {
    if (Ciudades <= 0) {
        Debug.LogError("ERROR: Ciudades <= 0 en makePopulation");
        return;
    }
    if (Individuos <= 0) {
        Debug.LogError("ERROR: Individuos <= 0 en makePopulation");
        return;
    }
    // ... resto
}
```

---

## 🎯 PRÓXIMOS PASOS

Implementar:
1. Validación exhaustiva en `inicia()`
2. Logging detallado al inicio
3. Logs por generación (cada 10 o cada 50)
4. Excepciones con contexto completo
5. Validaciones en `makePopulation()` y `makeCities()`
6. Confirmación de inicialización completa

---

**Estado:** 🔍 DIAGNÓSTICO COMPLETADO
