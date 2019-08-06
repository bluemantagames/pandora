namespace Pandora
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TeamComponent : MonoBehaviour
    {
        // Team assigned by the server
        static public int assignedTeam = 1;
        public virtual int team { get; set; }

        public bool IsOpponent() {
            return team != assignedTeam;
        }
    }
}