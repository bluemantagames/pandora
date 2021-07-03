using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Pandora
{
    public class HealthbarBehaviour : MonoBehaviour
    {
        public Sprite OpponentSprite, OpponentEmptyHealthbar;
        Sprite originalSprite, originalEmptyHealthbar;
        Image imageComponent, emptyHealthbarImageComponent;
        LineRenderer lineRenderer;
        public GameObject HPSeparator;
        public GameObject EmptyHealthbar;
        public GameObject HealthbarCanvas;
        public int HPSeparatorRange = 20;
        public LifeComponent LifeComponent;
        RectTransform rectTransform;


        void Start() {
            if (MapComponent.Instance.DisableHealthbars) {
                HealthbarCanvas.GetComponent<Canvas>().enabled = false;

                return;
            }
        }

        void Awake()
        {

            imageComponent = GetComponent<Image>();
            emptyHealthbarImageComponent = EmptyHealthbar.GetComponent<Image>();

            originalSprite = imageComponent.sprite;
            originalEmptyHealthbar = emptyHealthbarImageComponent.sprite;

            rectTransform = GetComponent<RectTransform>();

            RefreshColor();
        }

        public void RefreshColor()
        {
            var teamComponent = GetComponentInParent<TeamComponent>();

            if (teamComponent?.Team != TeamComponent.assignedTeam)
            {
                imageComponent.sprite = OpponentSprite;
                emptyHealthbarImageComponent.sprite = OpponentEmptyHealthbar;
            }
            else
            {
                imageComponent.sprite = originalSprite;
                emptyHealthbarImageComponent.sprite = originalEmptyHealthbar;
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