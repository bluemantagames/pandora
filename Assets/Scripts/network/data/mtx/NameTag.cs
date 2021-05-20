namespace Pandora.Network.Data.Mtx {
    public class NameTag {
        public string Id;

        public NameTag() {}

        public NameTag(Pandora.Messages.NameTag protoNameTag) =>
            new NameTag {
                Id = protoNameTag.NametagId
            };
    }
}