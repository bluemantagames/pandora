using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

public class TriangleRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var mapComponent = MapComponent.Instance;
        var collider = mapComponent.GetComponent<BoxCollider2D>();

        var mesh = gameObject.AddComponent<MeshFilter>().mesh;
        gameObject.AddComponent<MeshRenderer>();

        var right = (Vector2) collider.transform.position + collider.size;
        right.y = collider.transform.position.y;

        mesh.Clear();
        mesh.vertices = new Vector3[] { transform.InverseTransformPoint(collider.transform.position), transform.InverseTransformPoint(collider.transform.position + (Vector3) collider.size), transform.InverseTransformPoint(right) };
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.colors = new Color[] { Color.yellow, Color.yellow, Color.yellow };
    }

    // Update is called once per frame
    void Update()
    {

    }
}
