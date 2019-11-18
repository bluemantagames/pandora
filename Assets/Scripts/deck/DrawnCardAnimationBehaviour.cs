using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Pandora.Deck {
    public class DrawnCardAnimationBehaviour: MonoBehaviour {
        Animator animator;
        bool played = false;

        PlayableGraph graph;
        
        void Update() {
            if (played) return;

            var rectTransform = GetComponent<RectTransform>();

            var targetRectTransform = GameObject.Find("FirstCardPosition").GetComponent<RectTransform>();

            animator = GetComponent<Animator>();

            var xCurve = AnimationCurve.EaseInOut(0f, rectTransform.anchoredPosition.x, 1f, targetRectTransform.anchoredPosition.x);
            var yCurve = AnimationCurve.EaseInOut(0f, rectTransform.anchoredPosition.y, 1f, targetRectTransform.anchoredPosition.y);

            var clip = new AnimationClip();

            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", xCurve);
            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.y", yCurve);

            AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), clip, out graph);

            Debug.Log($"Playing from {rectTransform} to {targetRectTransform}");

            played = true;
        }

        void OnDisable () {
            graph.Destroy();
        }

    }
}