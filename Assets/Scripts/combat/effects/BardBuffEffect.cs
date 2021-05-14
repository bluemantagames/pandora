using UnityEngine;
using Pandora.Engine;
using Pandora.AI;

/// <summary>
/// Effect for the Bard unit.
/// Heal, increase attack/movement speed, increase attack damage for some time
/// </summary>
namespace Pandora.Combat.Effects
{
    public class BardBuffEffect : MonoBehaviour, EngineBehaviour, Effect
    {
        bool _isDisabled = false;
        Sprite test;

        public bool IsDisabled
        {
            get => _isDisabled;
            set => _isDisabled = value;
        }

        public GameObject Origin;
        Color originalColor;
        uint timePassed = 0;
        public uint TickMs = 1, DurationMs = 3000;
        public int MovementSpeedIncrease = 400;
        public int AttackSpeedIncrease = 200;

        public int DamageIncrease = 10;
        public int HealAmount = 500;
        public Color BuffedColor = Color.yellow;
        public string ComponentName => "BardBuff";

        bool applied = false;

        void SetStats(GameObject target, int movSpeedIncrease, int atkSpeedIncrease, int damageIncrease)
        {
            var meleeCombatComponent = target.GetComponent<MeleeCombatBehaviour>();
            var rangedCombatComponent = target.GetComponent<RangedCombatBehaviour>();
            var movementComponent = target.GetComponent<BasicEntityController>();

            // Change attack speed and damage
            if (meleeCombatComponent != null)
            {
                meleeCombatComponent.attackCooldownMs -= atkSpeedIncrease;
                meleeCombatComponent.backswingMs -= atkSpeedIncrease;

                meleeCombatComponent.damage += damageIncrease;
            }

            if (rangedCombatComponent != null)
            {
                rangedCombatComponent.attackCooldownMs -= atkSpeedIncrease;

                rangedCombatComponent.Damage += damageIncrease;
            }

            // Change movement speed
            if (movementComponent != null)
            {
                movementComponent.Speed += movSpeedIncrease;
                movementComponent.ResetPath();
            }
        }

        void Heal(GameObject target, int healAmount)
        {
            var lifeComponent = target.GetComponent<LifeComponent>();

            lifeComponent.Heal(healAmount);
        }

        public Effect Apply(GameObject origin, GameObject target)
        {
            var component = target.GetComponent<BardBuffEffect>();

            if (component != null)
            {
                component.Refresh();
            }
            else
            {
                component = target.AddComponent<BardBuffEffect>();
                component.Origin = origin;
                component.RefreshComponents();
            }

            return component;
        }

        void Start()
        {
            var rendererComponent = gameObject.GetComponentInChildren<SpriteRenderer>();
            var targetEntity = gameObject.GetComponent<EngineComponent>().Entity;
            originalColor = rendererComponent.color;


            if (rendererComponent && !targetEntity.IsStructure)
            {
                originalColor = rendererComponent.color;
                rendererComponent.color = BuffedColor;
            }
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (IsDisabled) return;

            if (!applied)
            {
                // Add the buff
                SetStats(gameObject, MovementSpeedIncrease, AttackSpeedIncrease, DamageIncrease);
                Heal(gameObject, HealAmount);

                applied = true;
            }

            timePassed += timeLapsed;

            if (timePassed % DurationMs == 0)
            {
                Unapply(gameObject);
            }
        }

        public void Unapply(GameObject target)
        {
            var component = target.GetComponent<BardBuffEffect>();
            var rendererComponent = target.GetComponent<SpriteRenderer>();
            var targetEntity = gameObject.GetComponent<EngineComponent>().Entity;

            component.IsDisabled = true;

            // Remove the buff
            SetStats(target, -MovementSpeedIncrease, -AttackSpeedIncrease, -DamageIncrease);

            if (rendererComponent && !targetEntity.IsStructure)
            {
                rendererComponent.color = originalColor;
            }

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