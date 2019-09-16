using UnityEngine;
using Pandora.Network;
using Pandora.Network.Messages;

namespace Pandora.Command
{
    public class CommandListener : MonoBehaviour
    {
        public uint DoubleTapMaxDelayMs = 200;

        float? lastTapMs = null;
        bool used = false;
        GroupComponent groupComponent;

        void Awake() {
            groupComponent = GetComponentInParent<GroupComponent>();
        }

        void OnMouseDown()
        {
            var usedCommand = groupComponent?.CommandInvoked ?? used;

            if (usedCommand) return;

            var tapTime = Time.time * 1000;
            var elapsed = tapTime - lastTapMs;

            if (elapsed == null || elapsed.Value > DoubleTapMaxDelayMs) // if not a double tap, return
            {
                lastTapMs = tapTime;

                return;
            }

            var id = GetComponentInParent<UnitIdComponent>().Id;
            
            NetworkControllerSingleton.instance.EnqueueMessage(
                new CommandMessage
                {
                    team = TeamComponent.assignedTeam,
                    unitId = id
                }
            );

            var commandComponent = GetComponentInParent<CommandBehaviour>();

            if (commandComponent != null && !NetworkControllerSingleton.instance.matchStarted)
            {
                commandComponent.InvokeCommand();
            }
            else
            {
                Debug.LogWarning($"Could not find command behaviour for game object {gameObject.name}");
            }

            used = true;

            if (groupComponent != null) {
                groupComponent.CommandInvoked = true;
            }
        }

    }
}