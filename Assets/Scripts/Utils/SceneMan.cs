using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMan : MonoBehaviour
{

    public void GoToScene(string _nextScene)
    {
        SceneManager.LoadScene(_nextScene);
    }
}
