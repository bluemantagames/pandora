using System.Collections.Generic;
using UnityEngine;

namespace Pandora{
    public class GroupComponent: MonoBehaviour {
        public List<GameObject> Objects;
        public List<GameObject> AliveObjects;
        public string OriginalId;
    }
}