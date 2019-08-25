namespace Pandora
{
    using Pandora.Combat;

    public class TowerTeamComponent : TeamComponent
    {
        public int engineTeam // this represents the engine entity team
        {
            get
            {
                var positionComponent = GetComponent<TowerPositionComponent>().towerPosition;
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

        public override int team // this represents the rendered-world tower team
        {
            get
            {
                var positionComponent = GetComponent<TowerPositionComponent>().towerPosition;
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
    }
}