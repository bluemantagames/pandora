namespace Pandora
{
    using System.Collections;
    using System.Collections.Generic;
    using Pandora.Command;
    using UnityEngine;
    using Pandora.Network;
    using Pandora.Network.Data;
    using Pandora.Combat;

    public class TeamComponent : MonoBehaviour
    {
        static GameObject _teamGameObject;

        static public TeamComponent assignedTeamComponent {
            get {
                if (_teamGameObject == null) {
                    _teamGameObject = new GameObject();

                    _teamGameObject.AddComponent<TeamComponent>();

                    var teamComponent = _teamGameObject.GetComponent<TeamComponent>();

                    teamComponent.Team = assignedTeam;

                    return teamComponent;
                } else {
                    return _teamGameObject.GetComponent<TeamComponent>();
                }
            }
        }

        public static Opponent Opponent = null;

        static public void Reset() {
            Opponent = null;
        }



        /// <summary>
        /// Team assigned to this client by the server. 
        /// 
        /// 1 by default (e.g. when client is not connected)
        ///</summary>
        static public int assignedTeam = 1;

        static public int opponentTeam {
            get => (assignedTeam == bottomTeam) ? topTeam : bottomTeam;
        }

        /// <summary>Team that occupies the top of the map</summary>
        static public int topTeam = 2;

        /// <summary>Team that occupies the bottom of the map</summary>
        static public int bottomTeam = 1;

        /// <summary>Team for this object</summary>
        public virtual int Team { get; set; }

        public virtual bool IsOpponent()
        {
            return Team != assignedTeam;
        }

        public bool IsTop()
        {
            return Team == topTeam;
        }

        public bool IsBottom()
        {
            return Team == bottomTeam;
        }

        public void Convert(int newTeam)
        {
            // cannot convert towers
            if (GetComponent<TowerPositionComponent>() != null) return;

            if (!GetComponentInChildren<CommandListener>().Used)
            {
                var id =
                    GetComponent<GroupComponent>()?.OriginalId ??
                    GetComponent<UnitIdComponent>()?.Id;

                var name = GetComponent<UnitIdComponent>().UnitName;

                if (Team == TeamComponent.assignedTeam)
                {
                    CommandViewportBehaviour.Instance.RemoveCommand(id);
                }
                else
                {
                    CommandViewportBehaviour.Instance.AddCommand(name, id);
                }

            }

            Team = newTeam;

            GetComponentInChildren<HealthbarBehaviour>()?.RefreshColor();

            var combatBehaviour = GetComponent<CombatBehaviour>();

            if (combatBehaviour.isAttacking)
                combatBehaviour.StopAttacking();
        }
    }
}