using UnityEngine;
using Pandora.Engine;

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
            Debug.Log("Debugging");

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