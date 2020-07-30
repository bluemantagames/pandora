using System;
using Pandora.Network.Messages;

namespace Pandora
{
    public class UnitSpawn: ICloneable 
    {
        public string UnitName;
        public int CellX;
        public int CellY;
        public int Team;
        public string Id;
        public DateTime Timestamp;
        public int ManaUsed;

        public UnitSpawn(SpawnMessage message) {
            UnitName = message.unitName;
            CellX = message.cellX;
            CellY = message.cellY;
            Team = message.team;
            Id = message.unitId;
            Timestamp = message.timestamp;
            ManaUsed = message.manaUsed;
        }

        public UnitSpawn(string unitName, GridCell cell, int team, string unitId, DateTime timestamp, int manaUsed) {
            UnitName = unitName;
            CellX = cell.vector.x;
            CellY = cell.vector.y;
            Team = team;
            Id = unitId;
            Timestamp = timestamp;
            ManaUsed = manaUsed;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}