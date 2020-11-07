using UnityEngine;
using System;
using QFSW.QC;

namespace Pandora {
    public class ConsoleCommands {
        [QFSW.QC.Command("spawn-unit-params")]
        [CommandDescription("Spawns an allied unit named <unitName> in the cell <gridCell> - Example: spawn-unit-params \"HalfOrc\" 14,7")]
        public static void SpawnUnit(string unitName, Vector2Int gridCell) {
            SpawnUnit(unitName, new GridCell(gridCell), TeamComponent.assignedTeam);
        }

        [QFSW.QC.Command("spawn-enemy-unit-params")]
        [CommandDescription("Spawns an allied unit named <unitName> in the cell <gridCell> - Example: spawn-enemy-unit-params \"HalfOrc\" 14,7. BEWARE! Y position is flipped, since you are spawning _as if_ you were your opponent")]
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