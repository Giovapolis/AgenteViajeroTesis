using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Cromosoma : MonoBehaviour
{
    //Atributos
    private List<int> recorrido;
    private double aptitud;

    //Random
    private System.Random rnd = new System.Random();

    public Cromosoma(List<int> recorrido)
    {
        this.recorrido = recorrido;
    }

    public List<int> Recorrido { get => recorrido; set => recorrido = value; }
    public double Aptitud { get => aptitud; set => aptitud = value; }

    public string mostrarRecorrido()
    {
        string arr = "";
        for (int i = 0; i < recorrido.Count; i++)
        {
            arr += (recorrido[i] + ",");
        }

        arr += " Aptitud: ";
        arr += aptitud;

        return arr;
    }

    public void mutacionXpuntos(int opc)
    {
        if (opc == 0)//Varios intercambios
        {
            Parallel.For(1, Recorrido.Count, (i) =>
            {
                int pos1 = rnd.Next(Recorrido.Count);
                int pos2 = rnd.Next(Recorrido.Count);
                int tmp = Recorrido[pos1];
                Recorrido[pos1] = Recorrido[pos2];
                Recorrido[pos2] = tmp;
            });
        }
        else if (opc == 1)//Un intercambio
        {
            int pos1 = UnityEngine.Random.Range(0, recorrido.Count);
            int pos2 = UnityEngine.Random.Range(0, recorrido.Count);
            int tmp = Recorrido[pos1];
            Recorrido[pos1] = Recorrido[pos2];
            Recorrido[pos2] = tmp;
        }
    }
}
