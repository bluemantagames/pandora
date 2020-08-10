namespace Pandora.Resource.Gold.Rewards {
    public interface GoldReward {
        string Id { get; }

        void RewardApply(MapComponent map, int team, int playerId);
    }
}