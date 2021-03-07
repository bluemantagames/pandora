using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Engine.Animations
{
    public class AnimationBezier : MonoBehaviour
    {
        public AnimationCurve Curve;
        public float SerializationTimeStep = 0.1f;
        public float SerializationMaxTime = 1f;

        public List<AnimationStep> GetSteps(int speed)
        {
            var steps = new List<AnimationStep>();

            for (var step = SerializationTimeStep; step <= SerializationMaxTime; step += SerializationTimeStep)
            {
                var stepPercentage = step * 100;
                var computedSpeed = speed * Curve.Evaluate(step);

                var animationStep = new AnimationStep
                {
                    stepPercentage = (int)stepPercentage,
                    speed = $"{computedSpeed}"
                };

                steps.Add(animationStep);
            }

            return steps;
        }
    }
}