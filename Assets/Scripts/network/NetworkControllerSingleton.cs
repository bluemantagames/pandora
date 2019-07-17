using RestSharp;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CRclone.Network {
    public class NetworkControllerSingleton {
        private string matchmakingHost = "http://localhost:8080";
        private string matchmakingUrl = "/matchmaking";
        private string matchToken = null;
        private Socket matchSocket = null;
        private Thread networkThread;

        private static NetworkControllerSingleton privateInstance = null;

        public static NetworkControllerSingleton instance {
            get {
                if (privateInstance == null) {
                    privateInstance = new NetworkControllerSingleton();
                } 

                return privateInstance;
            }
        }

        private NetworkControllerSingleton() {}

        public void StartMatchmaking() {
            var client = new RestClient(matchmakingHost);
            var request = new RestRequest(matchmakingUrl, Method.GET);

            client.Timeout = int.MaxValue; // request is long-polling - do not timeout

            client.ExecuteAsync<MatchmakingResponse>(request, response => {
                Debug.Log($"Match found, token: {response.Data.token}");

                matchToken = response.Data.token;

                lock(networkThread) {
                    if (networkThread == null) {
                        networkThread = new Thread(new ThreadStart(StartMatch));

                        networkThread.Start();
                    }
                }
            });
        }

        private void StartMatch() {
            var matchHost = "127.0.0.1";
            var matchPort = 9090;
            var dns = Dns.GetHostEntry(matchHost);

            Debug.Log($"Dns: {dns}");

            var address = dns.AddressList[0];
            var ipe = new IPEndPoint(address, matchPort);

            matchSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Debug.Log($"Connecting to {matchHost}:{matchPort}");

            matchSocket.Connect(ipe);
        }
    }

}