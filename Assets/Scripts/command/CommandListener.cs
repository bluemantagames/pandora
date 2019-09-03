using UnityEngine;

namespace Pandora.Command
{
    public class CommandListener : MonoBehaviour
    {
        public uint DoubleTapMaxDelayMs = 200;

        float? lastTapMs = null;

        void OnMouseDown()
        {
            var tapTime = Time.time * 1000;
            var elapsed = tapTime - lastTapMs;

            if (elapsed == null || elapsed.Value > DoubleTapMaxDelayMs)
            {
                lastTapMs = tapTime;

                return;
            }

            var commandComponent = GetComponent<CommandBehaviour>();

            if (commandComponent != null)
            {
                commandComponent.InvokeCommand();
            }
            else
            {
                Debug.LogWarning($"Could not find command behaviour for game object {gameObject.name}");
            }
        }

    }
}