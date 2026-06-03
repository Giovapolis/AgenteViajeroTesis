using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControlParametros : MonoBehaviour {

    private string sistemaOperativo;
    private int contadorParms = 0;

    [SerializeField]
    private GAController genetic;

    [SerializeField]
    private TMP_Text txt_generaciones;
    [SerializeField]
    private TMP_Text txt_ciudades;

    [SerializeField]
    private TMP_Text num_generaciones;
    [SerializeField]
    private TMP_Text num_ciudades;

    private int numGeneraciones = 500;
    private int numCiudades = 5;

    void Start() {
        sistemaOperativo = SystemInfo.operatingSystem.ToLower();
    }

    void Update() {
        if (Input.anyKeyDown) {
            if (sistemaOperativo.Contains("windows")) {
                ExecuteWindows();
            }
            if (sistemaOperativo.Contains("android")) {
                ExecuteAndroid();
            }
        }

        if (contadorParms == 0) {
            txt_generaciones.text = $"<uppercase><u>Numero de Generaciones</u></uppercase>";
            txt_ciudades.text = $"<uppercase>Numero de Ciudades</uppercase>";
        }
        else {
            txt_generaciones.text = $"<uppercase>Numero de Generaciones</uppercase>";
            txt_ciudades.text = $"<uppercase><u>Numero de Ciudades</u></uppercase>";
        }

        num_generaciones.text = numGeneraciones.ToString();
        num_ciudades.text = numCiudades.ToString();
    }

    void ExecuteWindows() {

        if (Input.GetKeyDown(KeyCode.JoystickButton0)) {
            genetic.N_generaciones = numGeneraciones;
            genetic.NumberOfCities = numCiudades;
            genetic.iniciarAG();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton1)) {
            genetic.detente();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton3)) {
            genetic.N_generaciones = numGeneraciones;
            genetic.recalcular();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton4)) {
            if (contadorParms == 0)
                contadorParms = 1;
            else
                contadorParms = 0;
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton6)) {
            if (contadorParms == 0 & numGeneraciones > 50) {
                numGeneraciones -= 50;
            }
            if (contadorParms == 1 & numCiudades > 5) {
                numCiudades -= 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton7)) {
            if (contadorParms == 0 & numGeneraciones < 5000) {
                numGeneraciones += 50;
            }
            if (contadorParms == 1 & numCiudades < 50) {
                numCiudades += 1;
            }
        }
    }

    void ExecuteAndroid() {
        if (Input.GetKeyDown(KeyCode.JoystickButton0)) {
            genetic.N_generaciones = numGeneraciones;
            genetic.NumberOfCities = numCiudades;
            genetic.iniciarAG();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton1)) {
            genetic.detente();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton2)) {
            genetic.N_generaciones = numGeneraciones;
            genetic.recalcular();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton3)) {
            if (contadorParms == 0)
                contadorParms = 1;
            else
                contadorParms = 0;
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton4)) {
            if (contadorParms == 0 & numGeneraciones > 50) {
                numGeneraciones -= 50;
            }
            if (contadorParms == 1 & numCiudades > 5) {
                numCiudades -= 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton5)) {
            if (contadorParms == 0 & numGeneraciones < 5000) {
                numGeneraciones += 50;
            }
            if (contadorParms == 1 & numCiudades < 50) {
                numCiudades += 1;
            }
        }
    }
}
