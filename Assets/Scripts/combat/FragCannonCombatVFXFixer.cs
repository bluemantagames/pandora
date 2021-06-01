using System;
using UnityEngine;
using Pandora;

namespace Pandora.Combat
{
    public class FragCannonCombatVFXFixer : MonoBehaviour, CombatVFXFixer
    {
        public ParticleSystemRenderer FrontShot;
        public ParticleSystemRenderer FrontLight;
        public ParticleSystemRenderer TopShot;
        public ParticleSystemRenderer TopLight;
        public ParticleSystemRenderer BackShot;
        public ParticleSystemRenderer BackLight;
        public ParticleSystemRenderer Trail;
        Quaternion originalLocalRotation;

        void Awake()
        {
            originalLocalRotation = transform.localRotation;
        }

        public void FixVFX(Vector2 source, Vector2 target)
        {
            var direction = (target - source).normalized;

            FixRotation(direction);
            FixOrder(direction);
        }

        void FixRotation(Vector2 direction)
        {
            var rotation = Quaternion.FromToRotation(Vector3.up, direction);

            transform.localRotation = rotation * originalLocalRotation;
        }

        void FixOrder(Vector2 direction)
        {
            FrontShot.sortingOrder = direction.y > 0 ? 10 : 30;
            FrontLight.sortingOrder = direction.y > 1 ? 11 : 31;

            TopShot.sortingOrder = 20;
            TopLight.sortingOrder = 21;

            BackShot.sortingOrder = direction.y < 0 ? 10 : 30;
            BackLight.sortingOrder = direction.y < 1 ? 11 : 31;
        }
    }
}