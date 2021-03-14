using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Combat;
using Pandora.Movement;

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

        /// <summary>
        /// Enable the development mode, in which the animation is
        /// calculated at runtime.
        /// </summary>
        public bool DevMode = false;


        SerializedAnimationsSingleton serializedAnimationsSingleton;
        int passedTicks = 0;
        int devUnitSpeed = 0;

        /// <summary>
        /// Retrieve the base unit speed.
        /// </summary>
        public int GetUnitSpeed(GameObject targetGameObject)
        {
            var projectileBehaviour = targetGameObject.GetComponent<ProjectileBehaviour>();
            var movementBehaviour = targetGameObject.GetComponent<MovementComponent>();

            var speed = projectileBehaviour != null
                ? projectileBehaviour.Speed
                : movementBehaviour != null
                ? movementBehaviour.Speed
                : 0;

            return speed;
        }

        /// <summary>
        /// Evaluate the unit speed based on the provided step.
        /// This function is non-deterministic and unsafe, it should only
        /// be used in the editor or before a serialization.
        /// </summary>
        public float EvaluateSpeed(int unitSpeed, int step)
        {
            var stepDecimal = step / 100f;
            var computedSpeed = unitSpeed * Curve.Evaluate(stepDecimal);

            return computedSpeed;
        }

        /// <summary>
        /// Takes the unit speed as a parameter and return
        /// a list of computed animation steps.
        /// </summary>
        public List<AnimationStep> GetSteps(int speed)
        {
            var steps = new List<AnimationStep>();

            for (var step = 0; step <= AnimationMaxTime; step += AnimationTimeStep)
            {
                var computedSpeed = EvaluateSpeed(speed, step);

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
        public int? GetCurrentAnimatedSpeed()
        {
            if (Disable) return null;

            if (DevMode)
            {
                var computedSpeed = (int)EvaluateSpeed(devUnitSpeed, AnimationCurrentTime);
                var engineSpeed = PandoraEngine.GetSpeed(computedSpeed);

                return engineSpeed;
            }

            var animation = serializedAnimationsSingleton.GetAnimation(AnimationName);

            if (animation == null) return null;

            int value;

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

        private Dictionary<int, int> GenerateAnimationMap(AnimationStepCollection savedAnimation)
        {
            var result = new Dictionary<int, int>();

            foreach (AnimationStep step in savedAnimation.steps)
            {
                var decodedSpeedDecimal = Decimal.Parse(step.speed);
                var decodedSpeed = Decimal.ToInt32(decodedSpeedDecimal);
                var engineSpeed = PandoraEngine.GetSpeed(decodedSpeed);

                result.Add(step.stepPercentage, engineSpeed);
            }

            return result;
        }

        void Awake()
        {
            serializedAnimationsSingleton = SerializedAnimationsSingleton.Instance;

            if (!serializedAnimationsSingleton.IsAnimationAlreadyRetrieved(AnimationName) && !DevMode)
            {
                Logger.Debug($"Animation not cached, retrieving...");

                var retrievedAnimation = GetSavedAnimation();
                var decodedAnimation = GenerateAnimationMap(retrievedAnimation);

                serializedAnimationsSingleton.SetAnimation(AnimationName, decodedAnimation);
            }

            if (DevMode)
            {
                devUnitSpeed = GetUnitSpeed(gameObject);
            }
        }
    }
}