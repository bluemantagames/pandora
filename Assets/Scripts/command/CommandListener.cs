using UnityEngine;
using Pandora.Network;
using Pandora.Network.Messages;

namespace Pandora.Command
{
    public class CommandListener : MonoBehaviour
    {
        public uint DoubleTapMaxDelayMs = 500;

        float? lastTapMs = null;
        public bool Used = false;

        void OnMouseDown()
        {
            if (Used) return;

            var tapTime = Time.time * 1000;
            var elapsed = tapTime - lastTapMs;

            if (elapsed == null || elapsed.Value > DoubleTapMaxDelayMs) // if not a double tap, return
            {
                lastTapMs = tapTime;

                return;
            }

            Command();
        }

        public void Command()
        {
            if (Used) return;

            var groupComponent = GetComponentInParent<GroupComponent>();
            var id = GetComponentInParent<UnitIdComponent>().Id;

            NetworkControllerSingleton.instance.EnqueueMessage(
                new CommandMessage
                {
                    team = TeamComponent.assignedTeam,
                    unitId = id
                }
            );

            var commandComponent = GetComponentInParent<CommandBehaviour>();

            Logger.Debug($"Calling command for {gameObject}");

            if (commandComponent != null && !NetworkControllerSingleton.instance.matchStarted)
            {
                commandComponent.InvokeCommand();
            }
            else
            {
                Logger.DebugWarning($"Could not find command behaviour for game object {gameObject.name}");
            }

            Used = true;

            if (groupComponent != null)
            {
                foreach (var gameObject in groupComponent.Objects)
                {
                    var commandListener = gameObject.GetComponentInChildren<CommandListener>();

                    Logger.Debug($"Using {gameObject} {string.Join(", ", groupComponent.Objects)}");

                    if (commandListener != null)
                    {
                        commandListener.Used = true;
                    }
                }
            }

            CommandViewportBehaviour.Instance.RemoveCommand(groupComponent?.OriginalId ?? id);
        }

    }
}