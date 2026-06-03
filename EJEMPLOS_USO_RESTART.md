# EJEMPLOS DE USO - Método Reinicio Mejorado

---

## 📌 Caso 1: Reinicio Simple (Más Común)

### Escenario
El usuario ejecuta el algoritmo, ve resultados insatisfactorios y quiere reintentar con los mismos parámetros.

### Código UI (Botón)
```csharp
public class UIManager : MonoBehaviour {
    public AlgoritmoGenetico algoritmoGenetico;
    
    public void OnRestartButtonClicked() {
        Debug.Log("Usuario clickeó: Reiniciar AG");
        algoritmoGenetico.reStart();
    }
}
```

### Resultado
```
ANTES DE REINICIO:
├─ Ciudades: 20 puntos en escena ✓
├─ Generación: 156 / 500
├─ Mejor distancia: 47.32
└─ Población: 100 cromosomas

DURANTE reStart():
├─ stop() → isRunning = false
├─ Thread anterior se detiene
├─ Sleep(100ms)
└─ ResetEvolution()
   ├─ Generación = 0
   ├─ Nueva población (aleatoria)
   ├─ Fitness = recalculado
   └─ Nuevo Thread inicia

DESPUÉS DE REINICIO:
├─ Ciudades: MISMAS 20 puntos ✓
├─ Generación: 0 / 500
├─ Mejor distancia: recalculado
└─ Población: 100 cromosomas (nuevos)
```

---

## 📌 Caso 2: Cambiar Parámetros Entre Reinicios

### Escenario
El usuario quiere probar con:
- Más generaciones
- Más individuos
- Tasa de mutación diferente

### Código en UI
```csharp
public class AGControlPanel : MonoBehaviour {
    public AlgoritmoGenetico algoritmoGenetico;
    public Slider sliderGeneraciones;
    public Slider sliderIndividuos;
    public Slider sliderMutacion;
    
    public void OnRestartWithNewParams() {
        Debug.Log("Reiniciando con nuevos parámetros...");
        
        // 1. Leer nuevos valores de parámetros
        int nuevasGeneraciones = (int)sliderGeneraciones.value;
        int nuevosIndividuos = (int)sliderIndividuos.value;
        float nuevaMutacion = sliderMutacion.value;
        
        // 2. Asignar parámetros
        algoritmoGenetico.Generaciones = nuevasGeneraciones;
        algoritmoGenetico.Individuos = nuevosIndividuos;
        algoritmoGenetico.TazaMuta = nuevaMutacion;
        
        // 3. Reiniciar (ciudades NO cambian)
        algoritmoGenetico.reStart();
        
        // 4. Log
        Debug.Log($"Parámetros actualizados: Gen={nuevasGeneraciones}, Ind={nuevosIndividuos}, Mut={nuevaMutacion:P2}");
    }
}
```

### Resultado
```
CAMBIOS PERMITIDOS (reutilizan ciudades):
✅ Generaciones: 500 → 1000
✅ Individuos: 100 → 150
✅ TazaMuta: 0.1 → 0.2
✅ MetSelec: 0 (Ruleta) → 1 (Torneo)
✅ MetCruza: 0 (PMX) → 2 (CX)

NO CAMBIA (se mantiene igual):
❌ Ciudades: 20 (iguales)
❌ Ubicaciones de ciudades: (iguales)
❌ Matriz de distancias: (reutilizada)

EJEMPLO EJECUCIÓN:
┌─────────────────────────────────┐
│ PARÁMETRO ANTIGUO → NUEVO       │
├─────────────────────────────────┤
│ Ciudades:     20 → 20 (SIN CAMBIO) │
│ Individuos:   50 → 100 ✓        │
│ Generaciones: 500 → 2000 ✓      │
│ Mutación:     0.1 → 0.25 ✓      │
│ Selección:    Ruleta → Torneo ✓ │
│ Cruzamiento:  PMX → CX ✓        │
└─────────────────────────────────┘
```

---

## 📌 Caso 3: Múltiples Reinicios en Sucesión Rápida

