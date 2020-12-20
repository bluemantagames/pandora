using UnityEngine;
using Pandora.Engine;

namespace Pandora {
    public class RiverBoundsBehaviour: MonoBehaviour {
        public bool IsTopBound;

        void Start() {
            var mapComponent = MapComponent.Instance;
            var engine = mapComponent.engine;

            var gridCell = new GridCell(
                mapComponent.mapSizeX / 2,
                (IsTopBound) ? mapComponent.RiverY + 1 : mapComponent.RiverY - 1
            );

            var enginePosition = engine.GridCellToPhysics(gridCell);

            var timestamp = PandoraEngine.SafeGenerateTimestamp(gameObject);
            var entity = MapComponent.Instance.engine.AddEntity(gameObject, 0, enginePosition, true, timestamp);

            entity.IsMapObstacle = true;
            entity.IsStructure = true;
            entity.Layer = Constants.SWIMMING_LAYER;
        }
    }

}