using UnityEngine;
using Pandora.Network;
using Pandora.Engine;
using Pandora.Combat;
using Pandora.Resource.Mana;
using System.Collections.Generic;

namespace Pandora.Command
{
    /// <summary>On double tap, all the dead zombies resuscitate after a set delay</summary>
    public class ZombieCommand : MonoBehaviour, CommandBehaviour
    {
        public long delayMs = 1000;

        List<GameObject> puppets = new List<GameObject>(30);

        public GameObject ZombiePuppet;



        public void InvokeCommand()
        {
            var sourceEntity = GetComponent<EngineComponent>().Entity;
            var engine = sourceEntity.Engine;
            var team = GetComponent<TeamComponent>().Team;
            var groupComponent = GetComponent<GroupComponent>();

            engine.DelayedJobs.Add(
                (delayMs, Respawn)
            );

            foreach (var zombie in groupComponent.Objects)
            {
                var commandListener = zombie.GetComponentInChildren<CommandListener>();
                var zombieId = zombie.GetComponent<UnitIdComponent>();
                var lifeComponent = zombie.GetComponent<LifeComponent>();

                var deathPosition = lifeComponent.DeathPosition;

                if (deathPosition == null)
                {
                    continue;
                }

                if (TeamComponent.assignedTeam == TeamComponent.topTeam)
                    deathPosition = MapComponent.Instance.Flip(deathPosition);

                var puppetPosition = MapComponent.Instance.GridCellToWorldPosition(deathPosition);

                puppets.Add(Instantiate(ZombiePuppet, puppetPosition, Quaternion.identity));
            }
        }

        public void Respawn()
        {
            Logger.Debug("[Zombie] Command invoked");

            foreach (var puppet in puppets)
            {
                Destroy(puppet);
            }

            var sourceEntity = GetComponent<EngineComponent>().Entity;
            var team = GetComponent<TeamComponent>().Team;
            var groupComponent = GetComponent<GroupComponent>();

            if (groupComponent == null)
            {
                return;
            }

            foreach (var zombie in groupComponent.Objects)
            {
                var commandListener = zombie.GetComponentInChildren<CommandListener>();
                var zombieId = zombie.GetComponent<UnitIdComponent>();
                var lifeComponent = zombie.GetComponent<LifeComponent>();

                // Disable other commands
                if (commandListener != null)
                {
                    commandListener.Used = true;
                }

                if (lifeComponent.DeathPosition == null)
                {
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
                    sourceEntity.Timestamp,
                    zombie.GetComponent<ManaCostComponent>().ManaCost
                );

                MapComponent.Instance.SpawnUnit(zombieSpawn);
            }
        }
    }
}