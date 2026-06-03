using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultButtonSelector : MonoBehaviour
{
    public GameObject defaultButton;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(defaultButton);
    }
}