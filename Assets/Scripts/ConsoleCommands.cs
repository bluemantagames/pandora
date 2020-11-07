using UnityEngine;
using System;
using QFSW.QC;

namespace Pandora {
    public class ConsoleCommands {
        [QFSW.QC.Command("spawn-unit-params")]
        public static void SpawnUnit(string unitName, Vector2Int gridCell) {
            SpawnUnit(unitName, new GridCell(gridCell), TeamComponent.assignedTeam);
        }

        [QFSW.QC.Command("spawn-enemy-unit-params")]
        public static void SpawnEnemyUnit(string unitName, Vector2Int gridCell) {
            SpawnUnit(unitName, new GridCell(gridCell), TeamComponent.opponentTeam);
        }

        static void SpawnUnit(string unitName, GridCell gridCell, int team) {
            var spawn = new UnitSpawn(unitName, gridCell, team, Guid.NewGuid().ToString(), DateTime.Now, 0);

            MapComponent.Instance.SpawnUnit(spawn);
        }

        static Vector2Int roundVector2(Vector2 vector) {
            return new Vector2Int(
                Mathf.RoundToInt(vector.x),
                Mathf.RoundToInt(vector.y)
            );
        }
    }

}