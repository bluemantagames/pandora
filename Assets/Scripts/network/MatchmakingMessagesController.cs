using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Pandora.Network.Data.Matchmaking;
using Pandora.Network;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using System.Text;
using Pandora;
using UnityEngine;

namespace Pandora.Network
{
    public class MatchmakingMessagesController
    {
        private ClientWebSocket ws = new ClientWebSocket();
        private LengthPrefixedWebsocketWrapper wsWrapper = null;

        public async UniTask Connect(bool prodHost, string authToken)
        {
            var wsURI = new Uri(
                !prodHost ?
                    "ws://127.0.0.1:8080/api/matchmaking/matchmaking-messages" :
                    "ws://pandora.bluemanta.games:8080/api/matchmaking/matchmaking-messages"
            );

            ws.Options.SetRequestHeader("Authorization", $"Bearer {authToken}");

            await ws.ConnectAsync(wsURI, CancellationToken.None);

            wsWrapper = new LengthPrefixedWebsocketWrapper(ws);
        }

        public async UniTask<MatchmakingMessage> Receive() {
            if (wsWrapper == null) {
                throw new Exception("Websocket not connected");
            }

            var bytes = await wsWrapper.Receive();
            var message = Encoding.UTF8.GetString(bytes);

            Logger.Debug($"Received message {message}");

            try {
                return JsonUtility.FromJson<MatchmakingMessage>(message);
            } catch (Exception e) {
                Logger.Debug(e.Message);

                return null;
            }
        }

        public async UniTask Disconnect() {
            await ws.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        }
    }
}