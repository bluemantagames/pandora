namespace Pandora.Network.Data.Mtx {
    public class CosmeticsMtx {
        public NameTag NameTag;


        public CosmeticsMtx() {}

        public CosmeticsMtx(Pandora.Messages.Cosmetics protoCosmetics) =>
            new CosmeticsMtx {
                NameTag = new NameTag(protoCosmetics.Nametag)
            };

    }
}