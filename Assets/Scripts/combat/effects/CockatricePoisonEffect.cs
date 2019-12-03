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

                component.RefreshComponents();
            }

            return component;
        }

        void Start()
        {
            spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();

            OriginalColor = spriteRenderer.color;

            spriteRenderer.color = Color.green;
        }

        public void TickUpdate(uint timeLapsed)
        {
            if (IsDisabled) return;

            timePassed += timeLapsed;

            if (timePassed % PoisonTickMs == 0)
            {
                var lifeComponent = gameObject.GetComponent<LifeComponent>();

                lifeComponent.AssignDamage((int)DamagePerTick);

                if (lifeComponent.IsDead) Unapply(gameObject);
            }

            if (timePassed % PoisonDurationMs == 0)
            {
                Unapply(gameObject);
            }
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