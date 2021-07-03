using UnityEngine;
using UnityEngine.UI;
using Pandora;
using Pandora.Network;
using Cysharp.Threading.Tasks;
using System.Net;
using System.Collections.Generic;


public class AuthBehaviour : MonoBehaviour
{
    private LoadingBehaviour loadingBehaviour;

    void Awake()
    {
        loadingBehaviour = GameObject.Find(Constants.LOADING_CANVAS_OBJECT_NAME)?.GetComponent<LoadingBehaviour>();
    }

    void Start()
    {
        _ = PlayGamesAuthentication();
    }

    /// <summary>
    /// Platform-aware play games auth caller
    /// </summary>
    private async UniTaskVoid PlayGamesAuthentication()
    {
#if UNITY_ANDROID
        var authenticated = await PlayGames.instance.Authenticate();

        if (authenticated)
        {
            _ = loadingBehaviour.LoadMainMenu();

            AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.LOGIN, new Dictionary<string, object>() {
                {"mode", "google-play"},
                {"failed", false}
            });
        }
        else
        {
            AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.LOGIN, new Dictionary<string, object>() {
                {"mode", "google-play"},
                {"failed", true}
            });
        }
#endif
    }
}
