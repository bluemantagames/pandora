﻿using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Engine.Animations
{
    public class AnimationBezier : MonoBehaviour
    {
        public AnimationCurve Curve;

        /// <summary>
        /// The animation name (the serialized file will have this name too).
        /// </summary>
        public string AnimationName = "testAnimation";

        /// <summary>
        /// The animation step, this defines the animation's granularity.
        /// Every engine tick the animation time will be increased by this value.
        /// </summary>
        public int AnimationTimeStep = 10;

        /// <summary>
        /// The animation max time is the last step value.
        /// When the animation time will reach this value the animation
        /// is considered complete.
        /// </summary>
        public int AnimationMaxTime = 100;

        /// <summary>
        /// The inizial animation time value.
        /// </summary>
        public int AnimationCurrentTime = 0;

        /// <summary>
        /// How many engine ticks are needed the the animation
        /// time increment.
        /// </summary>
        public int TicksForAnimationStep = 1;

        /// <summary>
        /// Defines if the speed has to restart when the animation
        /// is complete.
        /// </summary>
        public bool Loop = false;

        /// <summary>
        /// Disable the animation and fallback to the regular
        /// engine movement.
        /// </summary>
        public bool Disable = false;

        SerializedAnimationsSingleton serializedAnimationsSingleton;
        int passedTicks = 0;

        /// <summary>
        /// Takes the unit speed as a parameter and return
        /// a list of computed animation steps.
        /// </summary>
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

        /// <summary>
        /// Retrieve the current animated speed based on the
        /// internal animation status.
        /// </summary>
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

        /// <summary>
        /// Update the animation status to the next step.
        /// </summary>
        public void NextStep()
        {
            passedTicks += 1;

            if (passedTicks >= TicksForAnimationStep)
            {
                if (Loop && AnimationCurrentTime >= AnimationMaxTime)
                    AnimationCurrentTime = 0;
                else if (AnimationCurrentTime < AnimationMaxTime)
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