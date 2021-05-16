using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

namespace Pandora.Combat
{
    public class FragCannonShotBehaviour : MonoBehaviour, VFXShot
    {
        public Vector2 EnemyDirection { get; set; }
        float shotDirectionThreshold = 0.5f;
        bool fixedRotation = false;

        Quaternion FixedShotRotation()
        {
            var direction = GetShotDirection();

            int yRotation = 0;
            int xRotation = 0;

            if (direction.x == 1 && direction.y == -1)
                yRotation = 45;
            else if (direction.x == 1 && direction.y == 0)
                yRotation = 90;
            else if (direction.x == 1 && direction.y == 1)
                yRotation = 135;
            else if (direction.x == 0 && direction.y == 1)
                yRotation = 180;
            else if (direction.x == -1 && direction.y == 1)
                yRotation = 225;
            else if (direction.x == -1 && direction.y == 0)
                yRotation = 270;
            else if (direction.x == -1 && direction.y == -1)
                yRotation = 315;

            if (direction.y > 0)
                xRotation = -50;

            Logger.Debug($"[VFX] {EnemyDirection} {direction}");

            return Quaternion.Euler(xRotation, yRotation, 0);
        }

        Vector2Int GetShotDirection()
        {
            var direction = new Vector2Int();

            if (EnemyDirection != null)
            {
                direction.x = EnemyDirection.x > shotDirectionThreshold ? 1 : EnemyDirection.x < -shotDirectionThreshold ? -1 : 0;
                direction.y = EnemyDirection.y > shotDirectionThreshold ? 1 : EnemyDirection.y < -shotDirectionThreshold ? -1 : 0;
            }

            return direction;
        }

        void Update()
        {
            if (!fixedRotation)
            {
                transform.localRotation = FixedShotRotation();
                fixedRotation = true;
            }
        }
    }
}