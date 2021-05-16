using UnityEngine;
using Pandora.Combat;
using System.Collections.Generic;
using Pandora.AI;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Pandora.Engine.Animations
{
    public enum TestMode
    {
        Movement,
        Attack
    }

    public class AnimationTesterComponent : MonoBehaviour
    {
        public string UnitName;
        public int UnitX = 9;
        public int UnitY = 5;
        public int MovementLength = 5;
        public int EnemyX = 8;
        public int EnemyY = 12;
        public TestMode Mode = TestMode.Movement;
        public bool Disabled = false;
        public bool DisableAttack = false;
        public bool IsEnemyInvulnerable = true;

        bool isSpawned = false;
        bool isTurnedBack = false;
        MapComponent mapComponent = null;
        EngineEntity unitEntity = null;
        string logPrefix = "[AnimationTesterComponent]";

        void Awake()
        {
            // This should never be enabled in production
            Disabled = !Disabled ? !Debug.isDebugBuild : true;
        }

        void Start()
        {
            mapComponent = GetComponent<MapComponent>();
        }

        void Update()
        {
            if (mapComponent == null || Disabled) return;

            if (!isSpawned)
            {
                var unit = SpawnUnit();

                unitEntity = unit;
                isSpawned = unit != null;
            }

            if (isSpawned && unitEntity != null && Mode == TestMode.Movement)
            {
                var currentCell = unitEntity.GetCurrentCell();

                if (currentCell.vector.y >= (UnitY + MovementLength) && !isTurnedBack)
                {
                    Logger.Debug($"{logPrefix} Turning back...");
                    unitEntity.SetTarget(new GridCell(UnitX, UnitY));

                    isTurnedBack = true;
                }

                if (currentCell.vector.y <= UnitY && isTurnedBack)
                {
                    Logger.Debug($"{logPrefix} Turning back...");
                    unitEntity.SetTarget(new GridCell(UnitX, UnitY + MovementLength));

                    isTurnedBack = false;
                }
            }
        }

        private EngineEntity SpawnUnit()
        {
            Logger.Debug($"{logPrefix} Spawning unit: {UnitName}");

            mapComponent.SpawnCard(UnitName, 1, new GridCell(UnitX, UnitY));

            var entities = mapComponent.engine.Entities;
            var unitEntity = entities[entities.Count - 1];
            var unitLifeComponent = unitEntity.GameObject.GetComponent<LifeComponent>();

            unitLifeComponent.DisableDamage = true;

            if (Mode == TestMode.Movement)
            {
                unitEntity.SetTarget(new GridCell(UnitX, UnitY + MovementLength));
            }
            else if (Mode == TestMode.Attack)
            {
                // Spawn the enemy
                mapComponent.SpawnCard(UnitName, 2, new GridCell(EnemyX, EnemyY));
                var enemyEntity = entities[entities.Count - 1];

                var unitCombatBehaviour = unitEntity.GameObject.GetComponent<CombatBehaviour>();
                var enemyCombatBehaviour = enemyEntity.GameObject.GetComponent<CombatBehaviour>();
                var enemyLifeComponent = enemyEntity.GameObject.GetComponent<LifeComponent>();

                if (IsEnemyInvulnerable)
                    enemyLifeComponent.DisableDamage = true;

                enemyEntity.IsMovementPaused = true;
                unitEntity.IsMovementPaused = true;

                unitCombatBehaviour.IsDisabled = DisableAttack;
                enemyCombatBehaviour.IsDisabled = true;
            }

            return unitEntity;
        }
    }
}