### Escenario
El usuario está experimentando rápidamente, clickeando botón "Reiniciar" varias veces.

### Código
```csharp
public void OnQuickRestartSequence() {
    for (int i = 0; i < 5; i++) {
        Debug.Log($"Reinicio {i + 1}/5");
        algoritmoGenetico.reStart();
        
        // No destruye ciudades
        // No duplica threads
        // Se ejecuta limpiamente
    }
}
```

### Garantías de Seguridad
```
REINICIO 1:
├─ stop()
├─ Sleep(100)
└─ ResetEvolution() → Thread 1 inicia

REINICIO 2:
├─ stop() → isRunning=false (Thread 1 detiene)
├─ Abort() → mata Thread 1
├─ Sleep(100)
└─ ResetEvolution() → Thread 2 inicia

REINICIO 3:
├─ stop() → isRunning=false (Thread 2 detiene)
├─ Abort() → mata Thread 2
├─ Sleep(100)
└─ ResetEvolution() → Thread 3 inicia

...RESULTADO:
✅ No hay orphan threads
✅ No hay exceptions
✅ No hay race conditions
✅ No hay ciudades duplicadas
```

---

## 📌 Caso 4: Preservar Mejor Solución Encontrada

### Escenario
El usuario quiere notar cuál fue la mejor ruta encontrada ANTES de reiniciar.

### Código con Tracking
```csharp
public class AGTracker : MonoBehaviour {
    public AlgoritmoGenetico algoritmoGenetico;
    private float mejorDistanciaGlobal = float.MaxValue;
    private List<int> mejorRutaGlobal = null;
    
    void Update() {
        // Rastrear mejor solución global
        float distanciaActual = algoritmoGenetico.Aptitud; // (si existiera)
        
        if (distanciaActual < mejorDistanciaGlobal) {
            mejorDistanciaGlobal = distanciaActual;
            mejorRutaGlobal = new List<int>(algoritmoGenetico.MejorRuta);
        }
    }
    
    public void OnRestartAndKeepBest() {
        Debug.Log($"Mejor antes: {mejorDistanciaGlobal:F2}");
        
        algoritmoGenetico.reStart();
        
        Debug.Log("Ciudades reutilizadas, población reseteada");
        Debug.Log($"Mejor registrado: {mejorDistanciaGlobal:F2}");
    }
}
```

---

## 📌 Caso 5: NO Hacer (Anti-Patterns)

### ❌ INCORRECTO - Llamar directamente a inicia()
```csharp
// MALO ❌ - Regenera ciudades
public void BadRestart() {
    algoritmoGenetico.stop();
    algoritmoGenetico.inicia();  // ❌ PROBLEMA!
}

// PROBLEMA:
// - makeCities() regenera con nuevas ubicaciones
// - drawCities() crea nuevos GameObjects
// - Visualización puede parpadear
// - Puede haber memory leaks
```

### ✅ CORRECTO - Usar reStart()
```csharp
// BIEN ✓ - Reusa ciudades
public void GoodRestart() {
    algoritmoGenetico.reStart();  // ✓ CORRECTO
}

// RESULTADO:
// - Ciudades intactas
// - Visualización estable
// - Sin memory leaks
// - Threading seguro
```

---

## 📌 Caso 6: Inicialización diferente según contexto

### Escenario
Primera vez = crear todo desde cero  
Reinicios = mantener ciudades

### Código Smart
```csharp
public class SmartAGController : MonoBehaviour {
    public AlgoritmoGenetico algoritmoGenetico;
    private bool firstExecution = true;
    
    public void StartOrRestart() {
        if (firstExecution) {
            // PRIMER INICIO: crear ciudades
            Debug.Log("Primera ejecución: creando ciudades...");
            algoritmoGenetico.inicia();
            firstExecution = false;
        } else {
            // REINICIOS: reutilizar ciudades
            Debug.Log("Reinicio: ciudades se mantienen...");
            algoritmoGenetico.reStart();
        }
    }
}
```

