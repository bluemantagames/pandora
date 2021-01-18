using UnityEngine;
using System.Linq;
using Pandora.Combat;
using Pandora.Engine;
using Pandora.Movement;
using System.Collections.Generic;
using Pandora.UI;

namespace Pandora.Command
{
    /// <summary>
    /// On double tap, mermaids spawn a tornado on the closest bridge that pulls nearby units
    /// and moves towards the unit who is the nearest to the allied middle tower
    ///</summary>
    public class MermaidsCommand : MonoBehaviour, CommandBehaviour
    {
        public GameObject TornadoObject;
        public Vector2Int LeftBridgePosition, RightBridgePosition;

        EngineComponent engineComponent;

        void Start () {
            engineComponent = GetComponentInParent<EngineComponent>();
        }

        public List<EffectIndicator> FindTargets()
        {
            return new List<EffectIndicator> {
                new LaneIndicator(findLane())
            };
        }

        public void InvokeCommand()
        {
            var mapComponent = MapComponent.Instance;

            var engine = mapComponent.engine;
            var teamComponent = GetComponentInParent<TeamComponent>();

            var tornadoLane = findLane();
            var tornadoPosition = (tornadoLane == Lane.Right) ? RightBridgePosition : LeftBridgePosition;

            var tornado = Instantiate(TornadoObject, Vector3.zero, TornadoObject.transform.rotation);

            var entityPosition = engine.GridCellToPhysics(new GridCell(tornadoPosition));

            Logger.Debug($"Spawning tornado in {entityPosition}");

            var entity = engine.AddEntity(tornado, 0, entityPosition, false, PandoraEngine.SafeGenerateTimestamp(tornado));

            var tornadoEngineComponent = tornado.AddComponent<EngineComponent>();

            tornadoEngineComponent.Entity = entity;

            var tornadoTeamComponent = tornado.GetComponent<TeamComponent>();

            tornadoTeamComponent.Team = teamComponent.Team;
        }

        Lane findLane() {
            var mapComponent = MapComponent.Instance;
            var position = engineComponent.Entity.GetCurrentCell();

            return (position.vector.x > mapComponent.mapSizeX / 2) ? Lane.Right : Lane.Left;
        }
    }
}