using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WorkPackageApplication
{
    public partial class plcOfficeInfoSettings :Bentley.MicroStation.WinForms.Adapter
    {
        private Bentley.MicroStation.AddIn m_host;

        public plcOfficeInfoSettings(Bentley.MicroStation.AddIn _host)
        {
            InitializeComponent();
            m_host = _host;
            this.Name = "Place Office Info";
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

        }

        private void lblOccupant_Click(object sender, EventArgs e)
        {

        }
    }
}
