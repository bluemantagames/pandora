using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Analytics;

namespace Pandora
{
    public class AnalyticsSingleton
    {
        static AnalyticsSingleton _instance = null;
        bool isEnabled = false;

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
            isEnabled = !Debug.isDebugBuild;

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

            var value = eventValue.ToString();

            AnalyticsEvent.Custom(eventName, new Dictionary<string, object> { { "value", value } });
        }
    }
}
