using UnityEngine;
using Pandora;

namespace Pandora.Command
{
    public class ZombieDeathCallback : MonoBehaviour, DeathCallback
    {
        public GameObject DeadZombie;
        GameObject deadZombieInstance;

        public void OnDeath()
        {
            var groupComponent = GetComponent<GroupComponent>();

            if (groupComponent == null) return;

            if (groupComponent.IsEveryoneDead())
            {
                foreach (var zombie in groupComponent.Objects)
                {
                    zombie.GetComponent<ZombieDeathCallback>()?.RemoveDeadZombie();
                }
            }
            else
            {
                deadZombieInstance = Instantiate(DeadZombie, transform.position, DeadZombie.transform.rotation);

                deadZombieInstance.GetComponent<DeadZombieBehaviour>().RefreshColor(GetComponent<TeamComponent>());
            }
        }

        public void RemoveDeadZombie()
        {
            if (deadZombieInstance != null)
            {
                deadZombieInstance.GetComponent<SpriteRenderer>().enabled = false;

                Destroy(deadZombieInstance);
            }
        }

        public void OnRespawn() {
            deadZombieInstance?.GetComponent<DeadZombieBehaviour>().PlayVFX();

            RemoveDeadZombie();
        }
    }
}