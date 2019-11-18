﻿using UnityEngine;
using Pandora.Engine;
using Pandora.Network;

namespace Pandora.Command
{
    /// <summary>On double tap, the Half Orc will split into two melee units (the rider and its mount)</summary>
    public class HalfOrcCommand : MonoBehaviour, CommandBehaviour
    {
        public void InvokeCommand()
        {
            Debug.Log("[HalfOrc] Command invoked");

            var sourceEntity = GetComponent<EngineComponent>().Entity;
            var sourceId = GetComponent<UnitIdComponent>();
            var sourceLife = GetComponent<LifeComponent>();
            var sourceTeam = GetComponent<TeamComponent>().team;
            var currentCell = sourceEntity.GetCurrentCell().vector;

            var wolfCell = new GridCell(
                new Vector2(currentCell.x + 1, currentCell.y)
            );

            var riderCell = new GridCell(
                new Vector2(currentCell.x - 1, currentCell.y)
            );

            var wolfSpawn = new UnitSpawn(
                "HalfOrcWolf",
                wolfCell,
                sourceTeam,
                sourceId.Id + "-wolf",
                sourceEntity.Timestamp.AddSeconds(-1)
            );

            var riderSpawn = new UnitSpawn(
                "HalfOrcRider",
                riderCell,
                sourceTeam,
                sourceId.Id + "-rider",
                sourceEntity.Timestamp
            );
                
            sourceLife.Remove();
            MapComponent.Instance.SpawnUnit(wolfSpawn);
            MapComponent.Instance.SpawnUnit(riderSpawn);
        }
    }
}