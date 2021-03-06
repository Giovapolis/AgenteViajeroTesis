using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class CityController : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;

    public TspCity Data { get; set; }

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z)
            );
    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        curPosition.y = 0.15f;
        transform.position = curPosition;

        Data.Position = transform.position;
    }

    void Update()
    {
        transform.position = Data.Position;
    }
}