using RestSharp;
using UnityEngine;
using Pandora.Network.Data.Users;
using System.Threading.Tasks;

namespace Pandora.Network
{
    public class APIControllerSingleton
    {
        private bool isDebugBuild = Debug.isDebugBuild;

        private string apiHost
        {
            get
            {
                if (isDebugBuild)
                    return "http://localhost:8080/api";
                else
                    return "http://3bitpodcast.com:8080/api";
            }
        }

        private static APIControllerSingleton privateInstance = null;
        private RestClient client = null;

        private APIControllerSingleton()
        {
            client = new RestClient(apiHost);
        }

        public static APIControllerSingleton instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new APIControllerSingleton();
                }

                return privateInstance;
            }
        }

        /// <summary>
        /// Make a login request
        /// </summary>
        /// <param name="username">The username / email string</param>
        /// <param name="password">The password string</param>
        /// <returns>A Task with a LoginResponse (the token)</returns>
        public Task<IRestResponse<LoginResponse>> Login(string username, string password)
        {
            var request = new RestRequest("/users/login", Method.POST);
            var param = new LoginRequest { username = username, password = password };

            request.AddJsonBody(param);

            return client.ExecuteTaskAsync<LoginResponse>(request);
        }
    }
}