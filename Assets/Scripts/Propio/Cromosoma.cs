using System;
using System.Collections.Generic;

public class Cromosoma
{
    private Random random = new Random();
    private List<int> recorrido = new List<int>();
    private float aptitud = 0.0f;

    public List<int> Recorrido { get => recorrido; set => recorrido = value; }
    public float Aptitud { get => aptitud; set => aptitud = value; }

    public override string ToString()
    {
        string features = "";


        features += "Recorrido: ";

        for (int i = 0; i < recorrido.Count; i++)
        {
            features += recorrido[i].ToString() + ",";
        }
        features += "\n";

        features += "Aptitud: " + aptitud + "\n";
        return features;
    }

    public void MutarPorInsercion()
    {
        int index1 = random.Next(0, Recorrido.Count);
        int index2 = random.Next(0, Recorrido.Count);

        while (index1 == index2) // Asegurarse de que los índices sean diferentes
        {
            index2 = random.Next(0, Recorrido.Count);
        }

        int element = Recorrido[index1];

        if (index1 < index2)
        {
            Recorrido.RemoveAt(index1);
            Recorrido.Insert(index2 - 1, element);
        }
        else
        {
            Recorrido.RemoveAt(index1);
            Recorrido.Insert(index2, element);
        }
    }

    public void MutarPorIntercambio()
    {
        int indice1 = random.Next(0, Recorrido.Count);
        int indice2 = random.Next(0, Recorrido.Count);

        while (indice1 == indice2) // asegurarse de que los índices sean diferentes
        {
            indice2 = random.Next(0, Recorrido.Count);
        }

        // Intercambiar los valores de los nodos en los índices seleccionados
        int temp = Recorrido[indice1];
        Recorrido[indice1] = Recorrido[indice2];
        Recorrido[indice2] = temp;
    }

    public void MutarPorInversion()
    {
        int puntoInicio = random.Next(0, Recorrido.Count);
        int puntoFin = random.Next(puntoInicio, Recorrido.Count);

        // Invertir los genes en el subconjunto seleccionado
        Recorrido.Reverse(puntoInicio, puntoFin - puntoInicio + 1);
    }
}
