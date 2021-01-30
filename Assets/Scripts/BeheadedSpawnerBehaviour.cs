using UnityEngine;
using System.Collections.Generic;

namespace Pandora
{

    public class BeheadedSpawnerBehaviour: SpawnerBehaviour {
        public RuntimeAnimatorController RedKing, BlueKing, RedJester, BlueJester, RedQueen, BlueQueen;

        override public List<GameObject> Spawn(MapComponent map, UnitSpawn spawn) {
            var units = base.Spawn(map, spawn);

            units[0].GetComponent<UnitBehaviour>().RedController = RedKing;
            units[0].GetComponent<UnitBehaviour>().BlueController = BlueKing;
            units[0].GetComponent<UnitBehaviour>().SetupAnimationControllers();

            units[1].GetComponent<UnitBehaviour>().RedController = RedJester;
            units[1].GetComponent<UnitBehaviour>().BlueController = BlueJester;
            units[1].GetComponent<UnitBehaviour>().SetupAnimationControllers();

            units[2].GetComponent<UnitBehaviour>().RedController = RedQueen;
            units[2].GetComponent<UnitBehaviour>().BlueController = BlueQueen;
            units[2].GetComponent<UnitBehaviour>().SetupAnimationControllers();

            return units;
        }
    }
}