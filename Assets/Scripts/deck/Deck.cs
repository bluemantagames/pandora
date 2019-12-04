using Pandora.Deck.Event;
using Pandora.Events;
using System.Collections.Generic;

namespace Pandora.Deck {
    public interface Deck {
        /// </summary>Event bus where deck events are published</summary>
        EventBus<DeckEvent> EventBus { get; }

        /// <summary>Hand size</summary>
        int HandSize { get; }

        int MaxMulliganSize { get; }
        
        /// <summary>Deck currently played</summary>
        List<Card> Deck { get; }

        /// <summary>Take mulligan</summary>
        void MulliganTaken();
        
        /// <summary>Reject mulligan</summary>
        void MulliganReject();

    }
}