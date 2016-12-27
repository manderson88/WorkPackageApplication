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
    /// <summary>
    /// a form to display the list of classes that can be added to an element.
    /// The list is based on the list of schema names that are selected.
    /// </summary>
    public partial class classListForm : Bentley.MicroStation.WinForms.Adapter //Form //
    {
        Bentley.Windowing.WindowManager m_windowManager;
        //Bentley.Windowing.WindowContent m_contentMgr;
        public classListForm(Bentley.MicroStation.AddIn _host)
        {
            InitializeComponent();
            m_windowManager = Bentley.Windowing.WindowManager.GetForMicroStation();

            this.Name = "AddClassToElement";
            this.AutoSize = true;

          //  m_contentMgr = m_windowManager.DockPanel(this, this.Name, this.Text, Bentley.Windowing.DockLocation.Floating);
            AttachAsTopLevelForm(_host, true);
        }
        /// <summary>
        /// returns the list of the classes that are selected in the ui.
        /// </summary>
        /// <returns></returns>
        public IList<string> GetSelectedClasses()
        {
            List<string> pClasses = new List<string>();
            for (int i = 0; i < lbClassListCB.CheckedItems.Count; ++i)
            {
                string item = (string)lbClassListCB.CheckedItems[i];
                pClasses.Add(item.ToString());
            }
            return pClasses;
        }
        /// <summary>
        /// adds the class name to the list
        /// </summary>
        /// <param name="pClassName"></param>
        public void AddClassToLB(string pClassName)
        {
            this.lbClassListCB.Items.Add(pClassName, true);
        }
        /// <summary>
        /// checks to see if the class name is selected.  This is used because the code to get the class object is working 
        /// from the schema name.
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public Boolean isClassSelected(string targetName)
        {
            Boolean isFound = false;
            isFound = lbClassListCB.CheckedItems.Contains(targetName);
            return isFound;
        }
    }
}
