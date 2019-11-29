using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class CircleDrawer : MonoBehaviour
{


    [Range(0.1f, 100f)]
    public float radius = 1.0f;

    [Range(3, 256)]
    public int numSegments = 3;
    public Color c1 = new Color(0.5f, 0.5f, 0.5f, 1);

    void Awake()
    {
    }

    public void Update()
    {
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();

        lineRenderer.startColor = c1;
        lineRenderer.endColor = c1;
        
        lineRenderer.startWidth = radius;
        lineRenderer.endWidth = radius;

        lineRenderer.positionCount = numSegments + 1;

        lineRenderer.useWorldSpace = false;

        float deltaTheta = (float)(2.0 * Mathf.PI) / numSegments;
        float theta = 0f;

        for (int i = 0; i < numSegments + 1; i++)
        {
            float x = radius * Mathf.Cos(theta);

            float y = radius * Mathf.Sin(theta);

            Vector3 pos = new Vector3(x, y, -0.1f);

            lineRenderer.SetPosition(i, pos);

            theta += deltaTheta;
        }
    }
}