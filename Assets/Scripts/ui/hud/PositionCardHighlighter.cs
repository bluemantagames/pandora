using UnityEngine;
using UnityEngine.Animations;
using Pandora;

namespace Pandora.UI.HUD {
    public class PositionCardHighlighter: MonoBehaviour {
        Vector2 originalPosition;
        RectTransform rectTransform;
        float highlightAnimationTime = 0.15f;
        AnimationCurve currentCurve = null;
        bool destroyOnEnd = false;

        void Start() {
            rectTransform = GetComponent<RectTransform>();

            originalPosition = transform.localPosition;

            var targetPosition = transform.localPosition.y + rectTransform.rect.height * 0.25f;

            currentCurve = AnimationCurve.Linear(Time.time, originalPosition.y, Time.time + highlightAnimationTime, targetPosition);
        }

        public void Unhighlight() {
            currentCurve = AnimationCurve.Linear(Time.time, transform.localPosition.y, Time.time + highlightAnimationTime, originalPosition.y);

            destroyOnEnd = true;
        }

        void Update() {
            if (currentCurve != null) {
                var position = new Vector2(transform.localPosition.x, currentCurve.Evaluate(Time.time));

                transform.localPosition = position;

                if (Time.time >= currentCurve.keys[currentCurve.keys.Length - 1].time) {
                    currentCurve = null;

                    if (destroyOnEnd)
                        Destroy(this);
                }
            }
        }
    }
}