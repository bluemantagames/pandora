using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

namespace Pandora.Combat
{
    public class PositionCombatVFXFixer : MonoBehaviour, CombatVFXFixer
    {
        public bool ApplyToChildren = false;
        public Vector3 FixPositionTopLeft = new Vector3(0, 0, 0);
        public Vector3 FixPositionTopRight = new Vector3(0, 0, 0);
        public Vector3 FixPositionCenterLeft = new Vector3(0, 0, 0);
        public Vector3 FixPositionCenterRight = new Vector3(0, 0, 0);
        public Vector3 FixPositionBottomLeft = new Vector3(0, 0, 0);
        public Vector3 FixPositionBottomCenter = new Vector3(0, 0, 0);
        public Vector3 FixPositionBottomRight = new Vector3(0, 0, 0);

        Vector3 originalLocalPosition;
        Dictionary<int, Vector3> originalChildPositions = new Dictionary<int, Vector3>();

        void Awake()
        {
            originalLocalPosition = transform.localPosition;
        }

        public void FixVFX(Vector2Int enemyDirection, Vector2 rawDirection)
        {
            transform.localPosition = FixShotPosition(originalLocalPosition, enemyDirection);

            // Apply the same fixes to the children
            if (ApplyToChildren)
            {
                foreach (Transform child in transform)
                {
                    var childId = child.gameObject.GetInstanceID();
                    Vector3 originalChildPosition;

                    originalChildPositions.TryGetValue(childId, out originalChildPosition);

                    if (originalChildPosition == null)
                    {
                        originalChildPositions.Add(childId, child.localPosition);
                        originalChildPosition = child.localPosition;
                    }

                    child.localPosition = FixShotPosition(originalChildPosition, enemyDirection);
                }
            }
        }

        Vector3 FixShotPosition(Vector3 originalPosition, Vector2Int enemyDirection)
        {
            var fixedPosition = originalPosition;

            if (enemyDirection.x == 1 && enemyDirection.y == -1)
                fixedPosition += FixPositionBottomRight;
            else if (enemyDirection.x == 1 && enemyDirection.y == 0)
                fixedPosition += FixPositionCenterRight;
            else if (enemyDirection.x == 1 && enemyDirection.y == 1)
                fixedPosition += FixPositionTopRight;
            else if (enemyDirection.x == 0 && enemyDirection.y == -1)
                fixedPosition += FixPositionBottomCenter;
            else if (enemyDirection.x == -1 && enemyDirection.y == 1)
                fixedPosition += FixPositionTopLeft;
            else if (enemyDirection.x == -1 && enemyDirection.y == 0)
                fixedPosition += FixPositionCenterLeft;
            else if (enemyDirection.x == -1 && enemyDirection.y == -1)
                fixedPosition += FixPositionBottomLeft;

            return fixedPosition;
        }
    }
}