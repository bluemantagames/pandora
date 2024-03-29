using Pandora;
using Pandora.Engine;
using UnityEngine;
using System.Collections.Generic;

namespace Pandora.UI {
    public interface EffectIndicator {
        void visit(IndicatorsVisitor visitor);
    }
    
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

        public void visit(IndicatorsVisitor visitor)
        {
            visitor.visit(this);
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

        public void visit(IndicatorsVisitor visitor)
        {
            visitor.visit(this);
        }
    }


    /// <summary>
    /// Keeps track of a game object that's used as an indicator, managing its lifecycle
    /// <summary>
    public class GameObjectIndicator: EffectIndicator {
        public GameObject Indicator;

        public GameObjectIndicator(GameObject indicator)
        {
            Indicator = indicator;
        }

        public void visit(IndicatorsVisitor visitor)
        {
            visitor.visit(this);
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
        
        public void visit(IndicatorsVisitor visitor)
        {
            visitor.visit(this);
        }
    }


    /// <summary>
    /// Highlights one of the two lanes
    /// </summary>
    public class LaneIndicator: EffectIndicator {
        public Lane Lane;

        public LaneIndicator(Lane lane)
        {
            Lane = lane;
        }
        
        public void visit(IndicatorsVisitor visitor)
        {
            visitor.visit(this);
        }
    }

}