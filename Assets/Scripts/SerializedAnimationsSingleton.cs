using System;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Engine;
using Pandora.Engine.Animations;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Pandora
{
    public class SerializedAnimationsSingleton
    {
        string animationsFileFormat = "json";
        static SerializedAnimationsSingleton _instance = null;
        Dictionary<string, Dictionary<int, int>> serializedAnimations = new Dictionary<string, Dictionary<int, int>>();

        static public SerializedAnimationsSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SerializedAnimationsSingleton();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Retrieve the animation from the cache.
        /// </summary>
        public Dictionary<int, int> GetAnimation(string animationName)
        {
            Dictionary<int, int> animation = null;

            serializedAnimations.TryGetValue(animationName, out animation);

            return animation;
        }

        /// <summary>
        /// Cache an animation.
        /// </summary>
        public void SetAnimation(string animationName, Dictionary<int, int> animationSteps)
        {
            if (!serializedAnimations.ContainsKey(animationName))
                serializedAnimations.Add(animationName, animationSteps);
        }

        /// <summary>
        /// Check if the animation is already retrieved and cached.
        /// </summary>
        public bool IsAnimationAlreadyRetrieved(string animationName)
        {
            return serializedAnimations.ContainsKey(animationName);
        }

        /// <summary>
        /// Retrieve the directory path containing all the animations.
        /// </summary>
        public string GetAnimationsDirectory()
        {
            var animationPath = $"Text/GeneratedAnimations";

            return animationPath;
        }

        /// <summary>
        /// Take the animation name as a parameter and generate
        /// the relative animation file.
        /// </summary>  
        public string GenerateAnimationFileName(string animationName)
        {
            return $"{animationName}.{animationsFileFormat}";
        }

        /// <summary>
        /// Take an animation file name as a parameter and
        /// return the relative animation name.
        /// </summary>
        public string RetrieveAnimationNameFromFile(string animationFile)
        {
            return animationFile.Replace($".{animationsFileFormat}", "");
        }

        /// <summary>
        /// Takes a parsed animation file as a parameter and create
        /// a cached calculated animation.
        /// </summary>
        public Dictionary<int, int> GenerateAnimationMap(AnimationStepCollection savedAnimation)
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

        /// <summary>
        /// Load and parse a single animation file.
        /// </summary>
        public AnimationStepCollection LoadSingleAnimationFile(string animationName)
        {
            var animationsPath = GetAnimationsDirectory();
            var animationPath = $"{animationsPath}/{animationName}";
            var retrievedRawAnimation = Resources.Load<TextAsset>(animationPath).text;

            Logger.Debug($"Retrieved saved animation {animationName}");

            var parsedAnimation = JsonUtility.FromJson<AnimationStepCollection>(retrievedRawAnimation);

            return parsedAnimation;
        }

        /// <summary>
        /// Load all the animations in the specific directory.
        /// </summary>
        public void LoadAllAnimations(string[] animationNames)
        {
            var animationsPath = GetAnimationsDirectory();

            // We probably can load all the animation
            // not waiting the one before
            foreach (string animationName in animationNames)
            {
                Logger.Debug($"Loading {animationName} animation...");

                var parsedFile = LoadSingleAnimationFile(animationName);
                var animationMap = GenerateAnimationMap(parsedFile);

                SetAnimation(animationName, animationMap);
            }
        }
    }
}