using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pandora
{
    public class HealthbarSeparatorBehaviour : MonoBehaviour
    {
        LineRenderer lineRenderer;
        RectTransform rectTransform;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            rectTransform = GetComponent<RectTransform>();

            RefreshLine();
        }

        public void RefreshLine()
        {/*
            var positions = new List<Vector3> {
                transform.TransformPoint(new Vector2(rectTransform.rect.xMax, rectTransform.rect.yMin)),
                transform.TransformPoint(new Vector2(rectTransform.rect.xMax, rectTransform.rect.yMax))
            }.Select((v) => {
                v.z = -1;

                return v;
            });

            lineRenderer.SetPositions(positions.ToArray());

            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.005f;*/
        }
    }
}