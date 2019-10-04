using UnityEngine;
using System.Collections.Generic;

namespace Pandora.Engine
{
    public class EngineComponent : MonoBehaviour
    {
        List<EngineBehaviour> cachedComponents = new List<EngineBehaviour> { };

        /// <summary>Deterministically sorted list of Engine behaviours</summary>
        public List<EngineBehaviour> Components
        {
            get
            {
                if (cachedComponents.Count == 0)
                {
                    RefreshComponents();
                }

                return cachedComponents;
            }
        }

        public EngineEntity Entity;
        public bool DebugEntity = false;

        public PandoraEngine Engine
        {
            get
            {
                return MapComponent.Instance.engine;
            }
        }

        public void Remove()
        {
            Engine.RemoveEntity(Entity);
        }

        /// <summary>For perf reasons, this method must be called every time an EngineBehaviour component is added</summary>
        public void RefreshComponents()
        {
            cachedComponents = new List<EngineBehaviour>(
                GetComponents<EngineBehaviour>()
            );

            cachedComponents.Sort(CompareBehaviours);
        }

        int CompareBehaviours(EngineBehaviour first, EngineBehaviour second)
        {
            return first.ComponentName.CompareTo(second.ComponentName);
        }
    }
}