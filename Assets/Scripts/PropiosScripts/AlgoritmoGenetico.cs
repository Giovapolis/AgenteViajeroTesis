using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;
using System;

public class AlgoritmoGenetico : MonoBehaviour
{
    //UI
    public Slider slider_generaciones;
    public Slider slider_ciudades;
    public Text text_generaciones;
    public Text text_ciudades;
    public Slider slider_poblacion;
    public Text text_poblacion;
    public Text text_recorrido;

    //Parametros Modificables
    private uint cantidadDeCiudades;
    private uint cantidadDeCromosomas;

    //Elementos a Dibujar
    [SerializeField]
    private GameObject prefabCiudad;
    private LineRenderer lineaRecorrido;

    //Elementos de AG
    private List<Ciudad> ciudades = new List<Ciudad>();
    private List<Cromosoma> poblacion = new List<Cromosoma>();
    private Fitnes medidor = new Fitnes();
    private Seleccionador select = new Seleccionador();

    //Parametros AG
    private float tasaDeMutacion;
    private bool conElitismo = true;

    //Seleccionadores de ...
    private uint tipoMutacion;
    private uint tipoCruce;
    private uint tipoSeleccion;

    //Gets and Sets
    public List<Ciudad> Ciudades { get => ciudades; set => ciudades = value; }
    public List<Cromosoma> Poblacion { get => poblacion; set => poblacion = value; }

    // Start is called before the first frame update
    void Start()
    {
        lineaRecorrido = GetComponent<LineRenderer>();
        lineaRecorrido.material = new Material(Shader.Find("Sprites/Default"));
    }

    // Update is called once per frame
    void Update()
    {
        text_ciudades.text = slider_ciudades.value.ToString();
        text_generaciones.text = slider_generaciones.value.ToString();
        text_poblacion.text = slider_poblacion.value.ToString();
    }

    private void generadorDeCiudades()
    {
        for (int i = 0; i < cantidadDeCiudades; i++)
        {
            Ciudad tmp = new Ciudad();
            tmp.Numero = i.ToString();
            Ciudades.Add(tmp);
        }
    }

    private void colocadorDeCiudades()
    {
        for (int i = 0; i < Ciudades.Count; i++)
        {
            GameObject a = Instantiate(prefabCiudad, Ciudades[i].Posicion, Quaternion.identity);
            a.name = "City " + i;
            a.GetComponent<MovimientoCiudad>().Data = Ciudades[i];
            a.GetComponentInChildren<Text>().text = i.ToString();
        }
    }

    private void generadorCromosoma()
    {

        for (int i = 0; i < cantidadDeCromosomas; i++)
        {
            List<int> numeros = new List<int>();

            for (int j = 0; j < cantidadDeCiudades; j++)
            {
                int num = (int)UnityEngine.Random.Range(0, cantidadDeCiudades);
                do
                {
                    num = (int)UnityEngine.Random.Range(0, cantidadDeCiudades);
                } while (numeros.Contains(num));
                numeros.Add(num);
            }

            Poblacion.Add(new Cromosoma(numeros));
        }
    }

    void DestruirCiudades()
    {
        poblacion.Clear();
        ciudades.Clear();
        for (int i = 0; i < slider_ciudades.value; i++)
        {
            Destroy(GameObject.Find("City " + i), .01f);
        }
    }

    public void iniciarAG()
    {
        //Numero de vertices de la linea
        lineaRecorrido.positionCount = (int)slider_ciudades.value + 1;
        DestruirCiudades();
        //Iniciar conteo de tiempo de ejecucion
        var watch = System.Diagnostics.Stopwatch.StartNew();
        DestruirCiudades();

        cantidadDeCiudades = (uint)slider_ciudades.value;
        cantidadDeCromosomas = (uint)slider_poblacion.value;

        generadorDeCiudades();
        generadorCromosoma();
        medidor.AsignarAptitud(Poblacion, Ciudades);
        colocadorDeCiudades();

        select.init(Poblacion);

        List<Cromosoma> nuevaPoblacion;

        //Parallel.For(0, (int)slider_generaciones.value, (i) =>
        //Parallel.For(0, 1, (i) =>
        for (int i = 0; i < 1; i++)
        {
            //Seleccion
            nuevaPoblacion = select.SeleccionRuleta(Poblacion.Count - 1, Poblacion);
            nuevaPoblacion.Add(medidor.elitismo(Poblacion));
            //Cruza
            Debug.Log(poblacion[0].mostrarRecorrido());
            Debug.Log(poblacion[1].mostrarRecorrido());
            List<Cromosoma> hijos = Cruza(poblacion[0], poblacion[1]);
            Debug.Log(hijos[0].mostrarRecorrido());
            Debug.Log(hijos[1].mostrarRecorrido());
            //Mutacion
            mutar(nuevaPoblacion);
            //Aptitudes de Nueva Poblacion
            medidor.AsignarAptitud(nuevaPoblacion, Ciudades);
            //Crear nueva Poblacion
            Poblacion = nuevaPoblacion;
        };

        text_recorrido.text = medidor.elitismo(Poblacion).mostrarRecorrido();

        //Terminar y ver tiempo de ejecucion
        watch.Stop();
        //Debug.Log(watch.ElapsedMilliseconds);
    }

    private void mutar(List<Cromosoma> lista)
    {
        Parallel.ForEach(lista, (cromo) =>
        {
            cromo.mutacionXpuntos(0);
        });
    }

    public void reCalcular()
    {

    }

    //Cruza por orden (OX)
    public List<Cromosoma> Cruza(Cromosoma padre, Cromosoma madre)
    {
        List<Cromosoma> hijos = new List<Cromosoma>();

        int[] r1 = new int[padre.Recorrido.Count];
        int[] r2 = new int[padre.Recorrido.Count];

        int p1, p2;

        do
        {
            p1 = UnityEngine.Random.Range(1, padre.Recorrido.Count);
            p2 = UnityEngine.Random.Range(1, padre.Recorrido.Count);
        } while ((p1 == p2) || (p1 == 0 && p2 == padre.Recorrido.Count) || (p1 > p2));

        for (int i = p1; i < p2; i++)
        {
            r1[i] = madre.Recorrido[i];
            r2[i] = padre.Recorrido[i];
        }

        recorre(r1, padre, madre, p1, p2);
        recorre(r2, madre, padre, p1, p2);

        hijos.Add(new Cromosoma(r1.OfType<int>().ToList()));
        hijos.Add(new Cromosoma(r2.OfType<int>().ToList()));

        return hijos;
    }

    void recorre(int[] r,Cromosoma p,Cromosoma m,int p1,int p2)
    {
        for (int i = 0; i < p1; i++)
        {
            if (r.Contains(p.Recorrido[i]))
            {
                r[i] = m.Recorrido[i];
            }
            else
            {
                r[i] = p.Recorrido[i];
            }
        }

        for (int i = p2; i < p.Recorrido.Count; i++)
        {
            if (r.Contains(p.Recorrido[i]))
            {
                r[i] = m.Recorrido[i];
            }
            else
            {
                r[i] = p.Recorrido[i];
            }
        }
    }

}
