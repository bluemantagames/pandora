using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Combat;
using Pandora.Engine;

namespace Pandora
{

    // Bottom left grid cell
    public class TowerPositionComponent : MonoBehaviour
    {
        // this should not be used anywhere. It's here just to give a way to set tower positions in the unity editor
        public Vector2 position;
        public GridCell towerCell;
        public TowerPosition towerPosition = TowerPosition.TopLeft;

        /// <summary>
        /// This is the entity actually residing in this TowerPosition in the engine.
        /// We return this if we're bot team, or the corresponding flipped entity if we're top
        /// </summary>
        EngineEntity engineTowerEntity;

        public EngineEntity TowerEntity
        {
            get
            {
                if (TeamComponent.assignedTeam == TeamComponent.bottomTeam)
                {
                    return engineTowerEntity;
                }

                var flippedPosition = towerPosition.Flip();

                foreach (var component in transform.parent.GetComponentsInChildren<TowerPositionComponent>()) {
                    if (component.towerPosition == flippedPosition) {
                        return component.engineTowerEntity;
                    }
                }

                return null;
            }
        }

        public GridCell GetMapTarget()
        {
            if (towerPosition == TowerPosition.TopLeft || towerPosition == TowerPosition.TopRight)
            {
                return new GridCell(position.x + 1, position.y - 1);
            }
            else if (towerPosition == TowerPosition.BottomLeft || towerPosition == TowerPosition.BottomRight)
            {
                return new GridCell(position.x + 1, position.y + 3);
            }
            else if (towerPosition == TowerPosition.BottomMiddle)
            {
                return new GridCell(8, 3);
            }
            else if (towerPosition == TowerPosition.TopMiddle)
            {
                return new GridCell(8, 23);
            }
            else
            {
                return towerCell;
            }
        }

        public GridCell GetTowerCenter()
        {
            if (towerPosition == TowerPosition.TopLeft || towerPosition == TowerPosition.TopRight)
            {
                return new GridCell(position.x + 1, position.y + 1);
            }
            else if (towerPosition == TowerPosition.BottomLeft || towerPosition == TowerPosition.BottomRight)
            {
                return new GridCell(position.x + 1, position.y + 1);
            }
            else if (towerPosition == TowerPosition.BottomMiddle)
            {
                return new GridCell(position.x + 2, position.y + 2);
            }
            else if (towerPosition == TowerPosition.TopMiddle)
            {
                return new GridCell(position.x + 2, position.y + 2);
            }
            else
            {
                return towerCell;
            }
        }

        void Start()
        {
            towerCell = new GridCell(position);

            SpawnTowers();
        }

        public void SpawnTowers()
        {
            engineTowerEntity = GetComponent<TowerCombatBehaviour>().map.engine.AddEntity(gameObject, 0f, GetTowerCenter(), true, null);

            engineTowerEntity.IsStructure = true;
        }
    }
}
