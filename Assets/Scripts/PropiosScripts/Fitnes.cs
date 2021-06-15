using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class Fitnes : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AsignarAptitud(List<Cromosoma> lst,List<Ciudad> lst2)
    {
        Parallel.ForEach(lst, (cromos) =>
        {
            double aptitud = 0;
            Parallel.For(0, lst2.Count, (i) =>
              {
                  if (i == lst2.Count - 1)
                  {
                      aptitud += lst2[(int)cromos.Recorrido[i]].medirDistancia(lst2[(int)cromos.Recorrido[0]]);
                  }
                  else
                  {
                      aptitud += lst2[(int)cromos.Recorrido[i]].medirDistancia(lst2[(int)cromos.Recorrido[i+1]]);
                  }
              });
            cromos.Aptitud = aptitud;
        });
    }

    public Cromosoma elitismo(List<Cromosoma> lst)
    {
        return (from d in lst
                orderby d.Aptitud
                select d).First();
    }

}
