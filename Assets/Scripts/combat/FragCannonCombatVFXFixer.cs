using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

namespace Pandora.Combat
{
    public class FragCannonCombatVFXFixer : MonoBehaviour, CombatVFXFixer
    {
        public Vector2 EnemyDirection { get; set; }
        bool fixedRotation = false;

        public Quaternion FixedShotRotation(Vector2Int enemyDirection)
        {
            int yRotation = 0;
            int xRotation = 0;

            if (enemyDirection.x == 1 && enemyDirection.y == -1)
                yRotation = 45;
            else if (enemyDirection.x == 1 && enemyDirection.y == 0)
                yRotation = 90;
            else if (enemyDirection.x == 1 && enemyDirection.y == 1)
                yRotation = 135;
            else if (enemyDirection.x == 0 && enemyDirection.y == 1)
                yRotation = 180;
            else if (enemyDirection.x == -1 && enemyDirection.y == 1)
                yRotation = 225;
            else if (enemyDirection.x == -1 && enemyDirection.y == 0)
                yRotation = 270;
            else if (enemyDirection.x == -1 && enemyDirection.y == -1)
                yRotation = 315;

            if (enemyDirection.y > 0)
                xRotation = -50;

            return Quaternion.Euler(xRotation, yRotation, 0);
        }
    }
}