using UnityEngine;
using System.Collections.Generic;
using Pandora.Movement;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Pandora.Engine.Animations
{
    public class AnimationTesterComponent : MonoBehaviour
    {
        public string UnitName;
        public int TrackStartX = 5;
        public int TrackStartY = 4;
        public int TrackLength = 5;
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

            if (isSpawned && unitEntity != null)
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
            var entity = entities[entities.Count - 1];

            entity.SetEmptyPath();
            entity.SetTarget(new GridCell(TrackStartX, TrackStartY + TrackLength));

            return entity;
        }
    }
}