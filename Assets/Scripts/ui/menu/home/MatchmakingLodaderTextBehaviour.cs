using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MatchmakingLodaderTextBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public bool Enabled = false;
    public int TickTimeMs = 1000;

    public string SearchingText = "Searching";

    Text text;
    int ticks = 0;

    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Enabled) return;
    }

    public async UniTaskVoid StartLoader() {
        while (Enabled) {
            if (++ticks > 3) {
                ticks = 1;
            }

            text.text = string.Concat(SearchingText, new string('.', ticks));

            await UniTask.Delay(TickTimeMs);
        }
    }

    public void Disable() {
        Enabled = false;

        text.text = "";
    }

    public void Enable() {
        Enabled = true;

        _ = StartLoader();
    }
}
