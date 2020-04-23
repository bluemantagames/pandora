using UnityEngine;
using Pandora.Engine;
using Pandora.Movement;

namespace Pandora
{
    public class DebugEngineBehaviour : MonoBehaviour
    {
        void Start()
        {
            if (!Debug.isDebugBuild)
            {
                gameObject.SetActive(false);
            }
        }

        public void DebugEngine()
        {
            Logger.Debug("Debugging");

            foreach (Transform entity in MapComponent.Instance.gameObject.transform)
            {
                var component = entity.GetComponent<EngineComponent>();

                if (component != null && component.DebugEntity)
                {
                    component.Entity.PrintDebugInfo();
                }
            }
        }
    }

}