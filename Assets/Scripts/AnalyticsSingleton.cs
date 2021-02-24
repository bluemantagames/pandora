using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Analytics;

namespace Pandora
{
    public class AnalyticsSingleton
    {
        static AnalyticsSingleton _instance = null;
        bool isEnabled = false;

        static public string LOGIN_SCENE = "login_scene";
        static public string LOGIN = "login";
        static public string MENU_VIEW_CHANGE = "screen_visit"; // Unity standard event
        static public string MATCHMAKING_START = "matchmaking_start";
        static public string MATHCMAKING_MATCH_FOUND = "matchmaking_match_found";
        static public string MATCHMAKING_MATCH_START = "matchmaking_match_start";

        static public AnalyticsSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AnalyticsSingleton();
                }

                return _instance;
            }
        }

        private AnalyticsSingleton()
        {
            isEnabled = true;

            if (isEnabled)
            {
                Logger.Debug("Started analytics integration");
            }
            else
            {
                Logger.Debug("Disabled analytics integration");
            }
        }

        /// <summary>
        /// Track a custom event
        /// </summary>
        /// <param name="eventName">The event name</param>
        public void TrackEvent(string eventName)
        {
            if (!isEnabled) return;

            Debug.Log($"Tracking {eventName}");

            AnalyticsEvent.Custom(eventName);
        }

        /// <summary>
        /// Track a custom event with a numeric value
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="eventValue">The event value</param>
        public void TrackEvent(string eventName, float eventValue)
        {
            if (!isEnabled) return;

            Debug.Log($"Tracking {eventName}");

            var value = eventValue.ToString();

            AnalyticsEvent.Custom(eventName, new Dictionary<string, object> { { "value", value } });
        }

        /// <summary>
        /// Track a custom event with a custom set of associated values
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="eventValue">The event value</param>
        public void TrackEvent(string eventName, Dictionary<string, object> values)
        {
            if (!isEnabled) return;

            Debug.Log($"Tracking {eventName}");

            AnalyticsEvent.Custom(eventName, values);
        }
    }
}
