using UnityEngine;
using TMPro;

public class ControlAGPropio : MonoBehaviour {
    private string os;
    private int contadorParms = 0;

    [SerializeField]
    private AlgoritmoGenetico genetic;

    [SerializeField]
    private TMP_Text txt_generaciones;
    [SerializeField]
    private TMP_Text txt_ciudades;
    [SerializeField]
    private TMP_Text txt_mutacion;
    [SerializeField]
    private TMP_Text txt_individuos;
    [SerializeField]
    private TMP_Text txt_cruza;
    [SerializeField]
    private TMP_Text txt_seleccion;

    [SerializeField]
    private TMP_Text num_generaciones;
    [SerializeField]
    private TMP_Text num_ciudades;
    [SerializeField]
    private TMP_Text porcentaje_mutacion;
    [SerializeField]
    private TMP_Text num_individuos;
    [SerializeField]
    private TMP_Text met_cruza;
    [SerializeField]
    private TMP_Text met_seleccion;

    private int numGeneraciones = 100;
    private int numCiudades = 20;
    private float mutacion = 0.35f;
    private int individuos = 25;
    private int cruza = 0;
    private int seleccion = 0;

    void Start() {
        os = SystemInfo.operatingSystem.ToString().ToLower();
    }

    void Update() {

        if (os.Contains("windows")) {
            ExecuteWindows();
        }

        if (os.Contains("android")) {
            ExecuteAndroid();
        }

        switch (contadorParms) {
            case 0:
                txt_generaciones.text = $"<uppercase><u>Numero de Generaciones</u></uppercase>";
                txt_ciudades.text = $"<uppercase>Numero de Ciudades</uppercase>";
                txt_mutacion.text = $"<uppercase>Porcentaje de Mutacion</uppercase>";
                txt_individuos.text = $"<uppercase>Numero de Individuos</uppercase>";
                txt_cruza.text = $"<uppercase>Metodo de Cruza</uppercase>";
                txt_seleccion.text = $"<uppercase>Metodo de Seleccion</uppercase>";
                break;
            case 1:
                txt_generaciones.text = $"<uppercase>Numero de Generaciones</uppercase>";
                txt_ciudades.text = $"<uppercase><u>Numero de Ciudades</u></uppercase>";
                txt_mutacion.text = $"<uppercase>Porcentaje de Mutacion</uppercase>";
                txt_individuos.text = $"<uppercase>Numero de Individuos</uppercase>";
                txt_cruza.text = $"<uppercase>Metodo de Cruza</uppercase>";
                txt_seleccion.text = $"<uppercase>Metodo de Seleccion</uppercase>";
                break;
            case 2:
                txt_generaciones.text = $"<uppercase>Numero de Generaciones</uppercase>";
                txt_ciudades.text = $"<uppercase>Numero de Ciudades</uppercase>";
                txt_mutacion.text = $"<uppercase><u>Porcentaje de Mutacion</u></uppercase>";
                txt_individuos.text = $"<uppercase>Numero de Individuos</uppercase>";
                txt_cruza.text = $"<uppercase>Metodo de Cruza</uppercase>";
                txt_seleccion.text = $"<uppercase>Metodo de Seleccion</uppercase>";
                break;
            case 3:
                txt_generaciones.text = $"<uppercase>Numero de Generaciones</uppercase>";
                txt_ciudades.text = $"<uppercase>Numero de Ciudades</uppercase>";
                txt_mutacion.text = $"<uppercase>Porcentaje de Mutacion</uppercase>";
                txt_individuos.text = $"<uppercase><u>Numero de Individuos</u></uppercase>";
                txt_cruza.text = $"<uppercase>Metodo de Cruza</uppercase>";
                txt_seleccion.text = $"<uppercase>Metodo de Seleccion</uppercase>";
                break;
            case 4:
                txt_generaciones.text = $"<uppercase>Numero de Generaciones</uppercase>";
                txt_ciudades.text = $"<uppercase>Numero de Ciudades</uppercase>";
                txt_mutacion.text = $"<uppercase>Porcentaje de Mutacion</uppercase>";
                txt_individuos.text = $"<uppercase>Numero de Individuos</uppercase>";
                txt_cruza.text = $"<uppercase><u>Metodo de Cruza</u></uppercase>";
                txt_seleccion.text = $"<uppercase>Metodo de Seleccion</uppercase>";
                break;
            case 5:
                txt_generaciones.text = $"<uppercase>Numero de Generaciones</uppercase>";
                txt_ciudades.text = $"<uppercase>Numero de Ciudades</uppercase>";
                txt_mutacion.text = $"<uppercase>Porcentaje de Mutacion</uppercase>";
                txt_individuos.text = $"<uppercase>Numero de Individuos</uppercase>";
                txt_cruza.text = $"<uppercase>Metodo de Cruza</uppercase>";
                txt_seleccion.text = $"<uppercase><u>Metodo de Seleccion</u></uppercase>";
                break;
        }

        switch (cruza) {
            case 0:
                met_cruza.text = "PMX";
                break;
            case 1:
                met_cruza.text = "OX";
                break;
            case 2:
                met_cruza.text = "CX";
                break;
        }

        switch (seleccion) {
            case 0:
                met_seleccion.text = "Ruleta";
                break;
            case 1:
                met_seleccion.text = "Torneo";
                break;
            case 2:
                met_seleccion.text = "Ranking";
                break;
        }

        num_generaciones.text = numGeneraciones.ToString();
        num_ciudades.text = numCiudades.ToString();
        porcentaje_mutacion.text = (mutacion * 100).ToString() + "%";
        num_individuos.text = individuos.ToString();
    }


