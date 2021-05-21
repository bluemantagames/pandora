using System;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Pandora.UI.Menu.Home
{
    public enum MatchmakingWarning
    {
        NotEnoughCards
    }

    public class MatchmakingWarningTextBehaviour : MonoBehaviour
    {
        public LocalizedString LocalizedNotEnoughCardsWarn;
        string notEnoughCardsWarn = null;
        MatchmakingWarning? currentWarning = null;

        public void SetWarning(MatchmakingWarning? warning)
        {
            currentWarning = warning;

            ShowWarning(currentWarning);
        }

        void Awake()
        {
            LocalizedNotEnoughCardsWarn.GetLocalizedString().Completed += (handle) =>
                notEnoughCardsWarn = handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : null;
        }

        void Update()
        {
        }

        void ShowWarning(MatchmakingWarning? warning)
        {
            switch (warning)
            {
                case MatchmakingWarning.NotEnoughCards:
                    Display(notEnoughCardsWarn);
                    break;

                default:
                    Display("");
                    break;
            }
        }

        void Display(string text)
        {
            GetComponent<Text>().text = text;
        }
    }
}