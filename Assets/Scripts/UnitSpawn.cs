using System;
using Pandora.Network.Messages;

namespace Pandora
{
    public class UnitSpawn
    {
        public string UnitName;
        public int CellX;
        public int CellY;
        public int Team;
        public string Id;
        public DateTime? Timestamp;

        public UnitSpawn(SpawnMessage message) {
            UnitName = message.unitName;
            CellX = message.cellX;
            CellY = message.cellY;
            Team = message.team;
            Id = message.unitId;
            Timestamp = message.timestamp;
        }
    }
}