using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using WPWebSockets.Common;
//using System.Windows.Forms;
namespace WPWebSockets
{
        public class WebSocketLogger : IWebSocketLogger
        {
            public string m_message;
           
            public WebSocketLogger(string txt) : base() { m_message = txt; }
            public void Information(Type type, string format, params object[] args)
            {
                m_message = format;
                Trace.TraceInformation(format, args);
            }

            public void Warning(Type type, string format, params object[] args)
            {
                Trace.TraceWarning(format, args);
            }

            public void Error(Type type, string format, params object[] args)
            {
                Trace.TraceError(format, args);
            }

            public void Error(Type type, Exception exception)
            {
                Error(type, "{0}", exception);
            }
        }
}
