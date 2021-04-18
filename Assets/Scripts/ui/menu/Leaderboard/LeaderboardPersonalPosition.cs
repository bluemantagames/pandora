using Pandora;
using UnityEngine.UI;
using UnityEngine;

public class LeaderboardPersonalPosition : MonoBehaviour
{
    void Awake()
    {
        var leaderboardPosition = PlayerModelSingleton.instance.leaderboardPosition;
        var textComponent = GetComponent<Text>();

        textComponent.text = $"{leaderboardPosition}";
    }
}
