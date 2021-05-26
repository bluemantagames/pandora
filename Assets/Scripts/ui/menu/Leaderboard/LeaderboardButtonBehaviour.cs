using Pandora.UI.Modal;
using UnityEngine;
using Pandora;

namespace Pandora.UI.Menu.Leaderboard
{
    public class LeaderboardButtonBehaviour : MonoBehaviour
    {
        public LeaderboardViewBehaviour LeaderboardView;

        private MenuModalBehaviour modalContainer;

        void Awake()
        {
            var mainCanvas = GameObject.Find(Constants.MAIN_CANVAS_OBJECT_NAME).gameObject;
            modalContainer = mainCanvas.GetComponentInChildren<MenuModalBehaviour>();

            var leaderboardCanvas = LeaderboardView?.GetComponent<Canvas>();
            if (leaderboardCanvas != null) leaderboardCanvas.enabled = false;
        }

        public void OpenLeaderboard()
        {
            if (LeaderboardView == null) return;

            var leaderboardContainerCopy = Instantiate(LeaderboardView, transform.parent, true);
            var leaderboardCopyCanvas = leaderboardContainerCopy.GetComponent<Canvas>();
            var leaderboardView = leaderboardContainerCopy.GetComponent<LeaderboardViewBehaviour>();

            leaderboardView.LoadLeaderboard().Forget();

            modalContainer.AppendComponent(leaderboardContainerCopy.gameObject, new Vector3(1.3f, 1.3f, 1.3f));

            if (leaderboardCopyCanvas != null) leaderboardCopyCanvas.enabled = true;

            modalContainer.Show();
        }
    }
}