using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Pandora.Messages;
using Google.Protobuf;
using UnityEngine.Events;
using Pandora.Network.Messages;
using System.Collections.Concurrent;
using System.Linq;
using Pandora.Network.Data.Matchmaking;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Pandora.Network.Data;
using Pandora.Network.Data.Mtx;
using Pandora;

namespace Pandora.Network
{
    public class NetworkControllerSingleton
    {
        public bool ProdMatchmaking = false;

        string userMatchToken = null;
        Socket matchSocket = null;
        volatile Thread networkThread = null;
        volatile Thread receiveThread = null;
        volatile bool stopNetworkThread = false;
        public AsyncOperation GameSceneLoading;
        ConcurrentQueue<Message> queue = new ConcurrentQueue<Message>();
        ApiControllerSingleton apiControllerSingleton = ApiControllerSingleton.instance;
        PlayerModelSingleton playerModelSingleton = PlayerModelSingleton.instance;
        JWT jwt;
        int matchStartTimeout = 5; // seconds
        public int NotificationWaitTimeout = 30; // seconds
        public ConcurrentQueue<StepMessage> stepsQueue = new ConcurrentQueue<StepMessage>();
        public bool matchStarted = false;
        public UnityEvent<Opponent> matchStartEvent = new UnityEvent<Opponent>();
        public int? PlayerId = null;
        public string CurrentMatchToken = null;
        long? lastEnvelopeId = null;
        int reconnectionWaitMs = 500;

        private static NetworkControllerSingleton privateInstance = null;

        public static bool InjectException = false;

        public static NetworkControllerSingleton instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new NetworkControllerSingleton();
                }

