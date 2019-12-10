using UnityEngine;
using System.Collections.Generic;

namespace Pandora
{
    public class SpawnerBehaviour : MonoBehaviour, Spawner
    {
        public GameObject Unit;
        public Vector2Int[] Positions;

        public Vector2Int[] CellPositions {
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
                var id = $"{spawn.Id}-{unitNumber}";

                unit.name += $"-{id}";

                var timestamp = spawn.Timestamp?.AddSeconds(unitNumber);

                map.InitializeComponents(unit, new GridCell(spawn.CellX + position.x, spawn.CellY + position.y), spawn.Team, id, timestamp);

                units.Add(unit);

                var groupComponent = unit.AddComponent<GroupComponent>();

                groupComponent.Objects = units;
                groupComponent.AliveObjects = new List<GameObject>(units);
                groupComponent.OriginalId = spawn.Id;

                unitNumber++;
            }

            return units;
        }
    }
}