using UnityEngine;
using Pandora.Movement;
using Pandora.Engine;

namespace Pandora.Combat.Effects
{
    /// <summary>
    /// Effect applied by the Mermaids tornado. 
    ///
    /// It pulls units inside the tornado with a force proportional to the distance from the center
    /// </summary>
    public class MermaidsTornadoEffect : MonoBehaviour, EngineBehaviour, Effect
    {
        public string ComponentName => "MermaidsTornadoEffect";

        public int EngineUnitsRadius, EngineUnitsMaxForce, EngineUnitsMinForce;

        EngineEntity tornado, target;
        uint totalTimeLapsed = 0;

        PandoraEngine engine {
            get {
                return MapComponent.Instance.engine;
            }
        }

        bool isDisabled = false;

        public Effect Apply(GameObject origin, GameObject target)
        {
            var component = target.GetComponent<MermaidsTornadoEffect>();

            if (component == null)
            {
                component = target.AddComponent<MermaidsTornadoEffect>();

                component.EngineUnitsRadius = EngineUnitsRadius;
                component.EngineUnitsMaxForce = EngineUnitsMaxForce;
                component.EngineUnitsMinForce = EngineUnitsMinForce;

                Debug.Log($"Setting tornado target {target}");

                component.target = target.GetComponent<EngineComponent>().Entity;
                component.tornado = origin.GetComponent<EngineComponent>().Entity;

                target.GetComponent<EngineComponent>().RefreshComponents();
            }

            return component;
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (isDisabled) return;

            var distance = EngineUnitsRadius - engine.Distance(tornado.Position, target.Position);

            var force = distance * (decimal) (EngineUnitsMaxForce - EngineUnitsMinForce) / (decimal) EngineUnitsRadius;

            if (force == 0) force = (decimal) EngineUnitsMinForce;

            var path = Bresenham.GetEnumerator(target.Position, tornado.Position);

            for (var i = 0; i < force; i++) path.MoveNext();

            var position = path.Current;

            Debug.Log($"Unit is distant {distance + EngineUnitsRadius}, {force} has been applied, it moved to {position} ({tornado.Position} from {target.Position})");

            target.Position = position;

            target.GameObject.GetComponent<MovementComponent>().ResetPath();
        }

        public void Unapply(GameObject target)
        {
            var component = target.GetComponent<MermaidsTornadoEffect>();

            if (component != null)
            {
                component.isDisabled = true;

                Destroy(component);

                target.GetComponent<EngineComponent>().RefreshComponents();
            }
        }
    }
}