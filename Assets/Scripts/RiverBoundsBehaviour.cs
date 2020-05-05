using UnityEngine;
using Pandora.Engine;

namespace Pandora {
    public class RiverBoundsBehaviour: MonoBehaviour {
        public Vector2Int EnginePosition;

        void Start() {
            var timestamp = PandoraEngine.SafeGenerateTimestamp(gameObject);
            var entity = MapComponent.Instance.engine.AddEntity(gameObject, 0, EnginePosition, true, timestamp);

            entity.IsMapObstacle = true;
            entity.IsStructure = true;
            entity.Layer = Constants.SWIMMING_LAYER;
        }
    }

}