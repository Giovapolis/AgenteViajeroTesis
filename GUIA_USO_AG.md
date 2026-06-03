# 🚀 GUÍA DE USO - ALGORITMO GENÉTICO

## **INICIO RÁPIDO**

### **Paso 1: Verificar Asignaciones en Unity**

En el Inspector del GameObject con `ControlAGPropio`:
1. ✅ Asignar componente `AlgoritmoGenetico` en el campo
2. ✅ Asignar GameObject con `CityPrefab` (para visualización)
3. ✅ Asignar TextMeshPro para ver logs
4. ✅ Verificar que `ControlAGPropio` está activo

### **Paso 2: Configuración Inicial (Opcional)**

En `ControlAGPropio` se pueden ajustar valores por defecto:
```csharp
private int numGeneraciones = 100;      // Número de generaciones
private int numCiudades = 20;           // Número de ciudades a visitar
private float mutacion = 0.35f;         // Tasa de mutación (0.0 - 1.0)
private int individuos = 20;            // Tamaño de población
private int cruza = 0;                  // 0=PMX, 1=OX, 2=CX
private int seleccion = 0;              // 0=Ruleta, 1=Torneo, 2=Ranking
```

---

## **CONTROLES JOYSTICK**

### **Windows (Joystick/Gamepad)**

| Botón | Función | Acción |
|-------|---------|--------|
| **Button 0** (Y/Triángulo) | **INICIA AG** | Comienza evolución con parámetros actuales |
| **Button 1** (X/Cuadrado) | **DETIENE** | Pausa la evolución actual |
| **Button 3** (A/Cruz) | **REINICIA** | Comienza nuevo ciclo desde parámetros |
| **Button 4** (LB) | **SIGUIENTE PARÁMETRO** | Cicla entre 6 parámetros |
| **Button 6** (LT/L2) | **DISMINUYE** | Reduce valor del parámetro |
| **Button 7** (RT/R2) | **AUMENTA** | Aumenta valor del parámetro |

**Ejemplo:** Modificar número de generaciones:
1. Presionar Button 4 hasta que "Numero de Generaciones" esté subrayado
2. Presionar Button 6 para restar 50 generaciones
3. Presionar Button 7 para sumar 50 generaciones
4. Presionar Button 0 para INICIA AG

### **Android (Joystick Móvil)**

| Botón | Función |
|-------|---------|
| Button 0 | INICIA AG |
| Button 1 | DETIENE |
| Button 2 | REINICIA |
| Button 3 | SIGUIENTE PARÁMETRO |
| Button 4 | DISMINUYE |
| Button 5 | AUMENTA |

---

## **PARÁMETROS CONFIGURABLES**

### **0️⃣ NÚMERO DE GENERACIONES**
- **Rango:** 50 - 5000
- **Incremento:** ±50
- **Valor recomendado:** 100 - 500
- **Efecto:** Más generaciones = mejor solución (pero más lento)

### **1️⃣ NÚMERO DE CIUDADES**
- **Rango:** 5 - 50
- **Incremento:** ±1
- **Valor recomendado:** 10 - 30
- **Efecto:** Más ciudades = problema más difícil

### **2️⃣ TASA DE MUTACIÓN**
- **Rango Windows:** 20% - 50%
- **Rango Android:** 10% - 45%
- **Incremento:** ±5%
- **Valor recomendado:** 30% - 40%
- **Efecto:** Mayor mutación = más diversidad, menos convergencia

### **3️⃣ NÚMERO DE INDIVIDUOS (POBLACIÓN)**
- **Rango:** 25 - 500
- **Incremento:** ±25
- **Valor recomendado:** 30 - 100
- **Efecto:** Mayor población = más diversidad, más lento

### **4️⃣ MÉTODO DE CRUCE**
```
0 = PMX (Partially Mapped Crossover) [RECOMENDADO]
   ├─ Mapeo bidireccional entre padres
   ├─ Resuelve conflictos automáticamente
   └─ Mejor para TSP

1 = OX (Order Crossover)
   ├─ Mantiene ordem relativa
   ├─ Simple y rápido
   └─ Bueno para permutaciones

2 = CX (Cycle Crossover)
   ├─ Preserva ciclos cromosómicos
   ├─ Característico del padre 1
   └─ Experimental
```

### **5️⃣ MÉTODO DE SELECCIÓN**
```
0 = RULETA [RECOMENDADO]
│   ├─ Probabilidad proporcional a aptitud
│   ├─ Mejor para convergencia rápida
│   └─ Estándar en AGs

1 = TORNEO
│   ├─ N-way tournament selection
│   ├─ Mejor para evitar prematura convergence
│   └─ Más robusto en espacios noisy

2 = RANKING
    ├─ Basado en orden, no valor final
    ├─ Evita dominio de individuos muy buenos
    └─ Mejor para problemas balanceados
```

---

## **ESCENARIOS DE USO**

### **Escenario 1: CONVERGENCIA RÁPIDA**
```
Generaciones: 200
Ciudades: 10-15
Mutación: 20-25%
Individuos: 50
Cruce: PMX
Selección: Ruleta
→ Solución buena en ~1-2 minutos
```

### **Escenario 2: MÁXIMA PRECISIÓN**
```
Generaciones: 1000
Ciudades: 20-25
Mutación: 35-40%
Individuos: 100
Cruce: PMX
Selección: Torneo
→ Solución óptima en ~5-10 minutos
```

