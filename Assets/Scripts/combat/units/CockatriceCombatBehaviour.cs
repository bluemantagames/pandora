using UnityEngine;
using Pandora.Engine;

namespace Pandora.Combat.Units {
    public class CockatricePoisonEffect: MonoBehaviour, EngineBehaviour, Effect {
        public bool IsDisabled = false;
        public GameObject Target;
        public GameObject Origin;

        uint timePassed = 0;
        uint poisonTickMs = 200, positionDurationMs = 2000, damagePerTick = 30;

        public string ComponentName => "CockatricePoison";

        public Effect Apply(GameObject origin, GameObject target) {
            var component = origin.GetComponent<CockatricePoisonEffect>();

            if (component != null) {
                component.Refresh();
            } else {
                component = origin.AddComponent<CockatricePoisonEffect>();

                component.Target = target;
                component.Origin = origin;
            }

            return component;
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (IsDisabled) return;

            timePassed += timeLapsed;

            if (timePassed % poisonTickMs == 0) {
                var lifeComponent = Target.GetComponent<LifeComponent>();
                
                lifeComponent.AssignDamage(damagePerTick);

                if (lifeComponent.isDead) Unapply(Target);
            }

            if (timePassed % positionDurationMs == 0) {
                Unapply(Target);
            }
        }

        public void Unapply(GameObject target)
        {
            var component = target.GetComponent<CockatricePoisonEffect>();

            component.IsDisabled = false;

            Destroy(component);
        }

        public void Refresh() {
            timePassed = 0;
        }
    }

}