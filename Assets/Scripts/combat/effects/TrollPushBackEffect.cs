using UnityEngine;
using Pandora.Engine;
using Pandora.AI;
using Pandora.Pool;

namespace Pandora.Combat.Effects
{
    public class TrollPushBackEffect : MonoBehaviour, EngineBehaviour, Effect
    {
        bool _isDisabled = false;

        public bool IsDisabled
        {
            get => _isDisabled;
            set => _isDisabled = value;
        }

        public GameObject Origin;
        public Vector2Int OriginDirection;
        uint timePassed = 0;
        public uint TickMs = 1, DurationMs = 100;
        public int Force = 30;
        public string ComponentName => "TrollPushBack";

        public Effect Apply(GameObject origin, GameObject target)
        {
            var component = target.GetComponent<TrollPushBackEffect>();
            var targetEntity = target.GetComponent<EngineComponent>().Entity;

            if (targetEntity.IsStructure)
                return null;

            if (component != null)
            {
                component.Refresh();
            }
            else
            {
                component = target.AddComponent<TrollPushBackEffect>();
                component.Origin = origin;
                component.OriginDirection = origin.GetComponent<EngineComponent>().Entity.Direction;
                component.Force = Force;
                component.RefreshComponents();
            }

            return component;
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (IsDisabled) return;

            timePassed += timeLapsed;

            if (timePassed % TickMs == 0)
            {
                var newPosition = PoolInstances.Vector2IntPool.GetObject();
                var engineComponent = gameObject.GetComponent<EngineComponent>();
                var movementComponent = gameObject.GetComponent<BasicEntityController>();
                var engineEntity = engineComponent.Entity;

                var xForce = OriginDirection.x * Force;
                var yForce = OriginDirection.y * Force;

                newPosition.x = engineEntity.Position.x + xForce;
                newPosition.y = engineEntity.Position.y + yForce;
                engineEntity.Position = newPosition;

                movementComponent.ResetPath();
            }

            if (timePassed % DurationMs == 0)
            {
                Unapply(gameObject);
            }
        }

        public void Unapply(GameObject target)
        {
            var component = target.GetComponent<TrollPushBackEffect>();
            component.IsDisabled = true;

            Destroy(component);
            RefreshComponents();
        }

        public void Refresh()
        {
            timePassed = 0;
        }

        void RefreshComponents()
        {
            gameObject.GetComponent<EngineComponent>().RefreshComponents();
        }
    }

}