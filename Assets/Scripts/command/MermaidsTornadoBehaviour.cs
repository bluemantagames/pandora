using Pandora.Engine;
using Pandora.Combat.Effects;
using UnityEngine;

namespace Pandora.Command {
    public class MermaidsTornadoBehaviour : MonoBehaviour, EngineBehaviour
    {
        public string ComponentName => "MermaidsTornadoBehaviour";

        public GameObject TornadoEffectObject;
        public int EngineUnitsRadius;

        // TODO: Remove this, make the mermaids add it to the engine
        void Start() {
            var engine = MapComponent.Instance.engine;
            var entityPosition = engine.GridCellToPhysics(MapComponent.Instance.WorldPositionToGridCell(transform.position));

            Debug.Log($"Spawning tornado in {entityPosition}");
        
            var entity = engine.AddEntity(gameObject, 0, entityPosition, false, null);

            var engineComponent = gameObject.AddComponent<EngineComponent>();

            engineComponent.Entity = entity;
        }

        public void TickUpdate(uint timeLapsed)
        {
            var engineComponent = GetComponent<EngineComponent>();

            var entity = engineComponent.Entity;
            var engine = engineComponent.Engine;

            transform.position = engine.PhysicsToMap(entity.Position);

            foreach (var target in engine.FindInRadius(entity.Position, EngineUnitsRadius, false)) {
                if (target == entity || !target.IsRigid) continue;
                
                TornadoEffectObject.GetComponent<MermaidsTornadoEffect>().Apply(gameObject, target.GameObject);
            }
        }
    }
}