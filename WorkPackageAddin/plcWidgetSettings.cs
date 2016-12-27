using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BW = Bentley.Windowing;
using BMW = Bentley.MicroStation.WinForms;

namespace WorkPackageApplication
{
    public partial class plcWidgetSettings : BMW.Adapter //Form // 
    {
        Bentley.Windowing.WindowManager m_windowManager;
        public Bentley.Windowing.WindowContent m_contentMgr;
        private string m_widgetName;
        private string m_widgetID;
        private Bentley.MicroStation.AddIn m_host;
        ECApiExampleModifyWidgetCmd m_modCommand;
        /// <summary>
        /// a constructor for the Bentley Adapter class
        /// </summary>
        /// <param name="_host"></param>
        public plcWidgetSettings(Bentley.MicroStation.AddIn _host)
        {
            InitializeComponent();
            m_windowManager = Bentley.Windowing.WindowManager.GetForMicroStation();
            m_host = _host;
            btnUpdate.Hide();
            this.Name = "PlaceWidgetCmd";
            this.NETDockable = true;
            this.NETDocked = false;
            //this.AutoSize = true;
            //this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            m_contentMgr = m_windowManager.DockPanel(this, this.Name, this.Text, BW.DockLocation.Center);
         
            m_contentMgr.Undockable = true;
            
            this.AutoScroll = true;
            //m_contentMgr.AutoHideExpand();
        }
        /// <summary>
        /// gets the tag information
        /// </summary>
        public string tagInfo
        {
            get { return txtBoxTagID.Text; }
            set { txtBoxTagID.Text = value; }
        }
        /// <summary>
        /// gets the mfg name
        /// </summary>
        public string mfgName
        {
            get {return txtBoxMfgName.Text;}
            set { txtBoxMfgName.Text = value; }
        }
        public ECApiExampleModifyWidgetCmd hostCommand
        {
            set { m_modCommand = value;
            btnUpdate.Show();
            btnUpdate.Enabled = true;
            }
        }
       /* void setModifyCommand(ECApiExampleModifyWidgetCmd pCmd)
        {
            m_modCommand = pCmd;
            btnUpdate.Show();
            btnUpdate.Enabled = true;
        }*/
        /// <summary>
        /// invoked when the text item changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxMfgName_TextChanged(object sender, EventArgs e)
        {
            m_widgetName = txtBoxMfgName.Text;
        }

        private void txtBoxTagID_TextChanged(object sender, EventArgs e)
        {
            m_widgetID = txtBoxTagID.Text;
        }
 
        private void btnUpdate_Click(object sender, EventArgs e)
        {
         //not sure yet.
            if (null != m_modCommand)
                m_modCommand.UpdateECdata();
        }
    }
}
