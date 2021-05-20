namespace Pandora.Network.Data.Mtx {
    public class CosmeticsMtx {
        public NameTag NameTag;


        public CosmeticsMtx() {}

        public CosmeticsMtx(Pandora.Messages.Cosmetics protoCosmetics) {
            NameTag = new NameTag(protoCosmetics?.Nametag);
        }

    }
}