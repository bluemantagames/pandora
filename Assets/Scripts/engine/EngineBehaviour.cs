namespace Pandora.Engine {
    public interface EngineBehaviour {
        /// <summary>Component name used when sorting components. <b>MUST BE UNIQUE</b></summary>
        string ComponentName { get; }

        /// <summary>Called every tick</summary>
        void TickUpdate(uint timeLapsed);
    }
}