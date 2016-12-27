using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using WebSockets.Events;
using WebSockets.Common;

namespace WorkPackageApplication
{
    public partial class ChatForm : Bentley.MicroStation.WinForms.Adapter // Form //
    {
       // ChatWebSocketClient m_client;
      //  Thread m_thread;
        private BackgroundWorker m_worker = null;

        private volatile string msg = "";
        public ChatForm(Bentley.MicroStation.AddIn _host)
        {
            InitializeComponent();
            if (m_worker == null)
            {
                m_worker = new BackgroundWorker();
                m_worker.DoWork += new DoWorkEventHandler(m_worker_DoWork);
                m_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_worker_RunWorkerCompleted);
            }

          /*  WebSockets.WebSocketLogger logger = new WebSockets.WebSocketLogger();
            m_client = new ChatWebSocketClient(true, logger);
           
            m_client.TextFrame += Client_TextFrame;
            m_client.TextMultiFrame += Client_MultiFrame;
            m_client.ConnectionOpened += Client_ConnectionOpened;*/
        }
        #region 'Background worker event handlers'

        private void m_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                //if (Convert.ToString(e.Result) == "BackgroundWorker")
                {
                    txtRtnMessage.Text= e.Result.ToString();
                }
               
            }
        }

        private void m_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WebSockets.WebSocketLogger logger = new WebSockets.WebSocketLogger(msg);

            WSClientApp.TestClient(logger);

            e.Result = e.Argument;
        }

        #endregion
/*
        private void Client_ConnectionOpened(object sender, EventArgs e)
        {
            Trace.TraceInformation("Client: Connection Opened");
            var client = (ChatWebSocketClient)sender;

            // test sending a message to the server
            client.Send("Hi");
        }

        private void Client_TextFrame(object sender, TextFrameEventArgs e)
        {
            Trace.TraceInformation("Client: {0}", e.Text);
            var client = (ChatWebSocketClient)sender;
            txtRtnMessage.Text = e.Text;
            // lets test the close handshake
            //client.Dispose();
        }
        private void Client_MultiFrame(object sender, TextMultiFrameEventArgs e)
        {
            Trace.TraceInformation("Multi Client: {0}", e.Text);

        }
*/
        private void btnSend_Click(object sender, EventArgs e)
        {
            msg = txtMessage.Text;
            m_worker.RunWorkerAsync(txtMessage.Text);

            /*
            WebSockets.WebSocketLogger logger = new WebSockets.WebSocketLogger(msg);

            Thread clientThread = new Thread(new ParameterizedThreadStart(WSClientApp.TestClient));
            clientThread.IsBackground = true;
            clientThread.Start(logger);
            m_thread = clientThread;
             */ 
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            txtRtnMessage.Text = WSClientApp.m_message;
            
        }

        private void txtRtnMessage_TextChanged(object sender, EventArgs e)
        {
            txtRtnMessage.Text = WSClientApp.m_message;
        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {
            //WSClientApp.m_message = txtMessage.Text;
        }

        private void btnPing_Click(object sender, EventArgs e)
        {
            
            //WSClientApp.m_client.SendPing();
        }

        private void btnClose_Click_1(object sender, EventArgs e)
        {
            WSClientApp.m_client.Dispose();
        }
    }
}
