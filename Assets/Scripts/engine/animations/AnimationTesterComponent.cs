using UnityEngine;
using Pandora.Combat;
using System.Collections.Generic;
using Pandora.Movement;
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
        public int TrackStartX = 5;
        public int TrackStartY = 4;
        public int TrackLength = 5;
        public int EnemyY = 12;
        public TestMode Mode = TestMode.Movement;
        public bool Disabled = false;

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
                isSpawned = true;
            }

            if (isSpawned && unitEntity != null && Mode == TestMode.Movement)
            {
                var currentCell = unitEntity.GetCurrentCell();

                if (currentCell.vector.y >= (TrackStartY + TrackLength) && !isTurnedBack)
                {
                    Logger.Debug($"{logPrefix} Turning back...");
                    unitEntity.SetTarget(new GridCell(TrackStartX, TrackStartY));

                    isTurnedBack = true;
                }

                if (currentCell.vector.y <= TrackStartY && isTurnedBack)
                {
                    Logger.Debug($"{logPrefix} Turning back...");
                    unitEntity.SetTarget(new GridCell(TrackStartX, TrackStartY + TrackLength));

                    isTurnedBack = false;
                }
            }
        }

        private EngineEntity SpawnUnit()
        {
            if (mapComponent == null) return null;

            Logger.Debug($"{logPrefix} Spawning unit: {UnitName}");

            mapComponent.SpawnCard(UnitName, 1, new GridCell(TrackStartX, TrackStartY));

            var entities = mapComponent.engine.Entities;
            var unitEntity = entities[entities.Count - 1];
            var combatBehaviour = unitEntity.GameObject?.GetComponent<RangedCombatBehaviour>();

            // entity.IsMovementPaused = true;

            // var enemy = new Enemy(entities[3].GameObject);
            // enemy.IsTower = true;
            // combatBehaviour.AttackEnemy(enemy, 10);

            if (Mode == TestMode.Movement)
            {
                unitEntity.SetTarget(new GridCell(TrackStartX, TrackStartY + TrackLength));
            }
            else if (Mode == TestMode.Attack)
            {
                // Spawn the enemy
                mapComponent.SpawnCard(UnitName, 2, new GridCell(TrackStartX, EnemyY));
                var enemyEntity = entities[entities.Count - 1];

                var unitCombatBehaviour = unitEntity.GameObject.GetComponent<CombatBehaviour>();
                var enemyCombatBehaviour = enemyEntity.GameObject.GetComponent<CombatBehaviour>();

                enemyEntity.IsMovementPaused = true;
                unitEntity.IsMovementPaused = true;

                unitCombatBehaviour.ChangeDamage(0);
                enemyCombatBehaviour.ChangeDamage(0);
                enemyCombatBehaviour.IsDisabled = true;
            }

            return unitEntity;
        }
    }
}