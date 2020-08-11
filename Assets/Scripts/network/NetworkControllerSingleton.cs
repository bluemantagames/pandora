using RestSharp;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Pandora.Messages;
using UnityEngine.UI;
using Google.Protobuf;
using UnityEngine.Events;
using Pandora.Network.Messages;
using System.Collections.Concurrent;

namespace Pandora.Network
{
    public class NetworkControllerSingleton
    {
        public bool isDebugBuild = Debug.isDebugBuild;

        string matchmakingHost
        {
            get
            {
                Debug.Log($"Is debug build? {isDebugBuild}");

                if (isDebugBuild)
                    return "http://localhost:8080";
                else
                    return "http://3bitpodcast.com:8080";
            }
        }

        string matchmakingUrl = "/matchmaking";
        string matchToken = null;
        Socket matchSocket = null;
        Thread networkThread = null;
        Thread receiveThread = null;
        ConcurrentQueue<Message> queue = new ConcurrentQueue<Message>();
        int matchStartTimeout = 3; // seconds
        public ConcurrentQueue<StepMessage> stepsQueue = new ConcurrentQueue<StepMessage>();
        public bool matchStarted = false;
        public UnityEvent matchStartEvent = new UnityEvent();
        public int? PlayerId = null;
        public Boolean IsActive = false;

        private static NetworkControllerSingleton privateInstance = null;

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

        private NetworkControllerSingleton() { }

        public void StartMatchmaking(String username, List<String> deck)
        {
            IsActive = true;

            var client = new RestClient(matchmakingHost);
            var request = new RestRequest(matchmakingUrl, Method.GET);

            client.Timeout = int.MaxValue; // request is long-polling - do not timeout

            client.ExecuteAsync<MatchmakingResponse>(request, response =>
            {
                Logger.Debug($"Match found, token: {response.Data.token}");

                matchToken = response.Data.token;

                if (networkThread == null)
                {
                    networkThread = new Thread(new ParameterizedThreadStart(StartMatch));

                    networkThread.Start(
                        new MatchParams(username, deck)
                    );
                }
            });
        }

        public void StartMatch(object data)
        {
            if (data.GetType() != typeof(MatchParams))
            {
                return;
            }

            Debug.Log($"Connecting to the game server with token {matchToken}");

            var startTime = DateTime.Now;

            MatchParams matchParams = (MatchParams)data;

            var matchHost = (isDebugBuild) ? "127.0.0.1" : "3bitpodcast.com";
            var matchPort = 9090;
            var dns = Dns.GetHostEntry(matchHost);

            Logger.Debug($"Dns: {dns}");

            var address = dns.AddressList[0];
            var ipe = new IPEndPoint(address, matchPort);

            matchSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Logger.Debug($"Connecting to {matchHost}:{matchPort}");

            matchSocket.Connect(ipe);

            var join = new Join
            {
                Token = matchToken,
                Username = matchParams.Username
            };

            join.Deck.Add(matchParams.Deck);

            var envelope = new ClientEnvelope
            {
                Token = matchToken,
                Join = join
            };

            var bytes = envelope.ToByteArray();

            var lengthBytes = bytes.Length;

            SendMessage(
                envelope.ToByteArray()
            );

            receiveThread = new Thread(new ThreadStart(ReceiveLoop));

            receiveThread.Start();

            Message message;

            while (true)
            {
                // Return to matchmaking if match does not start in the predefined timeframe
                if (!matchStarted && DateTime.Now.Subtract(startTime).Seconds > matchStartTimeout)
                {
                    receiveThread.Abort();

                    networkThread = null;

                    StartMatchmaking(matchParams.Username, matchParams.Deck);

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

        public void ReceiveLoop()
        {
            while (true)
            {
                // TODO: Check if this impacts CPU and let the thread sleep a while if it does

                var sizeBytes = new Byte[4];

                matchSocket.Receive(sizeBytes, sizeBytes.Length, 0);

                if (BitConverter.IsLittleEndian)
                { // we receive bytes in big endian
                    Array.Reverse(sizeBytes);
                }

                var size = BitConverter.ToInt32(sizeBytes, 0);

                Logger.Debug($"Asking for {size} bytes");

                var messageBytes = new Byte[size];

                matchSocket.Receive(messageBytes, messageBytes.Length, 0);

                var envelope = ServerEnvelope.Parser.ParseFrom(messageBytes);

                HandleServerEnvelope(envelope);
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

                Debug.Log($"We're team {TeamComponent.assignedTeam}");

                matchStartEvent.Invoke();
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
                    else if (command.CommandCase == StepCommand.CommandOneofCase.GoldReward) {
                        commands.Add(
                            GenerateGoldRewardMessage(command)
                        );
                    }
                    else if (command.CommandCase == StepCommand.CommandOneofCase.UnitCommand)
                    {
                        commands.Add(
                            GenerateCommandMessage(command)
                        );
                    }
                }

                // (I don't really like the foreach here...)
                foreach (var playerInfo in envelope.Step.PlayerInfo)
                {
                    if (playerInfo.Id == PlayerId)
                    {
                        mana = playerInfo.Mana;
                        Debug.Log($"Player ({PlayerId}) received mana: {mana}");
                    }
                }

                Debug.Log("Enqueuing Step");

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

            matchSocket.Send(lengthBytes);
            matchSocket.Send(message);
        }

        public void Stop()
        {
            receiveThread?.Abort();
            networkThread?.Abort();
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
                elapsedMs = (int) command.GoldReward.ElapsedMs
            };
        }

    }

}