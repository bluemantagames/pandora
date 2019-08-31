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
        private string matchmakingHost = "http://localhost:8080";
        private string matchmakingUrl = "/matchmaking";
        private string matchToken = null;
        private Socket matchSocket = null;
        private Thread networkThread = null;
        private Thread receiveThread = null;
        private ConcurrentQueue<Message> queue = new ConcurrentQueue<Message>();
        public ConcurrentQueue<StepMessage> stepsQueue = new ConcurrentQueue<StepMessage>();
        public bool matchStarted = false;
        public UnityEvent matchStartEvent = new UnityEvent();
        public int? PlayerId = null;

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

        public void StartMatchmaking()
        {
            var client = new RestClient(matchmakingHost);
            var request = new RestRequest(matchmakingUrl, Method.GET);

            client.Timeout = int.MaxValue; // request is long-polling - do not timeout

            GameObject.Find("MatchmakingButton").GetComponent<Button>().interactable = false;

            client.ExecuteAsync<MatchmakingResponse>(request, response =>
            {
                Debug.Log($"Match found, token: {response.Data.token}");

                matchToken = response.Data.token;

                if (networkThread == null)
                {
                    networkThread = new Thread(new ThreadStart(StartMatch));

                    networkThread.Start();
                }
            });
        }

        public void StartMatch()
        {
            var matchHost = "127.0.0.1";
            var matchPort = 9090;
            var dns = Dns.GetHostEntry(matchHost);

            Debug.Log($"Dns: {dns}");

            var address = dns.AddressList[0];
            var ipe = new IPEndPoint(address, matchPort);

            matchSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Debug.Log($"Connecting to {matchHost}:{matchPort}");

            matchSocket.Connect(ipe);

            var join = new Join
            {
                Token = matchToken
            };

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
            { // TODO: Check if this impacts CPU and let the thread sleep a while if it does
                var isMessageDequeued = queue.TryDequeue(out message);

                if (isMessageDequeued)
                {
                    SendMessage(message.ToBytes(matchToken));
                }
            }
        }

        public void ReceiveLoop()
        {
            while (true)
            { // TODO: Check if this impacts CPU and let the thread sleep a while if it does
                var sizeBytes = new Byte[4];

                matchSocket.Receive(sizeBytes, sizeBytes.Length, 0);

                if (BitConverter.IsLittleEndian)
                { // we receive bytes in big endian
                    Array.Reverse(sizeBytes);
                }

                var size = BitConverter.ToInt32(sizeBytes, 0);

                Debug.Log($"Asking for {size} bytes");

                var messageBytes = new Byte[size];

                matchSocket.Receive(messageBytes, messageBytes.Length, 0);

                var envelope = ServerEnvelope.Parser.ParseFrom(messageBytes);

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

                    foreach (var command in envelope.Step.Commands)
                    {
                        if (command.CommandCase == StepCommand.CommandOneofCase.Spawn)
                        {
                            var spawnMessage =
                                new SpawnMessage {
                                    unitName = command.Spawn.UnitName,
                                    cellX = command.Spawn.X,
                                    cellY = command.Spawn.Y,
                                    team = command.Spawn.Team,
                                    timestamp = DateTimeOffset.FromUnixTimeSeconds((long) command.Timestamp).UtcDateTime
                                };

                            commands.Add(spawnMessage);
                        }
                    }

                    Debug.Log("Enqueuing Step");

                    stepsQueue.Enqueue(new StepMessage(envelope.Step.TimePassedMs, commands));
                }
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

            Debug.Log($"Sending {lengthBytes.Length} bytes as message length");

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
    }

}