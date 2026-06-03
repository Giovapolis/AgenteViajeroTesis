using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamepadButtonAClick : MonoBehaviour
{
    public string buttonA = "Fire1"; // Botón mapeado para "A" (configurado en Input Manager)
    
    void Update()
    {
        if (Input.GetButtonDown(buttonA))
        {
            SimulateButtonClick();
        }
    }

    private void SimulateButtonClick()
    {
        // Obtener el botón seleccionado actualmente en el EventSystem
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject != null)
        {
            Button button = selectedObject.GetComponent<Button>();
            if (button != null)
            {
                // Simular clic
                button.onClick.Invoke();
                Debug.Log("Botón pulsado: " + selectedObject.name);
            }
            else
            {
                // Intentar con otros componentes interactivos (e.g., Toggle)
                ExecuteEvents.Execute(selectedObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            }
        }
    }
}