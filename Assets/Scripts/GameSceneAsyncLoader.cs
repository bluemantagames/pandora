using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Pandora.Network;
using Pandora;
using Cysharp.Threading.Tasks;

public class GameSceneAsyncLoader : MonoBehaviour
{
    public GameObject PlayButton;
    public GameObject LoadingText;

    bool isExecuted = false;
    NetworkControllerSingleton networkControllerSingleton;

    void Awake()
    {
        networkControllerSingleton = NetworkControllerSingleton.instance;
    }

    private async UniTask StartLoading()
    {
        ShowLoader();

        // Used in dev to clear the cached dependencies
        // await AddressablesSingleton.instance.ClearDependenciesCache();

        Logger.Debug("Downloading dependencies...");

        await AddressablesSingleton.instance.DownloadDependencies(UpdateDownloadStatus);

        Logger.Debug("Preloading units...");

        UpdateLoadingAssets();

        await AddressablesSingleton.instance.LoadUnits();

        Logger.Debug("Preloading game scene...");

        networkControllerSingleton.GameSceneLoading = SceneManager.LoadSceneAsync("GameScene");
        networkControllerSingleton.GameSceneLoading.allowSceneActivation = false;

        ShowPlayButton();
    }

    void ShowLoader()
    {
        PlayButton.GetComponent<Canvas>().enabled = false;
        LoadingText.GetComponent<Canvas>().enabled = true;
    }

    void ShowPlayButton()
    {
        PlayButton.GetComponent<Canvas>().enabled = true;
        LoadingText.GetComponent<Canvas>().enabled = false;
    }

    void UpdateDownloadStatus(float progress)
    {
        var numericValue = progress * 100;
        var roundedValue = Math.Round(numericValue, 2);
        var progressText = $"Downloading... {roundedValue}%";

        Logger.Debug(progressText);
        LoadingText.GetComponent<Text>().text = progressText;
    }

    void UpdateLoadingAssets()
    {
        LoadingText.GetComponent<Text>().text = $"Loading assets...";
    }

    void Update()
    {
        if (!isExecuted)
        {
            isExecuted = true;
            _ = StartLoading();
        }
    }
}
