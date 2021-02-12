using UnityEngine;

namespace Pandora.UI {
    public class CircleIndicatorBehaviour: MonoBehaviour {
        public void Initialize(int radiusEngineUnits) {
            var position = transform.position;

            position.z = -2;

            GetComponent<MeshRenderer>().sortingOrder = 200; // VFX layer

            transform.position = position;

            transform.localScale = Vector2.one;

            var worldSpaceRadius = MapComponent.Instance.engine.PhysicsToWorld(new Vector2Int(radiusEngineUnits, radiusEngineUnits));
            var scale = worldSpaceRadius.x / transform.lossyScale.x;

            transform.localScale = new Vector2(scale, scale);
        }
    }
}