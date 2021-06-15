using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ciudad : MonoBehaviour
{
    private Vector3 posicion;
    private string numero;

    public Ciudad()
    {
        posicion = crearPosicionAleatoria();
    }

    private Vector3 crearPosicionAleatoria()
    {
        return new Vector3(
            UnityEngine.Random.Range(-11, 11),
            0.15f, 
            UnityEngine.Random.Range(-11, 11));
    }

    public string Numero { get => numero; set => numero = value; }
    public Vector3 Posicion { get => posicion; set => posicion = value; }

    public double medirDistancia(Ciudad c2) {
        return Vector3.Distance(Posicion, c2.Posicion);
    }
}
