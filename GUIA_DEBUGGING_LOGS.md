# 🔧 GUÍA DE DEBUGGING - Interpretar Logs del AG

**Objetivo:** Entender los nuevos logs para identificar problemas rápidamente

---

## 📋 Flujo de Logs Esperado

### 1️⃣ Inicio (Aparecen al clickear "Comenzar")

```
═════════════════════════════════════════════════════════════
>>> INICIA ALGORITMO GENÉTICO <<<
═════════════════════════════════════════════════════════════
⚠️ GENERACIONES INVÁLIDO: 0 → ajustado a 1
⚠️ CIUDADES AJUSTADO: 0 → 5
⚠️ INDIVIDUOS AJUSTADO: 0 → 25
⚠️ TAZA MUTACIÓN NEGATIVA: -0.5 → ajustado a 0.1
⚠️ MÉTODO SELECCIÓN INVÁLIDO: 5 → ajustado a 0 (Ruleta)
┌─ PARÁMETROS FINALES CONFIRMADOS ─────────────────────────┐
│ Ciudades:           5
│ Individuos:         25
│ Generaciones:       1000 ← NÚMERO OBJETIVO
│ Tasa Mutación:      10.0% (0.100)
│ Método Selección:   Ruleta (opción 0)
│ Método Cruzamiento: PMX (opción 0)
└───────────────────────────────────────────────────────────┘
✅ Población creada: 25 individuos con 5 ciudades cada uno
✅ Ciudades creadas: 5 puntos con ubicaciones aleatorias
✅ Matriz de distancias cacheada: 5×5 = 25 valores
🧬 THREAD DE EVOLUCIÓN INICIADO - Ejecutará 1000 generaciones
```

**✅ Lo que significa:**
- Todos los parámetros se validaron correctamente
- AG está listo para comenzar
- Se ejecutarán 1000 generaciones

---

### 2️⃣ Progreso (Aparece cada 25 generaciones)

```
  Gen 0001/1000 | Mejor distancia: 125.3456
  Gen 0025/1000 | Mejor distancia: 89.2341
  Gen 0050/1000 | Mejor distancia: 67.5432
  Gen 0075/1000 | Mejor distancia: 54.1234
  Gen 0100/1000 | Mejor distancia: 47.8901
  ... (continúa cada 25 gen)
  Gen 0975/1000 | Mejor distancia: 34.5612
  Gen 1000/1000 | Mejor distancia: 34.5612
```

**✅ Lo que significa:**
- El algoritmo está ejecutando generaciones
- La distancia está mejorando (número más bajo = mejor)
- Si ves el número de generación aumentando: ✅ FUNCIONA BIEN

---

### 3️⃣ Finalización (Aparece al completarse)

```
✅ AG COMPLETADO EXITOSAMENTE - 1000/1000 generaciones ejecutadas
   Mejor distancia encontrada: 34.5612
```

**✅ Lo que significa:**
- Se ejecutaron TODAS las generaciones configuradas
- El algoritmo terminó normalmente
- La mejor solución es 34.5612

---

## 🚨 Problemas y Sus Síntomas

### PROBLEMA 1: Generaciones Configuradas a 0

**Logs que verás:**
```
⚠️ GENERACIONES INVÁLIDO: 0 → ajustado a 1
┌─ PARÁMETROS FINALES CONFIRMADOS ─         ┐
│ Generaciones:       1 ← AJUSTADO AUTOMÁTICAMENTE
└───────────────────────────────────────────┘
  Gen 0001/1 | Mejor distancia: 125.3456
✅ AG COMPLETADO EXITOSAMENTE - 1/1 generaciones ejecutadas
```

**CAUSA:** No configuraste las generaciones en la UI  
**SOLUCIÓN:** Aumenta el valor de "Generaciones" en la UI ANTES de clickear "Comenzar"

---

### PROBLEMA 2: Algoritmo se Detiene en Generación X

**Logs que verás:**
```
  Gen 0050/1000 | Mejor distancia: 67.5432
❌ ERROR EN GENERACIÓN 51/1000
Excepción: IndexOutOfRangeException
Mensaje: Index was outside the bounds of the array.
StackTrace:
  at AlgoritmoGenetico.calculateAptitud...
```

**CAUSA:** Error de indexación (chromosome.Recorrido contiene índice inválido)  
**SOLUCIÓN:** 
1. Nota la generación donde falló (51)
2. Aumenta el número de ciudades (puede ser muy pequeño)
3. Reinicia el algoritmo

