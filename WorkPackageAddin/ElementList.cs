using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
#region "Bentley Namespaces"
using BMW = Bentley.MicroStation.WinForms;
using BMI = Bentley.MicroStation.InteropServices;
using BCOM = Bentley.Interop.MicroStationDGN;
using BM = Bentley.MicroStation;
using ECP = Bentley.EC.Persistence;
using ECPQ = Bentley.EC.Persistence.Query;
using ECO = Bentley.ECObjects;
using ECOI = Bentley.ECObjects.Instance;
using ECOS = Bentley.ECObjects.Schema;
using ECOX = Bentley.ECObjects.XML;
using ECSR = Bentley.ECSystem.Repository;
using ECSS = Bentley.ECSystem.Session;
using BECPC = Bentley.ECPlugin.Common;
using BDGNP = Bentley.DGNECPlugin;
using System.Diagnostics;
using Bentley.EC.Controls;
#endregion

namespace WorkPackageApplication
{
    /// <summary>
    /// this is a form to display element information returned from a query.
    /// </summary>
    public partial class ElementList : Bentley.MicroStation.WinForms.Adapter //Form
    {
        List<TagItemSet> itemList;
        BindingList<TagItemSet> bindingList;
        BindingSource source;
        ItemInfoForm m_Form;
        Bentley.MicroStation.AddIn m_host;
        
        //DataGridViewCheckBoxColumn checkColumn;
        public ElementList(Bentley.MicroStation.AddIn _host, List<TagItemSet> eList)
        {
            itemList = eList;
            bindingList = new BindingList<TagItemSet>(itemList);
            source = new BindingSource(bindingList, null);
            m_host = _host;
            InitializeComponent();
            //checkColumn = new DataGridViewCheckBoxColumn();
            //checkColumn.Name = "Hilite";
            //checkColumn.HeaderText = "Hilite";
            //checkColumn.Width = 50;
            ////checkColumn.ReadOnly = false;
            //checkColumn.TrueValue = true;
            //checkColumn.FalseValue = false;
            //checkColumn.FillWeight = 10; //if the datagridview is resized (on form resize) the checkbox won't take up too much; value is relative to the other columns' fill values
            //dgvElements.Columns.Add(checkColumn);
            
            
            dgvElements.AllowUserToAddRows = false;
            dgvElements.RowHeadersVisible = false;
            dgvElements.DataSource = source;
            dgvElements.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCellHandler);       
        }
        
        public Form parent { get; set; }

        public void ClearData()
        {
          //  dgvElements.Rows.Clear();
            dgvElements.DataSource = null;
        }
        public void SetData(List<TagItemSet> eList)
        {
            itemList = eList;
            bindingList = new BindingList<TagItemSet>(itemList);
            source = new BindingSource(bindingList, null);
            dgvElements.DataSource = source;
        }
        /// <summary>
        /// populates the classes attached to an element.
        /// </summary>
        /// <param name="pElement"></param>
        /// <returns></returns>
        private List<DataInfo> populateData(BCOM.Element pElement)
        {
            int _dataLength=0;
            List<DataInfo> eList = new List<DataInfo>();
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            System.Collections.Generic.IList<ECOI.IECInstance> pInstances = BDGNP.DgnECPersistence.GetAllInstancesOnElement(conn, (System.IntPtr)pElement.MdlElementRef(), (System.IntPtr)pElement.ModelReference.MdlModelRefP(), ECP.LoadModifiers.IncludeECQueryBackedDescriptor, ECP.LoadModifiers.IncludeECQueryBackedDescriptor, 2, null);
            foreach (ECOI.IECInstance pInstance in pInstances)
                if (pInstance.ContainsValues)
                {
                    System.Collections.Generic.IEnumerator<ECOI.IECPropertyValue> pVals = pInstance.GetEnumerator(true);
                    while (pVals.MoveNext())
                        if (!pVals.Current.IsNull)
                        {
                            DataInfo dInfo = new DataInfo();
                            //Debug.WriteLine(string.Format("the property is {0} is {1}", pVals.Current.AccessString, pVals.Current.XmlStringValue));
                            dInfo.PropName = pVals.Current.AccessString;
                            _dataLength += dInfo.PropName.Length;
                            dInfo.PropValue = pVals.Current.XmlStringValue;
                            _dataLength += dInfo.PropValue.Length;
                            eList.Add(dInfo);
                        }
                }
            WorkPackageAddin.CloseConnection(conn);
            WorkPackageAddin.ComApp.MessageCenter.AddMessage(string.Format("the length is {0}", _dataLength), string.Format("the length is {0}", _dataLength), BCOM.MsdMessageCenterPriority.Info, false);
            return eList;
        }
        /// <summary>
        /// this zooms to the element if the check box is selected.
        /// </summary>
        /// <param name="pElement"></param>
        private void ZoomToElement(BCOM.Element pElement)
        {
            BCOM.View oView;
            oView = WorkPackageAddin.ComApp.CommandState.LastView();
            BCOM.Range3d rng;
            BCOM.Point3d pntZoom;
            rng = pElement.Range;
            BCOM.Point3d objZoom;
            objZoom.X = (rng.High.X - rng.Low.X) * 1.5;
            objZoom.Y = (rng.High.Y - rng.Low.Y) * 1.5;
            objZoom.Z = (rng.High.Z - rng.Low.Z) * 1.5;

            pntZoom.X = rng.High.X - rng.Low.X;
            pntZoom.Y = rng.High.Y - rng.Low.Y;
            pntZoom.Z = rng.High.Z - rng.Low.Z;
            
            //oView.set_Origin(rng.Low);
            oView.set_Extents(objZoom);
            oView.Redraw();

            pntZoom = WorkPackageAddin.ComApp.Point3dAddScaled(rng.Low, pntZoom, 0.5);
           
            oView.ZoomAboutPoint(pntZoom, 1.0);
            oView.Redraw();
        }
        /// <summary>
        /// handles the double click on a cell in the data grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void dgvCellHandler(object sender, DataGridViewCellEventArgs e)
        {
            string elid;
            elid = dgvElements.Rows[e.RowIndex].Cells["filePos"].Value.ToString();
            string modelId = dgvElements.Rows[e.RowIndex].Cells["modelID"].Value.ToString();
            int modelPtr = Convert.ToInt32(modelId);

            BCOM.ModelReference oModel = WorkPackageAddin.ComApp.MdlGetModelReferenceFromModelRefP(modelPtr);
            BCOM.Element el = oModel.GetElementByID(Convert.ToInt32(elid));
           
            List<DataInfo> iList = populateData(el);

            if (m_Form != null)
                m_Form.Dispose();

            m_Form = new ItemInfoForm(m_host, iList);

            DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)dgvElements.Rows[e.RowIndex].Cells["Hilite"];
            if ((chk.Value == chk.TrueValue) && (el.IsGraphical))
                ZoomToElement(el);

            m_Form.AttachAsTopLevelForm(m_host, true);
            m_Form.Show();
        }

        private void closingElementList(object sender, FormClosingEventArgs e)
        {
            ClassesToFind cf = (ClassesToFind)parent;
            if(cf != null)
                cf.m_lstForm = null;
        }
    }
}
