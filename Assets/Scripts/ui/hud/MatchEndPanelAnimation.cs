using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pandora.UI.HUD
{
    public class MatchEndPanelAnimation : MonoBehaviour
    {
        public float FadeInTimeSeconds = 1f;
        public float PanelFinalAlpha = 0.40f, ChildrenFinalAlpha = 1f;
        public GameObject Panel;
        public bool ShouldAnimationPlay = false;
        Image panelImage;
        public GameObject Camera;
        public GameObject[] AnimatedAlphaObjects;

        AnimationCurve panelCurve, childrenCurve;

        // Start is called before the first frame update
        void Start()
        {
            panelImage = Panel.GetComponent<Image>();
        }

        // Update is called once per frame
        void Update()
        {
            if (panelCurve != null && childrenCurve != null)
            {
                var color = new Color(
                    panelImage.color.r,
                    panelImage.color.g,
                    panelImage.color.b,
                    panelCurve.Evaluate(Time.time)
                );

                panelImage.color = color;

                var childrenAlpha = childrenCurve.Evaluate(Time.time);

                foreach (var animatedObject in AnimatedAlphaObjects)
                {
                    var animatedGraphic = animatedObject.GetComponent<Graphic>();

                    if (animatedGraphic != null)
                    {
                        var childColor = new Color(
                            animatedGraphic.color.r,
                            animatedGraphic.color.g,
                            animatedGraphic.color.b,
                            childrenAlpha
                        );

                        animatedGraphic.color = childColor;
                    } else {
                        Debug.LogWarning($"Could not animate {animatedObject}");
                    }
                }

                if (isCurveOver(panelCurve))
                {
                    panelCurve = null;
                    childrenCurve = null;
                }
            }

            if (ShouldAnimationPlay)
            {
                ShouldAnimationPlay = false;

                StartAnimation(TeamComponent.assignedTeam);
            }
        }

        public void StartAnimation(int winnerTeam)
        {
            Panel.SetActive(true);

            panelCurve = AnimationCurve.Linear(Time.time, 0f, Time.time + FadeInTimeSeconds, PanelFinalAlpha);
            childrenCurve = AnimationCurve.Linear(Time.time, 0f, Time.time + FadeInTimeSeconds, ChildrenFinalAlpha);

            Camera.GetComponent<ZoomOutAnimation>().StartMatchEndAnimation();

            _ = GetComponentInChildren<MatchEndStendardsBehaviour>().PlayStendard(TeamComponent.assignedTeam == winnerTeam);
        }

        bool isCurveOver(AnimationCurve curve) =>
            curve.keys[curve.keys.Length - 1].time < Time.time;
    }
}