---

### PROBLEMA 3: Algoritmo se Detiene Prematuramente sin Error

**Logs que verás:**
```
  Gen 0050/1000 | Mejor distancia: 67.5432
  Gen 0075/1000 | Mejor distancia: 54.1234
⚠️ AG INTERRUMPIDO - Solo 100/1000 generaciones (isRunning=false)
```

**CAUSA:** Se clickeó el botón "Parar" (stop) o "Reiniciar"  
**SOLUCIÓN:** No clickees parar si quieres que continúe. Espera a que diga "COMPLETADO"

---

### PROBLEMA 4: Congelamiento de Unity

**Síntoma:** Unity no responde, está freezed

**Para Debuggear:**
1. Abre la Console de Unity (Window → Console)
2. Si ves estos logs, significa que NO es congelamiento de threading:
   - Logs de inicialización → ✅ sistema inició
   - Logs de generaciones → ✅ generaciones se ejecutan

3. Si NO ves NINGÚN log:
   - Puede ser que `inicia()` nunca fue llamado
   - Revisa si clickeaste el botón correcto
   - O si el script está asignado al GameObject correcto

4. Si ves logs pero se detiene:
   - Busca líneas que comiencen con `❌ ERROR`
   - Esa es tu pista de dónde se detuvo

---

### PROBLEMA 5: aptTotal <= 0 (Edge Case)

**Logs que verás:**
```
⚠️ WARNING: aptTotal <= 0 en SeleccionPorRuleta. Usando selección aleatoria uniforme.
```

**CAUSA:** Todos los cromosomas tienen la misma distancia (raro, pero posible)  
**IMPACTO:** El sistema usa selección aleatoria en lugar de por ruleta  
**SOLUCIÓN:** Nada, es un fallback automático. El algoritmo continúa.

---

### PROBLEMA 6: Valores Inválidos en Parámetros

**Logs que verás en Inicio:**
```
⚠️ GENERACIONES AJUSTADO: 0 → 1
⚠️ INDIVIDUOS AJUSTADO: 0 → 2
⚠️ CIUDADES AJUSTADO: 0 → 2
⚠️ TAZA MUTACIÓN NEGATIVA: -0.5 → ajustado a 0.1
⚠️ TAZA MUTACIÓN > 1: 1.5 → ajustado a 0.5
⚠️ MÉTODO SELECCIÓN INVÁLIDO: 5 → ajustado a 0
⚠️ MÉTODO CRUZAMIENTO INVÁLIDO: 10 → ajustado a 0
```

**CAUSA:** Configuraste valores inválidos en la UI  
**IMPACTO:** Se ajustan automáticamente a valores por defecto  
**SOLUCIÓN:** 
- **Generaciones:** > 0, recomendado 100-1000
- **Ciudades:** 2-30
- **Individuos:** 2-100+
- **Tasa Mutación:** 0.0 a 1.0 (porcentaje)
- **Métodos:** 0-2

---

## 🎯 Checklist de Debugging

### Si el AG no inicia:
- [ ] ¿Ves el log "INICIA ALGORITMO GENÉTICO"?
  - Si SÍ → continúa con siguiente check
  - Si NO → `inicia()` nunca fue llamado. Revisa los botones.

- [ ] ¿Ves el log "THREAD DE EVOLUCIÓN INICIADO"?
  - Si SÍ → thread se inició correctamente
  - Si NO → Error en inicialización. Revisa parámetros.

### Si el AG se detiene prematuramente:
- [ ] ¿Ves logs de generaciones?
  - Si SÍ → mira el número donde se detiene
  - Si NO → nunca entró al loop. Revisa parámetros.

- [ ] ¿Ves algún `❌ ERROR` en los logs?
  - Si SÍ → Lee el StackTrace, esa es tu pista
  - Si NO → Abriste stop() prematuramente

- [ ] ¿Dice "INTERRUMPIDO" al final?
  - Si SÍ → Se clickeó parar. Eso es normal.
  - Si NO → Error sin capturar. Revisa Console para excepciones.

### Si Unity se congela:
- [ ] ¿Ves el log "INICIA ALGORITMO GENÉTICO"?
  - Si NO → Congelamiento antes del AG. Problema en otro lado.
  - Si SÍ → AG inició. Congelamiento está EN el AG.

