using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using UnityEngine;
using Pandora.Messages;
using Pandora.Network.Messages;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Pandora.Network
{
    public class ReplayControllerSingleton
    {
        private Thread liveThread = null;
        private ClientWebSocket ws = new ClientWebSocket();
        private LengthPrefixedWebsocketWrapper wsWrapper;
        public bool IsDebugBuild = Debug.isDebugBuild;

        String WsBaseUri
        {
            get
            {
                if (IsDebugBuild)
                    return "ws://localhost:8080/live";
                else
                    return "ws://pandora.bluemanta.games:8080/live";
            }
        }

        public ConcurrentQueue<StepMessage> stepsQueue = new ConcurrentQueue<StepMessage>();
        public bool MatchStarted = true;
        public Boolean IsActive = false;

        private static ReplayControllerSingleton privateInstance = null;
        private static NetworkControllerSingleton networkSingleton = null;

        public static ReplayControllerSingleton instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new ReplayControllerSingleton();

                    networkSingleton = NetworkControllerSingleton.instance;
                }

                return privateInstance;
            }
        }

        private ReplayControllerSingleton()
        {
            ws.Options.SetBuffer(125000, 1);
        }

        public void StartLive(String matchToken)
        {
            IsActive = true;

            if (liveThread == null)
            {
                liveThread = new Thread(new ParameterizedThreadStart(LiveExec));

                liveThread.Start(matchToken);
            }
        }

        public async void LiveExec(object data)
        {
            wsWrapper = new LengthPrefixedWebsocketWrapper(ws);

            if (data.GetType() != typeof(String))
            {
                return;
            }

            var matchToken = (String)data;
            var targetUri = new Uri($"{WsBaseUri}/{matchToken}");

            try
            {
                await ws.ConnectAsync(targetUri, CancellationToken.None);
                Debug.Log($"[REPLAY] Connecting to {targetUri}...");

                await Task.WhenAll(Receive(ws));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                Debug.Log("[REPLAY] Closing WebSocket connection");
                if (ws != null) ws.Dispose();
            }
        }
        private async Task Receive(ClientWebSocket ws)
        {
            while (ws.State == WebSocketState.Open)
            {
                if (!MatchStarted)
                {
                    networkSingleton.matchStarted = true;
                    MatchStarted = true;
                }

                var messageBuffer = await wsWrapper.Receive();

                // Websocket has been closed
                if (messageBuffer.Length == 0) continue;

                var envelope = ServerEnvelope.Parser.ParseFrom(messageBuffer);

                Logger.Debug($"[REPLAY] Received {envelope}, message count is {wsWrapper.MessageCount}");

                networkSingleton.HandleServerEnvelope(envelope);
            }
        }
    }
}