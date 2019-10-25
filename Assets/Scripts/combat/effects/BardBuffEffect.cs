using UnityEngine;
using Pandora.Engine;
using Pandora.Movement;

namespace Pandora.Combat.Effects {
    public class BardBuffEffect: MonoBehaviour, EngineBehaviour, Effect {
        bool _isDisabled = false;

        public bool IsDisabled {
            get => _isDisabled;
            set => _isDisabled = value;
        }
    
        public GameObject Origin;
        uint timePassed = 0;
        public uint TickMs = 1, DurationMs = 3000;
        public int SpeedIncrease = 500;
        public int DamageIncrease = 30;
        public int CureAmount = 500;
        public string ComponentName => "BardBuff";

        void SetStats(GameObject target, int speedIncrease, int damageIncrease) {
            var meleeCombatComponent = target.GetComponent<MeleeCombatBehaviour>();
            var rangedCombarComponent = target.GetComponent<RangedCombatBehaviour>();
            var movementComponent = target.GetComponent<MovementComponent>();

            // Change attack speed and damage
            if (meleeCombatComponent != null) {
                meleeCombatComponent.attackCooldownMs -= speedIncrease;
                meleeCombatComponent.backswingMs -= speedIncrease;

                meleeCombatComponent.damage += damageIncrease;
            }

            if (rangedCombarComponent != null) {
                rangedCombarComponent.attackCooldownMs -= speedIncrease;

                rangedCombarComponent.Damage += damageIncrease;
            }

            // Change movement speed
            if (movementComponent != null) {
                movementComponent.Speed += speedIncrease;
            }
        }

        public Effect Apply(GameObject origin, GameObject target) {
            var component = target.GetComponent<BardBuffEffect>();

            if (component != null) {
                component.Refresh();
            }
            else {
                component = target.AddComponent<BardBuffEffect>();
                component.Origin = origin;
                component.RefreshComponents();

                // Add the buff
                SetStats(target, SpeedIncrease, DamageIncrease);
            }

            return component;
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (IsDisabled) return;

            timePassed += timeLapsed;

            if (timePassed % DurationMs == 0) {
                Unapply(gameObject);
            }
        }

        public void Unapply(GameObject target)
        {
            var component = target.GetComponent<BardBuffEffect>();
            component.IsDisabled = true;

            // Remove the buff
            SetStats(target, -SpeedIncrease, -DamageIncrease);

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