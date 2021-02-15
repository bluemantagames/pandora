using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pandora.Network;

public class GameSceneAsyncLoader : MonoBehaviour
{
    bool isLoaded = false;

    void Update()
    {
        if (!isLoaded)
        {
            isLoaded = true;

            var networkController = NetworkControllerSingleton.instance;

            networkController.GameSceneLoading = SceneManager.LoadSceneAsync("GameScene");

            networkController.GameSceneLoading.allowSceneActivation = false;
        }
    }
}
