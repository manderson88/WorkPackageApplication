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
    /// <summary>
    /// a simple locate command.  The list of classes are presented.  
    /// Select the classes to add to the selected element.
    /// </summary>
    internal class cmdAddClass : BCOM.ILocateCommandEvents
    {
        /// the private variables that are used in the application.
        #region "private variables"
        // These will get used over and over, so just get them once.
        private Bentley.MicroStation.AddIn m_AddIn;
        private BCOM.Application m_App;
        private ECSR.RepositoryConnection m_connection;
        private classListForm pForm;
        #endregion
        /// <summary>
        /// a static method to attach the command to.
        /// </summary>
        /// <param name="pAddIn"></param>
        internal static void StartLocateCommand(BM.AddIn pAddIn)
        {
            //These are needed because it is a static method
            Bentley.MicroStation.AddIn m_AddIn = pAddIn;
            BCOM.Application m_App = WorkPackageAddin.ComApp;
            cmdAddClass command = new cmdAddClass(m_AddIn);
            BCOM.CommandState commandState = m_App.CommandState;
            commandState.StartLocate(command);
            
        }

        #region "constructors"

        private cmdAddClass() { }
        /// <summary>
        /// the constructor for the command.
        /// </summary>
        /// <param name="pAddIn"></param>
        public cmdAddClass(BM.AddIn pAddIn)
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
            m_connection = WorkPackageAddin.OpenConnection();
            pForm = new classListForm(m_AddIn);
            System.Collections.Generic.IList<string> pSchemaList = null;
            //need to get the list from the first dialog to open this next one.
            //only get the selected schema to build the class list.
            try
            {
                pSchemaList = WorkPackageAddin.GetForm().GetSchemaList();
            }
            catch (System.NullReferenceException e)
            {//>?\
                Debug.Print(e.ToString());
                //the open command needs to run first to allow selection of the schema to attach.
                Bentley.MicroStation.InteropServices.Utilities.ComApp.CadInputQueue.SendKeyin("ecapiexample open");
                return;
            }

            for (int i = 0; i < pSchemaList.Count; ++i)
            {
                System.Collections.IEnumerator pClasses = WorkPackageAddin.GetClassesInSchema (WorkPackageAddin.LocateExampleSchema (m_connection,pSchemaList[i].ToString()));
                while (pClasses.MoveNext())
                {
                    ECOS.IECClass pClass = (ECOS.IECClass)pClasses.Current;
                    pForm.AddClassToLB(pClass.Name);
                }
            }
            pForm.Show();
        }

        /// <summary>
        /// The method called when the selected element is accepted.
        /// </summary>
        /// <param name="pElement"></param>
        /// <param name="pPoint"></param>
        /// <param name="iView"></param>
        public void Accept(BCOM.Element pElement, ref BCOM.Point3d pPoint, BCOM.View iView)
        {
            //here is where to add the ecdata...
            System.Collections.Generic.IList<string> pSchemaList = WorkPackageAddin.GetForm().GetSchemaList();
            for (int i = 0; i < pSchemaList.Count; ++i)
            {
                System.Collections.IEnumerator pClasses = WorkPackageAddin.GetClassesInSchema(WorkPackageAddin.LocateExampleSchema(m_connection, pSchemaList[i].ToString()));
                while (pClasses.MoveNext())
                {
                    ECOS.IECClass pClass = (ECOS.IECClass)pClasses.Current;
                    //compare to see if the class is selected then go through the instaniation process.
                    if (pForm.isClassSelected (pClass.Name))
                    {
                    ECOI.IECInstance pInstance = WorkPackageAddin.CreateECInstance(pClass.Schema.FullName, pClass.Name, m_connection);
                    //some classes are non instantiable so we will not get a class and must skip this.
                    if (pInstance != null)
                        {
                        pInstance.InstanceId = BDGNP.DgnECPersistence.CreatePartialInstanceId(m_connection, (IntPtr)m_App.ActiveModelReference.MdlModelRefP(), (ulong)pElement.ID);

                        // Get a reference to the PersistenceService, which is the ECFramework persistence API
                        ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
                        ECP.ChangeSet changes = new ECP.ChangeSet();
                        changes.Add(pInstance, ECP.ChangeSetElementState.New);
                        persistenceService.CommitChangeSet(m_connection, changes);
                        }
                    }
                }
            }
            /* */
        }
        /// <summary>
        /// called when the current command is being removed.  This allows us to detach the toolsettings dialog and close the connection to the data source.
        /// </summary>
        public void Cleanup()
        {
            pForm.Dispose();
            WorkPackageAddin.CloseConnection(m_connection);
        }
        public void Dynamics(ref BCOM.Point3d pPoint, BCOM.View iView, BCOM.MsdDrawingMode drawMode) { }
        public void LocateFailed() { }
        public void LocateFilter(BCOM.Element pElement, ref BCOM.Point3d pPoint, ref bool accept) { }
        public void LocateReset() { }
        #endregion ILocateCommandEvents
    }
}
