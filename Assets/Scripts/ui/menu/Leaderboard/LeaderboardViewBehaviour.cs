﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Pandora.Network;
using Pandora;
using Pandora.UI.Menu.Modal;
using System.Net;
using Pandora.Network.Data.Leaderboard;

namespace Pandora.UI.Menu.Leaderboard
{
    public class LeaderboardViewBehaviour : MonoBehaviour, ModalInit
    {
        public int UsersPerPage = 20;
        public GameObject SingleValueContainer;
        public GameObject ValuesContainer;
        public ScrollRect ScrollerContainer;
        public bool EnableInfiniteScroll = true;
        public float InfiniteScrollThreshold = 0.2f;

        private bool isLoading = false;
        private int currentPage = 1;
        private int lastResultCount = 0;

        public void Init() {
            GetComponent<Canvas>().enabled = true;

            LoadLeaderboard().Forget();
        }

        public async UniTaskVoid LoadLeaderboard()
        {
            Logger.Debug($"Triggered leaderboard load with page {currentPage}");

            var token = PlayerModelSingleton.instance.Token;

            if (token == null) return;

            isLoading = true;
            var response = await ApiControllerSingleton.instance.GetLeaderboard(currentPage, UsersPerPage, token);
            isLoading = false;

            if (response.StatusCode != HttpStatusCode.OK) return;

            var players = response.Body.players;

            lastResultCount = players.Count;

            AddPlayers(players);
        }

        public void OnEndDragDelegate()
        {
            if (!EnableInfiniteScroll) return;

            var verticalPosition = ScrollerContainer.verticalNormalizedPosition;

            if (verticalPosition < InfiniteScrollThreshold && !isLoading && lastResultCount > 0)
            {
                currentPage += 1;
                _ = LoadLeaderboard();
            }
        }

        private void AddPlayers(List<LeaderboardValue> players)
        {
            foreach (var player in players)
            {
                var playerContainer = Instantiate(SingleValueContainer, ValuesContainer.transform, false);
                playerContainer.transform.parent = ValuesContainer.transform;

                var playerContainerCanvas = playerContainer.GetComponent<Canvas>();
                var position = playerContainer.GetComponentInChildren<LeaderboardPosition>();
                var username = playerContainer.GetComponentInChildren<LeaderboardUsername>();
                var points = playerContainer.GetComponentInChildren<LeaderboardPoints>();

                var positionText = position?.GetComponent<Text>();
                var usernameText = username?.GetComponent<Text>();
                var pointsText = points?.GetComponent<Text>();

                positionText.text = $"{player.position}.";
                usernameText.text = $"{player.username}";
                pointsText.text = $"({player.points})";

                if (playerContainerCanvas != null) playerContainerCanvas.enabled = true;
            }
        }
    }
}