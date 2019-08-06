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
                var towerPosition = enemy.GetComponent<TowerPositionComponent>();

                if (towerPosition != null)
                {
                    return towerPosition.position;
                }
                else
                {
                    return GameObject.Find("Arena").GetComponent<MapComponent>().WorldPositionToGridCell(enemy.transform.position);
                }
            }
        }

        public Enemy(GameObject enemy)
        {
            this.enemy = enemy;
        }
    }

}