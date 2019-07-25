using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CRclone.Network
{
    public class TeamTextBehaviour : MonoBehaviour
    {
        private int team;

        void Awake()
        {
            team = TeamComponent.assignedTeam;

            UpdateText();
        }

        void Update()
        {
            if (team != TeamComponent.assignedTeam) {
                team = TeamComponent.assignedTeam;

                UpdateText();
            }
        }

        private void UpdateText() {
            GetComponent<Text>().text = $"Team: {team}";
        }
    }
}
