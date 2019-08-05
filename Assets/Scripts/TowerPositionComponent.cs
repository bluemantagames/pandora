using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CRclone
{

    // Bottom left grid cell
    public class TowerPositionComponent : MonoBehaviour
    {
        public Vector2 position;
        public TowerPosition towerPosition = TowerPosition.TopLeft; 
    }
}