    void ExecuteWindows() {

        if (Input.GetKeyDown(KeyCode.JoystickButton0)) {
            genetic.Ciudades = numCiudades;
            genetic.Individuos = individuos;
            genetic.Generaciones = numGeneraciones;
            genetic.TazaMuta = mutacion;  // Ahora en rango 0.0 - 1.0
            genetic.MetSelec = seleccion;
            genetic.MetCruza = cruza;
            genetic.inicia();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton1)) {
            genetic.stop();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton3)) {
            genetic.Ciudades = numCiudades;
            genetic.Individuos = individuos;
            genetic.Generaciones = numGeneraciones;
            genetic.TazaMuta = mutacion;  // Ahora en rango 0.0 - 1.0
            genetic.MetSelec = seleccion;
            genetic.MetCruza = cruza;
            genetic.reStart();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton4)) {
            contadorParms++;
            if (contadorParms > 5) {
                contadorParms = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton6)) {
            switch (contadorParms) {
                case 0:
                    numGeneraciones -= 50;
                    if (numGeneraciones < 50) {
                        numGeneraciones = 50;
                    }
                    break;
                case 1:
                    numCiudades--;
                    if (numCiudades < 5) {
                        numCiudades = 5;
                    }
                    break;
                case 2:
                    mutacion -= 0.05f;
                    if (mutacion < 0.10f) {
                        mutacion = 0.10f;
                    }
                    break;
                case 3:
                    individuos -= 25;
                    if (individuos < 25) {
                        individuos = 25;
                    }
                    break;
                case 4:
                    cruza--;
                    if (cruza < 0) {
                        cruza = 0;
                    }
                    break;
                case 5:
                    seleccion--;
                    if (seleccion < 0) {
                        seleccion = 0;
                    }
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton7)) {
            switch (contadorParms) {
                case 0:
                    if (numGeneraciones < 2000) {
                        numGeneraciones += 50;
                    }
                    break;
                case 1:
                    if (numCiudades < 35) {
                        numCiudades++;
                    }
                    break;
                case 2:
                    mutacion += 0.05f;
                    if (mutacion > 0.45f) {
                        mutacion = 0.45f;
                    }
                    break;
                case 3:
                    individuos += 25;
                    if (individuos > 1000) {
                        individuos = 1000;
                    }
                    break;
                case 4:
                    cruza++;
                    if (cruza > 2) {
                        cruza = 2;
                    }
                    break;
                case 5:
                    seleccion++;
                    if (seleccion > 2) {
                        seleccion = 2;
                    }
                    break;
            }
        }
    }

    void ExecuteAndroid() {
        if (Input.GetKeyDown(KeyCode.JoystickButton0)) {
            genetic.Ciudades = numCiudades;
            genetic.Individuos = individuos;
            genetic.Generaciones = numGeneraciones;
            genetic.TazaMuta = mutacion;  // Ahora en rango 0.0 - 1.0
            genetic.MetSelec = seleccion;
            genetic.MetCruza = cruza;
            genetic.inicia();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton1)) {
            genetic.stop();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton2)) {
            genetic.Ciudades = numCiudades;
            genetic.Individuos = individuos;
            genetic.Generaciones = numGeneraciones;
            genetic.TazaMuta = mutacion;  // Ahora en rango 0.0 - 1.0
            genetic.MetSelec = seleccion;
            genetic.MetCruza = cruza;
            genetic.reStart();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton3)) {
            contadorParms++;
            if (contadorParms > 5) {
                contadorParms = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton4)) {
            switch (contadorParms) {
                case 0:
                    numGeneraciones -= 50;
                    if (numGeneraciones < 50) {
                        numGeneraciones = 50;
                    }
                    break;
                case 1:
                    numCiudades--;
                    if (numCiudades < 5) {
                        numCiudades = 5;
                    }
                    break;
                case 2:
                    mutacion -= 0.05f;
                    if (mutacion < 0.10f) {
                        mutacion = 0.10f;
                    }
                    break;
                case 3:
                    individuos -= 25;
                    if (individuos < 25) {
                        individuos = 25;
                    }
                    break;
                case 4:
                    cruza--;
                    if (cruza < 0) {
                        cruza = 0;
                    }
                    break;
                case 5:
                    seleccion--;
                    if (seleccion < 0) {
                        seleccion = 0;
                    }
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton5)) {
            switch (contadorParms) {
                case 0:
                    if (numGeneraciones < 2000) {
                        numGeneraciones += 50;
                    }
                    break;
                case 1:
                    if (numCiudades < 35) {
                        numCiudades++;
                    }
                    break;
                case 2:
                    mutacion += 0.05f;
                    if (mutacion > 0.45f) {
                        mutacion = 0.45f;
                    }
                    break;
                case 3:
                    individuos += 25;
                    if (individuos > 1000) {
                        individuos = 1000;
                    }
                    break;
                case 4:
                    cruza++;
                    if (cruza > 2) {
                        cruza = 2;
                    }
                    break;
                case 5:
                    seleccion++;
                    if (seleccion > 2) {
                        seleccion = 2;
                    }
                    break;
            }
        }
    }
}
