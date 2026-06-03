using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlViews : MonoBehaviour
{

    public Transform[] vistas;
    public float velocidad;
    private Transform vistaActual;
    private int contador = 0;

    // Start is called before the first frame update
    void Start()
    {
        vistaActual = transform;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (contador == vistas.Length - 1)
            {
                contador = 0;
            }
            else
            {
                contador++;
            }
            vistaActual = vistas[contador];
        }
        
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(
            transform.position, vistaActual.position, Time.deltaTime * velocidad
        );
    }
}
