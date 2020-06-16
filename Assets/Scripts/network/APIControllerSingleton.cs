using RestSharp;
using UnityEngine;
using Pandora.Network.Data;
using Pandora.Network.Data.Users;
using System.Threading.Tasks;
using System.Net;

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
        /// Execute a RestSharp request and return an ApiResponse instead.
        /// ApiResponse can have a deserialized response object or an ApiError inside.
        /// </summary>
        /// <param name="request"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Task<ApiResponse<T>> ExecuteApiRequest<T>(RestRequest request, string token = null)
        {
            if (token != null)
            {
                // Add an authorization header
                request.AddHeader("Authorization", $"Bearer {token}");
            }

            return client.ExecuteTaskAsync<T>(request).ContinueWith<ApiResponse<T>>(reqTask =>
            {
                var response = reqTask.Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return new ApiResponse<T>(response.StatusCode, response.Data);
                }
                else
                {
                    var deserializedError = RestSharp.SimpleJson.DeserializeObject<ApiError>(response.Content);
                    return new ApiResponse<T>(response.StatusCode, deserializedError);
                }
            });
        }

        /// <summary>
        /// Make a login request
        /// </summary>
        /// <param name="username">The username / email string</param>
        /// <param name="password">The password string</param>
        /// <returns>A Task with a LoginResponse (the token)</returns>
        public Task<ApiResponse<LoginResponse>> Login(string username, string password)
        {
            var request = new RestRequest("/users/login", Method.POST);
            var param = new LoginRequest { username = username, password = password };

            request.AddJsonBody(param);

            return ExecuteApiRequest<LoginResponse>(request);
        }

        /// <summary>
        /// Make a signup request
        /// </summary>
        /// <param name="username">The username string</param>
        /// <param name="email">The email string</param>
        /// <param name="password">The password string</param>
        /// <returns>A Task with a LoginResponse (the token)</returns>
        public Task<ApiResponse<LoginResponse>> Signup(string username, string email, string password)
        {
            var request = new RestRequest("/users/signup", Method.POST);
            var param = new SignupRequest { username = username, email = email, password = password };

            request.AddJsonBody(param);

            return ExecuteApiRequest<LoginResponse>(request);
        }

        /// <summary>
        /// Get my info
        /// </summary>
        /// <param name="token">The token string</param>
        /// <returns>A Task with a MeResponse</returns>
        public Task<ApiResponse<MeResponse>> GetMe(string token)
        {
            var request = new RestRequest("/users/me", Method.GET);

            return ExecuteApiRequest<MeResponse>(request, token);
        }
    }
}