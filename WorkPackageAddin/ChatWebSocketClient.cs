using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSockets.Events;
using WebSockets.Common;
using System.Diagnostics;

namespace WorkPackageApplication
{
    public class ChatWebSocketClient : WebSockets.Client.WebSocketClient
    {
        public ChatWebSocketClient(bool noDelay, IWebSocketLogger logger)
            : base(noDelay, logger)
        {

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
