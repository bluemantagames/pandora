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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Pandora.Network.Data.Matchmaking;

namespace Pandora.Network
{
    public class NetworkControllerSingleton
    {
        public bool isDebugBuild = Debug.isDebugBuild;

        string userMatchToken = null;
        Socket matchSocket = null;
        Thread networkThread = null;
        Thread receiveThread = null;
        ConcurrentQueue<Message> queue = new ConcurrentQueue<Message>();
        ApiControllerSingleton apiControllerSingleton = ApiControllerSingleton.instance;
        ModelSingleton modelSingleton = ModelSingleton.instance;
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

        public async void StartMatchmaking()
        {
            if (modelSingleton.Token == null) return;

            IsActive = true;

            var response = await apiControllerSingleton.StartMatchmaking(modelSingleton.Token);

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

        public void StartMatch()
        {
            Debug.Log($"Connecting to the game server with token {userMatchToken}");

            var startTime = DateTime.Now;

            var matchHost = (isDebugBuild) ? "127.0.0.1" : "3bitpodcast.com";
            var matchPort = 9090;
            var dns = Dns.GetHostEntry(matchHost);

            Logger.Debug($"Dns: {dns}");

            var address = dns.AddressList[0];
            var ipe = new IPEndPoint(address, matchPort);

            var decodedUserMatchToken = new JwtSecurityToken(userMatchToken);
            var userMatchTokenPayload = decodedUserMatchToken.Claims.First(c => c.Type == "payload").Value;
            var decodedPayload = JsonUtility.FromJson<UserMatchTokenPayload>(userMatchTokenPayload);
            var matchToken = decodedPayload.matchToken;

            Debug.Log($"Decoded user match JWT, match token is: {matchToken}");

            matchSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Logger.Debug($"Connecting to {matchHost}:{matchPort}");

            matchSocket.Connect(ipe);

            var join = new Join
            {
                UserMatchToken = userMatchToken
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
    }

}