using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Pandora.Network;
using Pandora;

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

            var leaderboard = await ApiControllerSingleton.instance.GetLeaderboard(page, UsersPerPage, token);
        }
    }
}