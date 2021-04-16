using Pandora.UI.Modal;
using UnityEngine;
using Pandora;

public class LeaderboardButtonBehaviour : MonoBehaviour
{
    public GameObject LeaderboardContainer;

    private MenuModalBehaviour modalContainer;

    void Awake()
    {
        var mainCanvas = GameObject.Find(Constants.MAIN_CANVAS_OBJECT_NAME).gameObject;
        modalContainer = mainCanvas.GetComponentInChildren<MenuModalBehaviour>();

        var leaderboardCanvas = LeaderboardContainer?.GetComponent<Canvas>();
        if (leaderboardCanvas != null) leaderboardCanvas.enabled = false;
    }

    public void OpenLeaderboard()
    {
        if (LeaderboardContainer == null) return;

        var leaderboardContainerCopy = Instantiate(LeaderboardContainer, transform.parent, true);
        var leaderboardCopyCanvas = leaderboardContainerCopy.GetComponent<Canvas>();

        modalContainer.AppendComponent(leaderboardContainerCopy);
        if (leaderboardCopyCanvas != null) leaderboardCopyCanvas.enabled = true;

        modalContainer.Show();
    }
}
