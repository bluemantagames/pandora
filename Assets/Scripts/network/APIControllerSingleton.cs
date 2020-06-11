using RestSharp;
using UnityEngine;
using Pandora.Network.Data.Users;
using System.Threading.Tasks;

namespace Pandora.Network
{
    public class ApiControllerSingleton
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

        private static ApiControllerSingleton privateInstance = null;
        private RestClient client = null;

        private ApiControllerSingleton()
        {
            client = new RestClient(apiHost);
        }

        public static ApiControllerSingleton instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new ApiControllerSingleton();
                }

                return privateInstance;
            }
        }

        /// <summary>
        /// Deserialize a Json response using the usual
        /// deserializer used by RestSharp
        /// </summary>
        /// <param name="json">The Json string to deserialize</param>
        /// <typeparam name="T">The generic type to obtain from the deserialization</typeparam>
        /// <returns>An object T deserialized</returns>
        public static T DeserializeJsonResponse<T>(string json)
        {
            return RestSharp.SimpleJson.DeserializeObject<T>(json);
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