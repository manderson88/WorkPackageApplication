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
    class EditWidgetCmd:BCOM.ILocateCommandEvents
    {
        #region "private variables"
        // These will get used over and over, so just get them once.
        private Bentley.MicroStation.AddIn m_AddIn;
        private BCOM.Application m_App;
        private ECSR.RepositoryConnection m_connection;
        private plcWidgetSettings pForm;
        ECOI.IECInstance iInstance;
        #endregion

        internal static void StartEditWidgetCmd(BM.AddIn pAddIn)
        {
            //These are needed because it is a static method
            Bentley.MicroStation.AddIn m_AddIn = pAddIn;
            BCOM.Application m_App = WorkPackageAddin.ComApp;
            EditWidgetCmd command = new EditWidgetCmd(m_AddIn);
            BCOM.CommandState commandState = m_App.CommandState;
            commandState.StartLocate(command);
        }
            #region "constructors"
        
        private EditWidgetCmd() { }
        public EditWidgetCmd(BM.AddIn pAddIn)
        {
            m_App = WorkPackageAddin.ComApp;
            m_AddIn = pAddIn;
        }
        
        #endregion

        #region ILocateCommandEvents Members
        /// <summary>
        /// This utility method contains the code to retrieve the instance information that is associated to an element.
        /// The first step is to create the instance id (PEP)
        /// Next create the class to look for.
        /// Use the QueryHelper class to simplify the creation of a query for the specific item.
        /// Use the persistence service to run the query.
        /// The result returned is an IECInstance (if there were possibly more than one it is a collection)
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
        public void Accept(BCOM.Element Element, ref BCOM.Point3d Point, BCOM.View View)
        {
         
          iInstance = GetSingleInstance(Element);
          if (iInstance!=null)
          {
              string _tagInfo = iInstance.GetAsString("Tag");
              string _mfgInfo = iInstance.GetAsString("WidgetManufacturer");
              //MessageBox.Show("The tag info is " + _tagInfo + " and the mfg info is " + _mfgInfo); 
              pForm.mfgName = _mfgInfo;
              pForm.tagInfo = _tagInfo;
          }
          
            //string _tagInfo = pInstance.GetAsString("Tag");
            //string _mfgInfo = pInstance.GetAsString("WidgetManufacturer");
            //MessageBox.Show("The tag info is " + _tagInfo + " and the mfg info is " + _mfgInfo); 
            //pForm.mfgName = _mfgInfo;
            //pForm.tagInfo = _tagInfo;
          iInstance.SetAsString("Tag", pForm.tagInfo);
          iInstance.SetAsString("WidgetManufacturer", pForm.mfgName);

        }

        public void Cleanup()
        {
            iInstance.SetAsString("Tag", pForm.tagInfo);
            iInstance.SetAsString("WidgetManufacturer", pForm.mfgName);

            WorkPackageAddin.CloseConnection(m_connection);
            pForm.DetachFromMicroStation();
            pForm.Dispose();
        }

        public void Dynamics(ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
        {
            throw new NotImplementedException();
        }

        public void LocateFailed()
        {
            throw new NotImplementedException();
        }

        public void LocateFilter(BCOM.Element Element, ref BCOM.Point3d Point, ref bool Accepted)
        {
            throw new NotImplementedException();
        }

        public void LocateReset()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            m_connection = WorkPackageAddin.OpenConnection();
            pForm = new plcWidgetSettings(m_AddIn);
            pForm.AttachAsTopLevelForm(m_AddIn, true);
        }

        #endregion
    }
}
