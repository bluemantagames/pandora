using UnityEngine;

namespace Pandora.UI {
    public class RectangleIndicatorBehaviour: MonoBehaviour {
        public void Initialize(int widthEngineUnits, int heightEngineUnits) {
            var position = transform.position;

            position.z = -1;

            transform.position = position;

            transform.localScale = Vector2.one;

            var worldSpaceRadius = MapComponent.Instance.engine.PhysicsToWorld(new Vector2Int(widthEngineUnits, heightEngineUnits));

            transform.localScale = new Vector2(
                worldSpaceRadius.x / transform.lossyScale.x,
                worldSpaceRadius.y / transform.lossyScale.y
            );
        }
    }
}