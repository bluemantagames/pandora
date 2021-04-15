using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pandora.Network;

namespace Pandora
{
    [System.Serializable]
    public class UnitDamageLog
    {
        public string UnitName;
        public int Team;
        public int Damage;
    }

    [System.Serializable]
    public class SerializableTowersDamages
    {
        public TowerPosition Tower;
        public List<UnitDamageLog> CardsDamages;
    }

    [System.Serializable]
    public class SerializableTowersDamagesCollection
    {
        public List<SerializableTowersDamages> Damages;
    }

    [System.Serializable]
    public class SerializableMulliganCards
    {
        public List<string> MulliganCards;
    }

    public class MatchInfoSingleton
    {
        static MatchInfoSingleton _instance = null;

        Dictionary<TowerPosition, Dictionary<string, UnitDamageLog>> towersDamages =
            new Dictionary<TowerPosition, Dictionary<string, UnitDamageLog>>();

        List<string> mulliganCards = null;

        static public MatchInfoSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MatchInfoSingleton();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Log the unit damate to a specific tower.
        /// </summary>
        public void AddUnitTowerDamage(TowerPosition towerPosition, GameObject unit, int damage)
        {
            var unitIdComponent = unit.GetComponent<UnitIdComponent>();
            var teamComponent = unit.GetComponent<TeamComponent>();

            if (!towersDamages.ContainsKey(towerPosition))
            {
                towersDamages[towerPosition] = new Dictionary<string, UnitDamageLog>();
            }

            if (!towersDamages[towerPosition].ContainsKey(unitIdComponent.Id))
            {
                towersDamages[towerPosition][unitIdComponent.Id] = new UnitDamageLog
                {
                    UnitName = unitIdComponent.UnitName,
                    Damage = damage,
                    Team = teamComponent.Team
                };
            }
            else
            {
                towersDamages[towerPosition][unitIdComponent.Id].Damage += damage;
            }

            Logger.Debug($"Updated towers damages structure: {JsonUtility.ToJson(GetSerializableTowersDamages())}");
        }

        /// <summary>
        /// Clear the tower damages cache.
        /// </summary>
        public void ClearTowersDamages()
        {
            towersDamages.Clear();
        }

        /// <summary>
        /// Retrieve the towers damages structure.
        /// </summary>
        public Dictionary<TowerPosition, Dictionary<string, UnitDamageLog>> RetrieveTowersDamages()
        {
            return towersDamages;
        }

        /// <summary>
        /// Return a serializable Towers Damage structure.
        /// </summary>
        public SerializableTowersDamagesCollection GetSerializableTowersDamages()
        {
            List<SerializableTowersDamages> serializableStructure = new List<SerializableTowersDamages>();

            foreach (var entry in towersDamages)
            {
                var towerStruct = new SerializableTowersDamages
                {
                    Tower = entry.Key,
                    CardsDamages = entry.Value.Values.ToList()
                };

                serializableStructure.Add(towerStruct);
            }

            return new SerializableTowersDamagesCollection
            {
                Damages = serializableStructure
            };
        }

        /// <summary>
        /// Add the mulligan cards.
        /// </summary>
        public void AddMulliganCards(List<string> cards)
        {
            mulliganCards = cards;
        }

        /// <summary>
        /// Clear the mulligan cards.
        /// </summary>
        public void ClearMulliganCards()
        {
            mulliganCards?.Clear();
        }

        /// <summary>
        /// Return a serializable Mulligan Cards structure.
        /// </summary>
        public SerializableMulliganCards GetSerializableMulliganCards()
        {
            return new SerializableMulliganCards
            {
                MulliganCards = mulliganCards
            };
        }
    }
}