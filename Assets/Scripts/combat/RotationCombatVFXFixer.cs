using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

namespace Pandora.Combat
{
    public class RotationCombatVFXFixer : MonoBehaviour, CombatVFXFixer
    {
        public bool ApplyToChildren = false;
        public Vector3 FixDirectionTopLeft = new Vector3(0, 0, 0);
        public Vector3 FixDirectionTopCenter = new Vector3(0, 0, 0);
        public Vector3 FixDirectionTopRight = new Vector3(0, 0, 0);
        public Vector3 FixDirectionCenterLeft = new Vector3(0, 0, 0);
        public Vector3 FixDirectionCenterRight = new Vector3(0, 0, 0);
        public Vector3 FixDirectionBottomLeft = new Vector3(0, 0, 0);
        public Vector3 FixDirectionBottomCenter = new Vector3(0, 0, 0);
        public Vector3 FixDirectionBottomRight = new Vector3(0, 0, 0);
        Vector3 originalLocalRotation;

        void Awake()
        {
            originalLocalRotation = transform.localRotation.eulerAngles;
        }

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
            var fixedRotation = originalLocalRotation;

            if (enemyDirection.x == 1 && enemyDirection.y == -1)
                fixedRotation += FixDirectionBottomRight;
            else if (enemyDirection.x == 1 && enemyDirection.y == 0)
                fixedRotation += FixDirectionCenterRight;
            else if (enemyDirection.x == 1 && enemyDirection.y == 1)
                fixedRotation += FixDirectionTopRight;
            else if (enemyDirection.x == 0 && enemyDirection.y == -1)
                fixedRotation += FixDirectionBottomCenter;
            else if (enemyDirection.x == -1 && enemyDirection.y == 1)
                fixedRotation += FixDirectionTopLeft;
            else if (enemyDirection.x == -1 && enemyDirection.y == 0)
                fixedRotation += FixDirectionCenterLeft;
            else if (enemyDirection.x == -1 && enemyDirection.y == -1)
                fixedRotation += FixDirectionBottomLeft;
            else
                fixedRotation += FixDirectionTopCenter;

            var newRotation = Quaternion.Euler(fixedRotation.x, fixedRotation.y, fixedRotation.z);

            return newRotation;
        }
    }
}