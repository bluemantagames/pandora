using UnityEngine;
using Pandora.Network;
using Pandora.Engine;
using Pandora.Combat;

namespace Pandora.Command
{
    /// <summary>On double tap, all the dead zombies will resuscitate</summary>
    public class ZombieCommand : MonoBehaviour, CommandBehaviour
    {

        public void InvokeCommand()
        {
            Logger.Debug("[Zombie] Command invoked");
            
            var sourceEntity = GetComponent<EngineComponent>().Entity;
            var team = GetComponent<TeamComponent>().team;
            var groupComponent = GetComponent<GroupComponent>();

            if (groupComponent == null) {
                return;
            }

            foreach (var zombie in groupComponent.Objects) {
                var commandListener = zombie.GetComponentInChildren<CommandListener>();
                var zombieId = zombie.GetComponent<UnitIdComponent>();
                var lifeComponent = zombie.GetComponent<LifeComponent>();

                // Disable other commands
                if (commandListener != null) {
                    commandListener.Used = true;
                }

                if (lifeComponent.DeathPosition == null) {
                    continue;
                }

                var spawnPosition = lifeComponent.DeathPosition;

                if (team == TeamComponent.topTeam)
                    spawnPosition = MapComponent.Instance.Flip(spawnPosition);

                var zombieSpawn = new UnitSpawn(
                    "Zombie",
                    spawnPosition,
                    team,
                    zombieId + "-resurrected",
                    sourceEntity.Timestamp
                );

                MapComponent.Instance.SpawnUnit(zombieSpawn);
            }
        }
    }
}