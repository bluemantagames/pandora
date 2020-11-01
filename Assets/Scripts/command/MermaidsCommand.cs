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

        public List<EffectIndicator> FindTargets()
        {
            throw new System.NotImplementedException();
        }

        public void InvokeCommand()
        {
            var mapComponent = MapComponent.Instance;

            var position = GetComponentInParent<EngineComponent>().Entity.GetCurrentCell();

            var engine = mapComponent.engine;
            var teamComponent = GetComponentInParent<TeamComponent>();

            var tornadoPosition = (position.vector.x > mapComponent.mapSizeX / 2) ? RightBridgePosition : LeftBridgePosition;

            var tornado = Instantiate(TornadoObject, Vector3.zero, Quaternion.identity);

            var entityPosition = engine.GridCellToPhysics(new GridCell(tornadoPosition));

            Logger.Debug($"Spawning tornado in {entityPosition}");

            var entity = engine.AddEntity(tornado, 0, entityPosition, false, PandoraEngine.SafeGenerateTimestamp(tornado));

            var engineComponent = tornado.AddComponent<EngineComponent>();

            engineComponent.Entity = entity;

            var tornadoTeamComponent = tornado.GetComponent<TeamComponent>();

            tornadoTeamComponent.Team = teamComponent.Team;
        }
    }
}