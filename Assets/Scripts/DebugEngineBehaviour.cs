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
            /*
            Debug.Log("Debugging");

            foreach (Transform entity in MapComponent.Instance.gameObject.transform)
            {
                var component = entity.GetComponent<EngineComponent>();

                if (component != null && component.DebugEntity)
                {
                    component.Entity.PrintDebugInfo();
                }
            }*/

            var unit = MapComponent.Instance.gameObject.GetComponentInChildren<MovementComponent>().gameObject;
            var engineEntity = unit.GetComponent<EngineComponent>().Entity;

            engineEntity.IsEvading = true;

            engineEntity.SetTarget(new GridCell(20, 20));
        }
    }

}