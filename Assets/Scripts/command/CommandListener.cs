using UnityEngine;
using Pandora.Network;
using Pandora.Network.Messages;

namespace Pandora.Command
{
    public class CommandListener : MonoBehaviour
    {
        public uint DoubleTapMaxDelayMs = 200;

        float? lastTapMs = null;
        public bool Used = false;

        void OnMouseDown()
        {
            if (Used) return;

            var groupComponent = GetComponentInParent<GroupComponent>();

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

            Used = true;

            if (groupComponent != null) {
                foreach (var gameObject in groupComponent.Objects) {
                    Debug.Log($"Using {gameObject} {string.Join(", ", groupComponent.Objects)}");

                    gameObject.GetComponentInChildren<CommandListener>().Used = true;
                }
            }
        }

    }
}