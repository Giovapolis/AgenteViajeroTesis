using UnityEngine;

public class Ciudad 
{
    private string numCity;
    private Vector3 ubicacion;

    public string NumCity { get => numCity; set => numCity = value; }
    public Vector3 Ubicacion { get => ubicacion; set => ubicacion = value; }

    public override string ToString()
    {
        string ciudad = "Ciudad " + NumCity + " Ubicada en: " + Ubicacion;
        return ciudad;
    }
}
