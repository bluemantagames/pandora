using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

namespace Pandora.Combat
{
    public class FragCannonCombatVFXFixer : MonoBehaviour, CombatVFXFixer
    {
        public ParticleSystemRenderer SteamFront;
        public ParticleSystemRenderer LightFront;
        public ParticleSystemRenderer SteamBack;
        public ParticleSystemRenderer LightBack;
        public ParticleSystemRenderer SteamTop;
        public ParticleSystemRenderer LightTop;
        public ParticleSystemRenderer Trails;

        public int FixDirectionTopLeft = 225;
        public int FixDirectionTopRight = 135;
        public int FixDirectionCenterLeft = 270;
        public int FixDirectionCenterRight = 90;
        public int FixDirectionBottomLeft = 315;
        public int FixDirectionBottomCenter = 180;
        public int FixDirectionBottomRight = 35;
        public float HorizontalPositionFix = 0.5f;
        public float VerticalPositionFix = 0.5f;

        Vector3 originalLocalPosition;
        Vector3 steamFrontLocalPosition;
        Vector3 lightFrontLocalPosition;
        Vector3 steamBackLocalPosition;
        Vector3 lightBackLocalPosition;
        Vector3 trailsLocalPosition;

        void Awake()
        {
            originalLocalPosition = transform.localPosition;
            steamFrontLocalPosition = SteamFront.gameObject.transform.localPosition;
            lightFrontLocalPosition = LightFront.gameObject.transform.localPosition;
            steamBackLocalPosition = SteamBack.gameObject.transform.localPosition;
            lightBackLocalPosition = LightBack.gameObject.transform.localPosition;
            trailsLocalPosition = Trails.gameObject.transform.localPosition;
        }

        public void FixVFX(Vector2Int enemyDirection)
        {
            FixShotRotation(enemyDirection);
            FixShotPosition(enemyDirection);
            FixLevels(enemyDirection);
            FixRotatedVerticalPosition(enemyDirection);
        }

        void FixShotRotation(Vector2Int enemyDirection)
        {
            int yRotation = 0;
            int xRotation = 0;

            if (enemyDirection.x == 1 && enemyDirection.y == -1)
                yRotation = FixDirectionBottomRight;
            else if (enemyDirection.x == 1 && enemyDirection.y == 0)
                yRotation = FixDirectionCenterRight;
            else if (enemyDirection.x == 1 && enemyDirection.y == 1)
                yRotation = FixDirectionTopRight;
            else if (enemyDirection.x == 0 && enemyDirection.y == -1)
                yRotation = FixDirectionBottomCenter;
            else if (enemyDirection.x == -1 && enemyDirection.y == 1)
                yRotation = FixDirectionTopLeft;
            else if (enemyDirection.x == -1 && enemyDirection.y == 0)
                yRotation = FixDirectionCenterLeft;
            else if (enemyDirection.x == -1 && enemyDirection.y == -1)
                yRotation = FixDirectionBottomLeft;

            if (enemyDirection.y > 0)
                xRotation = -70;

            var newRotation = Quaternion.Euler(xRotation, yRotation, 0);

            transform.localRotation = newRotation;
        }

        void FixShotPosition(Vector2Int enemyDirection)
        {
            var fixedPosition = originalLocalPosition;

            if (enemyDirection.x > 0)
                fixedPosition.x += HorizontalPositionFix;
            else if (enemyDirection.x < -0)
                fixedPosition.x -= HorizontalPositionFix;

            transform.localPosition = fixedPosition;
        }

        void FixRotatedVerticalPosition(Vector2 enemyDirection)
        {
            var fixedSteamBack = steamBackLocalPosition;
            var fixedLightBack = lightBackLocalPosition;
            var fixedSteamFront = steamFrontLocalPosition;
            var fixedLightFront = lightFrontLocalPosition;
            var fixedTrails = trailsLocalPosition;

            if (enemyDirection.y < 0)
            {
                fixedSteamBack.y += VerticalPositionFix / 2;
                fixedLightBack.y += VerticalPositionFix / 2;
                fixedSteamFront.y -= VerticalPositionFix;
                fixedLightFront.y -= VerticalPositionFix;
                fixedTrails.y -= VerticalPositionFix;
            }

            SteamFront.gameObject.transform.localPosition = fixedSteamFront;
            LightFront.gameObject.transform.localPosition = fixedLightFront;
            SteamBack.gameObject.transform.localPosition = fixedSteamBack;
            LightBack.gameObject.transform.localPosition = fixedLightBack;
            Trails.gameObject.transform.localPosition = fixedTrails;
        }

        void FixLevels(Vector2Int enemyDirection)
        {
            if (enemyDirection.y > 0)
            {
                SteamFront.sortingOrder = 10;
                LightFront.sortingOrder = 11;

                SteamTop.sortingOrder = 20;
                LightTop.sortingOrder = 21;

                SteamBack.sortingOrder = 30;
                LightBack.sortingOrder = 31;
            }
            else if (enemyDirection.y < 0)
            {
                SteamFront.sortingOrder = 30;
                LightFront.sortingOrder = 31;

                SteamTop.sortingOrder = 10;
                LightTop.sortingOrder = 11;

                SteamBack.sortingOrder = 0;
                LightBack.sortingOrder = 1;
            }
        }
    }
}