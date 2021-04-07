using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MatchmakingLodaderTextBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public bool Enabled = false;
    public int TickTimeMs = 1000;
    public LocalizedString SearchingText;

    Text text;
    int ticks = 0;
    string prefixText = null;

    void Awake()
    {
        SearchingText.GetLocalizedString().Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                prefixText = handle.Result;
        };
    }

    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Enabled) return;
    }

    public async UniTaskVoid StartLoader()
    {
        while (Enabled)
        {
            if (++ticks > 3)
            {
                ticks = 1;
            }

            if (prefixText != null)
                text.text = string.Concat(prefixText, new string('.', ticks));

            await UniTask.Delay(TickTimeMs);
        }
    }

    public void Disable()
    {
        Enabled = false;

        text.text = "";
    }

    public void Enable()
    {
        Enabled = true;

        _ = StartLoader();
    }
}
