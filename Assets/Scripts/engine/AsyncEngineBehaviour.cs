using System.Threading.Tasks;

namespace Pandora.Engine {
    public interface AsyncEngineBehaviour {
        /// <summary>Component name used when sorting components. <b>MUST BE UNIQUE</b></summary>
        string ComponentName { get; }

        /// <summary>Called every tick</summary>
        Task AsyncTickUpdate(uint timeLapsed);
    }
}