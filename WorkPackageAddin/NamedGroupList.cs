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

namespace WorkPackageApplication
{
    public partial class NamedGroupList : Bentley.MicroStation.WinForms.Adapter //Form //
    {
        Bentley.Windowing.WindowManager m_windowManager;
        ECSR.RepositoryConnection m_connection;

        //Bentley.Windowing.WindowContent m_contentMgr;
        public NamedGroupList(Bentley.MicroStation.AddIn _host)
        {
            InitializeComponent();
            GatherNames();
            m_windowManager = Bentley.Windowing.WindowManager.GetForMicroStation();

            this.Name = "AddNamedGroupToElement";
            this.AutoSize = true;

          //  m_contentMgr = m_windowManager.DockPanel(this, this.Name, this.Text, Bentley.Windowing.DockLocation.Floating);
            AttachAsTopLevelForm(_host, true);
        }
       

      /*  public NamedGroupList()
        {
            InitializeComponent();
            GatherNames();
        }
       */

        private int GatherNames()
        {
            int namedGroupCount=0;
            BCOM.ElementScanCriteria oScP = new BCOM.ElementScanCriteriaClass();
            BCOM.ElementEnumerator ee;
            oScP.ExcludeAllTypes();
            oScP.IncludeType (BCOM.MsdElementType.NamedGroupHeader);
            ee = BMI.Utilities.ComApp.ActiveModelReference.Scan (oScP);

            while (ee.MoveNext())
            {
                namedGroupListbox.Items.Add (ee.Current.AsNamedGroupElement().Name,false);
                namedGroupCount++;
            };
            return namedGroupCount;
        }
        public IList<string> GetSelectedGroups()
        {
            List<string> pGrpList = new List<string>();
            for (int i = 0; i < namedGroupListbox.CheckedItems.Count; ++i)
            {
                string item = (string)namedGroupListbox.CheckedItems[i];
                pGrpList.Add(item.ToString());
            }
            return pGrpList;
        }
        private ECOI.IECInstance GetTargetInstance(BCOM.Element pElement)
        {
            System.Collections.Generic.IList<ECOI.IECInstance> pInstances = BDGNP.DgnECPersistence.GetAllInstancesOnElement(m_connection, (System.IntPtr)pElement.MdlElementRef(), (System.IntPtr)BMI.Utilities.ComApp.ActiveModelReference.MdlModelRefP(), 0, 0, 0, null);
            System.Collections.Generic.IEnumerator<ECOI.IECInstance> pEnum = pInstances.GetEnumerator();
            pEnum.MoveNext();
            return (ECOI.IECInstance)pEnum.Current;
        }
        private ECOI.IECRelationshipInstance GetMemberInfo(BCOM.NamedGroupElement ng, ECOI.IECInstance pInstance)
        {
            BCOM.NamedGroupMember[] members = ng.GetMembers();
            ECOI.IECRelationshipInstance pInstanceGroup = WorkPackageAddin.CreateRelationshipECInstance("ExampleNamedGrp.01.00", "",members.Length, m_connection);
            //ECOI.IECRelationshipInstanceCollection pcol = pInstanceGroup.GetRelationshipInstances();
            
            for (int i=0;i<members.Count();++i)
            {
                pInstanceGroup.Source = pInstance;  //this needs to be passed IN?
                pInstanceGroup.Target = GetTargetInstance(members[i].GetElement()); //this is found but what?
            }
            return pInstanceGroup;
        }
        private void AddECInstance(BCOM.NamedGroupElement ng)
        {
           
            string _schemaName = "ExampleNamedGrp.01.00";
            ECOI.IECInstance pInstance = WorkPackageAddin.CreateECInstance(_schemaName, "NamedGroupName", m_connection);
           // ECOI.IECRelationshipInstance pRInstance = WorkPackageAddin.CreateRelationshipECInstance (_schemaName,"NamedGroupName",m_connection);
            ECOI.IECRelationshipInstance  pRInstance = GetMemberInfo(ng,pInstance);
            //some classes are non instantiable so we will not get a class and must skip this.
            if (pRInstance != null)
            {
                pRInstance.InstanceId = BDGNP.DgnECPersistence.CreatePartialInstanceId (m_connection,(IntPtr)BMI.Utilities.ComApp.ActiveModelReference.MdlModelRefP(),(ulong)ng.ID);
                ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
                ECP.ChangeSet changes = new ECP.ChangeSet();
                changes.Add(pRInstance,ECP.ChangeSetElementState.New);
                persistenceService.CommitChangeSet (m_connection,changes);
         
            }
            
            
            if (pInstance != null)
            {
                //set the field name...
                pInstance.SetAsString("ItemSetName", ng.Name);

                pInstance.InstanceId = BDGNP.DgnECPersistence.CreatePartialInstanceId(m_connection, (IntPtr)BMI.Utilities.ComApp.ActiveModelReference.MdlModelRefP(), (ulong)ng.ID);

                // Get a reference to the PersistenceService, which is the ECFramework persistence API
                ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
                ECP.ChangeSet changes = new ECP.ChangeSet();
                changes.Add(pInstance, ECP.ChangeSetElementState.New);
                persistenceService.CommitChangeSet(m_connection, changes);
            }

        }
        /// <summary>
        /// apply the named group information to the named group element as ec attrs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NamedGroupApplyAttribute(object sender, EventArgs e)
        {
            BCOM.ElementScanCriteria oScP = new BCOM.ElementScanCriteriaClass();
            BCOM.ElementEnumerator ee;
            //BCOM.Element el = null;
            oScP.ExcludeAllTypes();
            oScP.IncludeType(BCOM.MsdElementType.NamedGroupHeader);
            ee = BMI.Utilities.ComApp.ActiveModelReference.Scan(oScP);
            m_connection = WorkPackageAddin.OpenConnection();
            IList<string> pGrpNames = GetSelectedGroups();
            for (int i = 0;i<pGrpNames.Count;i++)
            {
                while (ee.MoveNext())
                {
                    BCOM.NamedGroupElement pEl = ee.Current.AsNamedGroupElement();

                    if (pGrpNames[i].Equals (ee.Current.AsNamedGroupElement().Name))
                        AddECInstance(pEl);
                }
            }
            WorkPackageAddin.CloseConnection(m_connection);
        }
    }
}
