using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pandora
{
    public class SerializedAnimationsSingleton
    {
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

        public Dictionary<int, int> GetAnimation(string animationName)
        {
            Dictionary<int, int> animation = null;

            serializedAnimations.TryGetValue(animationName, out animation);

            return animation;
        }

        public void SetAnimation(string animationName, Dictionary<int, int> animationSteps)
        {
            serializedAnimations.Add(animationName, animationSteps);
        }

        public bool IsAnimationAlreadyRetrieved(string animationName)
        {
            return serializedAnimations.ContainsKey(animationName);
        }
    }
}