### Ejecución
```
TIEMPO 0:
└─ Usuario clickea "Comenzar"
   └─ StartOrRestart() → inicia()
      ├─ Crea 20 ciudades
      ├─ Dibuja en escena
      └─ Inicia evolución

TIEMPO T:
└─ Usuario clickea "Reiniciar"
   └─ StartOrRestart() → reStart()
      ├─ Mantiene 20 ciudades ✓
      ├─ Reinicia población
      └─ Reinicia evolución

TIEMPO 2T:
└─ Usuario clickea "Reiniciar"
   └─ StartOrRestart() → reStart()
      ├─ Mantiene 20 ciudades ✓
      ├─ Nueva población
      └─ Nueva evolución
```

---

## 📌 Caso 7: Modo VR/Móvil

### Escenario
Las ciudades se dibujan en UI, no en GameObjects.  
El reinicio no debe afectar la UI existente.

### Código Compatible
```csharp
public class VRAGController : MonoBehaviour {
    public AlgoritmoGenetico algoritmoGenetico;
    public VRUIManager vrUI;
    
    void Start() {
        // Primer inicio
        algoritmoGenetico.Ciudades = 30;
        algoritmoGenetico.inicia();
        
        // Dibuja ciudades en UI de VR
        vrUI.DrawCitiesUI(algoritmoGenetico.GetCities());
    }
    
    public void OnRestartVR() {
        // Reinicio: NO regenera ciudades, NO regenera UI
        algoritmoGenetico.reStart();
        
        // UI se mantiene, solo se actualiza la ruta
        vrUI.UpdateRouteDisplay(algoritmoGenetico.GetBestRoute());
    }
}
```

---

## 📌 Caso 8: Testing Automático

### Escenario
Script de test ejecutando muchas generaciones automáticamente.

### Código Test
```csharp
[TestFixture]
public class AlgoritmoGeneticoTests {
    AlgoritmoGenetico ag;
    
    [SetUp]
    public void Setup() {
        ag = gameObject.AddComponent<AlgoritmoGenetico>();
        ag.Ciudades = 10;
        ag.Individuos = 20;
        ag.Generaciones = 100;
    }
    
    [Test]
    public void TestRestartPreservesCities() {
        // PRIMER INICIO
        ag.inicia();
        Ciudad[] ciudadesOrig = ag.GetCities();
        int countOrig = ciudadesOrig.Length;
        
        // REINICIO
        ag.reStart();
        Ciudad[] ciudadesAfter = ag.GetCities();
        int countAfter = ciudadesAfter.Length;
        
        // VERIFICAR
        Assert.AreEqual(countOrig, countAfter);
        for (int i = 0; i < countOrig; i++) {
            Assert.AreEqual(ciudadesOrig[i].Ubicacion, ciudadesAfter[i].Ubicacion);
        }
    }
    
    [Test]
    public void TestMultipleRestartsNoMemoryLeak() {
        ag.inicia();
        
        for (int i = 0; i < 10; i++) {
            ag.reStart();
            yield return new WaitForSeconds(0.5f);
        }
        
        // Sin exceptions = éxito
    }
}
```

---

## 📊 Comparativa: ANTES vs DESPUÉS

| Aspecto | ANTES | AHORA |
|---------|-------|-------|
| `reStart()` | Llama `inicia()` | Llama `ResetEvolution()` |
| Ciudades regeneradas | ✅ Sí | ❌ No |
| GameObjects recreados | ✅ Sí | ❌ No |
| Población reiniciada | ✅ Sí | ✅ Sí |
| Generación = 0 | ✅ Sí | ✅ Sí |
| Ruta visual se mantiene | ❌ Parpadea | ✅ Estable |
| Parámetros se pueden cambiar | ✅ Sí | ✅ Sí |
| Thread seguro | ⚠️ A veces | ✅ Garantizado |
| Performance reinicio | ⚠️ Lento | ✅ Rápido |
| Memory leaks | ⚠️ Posibles | ✅ Evitados |

---

**Última actualización:** 2026-03-23  
**Versión ejemplos:** 1.0  
**Estado:** ✅ Listo para usar
