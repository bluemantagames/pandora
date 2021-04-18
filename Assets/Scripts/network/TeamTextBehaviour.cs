using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.Network.Data;

namespace Pandora.Network
{
    public class TeamTextBehaviour : MonoBehaviour
    {
        int team;
        bool matchStarted = false, textMatchUpdated = false;


        void Awake()
        {
            team = TeamComponent.assignedTeam;

            UpdateText();

            NetworkControllerSingleton.instance.matchStartEvent.AddListener(MatchStarted);
        }

        void Update()
        {
            if (team != TeamComponent.assignedTeam)
            {
                team = TeamComponent.assignedTeam;

                UpdateText();
            }

            if (matchStarted && !textMatchUpdated) {
                UpdateText();

                textMatchUpdated = true;
            }
        }

        void MatchStarted(Opponent opponent) {
            matchStarted = true;
        }

        void UpdateText()
        {
            GetComponent<Text>().text = $"Team: {team}";

            if (matchStarted)
            GetComponent<Text>().text += " - MATCH STARTED";
        }
    }
}
