using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Priority_Queue;
using Pandora;
using Pandora.Combat;
using UnityEngine.Profiling;
using Pandora.Engine;
using Pandora.Pool;

namespace Pandora.Movement
{
    public interface MovementBehaviour {
        MapComponent map { set; }
        MovementStateEnum LastState { set; }

        MovementState Move();
    }
}