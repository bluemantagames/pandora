using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

public class TriangleRenderer : MonoBehaviour
{
    public Material TriangleMaterial;

    // Start is called before the first frame update
    public void Initialize(Vector3 worldV1, Vector3 worldV2, Vector3 worldV3)
    {
        var mesh = gameObject.AddComponent<MeshFilter>().mesh;
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();

        mesh.Clear();
        mesh.vertices = new Vector3[] { transform.InverseTransformPoint(worldV1), transform.InverseTransformPoint(worldV2), transform.InverseTransformPoint(worldV3) };
        mesh.triangles = new int[] { 0, 1, 2 };

        meshRenderer.material = TriangleMaterial;

        var position = transform.position;

        position.z = -1;

        transform.position = position;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
