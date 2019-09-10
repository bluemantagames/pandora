using UnityEngine;
using System.Collections.Generic;

namespace Pandora
{
    public class HarpiesSpawner : MonoBehaviour, Spawner
    {
        public GameObject Harpy;

        public List<GameObject> Spawn(MapComponent map, UnitSpawn spawn)
        {
            var units = new List<GameObject> { };
            var yIncrement = (spawn.Team == TeamComponent.topTeam) ? -1 : 1;

            var firstHarpy = Instantiate(Harpy, map.gameObject.transform);

            map.InitializeComponents(firstHarpy, new GridCell(spawn.CellX, spawn.CellY), spawn.Team, $"{spawn.Id}-1", spawn.Timestamp);
            units.Add(firstHarpy);

            firstHarpy.AddComponent<GroupComponent>().Objects = units;

            var secondHarpy = Instantiate(Harpy, map.gameObject.transform);

            map.InitializeComponents(secondHarpy, new GridCell(spawn.CellX + 1, spawn.CellY + yIncrement), spawn.Team, $"{spawn.Id}-2", spawn.Timestamp);
            units.Add(secondHarpy);

            secondHarpy.AddComponent<GroupComponent>().Objects = units;

            var thirdHarpy = Instantiate(Harpy, map.gameObject.transform);

            map.InitializeComponents(thirdHarpy, new GridCell(spawn.CellX - 1, spawn.CellY + yIncrement), spawn.Team, $"{spawn.Id}-3", spawn.Timestamp);
            units.Add(thirdHarpy);

            thirdHarpy.AddComponent<GroupComponent>().Objects = units;

            return units;
        }
    }
}