### **Escenario 3: EXPLORACIÓN**
```
Generaciones: 500
Ciudades: 30-40
Mutación: 45-50%
Individuos: 75
Cruce: OX
Selección: Ranking
→ Mayor exploración del espacio
```

### **Escenario 4: TESTING/DEBUG**
```
Generaciones: 50
Ciudades: 5
Mutación: 30%
Individuos: 20
Cruce: PMX
Selección: Ruleta
→ Ejecución rápida para verificar funcionamiento
```

---

## **INTERPRETACIÓN DE RESULTADOS**

### **Log de Consola**
```
Inicia el AG
Población creada: 20 individuos
Ciudades creadas: 20 puntos
Generación: 0, Mejor aptitud: 156.45
Generación: 10, Mejor aptitud: 125.32    ← mejora de 19.9%
Generación: 20, Mejor aptitud: 110.18    ← mejora de 12.1%
...
Generación: 100, Mejor aptitud: 65.34    ← mejora de 68.5% total
AG COMPLETADO. Generaciones: 100, Mejor aptitud: 65.34
```

### **Interpretación**
- **Mejor aptitud = DISTANCIA TOTAL** (menor es mejor para TSP)
- **Mejora % = Progreso del AG**
- Si aptitud se estabiliza → convergencia alcanzada
- Si aptitud sigue bajando → AG aún explorando

### **Salida en UI (TMP_Text)**
```
Recorrido: 3,1,8,5,7,2,0,6,4,9,
Aptitud: 65.34
```

Esto significa: visitar ciudades en orden: C3→C1→C8→C5→C7→C2→C0→C6→C4→C9→C3

---

## **TROUBLESHOOTING**

### ❌ **"AG no arranca"**
- ✅ Verificar que AlgoritmoGenetico está asignado
- ✅ Verificar que TazaMuta está entre 0.0 y 1.0
- ✅ Verificar Console por errores

### ❌ **"Aptitud no disminuye"**
- ✅ Aumentar Generaciones
- ✅ Aumentar Mutación
- ✅ Aumentar Individuos
- ✅ Cambiar método de Selección

### ❌ **"Game freezea"**
- ✅ Reducir Ciudades
- ✅ Reducir Individuos
- ✅ Reducir Generaciones
- ✅ Usar Método de Selección más rápido (Ruleta)

### ❌ **"Botones no responden"**
- ✅ Conectar joystick/gamepad físico
- ✅ Verificar que Input.GetKeyDown() está en Update()
- ✅ Verificar KeyCode es correcto

### ❌ **"TextMeshPro no actualiza"**
- ✅ Asignar campo `recorrido` en Inspector
- ✅ Verificar que TMP_Text es válido
- ✅ Revisar Console por excepciones

---

## **MÉTODOS AVANZADOS**

### **Acceso Programático a Resultados**

```csharp
// Obtener mejor solución
Cromosoma mejor = ag.best();
Debug.Log($"Mejor recorrido: {string.Join(",", mejor.Recorrido)}");
Debug.Log($"Distancia total: {mejor.Aptitud}");

// Obtener progreso actual
int generacionActual = ag.GeneracionActual;
Debug.Log($"Generación: {generacionActual}");

// Obtener descripción completa
Debug.Log(ag.ToString());
```

### **Cambiar Parámetros en Tiempo Real**

```csharp
// Mientras AG está en ejecución:
ag.TazaMuta = 0.5f;      // ❌ Cambios ignorados si AG corre
ag.Ciudades = 30;        // ❌ No afecta ciudades ya creadas
ag.stop();               // ✅ Detener primero
ag.reStart();            // ✅ Reiniciar con nuevos parámetros
```

### **Usar Diferentes Métodos**

```csharp
// Antes de inicia()
ag.MetSelec = 1;  // Cambiar a Torneo
ag.MetCruza = 2;  // Cambiar a CX
ag.inicia();
```

---

## **OPTIMIZACIONES RECOMENDADAS**

1. **Para problemas pequeños (5-10 ciudades):**
   - Generaciones: 50
   - Individuos: 15

2. **Para problemas medianos (15-25 ciudades):**
   - Generaciones: 100-300
   - Individuos: 30-50

3. **Para problemas grandes (30+ ciudades):**
   - Generaciones: 500+
   - Individuos: 75-100
   - Aumentar Mutación
   - Usar Torneo o Ranking

---

## **VALIDACIÓN DE FUNCIONAMIENTO**

### **Checklist Inicial**

- [ ] Población se crea (Debug log)
- [ ] Ciudades se generan (Debug log)
- [ ] Aptitud se calcula (valores > 0)
- [ ] Generaciones avanzan (0, 10, 20, ...)
- [ ] Aptitud disminuye (mejora progresiva)
- [ ] AG se detiene al finalizar
- [ ] UI muestra mejor cromosoma
- [ ] Botones responden (cambios de parámetros visibles)

### **Test Simple**

1. Configurar: 50 generaciones, 5 ciudades
2. Presionar Button 0 (INICIA)
3. Esperar ~30 segundos
4. Verificar que aptitud disminuye
5. Verificar que cromosoma se muestra en UI
6. Resutado: ✅ Sistema funciona correctamente

---

**Última actualización:** 5 Marzo 2026  
**Versión:** 1.0
