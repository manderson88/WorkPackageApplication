using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPWebSockets.Events;
using WPWebSockets.Common;
using System.Diagnostics;
using WPWebSockets;

namespace WorkPackageApplication
{
    class WSClientApp
    {
        public static string m_message;
        public static ChatWebSocketClient m_client;
        public static Uri m_uri = new Uri("ws://localhost:8880/WPS");
        public static void ChatClient(object state)
        {
            Uri uri = new Uri("ws://localhost:8880/chat");
            WebSocketLogger wsl = (WebSocketLogger)state;

            m_client = new ChatWebSocketClient(true, wsl, 0);
            m_client.TextFrame += Client_TextFrame;
            m_client.TextFrame += Client_TextFrame;
            m_client.ConnectionOpened += Client_ConnectionOpened;
            m_client.TextMultiFrame += Client_MultiFrame;
            m_client.Ping += Client_ping;
            m_client.Pong += Client_pong;

            
            m_message = wsl.m_message;
            // test the open handshake
            //try
            {
                m_client.OpenBlocking(uri);
            }
        }
        public static void TestClient(object state)
        {
            
            var logger = new WPWebSockets.WebSocketLogger("test");

            m_client =  new ChatWebSocketClient(true, logger,0);
           // m_client += Client_TextFrame;

            //m_client.OpenBlocking(m_uri);
            //m_client.Send("test message");

           // using (var client = new ChatWebSocketClient(false, logger))
            {
             //   if (null == m_client)
             //       m_client = client;
             //   else
             //       client = m_client;

                Uri uri = new Uri("ws://localhost:8880/WPS");
                m_client.TextFrame += Client_TextFrame;
                m_client.ConnectionOpened += Client_ConnectionOpened;
                m_client.TextMultiFrame += Client_MultiFrame;
                m_client.Ping += Client_ping;
                m_client.Pong += Client_pong;

                WebSocketLogger wsl = (WebSocketLogger)state;
                m_message = wsl.m_message;
                // test the open handshake
                //try
                {
                    m_client.OpenBlocking(uri);
                }
                //catch (Exception e) { Debug.Print(e.Message); }
                //m_client.Send(wsl.m_message);

                //m_client = client;
            }

        }

        private static void Client_ping(object sender, EventArgs e)
        {
        }
        private static void Client_pong(object sender, EventArgs e)
        {
        }
        private static void Client_ConnectionOpened(object sender, EventArgs e)
        {
            Trace.TraceInformation("Client: Connection Opened");
            //var client = (ChatWebSocketClient)sender;

            // test sending a message to the server
            if(m_message == null)
                m_message = " ";

            m_client.Send(m_message);
            //m_client.Dispose();
        }

        private static void Client_TextFrame(object sender, TextFrameEventArgs e)
        {
           // m_client.OpenBlocking(m_uri);
            
            m_message = e.Text;
            Trace.TraceInformation("Client: {0}", e.Text);

            //var client = (ChatWebSocketClient)sender;
            //m_client.Send(e.Text);
            // lets test the close handshake
            m_client.Dispose();
        }
        private static void Client_MultiFrame(object sender, TextMultiFrameEventArgs e)
        {
            Trace.TraceInformation("Multi Client: {0}", e.Text);

        }
        public static void SendPing()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("PING");
            m_client.SendPing();
        }
        public static void SendPong()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("PONG");
            m_client.SendPong();
        }
    }
}
