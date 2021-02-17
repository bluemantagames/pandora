using System.Collections;
using System.Collections.Generic;
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
        LoadingText.GetComponent<Text>().text = $"Downloading... {progress * 100}%";
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
