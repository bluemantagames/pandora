﻿using UnityEngine;
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
        Color originalColor;
        uint timePassed = 0;
        public uint TickMs = 1, DurationMs = 3000;
        public int MovementSpeedIncrease = 400;
        public int AttackSpeedIncrease = 200;
        
        public int DamageIncrease = 10;
        public int CureAmount = 500;
        public Color BuffedColor = Color.yellow;
        public string ComponentName => "BardBuff";

        void SetStats(GameObject target, int movSpeedIncrease, int atkSpeedIncrease, int damageIncrease) {
            var meleeCombatComponent = target.GetComponent<MeleeCombatBehaviour>();
            var rangedCombarComponent = target.GetComponent<RangedCombatBehaviour>();
            var movementComponent = target.GetComponent<MovementComponent>();

            // Change attack speed and damage
            if (meleeCombatComponent != null) {
                meleeCombatComponent.attackCooldownMs -= atkSpeedIncrease;
                meleeCombatComponent.backswingMs -= atkSpeedIncrease;

                meleeCombatComponent.damage += damageIncrease;
            }

            if (rangedCombarComponent != null) {
                rangedCombarComponent.attackCooldownMs -= atkSpeedIncrease;

                rangedCombarComponent.Damage += damageIncrease;
            }

            // Change movement speed
            if (movementComponent != null) {
                movementComponent.Speed += movSpeedIncrease;
                movementComponent.ResetPath();
            }
        }

        void Cure(GameObject target, int cureAmount) {
            var lifeComponent = target.GetComponent<LifeComponent>();
            lifeComponent.lifeValue += cureAmount;
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
            }

            return component;
        }

        void Start() {
            var rendererComponent = gameObject.GetComponentInChildren<SpriteRenderer>();
            var targetEntity = gameObject.GetComponent<EngineComponent>().Entity;
            originalColor = rendererComponent.color;

            // Add the buff
            SetStats(gameObject, MovementSpeedIncrease, AttackSpeedIncrease, DamageIncrease);
            Cure(gameObject, CureAmount);

            if (rendererComponent && !targetEntity.IsStructure) {
                originalColor = rendererComponent.color;
                rendererComponent.color = BuffedColor;
            }
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
            var rendererComponent = target.GetComponent<SpriteRenderer>();
            var targetEntity = gameObject.GetComponent<EngineComponent>().Entity;

            component.IsDisabled = true;

            // Remove the buff
            SetStats(target, -MovementSpeedIncrease, -AttackSpeedIncrease, -DamageIncrease);

            if (rendererComponent && !targetEntity.IsStructure) {
                rendererComponent.color = originalColor;
            }

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