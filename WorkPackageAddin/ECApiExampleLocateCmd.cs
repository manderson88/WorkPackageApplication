#region "System Namespaces"
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using SRI=System.Runtime.InteropServices;
#endregion 

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
using System.Text;
#endregion

namespace WorkPackageApplication
{
    /// <summary>
    /// a simple locate command.  The graphic with the information is selected 
    /// then a dialog is populated with the attribute info.
    /// </summary>
    internal class ECApiExampleLocateCmd : BCOM.ILocateCommandEvents
       {

        #region "private variables"
        // These will get used over and over, so just get them once.
        private Bentley.MicroStation.AddIn m_AddIn;
        private BCOM.Application m_App;
        private ECSR.RepositoryConnection m_connection;
        //private plcWidgetSettings pForm;
        private ItemInfoForm pForm;

        #endregion
        private static bool traverseMethod(ECOS.IECClass baseClass, object arg)
        {
            Debug.WriteLine(string.Format("the class is {0} ", baseClass.Name));
            System.Collections.Generic.IEnumerator<ECOS.IECProperty> pFields = baseClass.GetEnumerator(true);
            
            return true;
        }
        private static bool GetClassNamesForElement(BCOM.Element pElement, 
                                                    StringBuilder clsName, 
                                                    ECSR.RepositoryConnection conn)
        {
            StringBuilder sb = new StringBuilder();
       
            string instID = BDGNP.DgnECPersistence.CreateInstanceId (conn,(System.IntPtr)pElement.ModelReference.MdlModelRefP(),(ulong)pElement.ID,"");

            System.Collections.Generic.IList<ECOI.IECInstance> pInstances = BDGNP.DgnECPersistence.GetAllInstancesOnElement(conn, (System.IntPtr)pElement.MdlElementRef(), (System.IntPtr)pElement.ModelReference.MdlModelRefP(), ECP.LoadModifiers.None, ECP.LoadModifiers.None, 1, null);
            //if there are no instances close the connection and return
            if (pInstances.Count == 0)
            {
                //WorkPackageAddin.CloseConnection(conn);
                return false;
            }

            foreach (ECOI.IECInstance pInstance in pInstances)
            {
                string[] fieldNames = new string[4];
                
                try
                {
                    fieldNames[0] = string.Format(" -- {0}", pInstance.ClassDefinition.Name);
                }
                catch (Bentley.ECObjects.ECObjectsException.PropertyNotFound nfe) 
                    { 
                        Debug.WriteLine(nfe.Message); 
                    }
                try
                {
                    fieldNames[1] = string.Format(" -- {0}", pInstance["ID"].XmlStringValue);   
                }
                catch (Bentley.ECObjects.ECObjectsException.PropertyNotFound nfe) 
                    { 
                        Debug.WriteLine(nfe.Message); 
                    }
                try
                {
                    fieldNames[2] = string.Format(" -- {0}", pInstance["Description"].XmlStringValue);  
                }
                catch (Bentley.ECObjects.ECObjectsException.PropertyNotFound nfe) 
                    { 
                        Debug.WriteLine(nfe.Message); 
                    }
                try
                {
                    fieldNames[3] = string.Format(" -- {0}", pInstance["NAME"].XmlStringValue);
                }
                catch (Bentley.ECObjects.ECObjectsException.PropertyNotFound nfe) 
                    { 
                        Debug.WriteLine(nfe.Message); 
                    }
                
                sb.Append(fieldNames[3]);
                sb.Append(fieldNames[2]);
                sb.Append(fieldNames[1]);
                sb.Append(fieldNames[0]);

                LocateClass.TraverseRelationship(pInstance, conn);

            }

            //WorkPackageAddin.CloseConnection(conn);
            clsName = sb;
            return true;
        }
        /// <summary>
        /// a static method that is the entry point for the command
        /// </summary>
        /// <param name="pAddIn"> the host addin needed for the form</param>
        internal static void StartLocateCommand(BM.AddIn pAddIn)
        {
            //These are needed because it is a static method
            Bentley.MicroStation.AddIn m_AddIn = pAddIn;
            BCOM.Application m_App = WorkPackageAddin.ComApp;
            ECApiExampleLocateCmd command = new ECApiExampleLocateCmd(m_AddIn);
            BCOM.CommandState commandState = m_App.CommandState;

            // if there is no fence or selection set then start a locate command.
            if ((!WorkPackageAddin.ComApp.ActiveDesignFile.Fence.IsDefined)&&
                (!WorkPackageAddin.ComApp.ActiveModelReference.AnyElementsSelected))
                commandState.StartLocate(command);
            else //iterate through the elements to get the properties?
            {
                ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();

                if (WorkPackageAddin.ComApp.ActiveDesignFile.Fence.IsDefined)
                {
                    BCOM.Fence oFence = WorkPackageAddin.ComApp.ActiveDesignFile.Fence;
                    int fenceCount = oFence.GetContents().BuildArrayFromContents().Length;
                    //tell how many elements and then proceed
                    if (DialogResult.OK == MessageBox.Show(
                        string.Format("the fence count is {0}", fenceCount), 
                        "Element Count", MessageBoxButtons.OKCancel))
                    {
                        List<TagItemSet> elList = new List<TagItemSet>();
                        BCOM.ElementEnumerator els = oFence.GetContents();
                        while (els.MoveNext())
                        {
                            StringBuilder clsNames = new StringBuilder();
                           if(GetClassNamesForElement(els.Current,clsNames,conn))
                           {
                           TagItemSet tset = new TagItemSet(
                               els.Current.ModelReference.MdlModelRefP(), 
                               els.Current.ID, els.Current.ModelReference.Name, 
                               clsNames.ToString(), "");
                           elList.Add(tset);
                           }
                        }
                        ElementList elsfrm = new ElementList(m_AddIn, elList);
                        elsfrm.Show();
                    }
                }
                else //must have a selection set.
                {
                    BCOM.ModelReference oModel = WorkPackageAddin.ComApp.ActiveModelReference;
                    int selectedCount = oModel.GetSelectedElements().BuildArrayFromContents().Length;
                    //tell how many elements
                    if (DialogResult.OK == MessageBox.Show(string.Format(
                        "the element count is {0}", selectedCount), 
                        "Element  Count", MessageBoxButtons.OKCancel))
                    {
                        List<TagItemSet> elList = new List<TagItemSet>();
                        BCOM.ElementEnumerator els = oModel.GetSelectedElements();
                        while (els.MoveNext())
                        {
                            StringBuilder clsNames = new StringBuilder();
                            if (GetClassNamesForElement(els.Current,clsNames,conn))
                            {
                            TagItemSet tset = new TagItemSet(els.Current.ModelReference.MdlModelRefP(), 
                                els.Current.ID, els.Current.ModelReference.Name,
                                clsNames.ToString(),"");
                            elList.Add(tset);
                            }
                        }
                        ElementList elsFrm = new ElementList(m_AddIn, elList);
                        elsFrm.Show();
                    }
                }
                WorkPackageAddin.CloseConnection(conn);
            }
        }

