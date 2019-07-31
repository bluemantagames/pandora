namespace CRclone {
    using CRclone.Combat;

    public class TowerTeamComponent: TeamComponent {
        public override int team { 
            get { 
                var currentTeam = TeamComponent.assignedTeam;

                return GetComponent<TowerCombatBehaviour>().isOpponent ? currentTeam + 1 : currentTeam;
            } 
            
            set {} 
         }

    }
}