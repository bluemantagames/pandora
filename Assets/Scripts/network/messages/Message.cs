namespace CRclone.Network.Messages {
    public interface Message {
        byte[] ToBytes(string matchToken);
    }
}