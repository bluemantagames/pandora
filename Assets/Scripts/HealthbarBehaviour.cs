using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Pandora
{
    public class HealthbarBehaviour : MonoBehaviour
    {
        public Sprite OpponentSprite;
        Sprite originalSprite;
        Image imageComponent;
        LineRenderer lineRenderer;
        public GameObject HPSeparator;
        public int HPSeparatorRange = 20;
        public LifeComponent LifeComponent;
        RectTransform rectTransform;

        void Awake()
        {
            imageComponent = GetComponent<Image>();
            originalSprite = imageComponent.sprite;

            rectTransform = GetComponent<RectTransform>();

            RefreshColor();
        }

        public void RefreshColor()
        {
            var teamComponent = GetComponentInParent<TeamComponent>();

            if (teamComponent?.Team != TeamComponent.assignedTeam)
            {
                imageComponent.sprite = OpponentSprite;
            }
            else
            {
                imageComponent.sprite = originalSprite;
            }
        }

        public void DrawSeparators()
        {
            if (LifeComponent == null)
            {
                Debug.LogError("LifeComponent is invalid, cannot draw separators");

                return;
            }

            var xMax = rectTransform.rect.xMax;
            var width = rectTransform.rect.width;
            var separatorsNum = LifeComponent.maxLife / HPSeparatorRange;
            var separatorsWidth = width / separatorsNum;

            for (var i = 1; i <= separatorsNum; i++)
            {
                var separatorX = separatorsWidth * i;
                var pos = transform.TransformPoint(new Vector2(rectTransform.rect.xMin + separatorX, 0));

                pos.z = -1;

                Instantiate(HPSeparator, pos, Quaternion.identity, transform);
            }
        }
    }
}