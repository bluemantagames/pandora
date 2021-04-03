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
            NetworkControllerSingleton.instance.NotificationWaitTimeout = ConfigManager.appConfig.GetInt("MATCHMAKING_NOTIFICATION_TIMEOUT_SECONDS");

            UnityEngine.Debug.Log($"Applied Remote Config {configResponse} {NetworkControllerSingleton.instance.NotificationWaitTimeout}");
        }
    }
}