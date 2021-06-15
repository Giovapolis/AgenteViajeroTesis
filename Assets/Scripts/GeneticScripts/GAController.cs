using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GAController : MonoBehaviour
{
    private GeneticAlgorithm m_ga, m_ga_aux;
    private Thread m_gaThread, m_gaThread_aux;
    public Slider generaciones;
    public Slider individuos;
    public Text recorrido;
    public Text txt_generaciones;
    public Text txt_ciudades;

    public GeneticAlgorithm getGA()
    {
        return m_ga;
    }

    public GameObject CityPrefab;
    private int n_generaciones;
    private int m_numberOfCities;
    private LineRenderer m_lr;

    private void AGStart()
    {
        n_generaciones = (int)generaciones.value;
        m_numberOfCities = (int)individuos.value;
        m_lr = GetComponent<LineRenderer>();
        m_lr.positionCount = m_numberOfCities + 1;
        m_lr.material = new Material(Shader.Find("Sprites/Default"));

        algoritmoGenetic();

        DrawCities();
    }

    //ALGORITMO GENETIC
    void algoritmoGenetic()
    {
        var fitness = new TspFitness(m_numberOfCities);
        var chromosome = new TspChromosome(m_numberOfCities);
        var crossover = new OrderedCrossover();
        var mutation = new ReverseSequenceMutation();
        var selection = new TournamentSelection();
        var population = new Population(50, 100, chromosome);
        m_ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
        m_ga.Termination = new GenerationNumberTermination(n_generaciones);

        m_ga.TaskExecutor = new ParallelTaskExecutor
        {
            MinThreads = 1,
            MaxThreads = 2
        };

        m_ga.GenerationRan += delegate
        {
            var distance = ((TspChromosome)m_ga.BestChromosome).Distance;
        };
    }

    public void iniciarAG()
    {
        DestruirCiudades();
        AGStart();
        m_gaThread = new Thread(() => m_ga.Start());
        m_gaThread.Start();
    }

    public void recalcular()
    {
        m_ga.Stop();
        m_gaThread.Abort();
        m_ga_aux = new GeneticAlgorithm(m_ga.Population, m_ga.Fitness, m_ga.Selection, m_ga.Crossover, m_ga.Mutation);
        m_ga_aux.Termination = new GenerationNumberTermination(n_generaciones);
        m_ga = m_ga_aux;
        m_gaThread = new Thread(() => m_ga.Start());
        m_gaThread.Start();
    }

    void Update()
    {
        DrawRouteGenetic();
        txt_generaciones.text = ((int)generaciones.value).ToString();
        txt_ciudades.text = ((int)individuos.value).ToString();
    }

    public void detente()
    {
        m_ga.Stop();
        m_gaThread.Abort();
    }

    void DrawCities()
    {
        var cities = ((TspFitness)m_ga.Fitness).Cities;

        for (int i = 0; i < m_numberOfCities; i++)
        {
            var city = cities[i];
            var go = Instantiate(CityPrefab, city.Position, Quaternion.identity) as GameObject;
            go.name = "City " + i;
            go.GetComponent<CityController>().Data = city;
            go.GetComponentInChildren<Text>().text = i.ToString();
        }

    }

    void DestruirCiudades()
    {
        for (int i = 0; i < m_numberOfCities; i++)
        {
            Destroy(GameObject.Find("City " + i), .01f);
        }
    }

    void dibujaCromosoma(TspChromosome c)
    {
        if (m_ga != null && c != null)
        {
            GeneticSharp.Domain.Chromosomes.Gene[] genes = c.GetGenes();
            string gen = "";
            for (int i = 0; i < genes.Length; i++)
            {
                if (i == genes.Length - 1)
                {
                    gen += genes[i];
                }
                else
                {
                    gen += genes[i] + "->";
                }
            }
            var distance = ((TspChromosome)m_ga.BestChromosome).Distance;
            gen += " : " + Math.Round(distance, 2);
            //print(gen);
            recorrido.text = gen;
        }
    }

    void DrawRouteGenetic()
    {
        TspChromosome c = null;
        if (m_ga != null)
        {
            try
            {
                c = m_ga.Population.CurrentGeneration.BestChromosome as TspChromosome;
            }
            catch (Exception)
            {
                Debug.Log("Error ");
            }
        }
        else
        {
            c = null;
        }
        if (c != null)
        {
            dibujaCromosoma(c);
            var genes = c.GetGenes();
            var cities = ((TspFitness)m_ga.Fitness).Cities;

            for (int i = 0; i < genes.Length; i++)
            {
                var city = cities[(int)genes[i].Value];
                Vector3 linposition = city.Position;
                linposition.y = 0.25f;
                m_lr.SetPosition(i, linposition);
            }

            var firstCity = cities[(int)genes[0].Value];
            Vector3 linpositionfinal = firstCity.Position;
            linpositionfinal.y = 0.5f;
            m_lr.SetPosition(m_numberOfCities, linpositionfinal);
        }
    }

}