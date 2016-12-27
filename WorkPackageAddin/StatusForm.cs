using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Reflection;

namespace WorkPackageApplication
{
    static class ControlExtension
    {
        delegate void SetPropertyValueHandler<TResult>(Control source,Expression<Func<Control,TResult>> selector,TResult value);

        public static void SetPropertyValue<TResult>(this Control source, Expression<Func<Control, TResult>> selector, TResult value)
        {
            if (source.InvokeRequired)
            {
                var del = new SetPropertyValueHandler<TResult>(SetPropertyValue);
                source.Invoke(del, new object[] { source, selector, value });
            }
            else
            {
                var propInfo = ((MemberExpression)selector.Body).Member as PropertyInfo;
                propInfo.SetValue(source, value, null);
            }

        }

        public static void UIThread(this Control @this, Action code)
        {
            if (@this.InvokeRequired)
                @this.BeginInvoke(code);
            else
                code.Invoke();
        }
    }

    public partial class StatusForm : Bentley.MicroStation.WinForms.Adapter //Form //
    {
        public string m_msg;

        public StatusForm()
        {
            InitializeComponent();
        }
        //public static void UIThread(this Control @this, Action code)
        //{
        //    if (@this.InvokeRequired)
        //        @this.BeginInvoke(code);
        //    else
        //        code.Invoke();
        //}
        public void SetStatus(string msg)
        {
            //this.UIThread(() => this.lblStatusMsg.Text = msg);
            lblStatusMsg.Invalidate();
            this.lblStatusMsg.SetPropertyValue(a => a.Text, msg);
        }
    }
}
