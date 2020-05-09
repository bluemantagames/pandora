using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pandora.Network
{
    public class LiveButton : MonoBehaviour
    {
        public bool GameSceneToLoad = false;
        public int SelectedTeam = 1;
        public GameObject Team1Button = null;
        public GameObject Team2Button = null;
        Color team1ButtonColor;
        Color team2ButtonColor;
        public Text MatchTokenObject;

        public void Connect()
        {
            var matchToken = MatchTokenObject.text;

            if (matchToken.Length <= 0) return;

            // Assign the team
            TeamComponent.assignedTeam = SelectedTeam;

            // Here starts the live
            ReplayControllerSingleton.instance.StartLive(matchToken);

            SceneManager.LoadScene("GameScene");
        }

        public void Start() {
            team1ButtonColor = Team1Button.GetComponent<Image>().color;
            team2ButtonColor = Team2Button.GetComponent<Image>().color;

            UpdateButtons();
        }

        void UpdateButtons() {
            var t1Alpha = SelectedTeam == 1 ? 1f : 0.3f;
            var t2Alpha = SelectedTeam == 2 ? 1f : 0.3f;

            Team1Button.GetComponent<Image>().color = 
                new Color(team1ButtonColor.r, team1ButtonColor.g, team1ButtonColor.b, t1Alpha);

            Team2Button.GetComponent<Image>().color = 
                new Color(team2ButtonColor.r, team2ButtonColor.g, team2ButtonColor.b, t2Alpha);
        }

        public void SetTeam1() {
            SelectedTeam = 1;
            UpdateButtons();
        }

        public void SetTeam2() {
            SelectedTeam = 2;
            UpdateButtons();
        }

        public void Cancel()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}