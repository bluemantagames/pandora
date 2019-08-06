using UnityEngine;
using Pandora.Movement;

namespace Pandora
{
    public class Enemy
    {
        public GameObject enemy;
        public GridCell enemyCell
        {
            get
            {
                var towerPosition = enemy.GetComponent<TowerPositionComponent>();

                if (towerPosition != null)
                {
                    return towerPosition.towerCell;
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