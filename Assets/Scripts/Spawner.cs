using System.Collections.Generic;
using UnityEngine;

namespace Pandora
{
    public interface Spawner
    {
        List<GameObject> Spawn(MapComponent map, UnitSpawn spawn);
    }
}