                return privateInstance;
            }
        }

        private NetworkControllerSingleton()
        {
            jwt = new JWT();

#if !UNITY_EDITOR
            ProdMatchmaking = true;
#endif
        }

        public void StartMatchmaking()
        {
            var deck = playerModelSingleton.GetActiveDeck();

            if (deck != null) ExecMatchmaking(deck, false).Forget();
        }

        public void StartMatchmaking(List<string> deck)
        {
            if (deck != null) ExecMatchmaking(deck, false).Forget();
        }

        public void StartDevMatchmaking(List<string> deck)
        {
            if (deck != null) ExecMatchmaking(deck, true).Forget();
        }

        public async UniTaskVoid ExecMatchmaking(List<string> deck, bool isDev)
        {
            // Wait for the game scene to be loaded before actually trying to join a match
            if (GameSceneLoading != null)
            {
                await UniTask.WaitUntil(() => GameSceneLoading.progress >= 0.9f);

                Debug.Log($"Starting matchmaking with progress {GameSceneLoading.progress}");
            }

            var cancellationSource = new CancellationTokenSource();

            if (!Application.isEditor)
            {
                var _ = Task.Delay(NotificationWaitTimeout * 1000, cancellationSource.Token).ContinueWith(task =>
                {
                    apiControllerSingleton.SendMatchmakingNotification(isDev, playerModelSingleton.Token, cancellationSource.Token);
                });
            }
            else
            {
                Logger.Debug("Push notification not sent because we are in the Unity Editor");
            }


            var response = isDev
                ? await apiControllerSingleton.StartDevMatchmaking(deck, playerModelSingleton.Token)
                : await apiControllerSingleton.StartMatchmaking(deck, playerModelSingleton.Token);

            cancellationSource.Cancel();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                userMatchToken = response.Body.token;

                if (networkThread == null)
                {
                    Logger.Debug($"Match found, token: {userMatchToken}");

                    networkThread = new Thread(new ThreadStart(StartMatch));
                    networkThread.Start();
                }
            }
        }

        public class StopSignal
        {
            public bool Stop = false;
        }


        public void StartMatch()
        {
            Debug.Log($"Connecting to the game server with token {userMatchToken}");

            var startTime = DateTime.Now;

            var matchHost = (ProdMatchmaking) ? Hosts.ProdMatch : Hosts.DevMatch;
            var matchPort = Hosts.MatchPort;

            IPHostEntry dns = null;

            while (dns == null)
            {
                try
                {
                    dns = Dns.GetHostEntry(matchHost);

                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);

                    Thread.Sleep(reconnectionWaitMs);
                }
            }

            Logger.Debug($"Dns: {dns}");

            var address = dns.AddressList[0];
            var ipe = new IPEndPoint(address, matchPort);

            var userMatchTokenClaims = jwt.DecodeJwtPayload<UserMatchTokenPayload>(userMatchToken);
            var matchToken = userMatchTokenClaims.matchToken;

            CurrentMatchToken = matchToken;

            Debug.Log($"Decoded user match JWT, match token is: {matchToken}");

            matchSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Logger.Debug($"Connecting to {matchHost}:{matchPort}");

            while (!matchSocket.Connected)
            {
                try
                {
                    matchSocket.Connect(ipe);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);

                    Thread.Sleep(reconnectionWaitMs);

                    Debug.Log("Trying to reconnect..");
                }
            }

            ReplayFrom replayFromId = null;

            if (lastEnvelopeId != null)
            {
                replayFromId = new ReplayFrom
                {
                    ReplayFromId = lastEnvelopeId.Value
                };
            }

            var join = new Join
            {
                UserMatchToken = userMatchToken,
                ReplayFromId = replayFromId
            };

            var envelope = new ClientEnvelope
            {
                Token = matchToken,
                Join = join
            };

            var bytes = envelope.ToByteArray();

            SendMessage(
                envelope.ToByteArray()
            );

            var stopSignal = new StopSignal();

            receiveThread = new Thread(new ParameterizedThreadStart(ReceiveLoop));
            receiveThread.Start(stopSignal);

            Message message;

            while (true)
            {
                if (stopNetworkThread)
                {
                    stopNetworkThread = false;

                    return;
                }

                // Return to matchmaking if match does not start in the predefined timeframe
                if (!matchStarted && DateTime.Now.Subtract(startTime).Seconds > matchStartTimeout)
                {
                    stopSignal.Stop = true;

                    receiveThread = null;
                    networkThread = null;

                    shutdownMatchSocket();

                    StartMatchmaking();

                    Debug.LogWarning("Timeout while joining a match, back to matchmaking");

                    break;
                }

                var isMessageDequeued = queue.TryDequeue(out message);

                if (isMessageDequeued)
                {
                    SendMessage(message.ToBytes(matchToken));
                }

                Thread.Sleep(100);
            }
        }

        public void ReceiveLoop(object param)
        {
            var signal = param as StopSignal;

            try
            {

                while (true)
                {
                    // TODO: Check if this impacts CPU and let the thread sleep a while if it does
                    if (signal.Stop || matchSocket == null)
                    {
                        break;
                    }

                    // Reconnection dev purpose
                    if (InjectException)
                    {
                        InjectException = false;

                        throw new Exception("Injected exception");
                    }

                    var sizeBytes = new Byte[4];

                    matchSocket.Receive(sizeBytes, sizeBytes.Length, 0);

                    if (BitConverter.IsLittleEndian)
                    { // we receive bytes in big endian
                        Array.Reverse(sizeBytes);
                    }

                    var size = BitConverter.ToInt32(sizeBytes, 0);

                    if (size == 0)
                    {
                        throw new Exception("Empty Receive call");
                    }

                    Logger.Debug($"Asking for {size} bytes");

                    var messageBytes = new Byte[size];

                    matchSocket.Receive(messageBytes, messageBytes.Length, 0);

                    var envelope = ServerEnvelope.Parser.ParseFrom(messageBytes);

                    HandleServerEnvelope(envelope);
                }
            }
            catch (Exception e)
            {
                try
                {
                    matchSocket?.Close();
                }
                finally
                {
                    Debug.LogError(e.Message);

                    if (matchSocket != null)
                        StartMatch();
                }
            }
        }

        public void HandleServerEnvelope(ServerEnvelope envelope)
        {
            Debug.Log($"Received {envelope}");

            if (envelope.MessageCase == ServerEnvelope.MessageOneofCase.Start)
            {
                matchStarted = true;

                TeamComponent.assignedTeam = envelope.Start.Team;
                PlayerId = envelope.Start.Id;

                var player = envelope.Start.Teams.First(team => team.TeamNumber == TeamComponent.opponentTeam)?.Players[0];

                Debug.Log($"We're team {TeamComponent.assignedTeam}");

                TeamComponent.Opponent = new Opponent
                {
                    Name = player.Name,
                    Position = (player.LeaderboardPosition != 0) ? player.LeaderboardPosition as int? : null,
                    Cosmetics = new CosmeticsMtx(player.Cosmetics)
                };

                matchStartEvent.Invoke(TeamComponent.Opponent);
            }

            if (envelope.MessageCase == ServerEnvelope.MessageOneofCase.Step)
            { // enqueue spawns and let the main thread handle it
                var commands = new List<Message> { };
                float? mana = null;

                foreach (var command in envelope.Step.Commands)
                {


                    if (command.CommandCase == StepCommand.CommandOneofCase.Spawn)
                    {
                        commands.Add(
                            GenerateSpawnMessage(command)
                        );
                    }
                    else if (command.CommandCase == StepCommand.CommandOneofCase.GoldReward)
                    {
                        commands.Add(
                            GenerateGoldRewardMessage(command)
                        );
                    }
                    else if (command.CommandCase == StepCommand.CommandOneofCase.UnitCommand)
                    {
                        if (command.CommandCase == StepCommand.CommandOneofCase.Spawn)
                        {
                            commands.Add(
                                GenerateSpawnMessage(command)
                            );
                        }
                        else if (command.CommandCase == StepCommand.CommandOneofCase.UnitCommand)
                        {
                            commands.Add(
                                GenerateCommandMessage(command)
                            );
                        }
                    }
                }

                Debug.Log($"Enqueuing Step {envelope.Step.TimePassedMs}");

                lastEnvelopeId = envelope.EnvelopeId;

                stepsQueue.Enqueue(new StepMessage(envelope.Step.TimePassedMs, commands, mana));
            }
        }

        public void EnqueueMessage(Message message)
        {
            // Enqueue the message instead of sending directly so that we do the blocking part in another thread
            // since this is usually executed by the main unity thread
            queue.Enqueue(message);
        }

        private void SendMessage(byte[] message)
        {
            var lengthBytes = BitConverter.GetBytes(message.Length);

            Logger.Debug($"Sending {lengthBytes.Length} bytes as message length");

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }

            if (matchSocket != null && matchSocket.Connected)
            {
                matchSocket.Send(lengthBytes);
                matchSocket.Send(message);
            }
        }

        public void Stop()
        {
            if (matchSocket != null)
            {
                shutdownMatchSocket();
            }

            receiveThread = null;

            stopNetworkThread = true;
            networkThread = null;
            lastEnvelopeId = null;

            stepsQueue = new ConcurrentQueue<StepMessage>();

            matchStarted = false;
        }

        public static SpawnMessage GenerateSpawnMessage(StepCommand command)
        {
            return new SpawnMessage
            {
                unitName = command.Spawn.UnitName,
                cellX = command.Spawn.X,
                cellY = command.Spawn.Y,
                team = command.Spawn.Team,
                timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)command.Timestamp).UtcDateTime,
                unitId = command.Spawn.UnitId,
                manaUsed = command.Spawn.ManaUsed,
                elapsedMs = command.Spawn.ElapsedMs
            };
        }

        public static CommandMessage GenerateCommandMessage(StepCommand command)
        {
            return new CommandMessage
            {
                team = command.UnitCommand.Team,
                unitId = command.UnitCommand.UnitId,
                elapsedMs = command.UnitCommand.ElapsedMs
            };
        }

        public static GoldRewardMessage GenerateGoldRewardMessage(StepCommand command)
        {
            return new GoldRewardMessage
            {
                playerId = command.GoldReward.PlayerId,
                team = command.GoldReward.Team,
                goldSpent = command.GoldReward.GoldSpent,
                rewardId = command.GoldReward.RewardId,
                elapsedMs = (int)command.GoldReward.ElapsedMs
            };
        }

        void shutdownMatchSocket() {
            var socket = matchSocket;
            
            // It's important to set the matchSocket to null before shutting it down so that
            // the ReceiveLoop thread knows this wasn't an unwanted disconnect, and doesn't try to reconnect again
            matchSocket = null;

            socket?.Shutdown(SocketShutdown.Both);
        }

    }

}
