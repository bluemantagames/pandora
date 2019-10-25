using UnityEngine;
using Pandora.Engine;

namespace Pandora.Combat.Effects {
    public class BardBuffEffect: MonoBehaviour, EngineBehaviour, Effect {
        bool _isDisabled = false;

        public bool IsDisabled {
            get => _isDisabled;
            set => _isDisabled = value;
        }
    
        public GameObject Origin;
        uint timePassed = 0;
        public uint TickMs = 1, DurationMs = 100;
        public string ComponentName => "BardBuff";

        public Effect Apply(GameObject origin, GameObject target) {
            var component = target.GetComponent<BardBuffEffect>();

            if (component != null) {
                component.Refresh();
            } else {
                component = target.AddComponent<BardBuffEffect>();
                component.Origin = origin;
                component.RefreshComponents();
            }

            return component;
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (IsDisabled) return;

            timePassed += timeLapsed;

            if (timePassed % TickMs == 0) {

            }

            if (timePassed % DurationMs == 0) {
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

        public void Refresh() {
            timePassed = 0;
        }

        void RefreshComponents() {
            gameObject.GetComponent<EngineComponent>().RefreshComponents();
        }
    }

}