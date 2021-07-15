using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraFade : MonoBehaviour
{
    public float fade = 1;
    public float f = 0;
    private Material material;

    public void FadeIn()
    {
        fade = 1;
        f = 0;
    }

    public void FadeOut()
    {
        fade = 0;
        f = 1;
    }

    private void Awake()
    {
        material = new Material(Shader.Find("Gio/Fade"));
        SceneMan.GetInstance();
    }

    private void Start()
    {
        FadeIn();
    }

    private void Update()
    {
        f = Mathf.Lerp(f, fade, Time.deltaTime * 10);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (f > 0.99f)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat("_fade", Mathf.Clamp01(f));
        Graphics.Blit(source, destination,material);
    }
}
