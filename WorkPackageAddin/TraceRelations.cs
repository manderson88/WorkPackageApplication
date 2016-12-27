#region "System Namespaces"
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SRI = System.Runtime.InteropServices;
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
#endregion

namespace WorkPackageApplication
{
    class TraceRelations : BCOM.ILocateCommandEvents
    {
        #region "private variables"
        // These will get used over and over, so just get them once.
        private Bentley.MicroStation.AddIn m_AddIn;
        private BCOM.Application m_App;
        private ECSR.RepositoryConnection m_connection;
        #endregion

        internal static void StartLocateCommand(BM.AddIn pAddIn)
        {
            //These are needed because it is a static method
            Bentley.MicroStation.AddIn m_AddIn = pAddIn;
            BCOM.Application m_App = WorkPackageAddin.ComApp;
            ECApiExampleLocateCmd command = new ECApiExampleLocateCmd(m_AddIn);
            BCOM.CommandState commandState = m_App.CommandState;
            commandState.StartLocate(command);
        }

        private TraceRelations() { }
        public TraceRelations(BM.AddIn pAddIn)
        {
            m_App = WorkPackageAddin.ComApp;
            m_AddIn = pAddIn;
        }



        #region ILocateCommandEvents Members
        /// <summary>
        /// This utility method contains the code to retrieve the instance information that is associated to an element.
        /// The first step is to create the instance id (PEP)
        /// Next create the class to look for.
        /// Use the QueryHelper class to simplify the creation of a query for the specific item.
        /// Use the persistence service to run the query.
        /// The result returned is an IEEInstance (if there were possibly more than one it is a collection)
        /// </summary>
        /// <param name="pElement"></param>
        /// <returns></returns>
        private ECOI.IECInstance GetSingleInstance(BCOM.Element pElement)
        {
            string instanceID = BDGNP.DgnECPersistence.CreatePartialInstanceId(m_connection, (System.IntPtr)pElement.ModelReference.MdlModelRefP(), (ulong)pElement.ID);
            ECOS.IECClass pClassDef;
            ECOS.IECSchema pSchema = WorkPackageAddin.LocateExampleSchema(m_connection, "OpenPlant_3D.01.02");
            pClassDef = pSchema["Widget"];

            ECPQ.ECQuery pQuery = ECPQ.QueryHelper.CreateQueryForInstanceId(pClassDef, instanceID, true);
            Bentley.EC.Persistence.PersistenceService psvc = Bentley.EC.Persistence.PersistenceServiceFactory.GetService();
            ECP.QueryResults pResult = psvc.ExecuteQuery(m_connection, pQuery, 100);

            ECOI.IECInstance iInstance;

            if (!pResult.TryGetElement(0, out iInstance))
                return null;

            return iInstance;
        }

        private void TraverseRelationship(ECOI.IECInstance pInstance)
        {
            ECOI.IECInstance result;
            String         instanceId      = pInstance.InstanceId;
            ECPQ.ECInstanceIdExpression idExpression    = new ECPQ.ECInstanceIdExpression(instanceId);
            ECPQ.ECQuery   query           = new ECPQ.ECQuery(pInstance.ClassDefinition);
 
            query.SelectClause.SelectAllProperties = true;
            ECPQ.QueryHelper.SelectAllRelatedInstances(query, true, 1); 
        
            query.WhereClause.Add (idExpression);
 
        //------------
        Bentley.Collections.IExtendedParameters extendedParameters = Bentley.Collections.ExtendedParameters.Create ();
        System.Collections.Generic.List<System.String> providerFilterList = new System.Collections.Generic.List<System.String> ();
        providerFilterList.Add ("Bentley.DGNECPlugin");
        Bentley.ECSystem.ProviderFilter.SetIn (extendedParameters, new Bentley.ECSystem.ProviderFilter (true, providerFilterList));
        //--------------------
        Bentley.EC.Persistence.PersistenceService psvc = Bentley.EC.Persistence.PersistenceServiceFactory.GetService();
        ECP.QueryResults results = psvc.ExecuteQuery(m_connection, query, -1, extendedParameters);
        
       if(results.Count > 0 /*&& results->Count == 1*/)
          result = results.GetElement(0);
  

        }
        public void Accept(BCOM.Element Element, ref BCOM.Point3d Point, BCOM.View View)
        {
            ECOI.IECInstance pInstance = GetSingleInstance(Element);
            TraverseRelationship(pInstance);
        }

        public void Cleanup()
        {
            WorkPackageAddin.CloseConnection(m_connection);
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        #endregion
    }
}