- [ ] ¿Ves logs de generaciones?
  - Si SÍ → Generaciones se ejecutan. No es infinite loop.
  - Si NO → Infinite loop en inicialización (makeCities, makePopulation).

- [ ] ¿Ves un `❌ ERROR` sin catch?
  - Si SÍ → Esa es la razón del congelamiento.
  - Si NO → Bug en threading. Revisa stop().

---

## 💡 Ejemplos de Debugging Real

### Ejemplo 1: "Mi AG ejecuta solo 1 generación"

**Logs:**
```
│ Generaciones:       1 ← AJUSTADO AUTOMÁTICAMENTE
  Gen 0001/1 | Mejor distancia: 125.3456
✅ AG COMPLETADO EXITOSAMENTE - 1/1 generaciones
```

**Problema:** Configuraste Generaciones = 0 en la UI  
**Solución:**
```csharp
// En ControlAGPropio.cs, aumenta el valor:
numGeneraciones = 1000;  // Antes era 0 o muy pequeño
```

---

### Ejemplo 2: "Mi AG se detiene en generación 256"

**Logs:**
```
  Gen 0200/1000 | Mejor distancia: 67.5432
  Gen 0225/1000 | Mejor distancia: 53.2341
❌ ERROR EN GENERACIÓN 256/1000
Excepción: IndexOutOfRangeException
Mensaje: Index was outside the bounds of the array.
StackTrace:
  at AlgoritmoGenetico.calculateAptitud(Cromosoma cromosoma) in
  AlgoritmoGenetico.cs:line 456
```

**Problema:** Problema en calculateAptitud() con índices fuera de rango  
**Solución:**
1. Aumenta el número de ciudades (puede ser problema de generación de recorridos)
2. Verifica que CrearRecorridoAleatorio() no genere índices inválidos
3. Revisa que ciudades no se modifiquen durante la evolución

---

### Ejemplo 3: "Mi algoritmo se ejecuta correctamente"

**Logs:**
```
═════════════════════════════════════════════════════════════
>>> INICIA ALGORITMO GENÉTICO <<<
═════════════════════════════════════════════════════════════
┌─ PARÁMETROS FINALES CONFIRMADOS ─────────────────────────┐
│ Ciudades:           20
│ Individuos:         100
│ Generaciones:       1000 ← NÚMERO OBJETIVO
│ Tasa Mutación:      15.0% (0.150)
│ Método Selección:   Ruleta (opción 0)
│ Método Cruzamiento: PMX (opción 0)
└───────────────────────────────────────────────────────────┘
✅ Población creada: 100 individuos con 20 ciudades
✅ Ciudades creadas: 20 puntos
✅ Matriz de distancias cacheada: 20×20 = 400 valores
🧬 THREAD DE EVOLUCIÓN INICIADO - Ejecutará 1000 generaciones
  Gen 0001/1000 | Mejor distancia: 145.3421
  Gen 0025/1000 | Mejor distancia: 98.2341
  Gen 0050/1000 | Mejor distancia: 76.5432
  ...
  Gen 1000/1000 | Mejor distancia: 42.1234
✅ AG COMPLETADO EXITOSAMENTE - 1000/1000 generaciones ejecutadas
   Mejor distancia encontrada: 42.1234
```

**Estado:** ✅ TODO ESTÁ BIEN - Algoritmo funcionando correctamente

---

## 🎓 Qué Buscar en los Logs

| Log | Significado |
|-----|------------|
| `INICIA ALGORITMO GENÉTICO` | ✅ Inicio correcto |
| `PARÁMETROS FINALES CONFIRMADOS` | ✅ Validación pasó |
| `Población creada: X individuos` | ✅ Población OK |
| `Ciudades creadas: X` | ✅ Ciudades OK |
| `THREAD DE EVOLUCIÓN INICIADO` | ✅ Thread arrancó |
| `Gen X/Y` | ✅ Progreso visible |
| `COMPLETADO EXITOSAMENTE` | ✅ Terminó correctamente |
| `⚠️ WARNING` | ⚠️ Advertencia (no fatal) |
| `❌ ERROR` | ❌ Error (fatal, se detiene) |
| `INTERRUMPIDO` | ⚠️ Parado por usuario |

---

**Última actualización:** 2026-03-23  
**Propósito:** Facilitar debugging rápido  
**Versión:** 1.0
