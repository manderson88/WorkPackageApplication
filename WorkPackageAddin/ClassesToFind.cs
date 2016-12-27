using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BCOM = Bentley.Interop.MicroStationDGN;
using BMI = Bentley.MicroStation.InteropServices;
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
namespace WorkPackageApplication
{
    /// <summary>
    /// This is a utility class to find things in the EC data
    /// </summary>
    public partial class ClassesToFind : Bentley.MicroStation.WinForms.Adapter // Form //
    {
        List<instancePair> m_itemList;
        BindingList<instancePair> bindingList;
        BindingSource source;
        string m_className;
        string m_schemaName;
        string m_field;
        string m_value;
        int m_relationOperator;
        public ElementList m_lstForm;
        /// <summary>
        /// the constructor for the winform
        /// </summary>
        /// <param name="_host">the host addin application</param>
        public ClassesToFind(Bentley.MicroStation.AddIn _host)
        {
            InitializeComponent();
            cbxClasses.Sorted = true;
            cbxFields.Sorted = true;
            cbxSchema.Sorted = true;
            cbxOperator.SelectedIndex = 5;
            m_relationOperator = 5;
            PopulateFields();
            m_itemList = new List<instancePair>();
            bindingList = new BindingList<instancePair>(m_itemList);
            source = new BindingSource(bindingList, null);
            dgvInstances.AllowUserToAddRows = false;
            dgvInstances.RowHeadersVisible = false;
            dgvInstances.DataSource = source;
            //dgvInstances.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvInstances.DataBindingComplete += (o, _) =>
            {
                var dataGridView = o as DataGridView;
                if (dataGridView != null)
                {
                    dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    dataGridView.Columns[dataGridView.ColumnCount - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            };
        }
        /// <summary>
        /// this will allow the  dropdown  to  size to the largest item  in the list
        /// 
        /// </summary>
        /// <param name="myCombo"></param>
        /// <returns></returns>
        int DropDownWidth(ComboBox myCombo)
        {
            int maxWidth = 0, temp = 0;
            foreach (var obj in myCombo.Items)
            {
                temp = TextRenderer.MeasureText(obj.ToString(), myCombo.Font).Width;
                if (temp > maxWidth)
                    maxWidth = temp;
            }
            return maxWidth;
        }
        /// <summary>
        /// Starts to populate the combo boxes.
        /// </summary>
        public void PopulateFields()
        {
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            string[] schemaNames = persistenceService.GetSchemaFullNames(conn);

            for (int i = 0; i < schemaNames.Length; ++i)
                cbxSchema.Items.Add(schemaNames[i]);

            WorkPackageAddin.CloseConnection(conn);
            cbxSchema.DropDownWidth =  DropDownWidth(cbxSchema)>0?DropDownWidth(cbxSchema):cbxSchema.DropDownWidth;
        }
        /// <summary>
        /// the schema combo box has been changed.  Now populate the classes combo box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxSchema_SelectedIndexChanged(object sender, EventArgs e)
        {
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            string schemaName = cbxSchema.GetItemText(cbxSchema.SelectedItem);
            ECOS.IECSchema pSchema = WorkPackageAddin.LocateExampleSchema(conn, schemaName);
            string[] clsNames = WorkPackageAddin.GetAllClassNamesInSchema(pSchema);
            cbxClasses.Items.Clear();
            cbxClasses.SelectedIndex = -1;
            cbxFields.Items.Clear();
            cbxFields.SelectedIndex = -1;

            for (int i = 0; i < clsNames.Length; ++i)
                cbxClasses.Items.Add(clsNames[i]);

            WorkPackageAddin.CloseConnection(conn);
            cbxClasses.DropDownWidth = DropDownWidth(cbxClasses)>0?DropDownWidth(cbxClasses):cbxClasses.DropDownWidth;

        }
        /// <summary>
        /// The button callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            m_schemaName = cbxSchema.GetItemText(cbxSchema.SelectedItem);
            m_className = cbxClasses.GetItemText(cbxClasses.SelectedItem);
            m_field = cbxFields.GetItemText(cbxFields.SelectedItem);
            m_value = txtValue.Text;
            m_itemList = LocateClass.FindInstanceByClassAndProperty(m_schemaName, m_className, m_field, m_value, m_relationOperator , false);
            bindingList = new BindingList<instancePair>(m_itemList);
            source = new BindingSource(bindingList, null);
            dgvInstances.DataSource = source;
        }
        /// <summary>
        /// The classname has been selected now find the fields to select from.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxClassName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            string schemaName = cbxSchema.GetItemText(cbxSchema.SelectedItem);
            m_schemaName = schemaName;
            string clsName = cbxClasses.GetItemText(cbxClasses.SelectedItem);
            m_className = clsName;
            ECOS.IECSchema pSchema = WorkPackageAddin.LocateExampleSchema(conn, schemaName);
            ECOS.IECClass pClass = pSchema.GetClass(clsName);
            string clsProp;
            IEnumerator<ECOS.IECProperty> clsEnum = pClass.GetEnumerator();
            cbxFields.Items.Clear();
            cbxFields.SelectedIndex = -1;
            while (clsEnum.MoveNext())
            {
                ECOS.IECProperty prop = clsEnum.Current;
                clsProp = prop.Name;
                cbxFields.Items.Add(clsProp);
            }
            WorkPackageAddin.CloseConnection(conn);

            int ddw = DropDownWidth(cbxFields);
            cbxFields.DropDownWidth = ddw > 0 ? ddw : cbxFields.DropDownWidth;
        }
        /// <summary>
        /// the Field has been selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_field = cbxFields.GetItemText(cbxFields.SelectedItem);
        }
        /// <summary>
        /// The callback for the double click event on the datagroup window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvInstances_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string instanceValue = dgvInstances.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            List<TagItemSet> elList = new List<TagItemSet>();
            //build the list of elements that fit the query.    
            elList = LocateClass.FindClassAndValueRelated(m_schemaName, m_className, m_field, instanceValue, true, false);
            if ((elList != null) && (elList.Count > 0))
            {
                for (int i = 0; i < elList.Count; ++i)
                {
                    //try to get the element and hilite it by putting it in a selection set.
                    try
                    {
                        BCOM.Element el=null;

                        if (elList[i].modelID != 0)
                        {
                            BCOM.ModelReference oModel = WorkPackageAddin.ComApp.MdlGetModelReferenceFromModelRefP((int)elList[i].modelID);
                            el = oModel.GetElementByID(elList[i].filePos);
                        }
                        else
                        {
                            el = WorkPackageAddin.ComApp.ActiveModelReference.GetElementByID(elList[i].filePos);
                        }

                        if ((el.IsGraphical)&&(!el.IsComponentElement))
                            WorkPackageAddin.ComApp.ActiveModelReference.SelectElement(el, true);
                    }
                    catch (System.Runtime.InteropServices.COMException ex) { Debug.WriteLine(ex.Message); }
                }

                if (m_lstForm == null)
                {
                    m_lstForm = new ElementList(WorkPackageAddin.MyAddin, elList);
                    m_lstForm.parent = this;
                }
                else
                {
                    m_lstForm.Hide();
                    m_lstForm.ClearData();
                    m_lstForm.SetData(elList);
                }

                m_lstForm.Show(this);
            }
        }
        /// <summary>
        /// select the relational operation to use.  This allows for more complex
        /// queries.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxOperator_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_relationOperator = cbxOperator.SelectedIndex;
        }                                                                                                                                  
    }
}
