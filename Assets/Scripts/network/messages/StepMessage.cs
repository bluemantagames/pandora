using System.Collections.Generic;

namespace Pandora.Network.Messages {
    public class StepMessage {
        /// <summary>Time passed in ms since last step </summary>
        public uint StepTimeMs { get; private set; }
        public List<Message> Commands { get; private set; }
        public float? mana { get; private set; }

        public StepMessage(uint time, List<Message> commands, float? mana) {
            this.StepTimeMs = time;
            this.Commands = commands;
            this.mana = mana;
        }
    }
}