using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPWebSockets.Events;
using WPWebSockets.Common;
using System.Diagnostics;

namespace WorkPackageApplication
{
    public class ChatWebSocketClient : WPWebSockets.Client.WebSocketClient
    {
        public ChatWebSocketClient(bool noDelay, IWebSocketLogger logger,Int64 _UUID)
            : base(noDelay, logger, _UUID)
        {

        }
        public void SendClose()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("Client Close");
            base.Send(WebSocketOpCode.ConnectionClose, buffer);
        }
        public override void Send(string text)
        {
            if (text != null)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(text);
                base.Send(WebSocketOpCode.TextFrame, buffer);
            }
        }
        public void SendPing()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("PING");
            base.Send(WebSocketOpCode.Ping, buffer);
        }
        public void SendPong()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("PONG");
            base.Send(WebSocketOpCode.Pong, buffer);
        }
    }
}
