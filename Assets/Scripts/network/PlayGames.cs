using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Cysharp.Threading.Tasks;
using Pandora.Network;
using System.Net;

#if UNITY_ANDROID
namespace Pandora.Network
{
    public class PlayGames
    {
        private static PlayGames privateInstance = null;
        public bool IsAuthenticated = false;

        private PlayGames()
        {
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                    .RequestEmail()
                    .RequestServerAuthCode(false)
                    .RequestIdToken()
                    .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true;

            PlayGamesPlatform.Activate();
        }

        public static PlayGames instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new PlayGames();
                }

                return privateInstance;
            }
        }

        private UniTask<SignInStatus> ClientAuthentication()
        {
            var task = new UniTaskCompletionSource<SignInStatus>();

            if (PlayGamesPlatform.Instance != null)
            {
                PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (result) =>
                {
                    task.TrySetResult(result);
                });
            }
            else
            {
                task.TrySetCanceled();
            }

            return task.Task;
        }

        public async UniTask<bool> Authenticate()
        {
            if (PlayGamesPlatform.Instance == null) return false;

            var apiController = ApiControllerSingleton.instance;
            var playerModelSingleton = PlayerModelSingleton.instance;

            var result = await ClientAuthentication();

            if (result == SignInStatus.Success)
            {
                var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();

                Logger.Debug($"Obtained server auth code: {serverAuthCode}");

                var googleSignInResponse = await apiController.GoogleSignIn(serverAuthCode);

                if (googleSignInResponse.StatusCode == HttpStatusCode.OK && googleSignInResponse.Body != null)
                {
                    var token = googleSignInResponse.Body.token;

                    Logger.Debug($"Logged in successfully with the token: {token}");

                    playerModelSingleton.Token = token;

                    return true;
                }
                else
                {
                    Logger.Debug($"Status code {googleSignInResponse.StatusCode} while logging in: {googleSignInResponse.Error.message}");
                    return false;
                }
            }
            else
            {
                Logger.Debug($"Google Play Games sigin error: {result}");
                return false;
            }
        }
    }
}
#endif