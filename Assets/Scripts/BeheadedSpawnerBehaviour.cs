using UnityEngine;
using System.Collections.Generic;

namespace Pandora
{

    public class BeheadedSpawnerBehaviour : SpawnerBehaviour
    {
        public RuntimeAnimatorController RedKing, BlueKing, RedJester, BlueJester, RedQueen, BlueQueen;

        override public List<GameObject> Spawn(MapComponent map, UnitSpawn spawn)
        {
            var units = base.Spawn(map, spawn);

            units[0].GetComponent<ArenaEntityBehaviour>().RedController = RedKing;
            units[0].GetComponent<ArenaEntityBehaviour>().BlueController = BlueKing;
            units[0].GetComponent<ArenaEntityBehaviour>().SetupAnimationControllers();

            units[1].GetComponent<ArenaEntityBehaviour>().RedController = RedJester;
            units[1].GetComponent<ArenaEntityBehaviour>().BlueController = BlueJester;
            units[1].GetComponent<ArenaEntityBehaviour>().SetupAnimationControllers();

            units[2].GetComponent<ArenaEntityBehaviour>().RedController = RedQueen;
            units[2].GetComponent<ArenaEntityBehaviour>().BlueController = BlueQueen;
            units[2].GetComponent<ArenaEntityBehaviour>().SetupAnimationControllers();

            return units;
        }
    }
}