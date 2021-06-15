using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlViews : MonoBehaviour
{

    public Transform[] vistas;
    public float velocidad;
    private Transform vistaActual;

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
            vistaActual = vistas[0];
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            vistaActual = vistas[1];
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            vistaActual = vistas[2];
        }
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(
            transform.position, vistaActual.position, Time.deltaTime * velocidad
        );
    }
}
