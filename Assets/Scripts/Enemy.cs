using UnityEngine;
using Pandora.AI;
using Pandora.Engine;

namespace Pandora
{
    public class Enemy
    {
        public bool IsTower = false;
        public GameObject enemy;
        public GridCell enemyCell
        {
            get
            {
                var towerPosition = enemy.GetComponent<TowerPositionComponent>();

                if (towerPosition != null)
                {
                    return towerPosition.GetTowerCenter();
                }
                else
                {
                    return GameObject.Find("Arena").GetComponent<MapComponent>().GetCell(enemy);
                }
            }
        }

        public EngineEntity enemyEntity
        {
            get
            {
                return enemy.GetComponent<EngineComponent>().Entity;
            }
        }

        public override string ToString()
        {
            return $"Enemy(GameObject: {enemy} ({enemy.name}), GridCell: {enemyCell})";
        }

        public Enemy(GameObject enemy)
        {
            this.enemy = enemy;

            IsTower = enemy.GetComponent<TowerPositionComponent>() != null;
        }
    }

}