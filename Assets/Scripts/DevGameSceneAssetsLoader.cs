using UnityEngine;
using Pandora;
using Cysharp.Threading.Tasks;

public class DevGameSceneAssetsLoader : MonoBehaviour
{

    bool isExecuted = false;

    private async UniTask StartLoading()
    {
        await AddressablesSingleton.instance.LoadUnits();
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
