using Pandora.Deck.Event;
using Pandora.Events;
using System.Collections.Generic;

namespace Pandora.Deck {
    public interface Deck {
        /// </summary>Event bus where deck events are published</summary>
        EventBus<DeckEvent> EventBus { get; }

        /// <summary>Hand size</summary>
        uint HandSize { get; }
        
        /// <summary>Deck currently played</summary>
        List<Card> Deck { get; }

        /// <summary>Take mulligan</summary>
        void TakeMulligan();
        
        /// <summary>Reject mulligan</summary>
        void RejectMulligan();

    }
}