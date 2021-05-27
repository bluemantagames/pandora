using System;
using UnityEngine;
using Pandora;

namespace Pandora.Combat
{
    public class FragCannonCombatVFXFixer : MonoBehaviour, CombatVFXFixer
    {
        public ParticleSystem FrontShot;
        public ParticleSystem TopShot;
        public ParticleSystem BackShot;
        public ParticleSystem Trail;
        ParticleSystem.ShapeModule originalFrontShotShape;
        ParticleSystem.ShapeModule originalTopShotShape;
        ParticleSystem.ShapeModule originalBackShotShape;
        ParticleSystem.ShapeModule originalTrailShape;
        Vector3 originalFrontShotRotation;
        Vector3 originalTopShotRotation;
        Vector3 originalBackShotRotation;
        Vector3 originalTrailRotation;
        Quaternion originalLocalRotation;

        void Awake()
        {
            originalFrontShotShape = FrontShot.shape;
            originalTopShotShape = TopShot.shape;
            originalBackShotShape = BackShot.shape;
            originalTrailShape = Trail.shape;

            originalFrontShotRotation = FrontShot.shape.rotation;
            originalTopShotRotation = TopShot.shape.rotation;
            originalBackShotRotation = BackShot.shape.rotation;
            originalTrailRotation = Trail.shape.rotation;
            originalLocalRotation = transform.localRotation;
        }

        public void FixVFX(Vector2 source, Vector2 target)
        {
            var direction = (target - source).normalized;
            var rotation = Quaternion.FromToRotation(Vector3.up, direction);

            transform.localRotation = rotation * originalLocalRotation;
        }
    }
}