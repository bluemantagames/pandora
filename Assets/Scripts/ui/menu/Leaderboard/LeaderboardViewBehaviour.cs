using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Pandora.Network;
using Pandora;
using System.Net;

namespace Pandora.UI.Menu.Leaderboard
{
    public class LeaderboardViewBehaviour : MonoBehaviour
    {
        public int UsersPerPage = 50;
        public GameObject SingleValueContainer;
        public GameObject ValuesContainer;

        public async UniTaskVoid LoadLeaderboard(int page)
        {
            var token = PlayerModelSingleton.instance.Token;

            if (token == null) return;

            var response = await ApiControllerSingleton.instance.GetLeaderboard(page, UsersPerPage, token);

            if (response.StatusCode != HttpStatusCode.OK) return;

            var players = response.Body.players;

            foreach (var player in players)
            {
                var playerContainer = Instantiate(SingleValueContainer, ValuesContainer.transform, false);
                playerContainer.transform.parent = ValuesContainer.transform;

                var playerContainerCanvas = playerContainer.GetComponent<Canvas>();
                var position = playerContainer.GetComponentInChildren<LeaderboardPosition>();
                var username = playerContainer.GetComponentInChildren<LeaderboardUsername>();

                var positionText = position?.GetComponent<Text>();
                var usernameText = username?.GetComponent<Text>();

                positionText.text = $"{player.position}.";
                usernameText.text = $"{player.username}";

                // LayoutRebuilder.ForceRebuildLayoutImmediate(position?.GetComponent<RectTransform>());
                // LayoutRebuilder.ForceRebuildLayoutImmediate(username?.GetComponent<RectTransform>());

                if (playerContainerCanvas != null) playerContainerCanvas.enabled = true;
            }
        }
    }
}