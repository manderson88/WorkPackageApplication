using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bentley.EC.Controls;
using BCOM = Bentley.Interop.MicroStationDGN;

namespace WorkPackageApplication
{
    public partial class ItemInfoForm :  Bentley.MicroStation.WinForms.Adapter //Form //
    {
        List<DataInfo> itemList;
        BindingList<DataInfo> bindingList;
        BindingSource source;

        public ItemInfoForm(Bentley.MicroStation.AddIn _host, List<DataInfo> eList)
        {
            itemList = eList;
            bindingList = new BindingList<DataInfo>(itemList);
            source = new BindingSource(bindingList, null);

            InitializeComponent();

            dgInfoViewer.AllowUserToAddRows = false;
            dgInfoViewer.RowHeadersVisible = false;
            dgInfoViewer.DataSource = source;
            dgInfoViewer.Columns["PropName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgInfoViewer.Columns["PropValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgInfoViewer.Columns["PropName"].SortMode = DataGridViewColumnSortMode.Automatic;
           // dgInfoViewer.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCellHandler);
        }
    }
}
