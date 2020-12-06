using UnityEngine;
using UnityEngine.Animations;
using Pandora;

namespace Pandora.UI.HUD {
    public class CardHighlighter: MonoBehaviour {
        Vector2 originalPosition;
        RectTransform rectTransform;
        float highlightAnimationTime = 0.15f;

        void Start() {
            rectTransform = GetComponent<RectTransform>();

            originalPosition = rectTransform.anchoredPosition;

            var targetPosition = originalPosition.y + rectTransform.rect.height * 0.15f;

            var curve = AnimationCurve.Linear(0f, originalPosition.y, highlightAnimationTime, targetPosition);

            var clip = new AnimationClip();

            clip.legacy = true;

            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.y", curve);

            var animation = GetComponent<Animation>();

            animation.AddClip(clip, clip.name);
            animation.Play(clip.name);
        }

        public void Unhighlight() {
            var curve = AnimationCurve.Linear(0f, rectTransform.anchoredPosition.y, highlightAnimationTime, originalPosition.y);

            var clip = new AnimationClip();

            clip.legacy = true;

            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.y", curve);

            var animation = GetComponent<Animation>();

            animation.AddClip(clip, clip.name);
            animation.Play(clip.name);

            Destroy(this);
        }
    }
}