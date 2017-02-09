using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BIM = Bentley.Interop.MicroStationDGN;

namespace WorkPackageApplication
{
    public partial class LocationInformationFrm : Form //Bentley.MicroStation.WinForms.Adapter //
    {
        List<LocationInformation> itemList;
        BindingList<LocationInformation> bindingList;
        BindingSource source;

        public LocationInformationFrm(Bentley.MicroStation.AddIn _host, List<LocationInformation> eList)
        {

            itemList = eList;
            bindingList = new BindingList<LocationInformation>(itemList);
            source = new BindingSource(bindingList, null);

            InitializeComponent();

            dgLocationInfo.AllowUserToAddRows = false;
            dgLocationInfo.RowHeadersVisible = false;
            dgLocationInfo.DataSource = source;
        }
        public void ClearData()
        {
            //  dgvElements.Rows.Clear();
            dgLocationInfo.DataSource = null;
        }
        public void SetData(List<LocationInformation> eList)
        {
            itemList = eList;
            bindingList = new BindingList<LocationInformation>(itemList);
            source = new BindingSource(bindingList, null);
            dgLocationInfo.DataSource = source;
        }
    }
}
