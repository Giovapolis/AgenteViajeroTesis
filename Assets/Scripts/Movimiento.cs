using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;

public class Movimiento : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private float speed = 8f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * y;

        controller.Move(move * speed * Time.deltaTime);
    }
}
