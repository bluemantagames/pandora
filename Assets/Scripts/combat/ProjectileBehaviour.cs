using UnityEngine;
using Pandora;

namespace Pandora.Combat {
    public interface ProjectileBehaviour {
        Enemy target { get; set; }
        GameObject parent { get; set; }
        GameObject originalPrefab { get; set; }
        MapComponent map { set; }

        int Speed { get; }

        void Init();
    }
}