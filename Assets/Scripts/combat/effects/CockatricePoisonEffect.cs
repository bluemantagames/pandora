using UnityEngine;
using Pandora.Engine;

namespace Pandora.Combat.Effects
{
    public class CockatricePoisonEffect : MonoBehaviour, EngineBehaviour, Effect
    {
        bool _isDisabled = false;

        public bool IsDisabled
        {
            get => _isDisabled;
            set => _isDisabled = value;
        }

        public GameObject Origin;

        uint timePassed = 0;
        public uint PoisonTickMs = 200, PoisonDurationMs = 2000, DamagePerTick = 30;
        public Color OriginalColor;
        GameObject target;

        public string ComponentName => "CockatricePoison";

        public SpriteRenderer spriteRenderer;

        public Effect Apply(GameObject origin, GameObject target)
        {
            var component = target.GetComponent<CockatricePoisonEffect>();

            if (component != null)
            {
                component.Refresh();
            }
            else
            {
                component = target.AddComponent<CockatricePoisonEffect>();

                component.Origin = origin;
                component.PoisonTickMs = PoisonTickMs;
                component.PoisonDurationMs = PoisonDurationMs;
                component.DamagePerTick = DamagePerTick;
                component.target = target;

                component.Init();

                component.RefreshComponents();
            }

            return component;
        }

        void Init()
        {
            spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();

            OriginalColor = spriteRenderer.color;

            spriteRenderer.color = Color.green;
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (IsDisabled) return;

            if (timePassed % PoisonTickMs == 0)
            {
                var lifeComponent = gameObject.GetComponent<LifeComponent>();

                lifeComponent.AssignDamage((int)DamagePerTick, new Debuff(Origin, this));

                if (lifeComponent.IsDead) Unapply(gameObject);
            }

            if (timePassed != 0 && timePassed % PoisonDurationMs == 0)
            {
                Unapply(gameObject);
            }

            timePassed += timeLapsed;
        }

        public void Unapply(GameObject target)
        {
            var component = target.GetComponent<CockatricePoisonEffect>();

            component.IsDisabled = true;

            component.spriteRenderer.color = component.OriginalColor;

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