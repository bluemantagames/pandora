using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Animations
{
    /// <summary>
    /// Unity animations seem to have trouble animating world positions for UI elements,
    /// hence this script
    /// </summary>
    public class CustomTransformAnimation : MonoBehaviour
    {
        AnimationCurve xCurve, yCurve;

        public void SetCurves(AnimationCurve xCurve, AnimationCurve yCurve) {
            this.xCurve = xCurve;
            this.yCurve = yCurve;
        }

        // Update is called once per frame
        void Update()
        {
            var position = GetComponent<RectTransform>().position;

            if (xCurve != null) {
                position.x = xCurve.Evaluate(Time.time);

                if (isCurveOver(xCurve)) xCurve = null;
            }

            if (yCurve != null) {
                position.y = yCurve.Evaluate(Time.time);

                if (isCurveOver(yCurve)) yCurve = null;
            }

            GetComponent<RectTransform>().position = position;
        }

        bool isCurveOver(AnimationCurve curve) =>
            Time.time >= curve.keys[curve.length - 1].time;
    }

}