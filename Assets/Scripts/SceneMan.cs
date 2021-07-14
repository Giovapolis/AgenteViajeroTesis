using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMan : MonoBehaviour
{
    private static SceneMan instance;
    private bool llamarScene = false;

    string next = "";

    public static SceneMan GetInstance()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("SceneManager");
            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<SceneMan>();
        }
        return instance;
    }

    IEnumerator DelayNextScene()
    {
        llamarScene = true;
        GameObject.Find("Main Camera").GetComponent<CamaraFade>().FadeOut();
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(next);
        llamarScene = false;
    }

    public void GoToScene(string _nextScene)
    {
        if (llamarScene){return;}
        next = _nextScene;
        StartCoroutine(DelayNextScene());
    }
}
