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

namespace Pandora.AI
{
    public interface EntityController
    {
        MapComponent map { set; }
        MovementStateEnum LastState { set; }
        int Speed { get; }

        MovementState Move();
        void ResetPath();

        Vector2 WalkingDirection { get; }
    }
}