using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Seleccionador : MonoBehaviour
{
    public static List<double> valoresEsperados;
    public static double frecuenciaEsperadaTotal;
    public static double r;
    public static System.Random rnd = new System.Random();

    public void init(List<Cromosoma> lst)
    {
        valoresEsperados = new List<double>();
        frecuenciaEsperadaTotal = 0;

        //Obtener frecuenciaEsperadaTotal
        Parallel.ForEach(lst, (cromo) =>
         {
             frecuenciaEsperadaTotal += cromo.Aptitud;
         });

        frecuenciaEsperadaTotal = frecuenciaEsperadaTotal / lst.Count;

        //Obtener Valores Esperados
        for (int i = 0; i < lst.Count; i++)
        {
            valoresEsperados.Add(frecuenciaEsperadaTotal * lst[i].Aptitud);
        }

    }

    public List<Cromosoma> SeleccionRuleta(int nSelecciones,List<Cromosoma> lst)
    {
        List<Cromosoma> mejoresIndividuos = new List<Cromosoma>();

        double tmp = 0.0;
        for (int i = 0; i < nSelecciones; i++)
        {
            r = frecuenciaEsperadaTotal * rnd.NextDouble() * valoresEsperados[i];
            for (int j = 0; j < lst.Count; j++)
            {
                if (tmp >= r)
                {
                    mejoresIndividuos.Add(lst[j]);
                    tmp = 0.0;
                    break;
                }
                else
                {
                    tmp += valoresEsperados[j];
                }
            }
        }

        while(mejoresIndividuos.Count < nSelecciones)
        {
            int idx = rnd.Next(lst.Count);
            mejoresIndividuos.Add(lst[idx]);
        }

        return mejoresIndividuos;
    }
}
