using UnityEngine;
using CRclone.Movement;

namespace CRclone
{
    public class Enemy
    {
        public GameObject enemy;
        public Vector2 enemyCell
        {
            get
            {
                return enemy.GetComponent<MovementComponent>().map.WorldPositionToGridCell(enemy.transform.position);
            }
        }

        public Enemy(GameObject enemy)
        {
            this.enemy = enemy;
        }
    }

}