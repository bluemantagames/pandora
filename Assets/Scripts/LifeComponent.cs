﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.Engine;
using Pandora.Combat;
using Pandora.Network;
using Pandora.Resource;
using Pandora.Resource.Mana;

namespace Pandora
{

    public class LifeComponent : MonoBehaviour
    {
        public int lifeValue = 100;
        Image mask;
        float maskOriginalSize;
        public int maxLife;
        public bool IsDead = false;
        public GridCell DeathPosition = null;
        public bool DisableDamage = false;
        WalletsComponent walletsComponent;
        TeamComponent teamComponent;
        TowerTeamComponent towerTeamComponent;
        TowerPositionComponent towerPositionComponent;
        GroupComponent groupComponent;
        MatchInfoSingleton matchInfoSingleton;

        public GameObject DeathVFX;
        public float DeathVFXSpeedMultiplier = 2f;

        bool isTower;
        public int TowerGoldRewards = 10, GoldReward = 13;

        int currentGoldReward = 1;

        void Start()
        {
            var healthbarBehaviour = GetComponentInChildren<HealthbarBehaviour>();
            mask = healthbarBehaviour.gameObject.transform.parent.gameObject.GetComponent<Image>();

            healthbarBehaviour.LifeComponent = this;

            maskOriginalSize = mask.rectTransform.rect.width;
            maxLife = lifeValue;

            healthbarBehaviour.DrawSeparators();

            walletsComponent = MapComponent.Instance.GetComponent<WalletsComponent>();

            towerTeamComponent = GetComponent<TowerTeamComponent>();
            towerPositionComponent = GetComponent<TowerPositionComponent>();
            teamComponent = GetComponent<TeamComponent>();

            matchInfoSingleton = MatchInfoSingleton.Instance;

            var groupComponent = GetComponent<GroupComponent>();

            isTower = towerTeamComponent != null;
        }

        public void Remove()
        {
            GetComponent<CombatBehaviour>().StopAttacking();

            foreach (var rigidBody in GetComponentsInChildren<Rigidbody2D>())
            {
                rigidBody.simulated = false;
            }

            foreach (var sprite in GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.enabled = false;
            }

            foreach (Transform child in this.transform)
            {
                child.gameObject.SetActive(false);
            }

            var engineComponent = GetComponent<EngineComponent>();

            if (engineComponent != null)
            {
                engineComponent.Remove();
            }

            foreach (var towerCombatBehaviour in MapComponent.Instance.gameObject.GetComponentsInChildren<TowerCombatBehaviour>())
            {
                if (towerCombatBehaviour.CurrentTarget == gameObject)
                {
                    towerCombatBehaviour.StopAttacking();
                }
            }

            IsDead = true;

            Destroy(gameObject, 1000);
        }

        private void SetDeathPosition()
        {
            var sourceEntity = GetComponent<EngineComponent>().Entity;

            DeathPosition = sourceEntity.GetCurrentCell();
        }

        public void Kill(DamageSource source)
        {
            AssignDamage(lifeValue, source);
        }

        public void Heal(int amount)
        {
            lifeValue += amount;

            if (lifeValue > maxLife)
            {
                lifeValue = maxLife;
            }

            RefreshHealthbar();
        }

        public void AssignDamage(int value, DamageSource source)
        {
            if (DisableDamage) return;

            lifeValue -= value;

            RefreshHealthbar();

            if (isTower && towerTeamComponent.EngineTeam != TeamComponent.assignedTeam)
            {
                var currentRewardLifeTarget = maxLife - (currentGoldReward * (new Decimal(maxLife) / new Decimal(TowerGoldRewards)));

                if (lifeValue <= currentRewardLifeTarget)
                {
                    currentGoldReward++;

                    walletsComponent.GoldWallet.AddResource(GoldReward);
                }
            }

            if (lifeValue <= 0)
            {
                var manaCostComponent = GetComponent<ManaCostComponent>();

                var idComponent = GetComponent<UnitIdComponent>();

                var groupComponent = GetComponent<GroupComponent>();

                if (DeathVFX != null)
                {
                    var vfx = Instantiate(DeathVFX, transform.position, DeathVFX.transform.rotation);

                    multiplySpeed(vfx.GetComponent<ParticleSystem>());

                    foreach (var system in vfx.GetComponentsInChildren<ParticleSystem>())
                    {
                        multiplySpeed(system);
                    }
                }

                IsDead = true;
                GetComponent<CombatBehaviour>().OnDead();

                if (groupComponent != null)
                {
                    groupComponent.AliveObjects.RemoveAll((unit) => unit.name == gameObject.name);

                    if (groupComponent.IsEveryoneDead())
                    {
                        CommandViewportBehaviour.Instance.RemoveCommand(groupComponent.OriginalId);
                    }
                }
                else if (idComponent != null)
                {
                    CommandViewportBehaviour.Instance.RemoveCommand(idComponent.Id);
                }

                SetDeathPosition();
                Remove();

                var deathCallback = GetComponent<DeathCallback>();

                if (deathCallback != null) deathCallback.OnDeath();

                // Check if the component is a middle tower
                var towerPositionComponent = GetComponent<TowerPositionComponent>();
                var engineComponent = GetComponentInParent<EngineComponent>();

                if (towerPositionComponent != null && engineComponent != null)
                {
                    switch (towerPositionComponent.WorldTowerPosition)
                    {
                        case TowerPosition.BottomMiddle:
                            EndGameSingleton.Instance.SetWinner(
                                TeamComponent.assignedTeam == 1 ? 2 : 1,
                                engineComponent.Engine.TotalElapsed
                            );

                            break;

                        case TowerPosition.TopMiddle:
                            EndGameSingleton.Instance.SetWinner(
                                TeamComponent.assignedTeam,
                                engineComponent.Engine.TotalElapsed
                            );
                            break;
                    }
                }

                // Remove mana reserve
                var manaReservedBehaviour = GetComponent<ManaCostsBehaviour>();

                if (manaReservedBehaviour != null)
                {
                    ManaSingleton.Instance.RemoveManaUpperReserve(idComponent.Id);
                }

                // TODO: Play "die" animation
            }

            // Tower damage analytics
            if (isTower && towerPositionComponent != null)
            {
                matchInfoSingleton.AddUnitTowerDamage(
                    towerPositionComponent.WorldTowerPosition,
                    source.GameObject,
                    value
                );
            }
        }

        void RefreshHealthbar()
        {
            float lifePercent = (float)lifeValue / (float)maxLife;

            mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lifePercent * maskOriginalSize);
        }

        void multiplySpeed(ParticleSystem system)
        {
            system.Stop();

            var main = system.main;

            main.simulationSpeed *= DeathVFXSpeedMultiplier;

            system.Play();
        }

    }

}