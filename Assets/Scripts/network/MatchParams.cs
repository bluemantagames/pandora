using System.Collections.Generic;

public class MatchParams {
    public string Username { get; }
    public List<string> Deck { get; }

    public MatchParams(string username, List<string> deck) {
        Username = username;
        Deck = deck;
    }
}