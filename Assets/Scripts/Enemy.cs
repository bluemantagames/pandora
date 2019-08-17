using UnityEngine;
using Pandora.Movement;
using Pandora.Engine;

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
                    return towerPosition.GetMapTarget();
                }
                else
                {
                    return GameObject.Find("Arena").GetComponent<MapComponent>().GetCell(enemy);
                }
            }
        }

        public EngineEntity enemyEntity {
            get {
                var towerPosition = enemy.GetComponent<TowerPositionComponent>();

                if (towerPosition != null)
                {
                    return towerPosition.towerEntity;
                }
                else
                {
                    return enemy.GetComponent<MovementComponent>().engineEntity;;
                }

            }
        }

        public override string ToString() {
            return $"Enemy(GameObject: {enemy}, GridCell: {enemyCell})";
        }

        public Enemy(GameObject enemy)
        {
            this.enemy = enemy;
        }
    }

}