using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class ContolInterfazGenetic : MonoBehaviour
{
    private int seleccion = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update(){
        
        if (Input.anyKeyDown){
            switch(GetPressedKey()){
                case KeyCode.JoystickButton0:
                    seleccion = 0;
                    break;
                case KeyCode.JoystickButton1:
                    seleccion = 1;
                    break;
                case KeyCode.JoystickButton2:
                    seleccion = 2;
                    break;
                case KeyCode.JoystickButton3:
                    seleccion = 3;
                    break;
                case KeyCode.JoystickButton4:
                    Debug.Log("Reduce");
                    break;
                case KeyCode.JoystickButton5:
                    Debug.Log("Aumenta");
                    break;
                case KeyCode.JoystickButton6:
                    Debug.Log("Detener");
                    break;
                case KeyCode.JoystickButton7:
                    Debug.Log("Iniciar");
                    break;
            }
        }    
    }

    // Método para obtener la tecla presionada
    private KeyCode GetPressedKey()
    {
        // Iterar por todas las teclas posibles
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                return keyCode;
            }
        }
        return KeyCode.None;
    }
}
