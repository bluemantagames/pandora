using System.Collections.Generic;
using UnityEngine;
using Pandora.Network;

namespace Pandora
{
    public class UnitDamageLog
    {
        public string UnitName;
        public int Team;
        public int Damage;
    }

    public class MatchInfoSingleton
    {
        static MatchInfoSingleton _instance = null;

        Dictionary<TowerPosition, Dictionary<string, UnitDamageLog>> towersDamages =
            new Dictionary<TowerPosition, Dictionary<string, UnitDamageLog>>();

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

            if (towersDamages[towerPosition] == null)
            {
                towersDamages[towerPosition] = new Dictionary<string, UnitDamageLog>();
            }

            if (towersDamages[towerPosition][unitIdComponent.Id] == null)
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
    }
}