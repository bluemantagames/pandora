using Pandora;
using Pandora.Engine;
using UnityEngine;
using System.Collections.Generic;

namespace Pandora.UI {
    public class EffectIndicator {}
    
    /// <summary>
    /// Place a circular indicator, of a certain radius, on a point in the arena
    /// <summary>
    public class CircleRangeIndicator: EffectIndicator {
        public int RadiusEngineUnits;
        public Vector2Int PositionEngineUnits;

        public CircleRangeIndicator(int radiusEngineUnits, Vector2Int positionEngineUnits)
        {
            RadiusEngineUnits = radiusEngineUnits;
            PositionEngineUnits = positionEngineUnits;
        }

        public CircleRangeIndicator(int radiusEngineUnits, GridCell position) {
            RadiusEngineUnits = radiusEngineUnits;
            PositionEngineUnits = MapComponent.Instance.engine.GridCellToPhysics(position);
        }
    }


    /// <summary>
    /// Place a circular indicator, of a certain radius, following a unit or a structure
    /// <summary>
    public class FollowingCircleRangeIndicator: EffectIndicator {
        public int RadiusEngineUnits;
        public GameObject Followed;

        public FollowingCircleRangeIndicator(int radiusEngineUnits, GameObject followed)
        {
            RadiusEngineUnits = radiusEngineUnits;
            Followed = followed;
        }
    }

    /// <summary>
    /// Highlight multiple engine entities
    /// </summary>
    public class EntitiesIndicator: EffectIndicator {
        public List<EngineEntity> Entities;

        public EntitiesIndicator(List<EngineEntity> entities)
        {
            Entities = entities;
        }
    }

}