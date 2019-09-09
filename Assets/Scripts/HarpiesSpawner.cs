using UnityEngine;
using System.Collections.Generic;

namespace Pandora {
    public class HarpiesSpawner: MonoBehaviour, Spawner {
        public GameObject Harpy;

        public List<GameObject> Spawn(MapComponent map, UnitSpawn spawn) {
            var units = new List<GameObject> {};
            var yIncrement = (spawn.Team == TeamComponent.topTeam) ? -1 : 1;

            var firstHarpy = Instantiate(Harpy);

            map.InitializeComponents(firstHarpy, new GridCell(spawn.CellX, spawn.CellY), spawn.Team, spawn.Id, spawn.Timestamp);
            units.Add(firstHarpy);

            var secondHarpy = Instantiate(Harpy);

            map.InitializeComponents(secondHarpy, new GridCell(spawn.CellX + 1, spawn.CellY + yIncrement), spawn.Team, spawn.Id + "2", spawn.Timestamp);
            units.Add(secondHarpy);

            var thirdHarpy = Instantiate(Harpy);

            map.InitializeComponents(thirdHarpy, new GridCell(spawn.CellX - 1, spawn.CellY + yIncrement), spawn.Team, spawn.Id + "1", spawn.Timestamp);
            units.Add(thirdHarpy);

            return units;
        }
    }
}