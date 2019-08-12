using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pandora.Network
{
    public class TeamTextBehaviour : MonoBehaviour
    {
        private int team;
        Button matchmakingButton;

        void Awake()
        {
            team = TeamComponent.assignedTeam;

            matchmakingButton = GameObject.Find("MatchmakingButton").GetComponent<Button>();

            UpdateText();
        }

        void Update()
        {
            if (team != TeamComponent.assignedTeam)
            {
                team = TeamComponent.assignedTeam;

                if (!matchmakingButton.interactable)
                {
                    matchmakingButton.interactable = true;
                }

                UpdateText();
            }
        }

        private void UpdateText()
        {
            GetComponent<Text>().text = $"Team: {team}";
        }
    }
}
