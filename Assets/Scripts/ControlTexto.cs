using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlTexto : MonoBehaviour
{
    [SerializeField]
    Transform target;
    [SerializeField]
    Camera camaraPrincipal;

    void Start()
    {
        if (target==null)
        {
            target = transform;
        }

        if (camaraPrincipal == null)
        {
            camaraPrincipal = Camera.main;
        }
    }

    void Update()
    {
        Quaternion panel = camaraPrincipal.transform.rotation.normalized;
        panel.z = 0;
        transform.rotation = panel.normalized;
    }
}
