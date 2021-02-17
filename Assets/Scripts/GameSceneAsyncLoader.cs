using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pandora.Network;
using Pandora;
using Cysharp.Threading.Tasks;

public class GameSceneAsyncLoader : MonoBehaviour
{
    bool isExecuted = false;
    NetworkControllerSingleton networkControllerSingleton;

    void Awake()
    {
        networkControllerSingleton = NetworkControllerSingleton.instance;
    }

    private async UniTask StartLoading()
    {
        Logger.Debug("Downloading dependencies...");

        await AddressablesSingleton.instance.DownloadDependencies();

        Logger.Debug("Preloading units...");

        await AddressablesSingleton.instance.LoadUnits();

        Logger.Debug("Preloading game scene...");

        networkControllerSingleton.GameSceneLoading = SceneManager.LoadSceneAsync("GameScene");
        networkControllerSingleton.GameSceneLoading.allowSceneActivation = false;
    }

    //#if !UNITY_EDITOR
    void Update()
    {
        if (!isExecuted)
        {
            isExecuted = true;
            _ = StartLoading();
        }
    }
    //#endif
}
