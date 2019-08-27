using UnityEngine;

namespace Pandora.Engine {
    public class EngineComponent : MonoBehaviour {
        public EngineEntity Entity;

        public PandoraEngine Engine {
            get {
                return MapComponent.Instance.engine;
            }
        }

        public void Remove() {
            Engine.RemoveEntity(Entity);
        }
    }
}