using RestSharp;
using UnityEngine;

namespace CRclone.Network {
    public class NetworkControllerSingleton {
        private string matchmakingHost = "http://localhost:8080";
        private string matchmakingUrl = "/matchmaking";

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
                Debug.Log(response.Data.token);
            });
        }

    }

}