        #region "constructors"
        
        private ECApiExampleLocateCmd() { }
        public ECApiExampleLocateCmd(BM.AddIn pAddIn)
        {
            m_App = WorkPackageAddin.ComApp;
            m_AddIn = pAddIn;
        }
        
        #endregion

        #region "ILocateCommandEvents"
        /// <summary>
        /// The entry to the command This will open the connection to the data source and create the form to display.
        /// </summary>
        public void Start()
        {
            BCOM.LocateCriteria _locateCriteria;
            _locateCriteria = m_App.CommandState.CreateLocateCriteria(false);
            m_App.CommandState.SetLocateCriteria(_locateCriteria);

            m_connection = WorkPackageAddin.OpenConnection();
        }

        private void locCmd_textChanged(object sender, EventArgs e)
        {

        }
       /// <summary>
       /// populates the list of element properties for the class
       /// </summary>
       /// <param name="pElement"></param>
       /// <param name="clsName"></param>
       /// <returns></returns>
        private List<DataInfo> populateData(BCOM.Element pElement, out string clsName)
        {
            clsName = "None Found";
            List<DataInfo> eList = new List<DataInfo>();
            System.Collections.Generic.IList<ECOI.IECInstance> pInstances = 
                BDGNP.DgnECPersistence.GetAllInstancesOnElement(m_connection, 
                (System.IntPtr)pElement.MdlElementRef(), 
                (System.IntPtr)pElement.ModelReference.MdlModelRefP(), 
                ECP.LoadModifiers.None, ECP.LoadModifiers.None, 1, null);
            foreach(ECOI.IECInstance pInstance in pInstances)
            if (pInstance.ContainsValues)
            {
                clsName = pInstance.ClassDefinition.Name;
                System.Collections.Generic.IEnumerator<ECOI.IECPropertyValue> pVals = pInstance.GetEnumerator(true);
                while (pVals.MoveNext())
                    if (!pVals.Current.IsNull)
                    {
                        DataInfo dInfo = new DataInfo();
                        Debug.WriteLine(string.Format("the property is {0} is {1}", 
                            pVals.Current.AccessString, pVals.Current.XmlStringValue));
                        dInfo.PropName = pVals.Current.AccessString;
                        dInfo.PropValue = pVals.Current.XmlStringValue;
                        eList.Add(dInfo);
                    }
            }
            return eList;
        }
        /// <summary>
        /// The method called when the selected element is accepted.
        /// </summary>
        /// <param name="pElement"></param>
        /// <param name="pPoint"></param>
        /// <param name="iView"></param>
        public void Accept(BCOM.Element pElement, ref BCOM.Point3d pPoint, BCOM.View iView)
        {
            //destroy the old one.
            if (pForm != null)
                pForm.Dispose();
            string clsName;
            List<DataInfo> eList = populateData(pElement,out clsName);
            pForm = new ItemInfoForm(m_AddIn, eList);
            pForm.Text = clsName;
            pForm.AttachAsTopLevelForm(m_AddIn, true);
            pForm.Show();
             
        }
        /// <summary>
        /// called when the current command is being removed.  This allows us to detach the toolsettings dialog and close the connection to the data source.
        /// </summary>
        public void Cleanup ()
        {
            WorkPackageAddin.CloseConnection(m_connection);
            if (pForm != null)
            {
                pForm.DetachFromMicroStation();
                pForm.Dispose();
            }
        }
        public void Dynamics(ref BCOM.Point3d pPoint,BCOM.View iView,BCOM.MsdDrawingMode drawMode) {}
        public void LocateFailed () {}
        public void LocateFilter (BCOM.Element pElement,ref BCOM.Point3d pPoint,ref bool accept)
        {        }
        public void LocateReset () { }
        #endregion ILocateCommandEvents
    }
}
