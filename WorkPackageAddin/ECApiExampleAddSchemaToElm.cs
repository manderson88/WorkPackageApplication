using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WorkPackageApplication
{
    public partial class ECApiExampleAddSchemaToElm : Bentley.MicroStation.WinForms.Adapter // Form //
    {
        Bentley.Windowing.WindowManager m_windowManager;
        //Bentley.Windowing.WindowContent m_contentMgr;
        /// <summary>
        /// Constructor for the form.
        /// </summary>
        /// <param name="_host">the addin application that the form is associated to.</param>
        public ECApiExampleAddSchemaToElm(Bentley.MicroStation.AddIn _host)
        {
            InitializeComponent();
            m_windowManager = Bentley.Windowing.WindowManager.GetForMicroStation();

            this.Name = "AddSchemaToElement";
            this.AutoSize = true;

            //m_contentMgr = m_windowManager.DockPanel(this, this.Name, this.Text, Bentley.Windowing.DockLocation.Floating);
            AttachAsTopLevelForm(_host, true);
        }
        /// <summary>
        /// Add the schema that are in the file to a checked list.
        /// </summary>
        /// <param name="strSchemaName"></param>
        public void AddToSchemaList(String strSchemaName)
        {
            this.lbSchemasCB.Items.Add(strSchemaName, false);
        }
        /// <summary>
        /// get the selected schema names for futher work.
        /// </summary>
        /// <returns></returns>
        public IList<string> GetSchemaList()
        {
            List<string> pSchemas = new List<string>() ;
            for (int i = 0; i < lbSchemasCB.CheckedItems.Count; ++i)
            {
                string item = (string)lbSchemasCB.CheckedItems[i];
                pSchemas.Add(item.ToString());
            }
            return pSchemas;
        }
        /// <summary>
        /// launch the comman to add empty classes to the graphic.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdBtnAddToElm_Click(object sender, EventArgs e)
        {
            KeyinCommands.ECApiExampleAddClassesToElement("");
        }
    }
}
