using RestSharp;
using UnityEngine;
using Pandora.Network.Data;
using Pandora.Network.Data.Users;
using Pandora.Network.Data.Matchmaking;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;

namespace Pandora.Network
{
    public class ApiControllerSingleton
    {
        public bool IsDebugBuild = Debug.isDebugBuild;

        private string apiHost
        {
            get
            {
                if (IsDebugBuild)
                    return "http://127.0.0.1:8080/api";
                else
                    return "http://pandora.bluemanta.games:8080/api";
            }
        }

        private static ApiControllerSingleton privateInstance = null;
        
        private RestClient _client = null;

        private RestClient client {
            get {
                if (_client == null) {
                    Logger.Debug($"Connecting to API gateway: {apiHost}");

                    _client = new RestClient(apiHost);

                    // Using the unity serializer
                    var customSerializer = new UnityJsonSerializer();

                    _client.UseSerializer(() => customSerializer);
                }

                return _client;
            }
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

                // Debug.Log(response.Content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return new ApiResponse<T>(response.StatusCode, response.Data);
                }
                else
                {
                    if (response.ContentType == "application/json")
                    {
                        var deserializedError = RestSharp.SimpleJson.DeserializeObject<ApiError>(response.Content);
                        return new ApiResponse<T>(response.StatusCode, deserializedError);
                    }
                    else
                    {
                        var genericApiError = new ApiError { message = response.Content };
                        return new ApiResponse<T>(response.StatusCode, genericApiError);
                    }
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

        /// <summary>
        /// Update a deck slot
        /// </summary>
        /// <param name="deckSlotId">The deck slot's id</param>
        /// <param name="deck">The deck as a Json list</param>
        /// <param name="token">The token string</param>
        /// <returns>A Task with a string</returns>
        public Task<ApiResponse<string>> DeckSlotUpdate(long deckSlotId, List<string> deck, string token)
        {
            var request = new RestRequest($"/users/me/deckSlots/{deckSlotId}", Method.PUT);
            var param = new DeckSlotUpdateRequest { deck = deck };

            request.AddJsonBody(param);

            return ExecuteApiRequest<string>(request, token);
        }

        /// <summary>
        /// Update the active deck slot
        /// </summary>
        /// <param name="deckSlotId">The deck slot's id</param>
        /// <param name="token">The token string</param>
        /// <returns>A Task with a string</returns>
        public Task<ApiResponse<string>> ActiveDeckSlotUpdate(long deckSlotId, string token)
        {
            var request = new RestRequest("/users/me/activeDeckSlot", Method.PUT);
            var param = new ActiveDeckSlotUpdateRequest { deckSlot = deckSlotId };

            request.AddJsonBody(param);

            return ExecuteApiRequest<string>(request, token);
        }

        /// <summary>
        /// Start the matchmaking (long polling)
        /// </summary>
        /// <param name="token">The token string</param>
        /// <returns>A Task with a MatchmakingResponse</returns>
        public Task<ApiResponse<MatchmakingResponse>> StartMatchmaking(List<string> deck, string token)
        {
            var request = new RestRequest("/matchmaking", Method.POST);
            var param = new MatchmakingRequest { deck = deck };

            request.Timeout = int.MaxValue;
            request.AddJsonBody(param);

            return ExecuteApiRequest<MatchmakingResponse>(request, token);
        }

        /// <summary>
        /// Start the matchmaking in dev mode (long polling)
        /// </summary>
        /// <param name="token">The token string</param>
        /// <returns>A Task with a MatchmakingResponse</returns>
        public Task<ApiResponse<MatchmakingResponse>> StartDevMatchmaking(List<string> deck, string token)
        {
            var request = new RestRequest("/dev-matchmaking", Method.POST);
            var param = new MatchmakingRequest { deck = deck };

            request.Timeout = int.MaxValue;
            request.AddJsonBody(param);

            return ExecuteApiRequest<MatchmakingResponse>(request, token);
        }
    }
}