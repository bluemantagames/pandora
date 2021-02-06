using System.Collections.Generic;
using UnityEngine;

namespace Pandora{
    public class GroupComponent: MonoBehaviour {
        public List<GameObject> Objects;

        // This instance is shared between all the GroupComponents
        public List<GameObject> AliveObjects;
        public string OriginalId;

        public bool IsEveryoneDead() => AliveObjects.Count == 0;
    }
}