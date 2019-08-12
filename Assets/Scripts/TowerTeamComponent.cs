namespace Pandora
{
    using Pandora.Combat;

    public class TowerTeamComponent : TeamComponent
    {
        public override int team
        {
            get
            {
                var currentTeam = TeamComponent.assignedTeam;

                return GetComponent<TowerCombatBehaviour>().isOpponent ? currentTeam + 1 : currentTeam;
            }

            set { }
        }

        public override bool IsOpponent()
        {
            return GetComponent<TowerCombatBehaviour>().isOpponent;
        }

    }
}