using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CRclone
{

    // Bottom left grid cell
    public class TowerPositionComponent : MonoBehaviour
    {
        public Vector2 position;
        public GridCell towerCell;
        public TowerPosition towerPosition = TowerPosition.TopLeft; 

        void Start() {
            towerCell = new GridCell(position);
        }
    }
}
