using UnityEngine;
using Unity.RemoteConfig;
using Pandora.Network;

namespace Pandora.Analytics
{
    struct EmptyStruct {}

    public class RemoteConfigInit : MonoBehaviour
    {

        void Start()
        {
            ConfigManager.FetchCompleted += ApplyRemoteSettings;

            ConfigManager.FetchConfigs(new EmptyStruct(), new EmptyStruct());
        }

        void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            var matchmakingNotificationTimeout = ConfigManager.appConfig.GetInt("MATCHMAKING_NOTIFICATION_TIMEOUT_SECONDS");

            // Set a safe lower limit in case of misconfigurations
            if (matchmakingNotificationTimeout > 5)
                NetworkControllerSingleton.instance.NotificationWaitTimeout = matchmakingNotificationTimeout;

            UnityEngine.Debug.Log($"Applied Remote Config {configResponse} {NetworkControllerSingleton.instance.NotificationWaitTimeout}");
        }
    }
}