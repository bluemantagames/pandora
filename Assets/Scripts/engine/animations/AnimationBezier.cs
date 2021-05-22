using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Combat;
using Pandora.AI;
using Cysharp.Threading.Tasks;
using System.Globalization;
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
        Dictionary<int, int> loadedAnimation = null;

        /// <summary>
        /// Retrieve the base unit speed.
        /// </summary>
        public int GetUnitSpeed(GameObject targetGameObject)
        {
            var projectileBehaviour = targetGameObject.GetComponent<ProjectileBehaviour>();
            var movementBehaviour = targetGameObject.GetComponent<BasicEntityController>();

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

            if (serializedAnimationsSingleton == null)
                serializedAnimationsSingleton = SerializedAnimationsSingleton.Instance;

            for (var step = 0; step <= AnimationMaxTime; step += AnimationTimeStep)
            {
                var computedSpeed = EvaluateSpeed(speed, step);
                var strComputedSpeed = serializedAnimationsSingleton.FormatNumber(computedSpeed);

                var animationStep = new AnimationStep
                {
                    stepPercentage = step,
                    speed = $"{strComputedSpeed}"
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
            else if (loadedAnimation != null)
            {
                int value;

                loadedAnimation.TryGetValue(AnimationCurrentTime, out value);

                return value;
            }
            else
            {
                return null;
            }
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

        void LoadAnimation()
        {
            var retrievedAnimation = serializedAnimationsSingleton.LoadSingleAnimationFile(AnimationName);
            var decodedAnimation = serializedAnimationsSingleton.GenerateAnimationMap(retrievedAnimation);

            serializedAnimationsSingleton.SetAnimation(AnimationName, decodedAnimation);
        }

        void Awake()
        {
            serializedAnimationsSingleton = SerializedAnimationsSingleton.Instance;
            devUnitSpeed = GetUnitSpeed(gameObject);

            if (!serializedAnimationsSingleton.IsAnimationAlreadyRetrieved(AnimationName) && !DevMode)
            {
                Logger.Debug($"[AnimationCurves] Animation {AnimationName} not cached, retrieving...");
                LoadAnimation();
            }

            // Loading the cached animation
            loadedAnimation = serializedAnimationsSingleton.GetAnimation(AnimationName);
        }
    }
}