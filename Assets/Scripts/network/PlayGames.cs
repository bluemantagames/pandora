using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace Pandora.Network
{
    public class PlayGames
    {
        public static void Authenticate()
        {
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                .RequestEmail()
                .RequestServerAuthCode(false)
                .RequestIdToken()
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true;

            var instance = PlayGamesPlatform.Activate();

            instance.Authenticate(SignInInteractivity.CanPromptOnce, (result) =>
            {
                if (result == SignInStatus.Success)
                {
                    var serverAuthCode = instance.GetServerAuthCode();

                    Logger.Debug($"Obtained server auth code: {serverAuthCode}");
                }
                else
                {
                    Logger.Debug($"Google Play Games sigin error: {result}");
                }
            });
        }
    }
}