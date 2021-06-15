using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoCiudad : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;

    public Ciudad Data { get; set; }

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset =
            gameObject.transform.position -
            Camera.main.ScreenToWorldPoint(
                new Vector3(
                    Input.mousePosition.x, Input.mousePosition.y, screenPoint.z
                    )
                );
    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(
            Input.mousePosition.x, Input.mousePosition.y, screenPoint.z
            );
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        curPosition.y = 0.15f;
        transform.position = curPosition;

        Data.Posicion = transform.position;
    }

    void Update()
    {
        transform.position = Data.Posicion;
    }
}
