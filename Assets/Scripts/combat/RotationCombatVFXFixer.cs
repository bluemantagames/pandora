using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

namespace Pandora.Combat
{
    public class RotationCombatVFXFixer : MonoBehaviour, CombatVFXFixer
    {
        public bool ApplyToChildren = false;
        public int FixDirectionTopLeft = 225;
        public int FixDirectionTopRight = 135;
        public int FixDirectionCenterLeft = 270;
        public int FixDirectionCenterRight = 90;
        public int FixDirectionBottomLeft = 315;
        public int FixDirectionBottomCenter = 180;
        public int FixDirectionBottomRight = 35;

        public void FixVFX(Vector2Int enemyDirection, Vector2 rawDirection)
        {
            transform.localRotation = FixShotRotation(enemyDirection);

            // Apply the same fixes to the children
            if (ApplyToChildren)
            {
                foreach (Transform child in transform)
                {
                    child.localRotation = FixShotRotation(enemyDirection);
                }
            }
        }

        Quaternion FixShotRotation(Vector2Int enemyDirection)
        {
            var xRotation = 0;
            var yRotation = 0;
            var zRotation = 0;

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
                xRotation = -40;

            var newRotation = Quaternion.Euler(xRotation, yRotation, zRotation);

            return newRotation;
        }
    }
}