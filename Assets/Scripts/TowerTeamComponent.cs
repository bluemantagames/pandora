namespace Pandora
{
    using Pandora.Combat;

    public class TowerTeamComponent : TeamComponent
    {
        /// <summary>
        /// Engine entity team
        /// </summary>
        public int EngineTeam
        {
            get
            {
                var positionComponent = GetComponent<TowerPositionComponent>().EngineTowerPosition;
                var opposingTeam = (assignedTeam == topTeam) ? bottomTeam : topTeam;

                int team = assignedTeam;

                if ((team == topTeam && positionComponent.IsBottom()) || (team == bottomTeam && positionComponent.IsTop()))
                {
                    team = opposingTeam;
                }

                return team;
            }

            set { }
        }


        /// <summary>
        /// Rendered-world tower team
        /// </summary>
        public override int Team 
        {
            get
            {
                var positionComponent = GetComponent<TowerPositionComponent>().WorldTowerPosition;
                var opposingTeam = (assignedTeam == topTeam) ? bottomTeam : topTeam;

                int team = assignedTeam;

                if (positionComponent.IsTop())
                {
                    team = opposingTeam;
                }

                return team;
            }

            set { }
        }


        public override bool IsOpponent() {
            return EngineTeam != assignedTeam;
        }
    }
}