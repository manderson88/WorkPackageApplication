using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
namespace WorkPackageApplication
{
    /// <summary>
    /// a form to select a schema, class and property to query the current ECRepository
    /// </summary>
    public partial class QueryForm : Bentley.MicroStation.WinForms.Adapter //Form //
    {
        public string m_schemaName;
        public string m_className;
        public string m_propName;
        public string m_value;
        public Boolean m_bAllClasses;
        public Boolean m_bUseLike;

        public QueryForm(Bentley.MicroStation.AddIn _host)
        {
            InitializeComponent();
            PopulateFields();
            cbxSchemas.Sorted = true;
            cbxClasses.Sorted = true;
            cbxProperty.Sorted = true;
        }
        /// <summary>
        /// found this sample code on dynamically setting the width of the drop down.
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
                {
                    maxWidth = temp;
                }
            }
            return maxWidth;
        }
        /// <summary>
        /// populates the combo box of schema names.
        /// </summary>
        public void PopulateFields()
        {
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            string[] schemaNames = persistenceService.GetSchemaFullNames(conn);
            
            for (int i=0;i<schemaNames.Length;++i)
                cbxSchemas.Items.Add(schemaNames[i]);

            WorkPackageAddin.CloseConnection(conn);
            cbxSchemas.DropDownWidth = DropDownWidth(cbxSchemas) > 0 ? DropDownWidth(cbxSchemas) : cbxSchemas.DropDownWidth;
        }
        /// <summary>
        /// on the selected entry populates the combo box for classes available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxSchemas_SelectedIndexChanged(object sender, EventArgs e)
        {
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            string schemaName = cbxSchemas.GetItemText(cbxSchemas.SelectedItem);
            ECOS.IECSchema pSchema = WorkPackageAddin.LocateExampleSchema(conn, schemaName);
            string [] clsNames = WorkPackageAddin.GetAllClassNamesInSchema(pSchema);
            
            cbxClasses.Items.Clear();
            cbxProperty.Items.Clear();
            cbxProperty.SelectedIndex = -1;

            for (int i=0;i<clsNames.Length;++i)
                cbxClasses.Items.Add(clsNames[i]);

            WorkPackageAddin.CloseConnection(conn);
            cbxClasses.DropDownWidth = DropDownWidth(cbxClasses) > 0 ? DropDownWidth(cbxClasses) : cbxClasses.DropDownWidth;
        }
        /// <summary>
        /// on selecting a class this populates the properties that are available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            string schemaName = cbxSchemas.GetItemText(cbxSchemas.SelectedItem);
            ECOS.IECSchema pSchema = WorkPackageAddin.LocateExampleSchema(conn, schemaName);
            string clsName = cbxClasses.GetItemText(cbxClasses.SelectedItem);
            ECOS.IECClass pClass = pSchema.GetClass(clsName);
            string clsProp;
            IEnumerator<ECOS.IECProperty> clsEnum = pClass.GetEnumerator();
            
            cbxProperty.Items.Clear();
            cbxProperty.SelectedIndex = -1;

            while(clsEnum.MoveNext())
            {
                ECOS.IECProperty prop = clsEnum.Current;
                clsProp = prop.Name;
                cbxProperty.Items.Add(clsProp);
            }
            WorkPackageAddin.CloseConnection(conn);
            cbxProperty.DropDownWidth = DropDownWidth(cbxProperty) > 0 ? DropDownWidth(cbxProperty) : cbxProperty.DropDownWidth;
        }
        /// <summary>
        /// runs the query that has been selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKBtn_Click(object sender, EventArgs e)
        {
            m_schemaName = cbxSchemas.GetItemText(cbxSchemas.SelectedItem);
            m_className = cbxClasses.GetItemText(cbxClasses.SelectedItem);
            m_propName = cbxProperty.GetItemText(cbxProperty.SelectedItem);
            m_value = txtValue.Text;
            m_bAllClasses = cbAllClasses.Checked;
            m_bUseLike = cbLike.Checked;
            this.Hide();
        }

    }
}
