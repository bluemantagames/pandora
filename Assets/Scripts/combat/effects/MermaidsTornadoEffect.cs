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

        private int engineUnitsRadius;

        public int EngineUnitsRadius {
            get => engineUnitsRadius;
            set => engineUnitsRadius = value;

        }

        public int EngineUnitsMaxForce, EngineUnitsMinForce;
        public bool DebugTornado;

        EngineEntity tornado, target;

        PandoraEngine engine
        {
            get
            {
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

                component.EngineUnitsMaxForce = EngineUnitsMaxForce;
                component.EngineUnitsMinForce = EngineUnitsMinForce;
                component.DebugTornado = DebugTornado;

                Logger.Debug($"Setting tornado target {target}");

                component.target = target.GetComponent<EngineComponent>().Entity;
                component.tornado = origin.GetComponent<EngineComponent>().Entity;

                target.GetComponent<EngineComponent>().RefreshComponents();
            }

            return component;
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (isDisabled) return;

            var distance = engine.Distance(tornado.Position, target.Position);

            if (distance > EngineUnitsRadius) Unapply(target.GameObject);

            var distanceFromEdge = EngineUnitsRadius - distance;

            var force = distanceFromEdge * (decimal)(EngineUnitsMaxForce - EngineUnitsMinForce) / (decimal)EngineUnitsRadius;

            if (force == 0) force = (decimal)EngineUnitsMinForce;

            var path = Bresenham.GetEnumerator(target.Position, tornado.Position);

            for (var i = 0; i < force; i++) path.MoveNext();

            var position = path.Current;

            if (DebugTornado) {
                Logger.Debug($"Unit is distant {distanceFromEdge + EngineUnitsRadius}, {force} has been applied, it moved to {position} ({tornado.Position} from {target.Position})");
            }

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