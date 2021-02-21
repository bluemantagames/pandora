using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Pandora.Network;
using Pandora;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameSceneAsyncLoader : MonoBehaviour
{
    public GameObject PlayButton;
    public GameObject LoadingText;
    public LocalizedString LocalizedDownloadingText;
    public LocalizedString LocalizedLoadingText;

    string downloadingText = null;
    string loadingText = null;

    bool isExecuted = false;
    NetworkControllerSingleton networkControllerSingleton;

    void Awake()
    {
        networkControllerSingleton = NetworkControllerSingleton.instance;

        LocalizedDownloadingText.GetLocalizedString().Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                downloadingText = handle.Result;
        };

        LocalizedLoadingText.GetLocalizedString().Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                loadingText = handle.Result;
        };
    }

    private async UniTask StartLoading()
    {
        ShowLoader();

        // Used in dev to clear the cached dependencies
        // await AddressablesSingleton.instance.ClearDependenciesCache();

        var addressablesSize = await AddressablesSingleton.instance.GetAddressablesSize();

        if (addressablesSize != 0)
        {
            Logger.Debug("Downloading dependencies...");

            await AddressablesSingleton.instance.DownloadDependencies(UpdateDownloadStatus);
        }

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
        if (downloadingText == null) return;

        var numericValue = progress * 100;
        var roundedValue = Math.Round(numericValue, 1);
        var progressText = $"{downloadingText} \n{roundedValue}%";

        LoadingText.GetComponent<Text>().text = progressText;
    }

    void UpdateLoadingAssets()
    {
        if (loadingText == null) return;

        LoadingText.GetComponent<Text>().text = loadingText;
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
