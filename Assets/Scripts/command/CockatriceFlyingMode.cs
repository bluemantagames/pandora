using UnityEngine;
using System.Linq;
using Pandora.Combat;
using Pandora.Engine;
using Pandora.AI;

namespace Pandora.Command
{
    /// <summary>
    /// On command, Cockatrice starts flying and deals huge amounts of damage to the nearest enemy.
    ///
    /// It keeps flying for X seconds
    /// </summary>
    public class CockatriceFlyingMode : MonoBehaviour, EngineBehaviour
    {
        public string ComponentName => "CockatriceFlyingMode";

        public int FlyingTimeMs;

        uint totalTimeLapsed = 0;

        bool IsDisabled = false;

        public void TickUpdate(uint timeLapsed)
        {
            if (IsDisabled) return;

            totalTimeLapsed += timeLapsed;

            if (totalTimeLapsed >= FlyingTimeMs)
            {
                gameObject.layer = Constants.GROUND_LAYER;

                IsDisabled = true;
            }
        }
    }
}