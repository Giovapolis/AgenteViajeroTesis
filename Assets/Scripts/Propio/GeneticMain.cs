using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;

public class GeneticMain : MonoBehaviour {
    [SerializeField]
    public TMP_Text recorrido;
    public GameObject CityPrefab;

    private AlgoritmoGenetico Ag = null;
    private Action<List<Cromosoma>, List<int>> sendMetod;
    private System.Random rnd = new System.Random();

    private int metodSelect = 0;
    private int metodCruza = 0;

    public int MetodSelect { get => metodSelect; set => metodSelect = value; }
    public int MetodCruza { get => metodCruza; set => metodCruza = value; }

    void Start() {

    }

    void Update() {
        if (Ag != null) {
            recorrido.text = (Ag.best()).ToString();
        }
    }

    public void startAg() {
        //switch (MetodSelect) {
        //    case 0:
        //        sendMetod = SeleccionPorRuleta;
        //        break;
        //    case 1:
        //        sendMetod = SeleccionPorRangoLineal;
        //        break;
        //    case 2:
        //        sendMetod = selectTournament;
        //        break;
        //}

        //Ag = new AlgoritmoGenetico(
        //    (int)individuosAG.value,
        //    (int)ciudades.value,
        //    (int)generaciones.value,
        //    (tazaMutacion.value * 5) / 100,
        //    sendMetod,
        //    CityPrefab);

        Ag.inicia();
    }

    public void stopAg() {
        Ag.stop();
    }

    public void reStartAg() {
        if (Ag != null) {
            Ag.reStart();
        }
        else {
            Debug.Log("Funcion de reinicio null");
            startAg();
        }

    }

    //void SeleccionPorRuleta(List<Cromosoma> poblacion, List<int> betters) {
    //    Debug.Log("Seleccion por Rango Ruleta");
    //    List<Cromosoma> seleccionados = new List<Cromosoma>();
    //    double aptitudTotal = poblacion.Sum(i => i.Aptitud);
    //    for (int i = 0; i < ciudades.value; i++) {
    //        double probabilidad = rnd.NextDouble() * aptitudTotal;
    //        double acumulado = 0;
    //        foreach (Cromosoma individuo in poblacion) {
    //            acumulado += individuo.Aptitud;
    //            if (acumulado > probabilidad) {
    //                seleccionados.Add(individuo);
    //                break;
    //            }
    //        }
    //    }
    //}

    //void SeleccionPorRangoLineal(List<Cromosoma> poblacion, List<int> betters) {
    //        Debug.Log("Seleccion por Rango Lineal");
    //        List<Cromosoma> seleccionados = new List<Cromosoma>();
    //        int N = poblacion.Count;
    //        double a = 1.5; // parámetro de ajuste
    //        double b = -0.5; // parámetro de ajuste
    //        poblacion = poblacion.OrderByDescending(i => i.Aptitud).ToList();

    //        for (int i = 0; i < ciudades.value; i++) {
    //            double r = rnd.NextDouble();
    //            double j = (a - b) / (N - 1) * i + b;
    //            int index = (int)Math.Floor((j + r) * N);
    //            seleccionados.Add(poblacion[index]);
    //        }
    //    }

    //void selectTournament(List<Cromosoma> poblacion, List<int> betters) {
    //        Debug.Log("Seleccion por torneo");
    //        betters.Clear();
    //        for (int i = 0; i < poblacion.Count; i++) {
    //            // Seleccionar dos individuos aleatorio de la población para enfrentar
    //            int idx1 = rnd.Next(0, poblacion.Count);
    //            int idx2 = rnd.Next(0, poblacion.Count);

    //            // Seleccionar el individuo con mejor aptitud
    //            if (poblacion[idx1].Aptitud >= poblacion[idx2].Aptitud) {
    //                betters.Add(idx1);
    //            }
    //            else {
    //                betters.Add(idx2);
    //            }
    //        }
    //    }
}