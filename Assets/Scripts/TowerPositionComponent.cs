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
        public EngineEntity towerEntity;

        public GridCell GetMapTarget() {
            if (towerPosition == TowerPosition.TopLeft || towerPosition == TowerPosition.TopRight) {
                return new GridCell(position.x + 1, position.y - 1);
            } else if (towerPosition == TowerPosition.BottomLeft || towerPosition == TowerPosition.BottomRight) {
                return new GridCell(position.x + 1, position.y + 3);
            } else if (towerPosition == TowerPosition.BottomMiddle) {
                return new GridCell(8, 3);
            } else if (towerPosition == TowerPosition.TopMiddle) {
                return new GridCell(8, 23);
            } else {
                return towerCell;
            }
        }

        void Start() {
            towerCell = new GridCell(position);

            towerEntity = GetComponent<TowerCombatBehaviour>().map.engine.AddEntity(gameObject, 0f, towerCell, true);
        }
    }
}
