using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Engine.Animations
{
    public class AnimationBezier : MonoBehaviour
    {
        public AnimationCurve Curve;
        public string AnimationName = "testAnimation";
        public int AnimationTimeStep = 10;
        public int AnimationMaxTime = 100;
        public int AnimationCurrentTime = 0;
        public int AnimationLength = 10;
        public bool Disable = false;

        SerializedAnimationsSingleton serializedAnimationsSingleton;
        int passedTicks = 0;

        public List<AnimationStep> GetSteps(int speed)
        {
            var steps = new List<AnimationStep>();

            for (var step = 0; step <= AnimationMaxTime; step += AnimationTimeStep)
            {
                var stepDecimal = step / 100f;
                var computedSpeed = speed * Curve.Evaluate(stepDecimal);

                var animationStep = new AnimationStep
                {
                    stepPercentage = step,
                    speed = $"{computedSpeed}"
                };

                steps.Add(animationStep);
            }

            return steps;
        }

        public Decimal? GetCurrentAnimatedSpeed()
        {
            if (Disable) return null;

            var animation = serializedAnimationsSingleton.GetAnimation(AnimationName);

            if (animation == null) return null;

            Decimal value;

            animation.TryGetValue(AnimationCurrentTime, out value);

            Logger.Debug($"Retrieved animated speed: {value}");

            return value;
        }

        public void NextStep()
        {
            passedTicks += 1;

            if (passedTicks >= AnimationLength)
            {
                if (AnimationCurrentTime < AnimationMaxTime)
                    AnimationCurrentTime += AnimationTimeStep;

                passedTicks = 0;
            }
        }

        private AnimationStepCollection GetSavedAnimation()
        {
            var projectPath = Application.dataPath;
            var animationPath = $"{projectPath}/GeneratedAnimations/{AnimationName}.json";

            var sr = new StreamReader(animationPath);
            var fileContent = sr.ReadToEnd();
            sr.Close();

            Logger.Debug($"Retrieved saved animation: {fileContent}");

            var parsedAnimation = JsonUtility.FromJson<AnimationStepCollection>(fileContent);

            return parsedAnimation;
        }

        private Dictionary<int, Decimal> GenerateAnimationMap(AnimationStepCollection savedAnimation)
        {
            var result = new Dictionary<int, Decimal>();

            foreach (AnimationStep step in savedAnimation.steps)
            {
                var decodedDecimal = Decimal.Parse(step.speed);
                result.Add(step.stepPercentage, decodedDecimal);
            }

            return result;
        }

        void Awake()
        {
            serializedAnimationsSingleton = SerializedAnimationsSingleton.Instance;

            if (!serializedAnimationsSingleton.IsAnimationAlreadyRetrieved(AnimationName))
            {
                Logger.Debug($"Animation not cached, retrieving...");

                var retrievedAnimation = GetSavedAnimation();
                var decodedAnimation = GenerateAnimationMap(retrievedAnimation);

                serializedAnimationsSingleton.SetAnimation(AnimationName, decodedAnimation);
            }
        }
    }
}