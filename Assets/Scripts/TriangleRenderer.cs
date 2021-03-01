using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

public class TriangleRenderer : MonoBehaviour
{
    public Material TriangleMaterial;

    /// <summary>
    /// Procedurally generates the triangle mesh. Positions are then reordered clockwise.
    /// This is important because counter-clockwise vertices are rendered facing away
    /// from the camera
    /// </summary>
    public void Initialize(Vector3 worldV1, Vector3 worldV2, Vector3 worldV3)
    {
        var mesh = gameObject.AddComponent<MeshFilter>().mesh;
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();

        mesh.Clear();

        var vertices = new Vector3[] { transform.InverseTransformPoint(worldV1), transform.InverseTransformPoint(worldV2), transform.InverseTransformPoint(worldV3) };

        float? minX = null, maxX = null;
        Vector3 v1 = Vector3.zero, v2 = Vector3.zero, v3 = Vector3.zero;

        // reorder them clockwise
        foreach (var vertex in vertices) {
            if (!minX.HasValue || minX.Value > vertex.x) {
                minX = vertex.x;

                v1 = vertex;
            }

            if (!maxX.HasValue || maxX.Value < vertex.x) {
                maxX = vertex.x;

                v3 = vertex;
            }
        }

        foreach (var vertex in vertices) {
            if (v1 != vertex && v3 != vertex) {
                v2 = vertex;

                continue;
            }
        }

        // swap v2 and v3 if v2 is under v1
        if (v2.y < v1.y) {
            var oldV3 = v3;
            v3 = v2;
            v2 = oldV3;
        }

        mesh.vertices = new Vector3[] { v1, v2, v3 };
        mesh.triangles = new int[] { 0, 1, 2 };

        meshRenderer.material = TriangleMaterial;

        var position = transform.position;

        position.z = -2;

        transform.position = position;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
