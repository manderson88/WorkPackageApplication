using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BW = Bentley.Windowing;
using BMW = Bentley.MicroStation.WinForms;

namespace WorkPackageApplication
{
    /// <summary>
    /// a class to put into the toolsettins dialog.
    /// this class will have the UI to gather information from the user
    /// </summary>
    public partial class plcWorkTaskSettings : BMW.Adapter //Form //
    {
        private Bentley.MicroStation.AddIn m_host;
        private string m_type;//these are the bolt or weld
        private string m_orientation; //horiz of vert
        private string m_dimension; //not used.
        private string m_process; //BB BC PP PV
        /// <summary>
        /// a constructor to pass in the host application.
        /// </summary>
        /// <param name="_host"></param>
        public plcWorkTaskSettings(Bentley.MicroStation.AddIn _host)
        {
            InitializeComponent();
            cbWTType.DataSource = Enum.GetValues(typeof(workTasks));
            cbWTOrientation.DataSource = Enum.GetValues(typeof(wtOrientations));
            cbtDescription.DataSource = Enum.GetValues(typeof(wtType));
            m_host = _host;
            this.Name = "Place Work Task ID";
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

        }
        /// <summary>
        /// gets the value of the work task type
        /// </summary>
        /// <returns></returns>
        public string GetWTType()
        {
            return m_type;
        }
        public string GetWTDescription()
        {
            return m_process;
        }
        /// <summary>
        /// gets the value of the work task orientation
        /// </summary>
        /// <returns></returns>
        public string GetWTOrientation()
        {
            return m_orientation;
        }
        /// <summary>
        /// gets the value of the work task diameter.
        /// </summary>
        /// <returns></returns>
        public string GetWTDimension()
        {
            return m_dimension;
        }
        /// <summary>
        /// the method that is called when the combo box changes value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WTTChanged(object sender, EventArgs e)
        {
            m_type = cbWTType.Text;
        }
        /// <summary>
        /// the method that is called when the combo box changes value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WTOChanged(object sender, EventArgs e)
        {
            m_orientation = cbWTOrientation.Text;
        }
        /// <summary>
        /// the method that is called when the focus leaves the text field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtWTDimension_Leave(object sender, EventArgs e)
        {
            m_dimension = txtWTDimension.Text;
        }

        private void WTDChanged(object sender, EventArgs e)
        {
            m_process = cbtDescription.Text;
        }
    }
}
