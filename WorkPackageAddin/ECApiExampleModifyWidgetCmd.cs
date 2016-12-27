using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
#endregion
namespace WorkPackageApplication
{
    /// <summary>
    /// A simple class that will present a dilaog to update the 
    /// ECAttrs for the DGNECPluginBasics schema
    /// </summary>
    public class ECApiExampleModifyWidgetCmd : BCOM.ILocateCommandEvents
    {
        #region "private variables"
        // These will get used over and over, so just get them once.
        private Bentley.MicroStation.AddIn m_AddIn;
        private BCOM.Application m_App;
        private ECSR.RepositoryConnection m_connection;
        public plcWidgetSettings pForm;
        private ECOI.IECInstance m_iInstance;
        #endregion
        /// <summary>
        /// The public entry point to create the locate class.
        /// </summary>
        /// <param name="pAddIn"></param>
        internal static void StartModifyCommand(BM.AddIn pAddIn)
        {
            //These are needed because it is a static method
            Bentley.MicroStation.AddIn m_AddIn = pAddIn;
			BCOM.Application m_App = WorkPackageAddin.ComApp;
            ECApiExampleModifyWidgetCmd command = new ECApiExampleModifyWidgetCmd(m_AddIn);
            BCOM.CommandState commandState = m_App.CommandState;
            commandState.StartLocate(command);
        }

        #region "constructors"
        
        private ECApiExampleModifyWidgetCmd() { }
        public ECApiExampleModifyWidgetCmd(BM.AddIn pAddIn)
        {
            m_App = WorkPackageAddin.ComApp;
            m_AddIn = pAddIn;
        }
        /// <summary>
        /// Public method that is called from the UI to update the EC data.
        /// </summary>
        public void UpdateECdata()
        {
            m_iInstance.SetAsString("Tag", pForm.tagInfo);
            m_iInstance.SetAsString("WidgetManufacturer", pForm.mfgName);
            using (ECP.ChangeSet changesMade = new ECP.ChangeSet())
            {
                Bentley.EC.Persistence.PersistenceService psvc = Bentley.EC.Persistence.PersistenceServiceFactory.GetService();
                changesMade.Add(m_iInstance, ECP.ChangeSetElementState.Modified);
                psvc.CommitChangeSet(m_connection, changesMade);
            }

        }
        #endregion
        /// <summary>
        /// gets the Instance data information for the DGNECPluginBasics
        /// </summary>
        /// <param name="pElement"></param>
        /// <returns></returns>
        private ECOI.IECInstance GetSingleInstance(BCOM.Element pElement)
        {
            string instanceID = BDGNP.DgnECPersistence.CreatePartialInstanceId(m_connection, (System.IntPtr)pElement.ModelReference.MdlModelRefP(), (ulong)pElement.ID);
            ECOS.IECClass pClassDef;
            ECOS.IECSchema pSchema = WorkPackageAddin.LocateExampleSchema(m_connection, "DgnECPluginBasics.01.00");
            pClassDef = pSchema["Widget"];

            ECPQ.ECQuery pQuery = ECPQ.QueryHelper.CreateQueryForInstanceId(pClassDef, instanceID, true);
            Bentley.EC.Persistence.PersistenceService psvc = Bentley.EC.Persistence.PersistenceServiceFactory.GetService();
            ECP.QueryResults pResult = psvc.ExecuteQuery(m_connection, pQuery, 100);

            ECOI.IECInstance iInstance;

            if (!pResult.TryGetElement(0, out iInstance))
                return null;

            return iInstance;
        }
        #region ILocateCommandEvents Members

        public void Accept(BCOM.Element Element, ref BCOM.Point3d Point, BCOM.View View)
        {
            //ECOI.IECInstance iInstance;
            m_iInstance = null;
            m_iInstance = GetSingleInstance(Element);
            if (null != m_iInstance)
            {
                //show form.
                pForm = new plcWidgetSettings(m_AddIn);
                string _tagInfo="";
                string _mfgInfo="";
                try
                {
                    _tagInfo = m_iInstance.GetAsString("Tag");
                }catch (Bentley.ECObjects.ECObjectsException e)
                {
                    Debug.Print(e.ToString());
                }

                try
                {
                    _mfgInfo = m_iInstance.GetAsString("WidgetManufacturer");
                }
                catch (Bentley.ECObjects.ECObjectsException e)
                {
                    Debug.Print(e.ToString());
                }
                //MessageBox.Show("The tag info is " + _tagInfo + " and the mfg info is " + _mfgInfo); 
                pForm.mfgName = _mfgInfo;
                pForm.tagInfo = _tagInfo;
                pForm.hostCommand=this;
                pForm.AttachAsTopLevelForm(m_AddIn, true);
                pForm.Show();
            }
            else
            {
                m_App.CadInputQueue.SendKeyin("WorkPackageAddin Open");
            }
        }

        public void Cleanup()
        {
            WorkPackageAddin.CloseConnection(m_connection);
            if (null != pForm)
            {
                pForm.DetachFromMicroStation();
                pForm.Dispose();
            }
        }

        public void Dynamics(ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
        {
        }

        public void LocateFailed()
        {
        }

        public void LocateFilter(BCOM.Element Element, ref BCOM.Point3d Point, ref bool Accepted)
        {
        }

        public void LocateReset()
        {
        }

        public void Start()
        {
            //Instantiate the Form and Tell Microstation it is a tool settings
            m_connection = WorkPackageAddin.OpenConnection();
        }

        #endregion
    }
}
