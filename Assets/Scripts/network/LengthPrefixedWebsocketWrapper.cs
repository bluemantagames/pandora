using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace Pandora.Network
{
    public class LengthPrefixedWebsocketWrapper
    {
        public ClientWebSocket Underlying;
        
        public int MessageCount = 0;

        public LengthPrefixedWebsocketWrapper(ClientWebSocket client)
        {
            Underlying = client;
        }


        public async Task<byte[]> Receive()
        {
            var sizeBytes = new Byte[4];

            await Underlying.ReceiveAsync(new ArraySegment<byte>(sizeBytes), CancellationToken.None);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(sizeBytes);
            }

            var size = BitConverter.ToInt32(sizeBytes, 0);
            var messageBuffer = new Byte[size];

            var result = await Underlying.ReceiveAsync(new ArraySegment<byte>(messageBuffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await Underlying.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                return new byte[0];
            }

            MessageCount++;

            return messageBuffer;
        }
    }

}