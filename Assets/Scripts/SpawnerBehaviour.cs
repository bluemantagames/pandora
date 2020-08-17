using UnityEngine;
using System.Collections.Generic;

namespace Pandora
{
    public class SpawnerBehaviour : MonoBehaviour, Spawner
    {
        public GameObject Unit;
        public Vector2Int[] Positions;

        public Vector2Int[] CellPositions
        {
            get => Positions;
        }

        public List<GameObject> Spawn(MapComponent map, UnitSpawn spawn)
        {
            var units = new List<GameObject> { };
            var yIncrement = (spawn.Team == TeamComponent.topTeam) ? -1 : 1;
            var unitNumber = 1;


            foreach (var position in Positions)
            {
                var unit = Instantiate(Unit, map.gameObject.transform);
                var singleUnitSpawn = spawn.Clone() as UnitSpawn;

                var id = $"{spawn.Id}-{unitNumber}";

                singleUnitSpawn.Id = id;
                unit.name += $"-{id}";

                singleUnitSpawn.Timestamp = singleUnitSpawn.Timestamp.AddSeconds(unitNumber);

                map.InitializeComponents(unit, new GridCell(spawn.CellX + position.x, spawn.CellY + position.y), singleUnitSpawn);

                units.Add(unit);

                var groupComponent = unit.AddComponent<GroupComponent>();

                groupComponent.Objects = units;
                groupComponent.OriginalId = spawn.Id;

                unitNumber++;
            }

            var unitList = new List<GameObject>(units);

            foreach (var unit in units)
            {
                unit.GetComponent<GroupComponent>().AliveObjects = unitList;
            }

            return units;
        }
    }
}