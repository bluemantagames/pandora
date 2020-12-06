using System.Collections.Generic;
using Pandora.UI;

namespace Pandora.Command {
    public interface CommandBehaviour {
        void InvokeCommand();

        List<EffectIndicator> FindTargets();
    }
}