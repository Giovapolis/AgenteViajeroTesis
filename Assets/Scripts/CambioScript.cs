using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CambioScript : MonoBehaviour
{
    [SerializeField]
    private GameObject ui;

    private BoxCollider boxCollider;
    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (boxCollider.CompareTag("Genetic")) {
            if (other.CompareTag("Player"))
            {
                ui.GetComponent<ControlParametros>().enabled = !(ui.GetComponent<ControlParametros>().isActiveAndEnabled);
            }
        }
        if (boxCollider.CompareTag("Propio")) {
            if (other.CompareTag("Player"))
            {
                ui.GetComponent<ControlAGPropio>().enabled = !(ui.GetComponent<ControlAGPropio>().isActiveAndEnabled);
            }
        }
    }
}
