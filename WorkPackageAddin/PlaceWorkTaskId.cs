/*--------------------------------------------------------------------------------------+
|
|     $Source: WorkPackageAddin.cs,v $
|    $RCSfile: WorkPackageAddin.cs,v $
|   $Revision: 1.1 $
|
|  $Copyright: (c) 2013 Bentley Systems, Incorporated. All rights reserved. $
|
+--------------------------------------------------------------------------------------*/
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
    public enum workTasks {BoltUp,FieldWeld};
    public enum wtOrientations {Horizontal,Vertical};
    public enum wtType {BeamToBeam, BeamToConcrete, PipeToPipe, PipeToValve };
   

    class PlaceWorkTaskId : BCOM.IPrimitiveCommandEvents
    {
        #region "Private Variables"

		// These will get used over and over, so just get them once.
		private Bentley.MicroStation.AddIn  m_AddIn;
		private BCOM.Application m_App;
        //private string workTaskType="";
        //private string workTaskSize="";
       // private string workTaskOrientation="";//true for vertical false for horizontal
        private ECSR.RepositoryConnection m_connection;
        private plcWorkTaskSettings m_toolsettings;
        //private BCOM.LineElement m_PointElement = null;
        private bool bFirstSelected;
        private bool bSecondSelected;
        public BCOM.Element m_firstElmSelected;
        public BCOM.Element m_secondElmSelected;
		#endregion
        /// <summary>
        /// hide the default constructor
        /// </summary>
        private PlaceWorkTaskId (){}
        /// <summary>
        /// the constructor will take a reference to the host application
        /// </summary>
        /// <param name="addIn"></param>
        internal PlaceWorkTaskId(Bentley.MicroStation.AddIn addIn)
        {
            //Initialize class variables
			m_AddIn = addIn;
			m_App = WorkPackageAddin.ComApp;
        }
        /// <summary>
        /// a Static method to be called from the keyin.  This method will construct an instance of the class
        /// and start the command class.
        /// </summary>
        /// <param name="addIn"></param>
        internal static void PlaceWorkTaskCommand (Bentley.MicroStation.AddIn addIn)
        {
           Bentley.MicroStation.AddIn pAddIn = addIn;
			BCOM.Application pApp = WorkPackageAddin.ComApp;

			//Create a PlaceRouteCommand object
            PlaceWorkTaskId pwidCommand = new PlaceWorkTaskId(addIn);
            BCOM.CommandState commandState = pApp.CommandState;
			pApp.CommandState.StartPrimitive (pwidCommand, false);
			// Record the name that is saved in the Undo buffer and shown as the prompt.
			pApp.CommandState.CommandName = "Place WorkTask Command";
        }
        /// <summary>
        /// a method to attach the ECAttributes to the element that is being placed.
        /// </summary>
        /// <param name="pMarker"></param>
        private void AttachECData(BCOM.Element pMarker)
        {
            ECOI.IECInstance pInstance = WorkPackageAddin.CreateECInstance("SWAWorkTask.01.00", "WorkTaskInfo", m_connection);
            pInstance.SetAsString ("WorkTaskType",m_toolsettings.GetWTType());  //access string and value string
            pInstance.SetAsString ("WorkTaskOrientation",m_toolsettings.GetWTOrientation());
            pInstance.SetAsString("WorkTaskProcess", m_toolsettings.GetWTDescription());
            pInstance.SetAsString ("WorkTaskBasicDimension",m_toolsettings.GetWTDimension());
            pInstance.SetAsString("RootElement", m_firstElmSelected.ID.ToString());
            pInstance.SetAsString("ConnectionElement", m_secondElmSelected.ID.ToString());

            pInstance.InstanceId = BDGNP.DgnECPersistence.CreatePartialInstanceId(m_connection, (IntPtr)m_App.ActiveModelReference.MdlModelRefP(), (ulong)pMarker.ID);
            
            // Get a reference to the PersistenceService, which is the ECFramework persistence API
            ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            ECP.ChangeSet changes = new ECP.ChangeSet();
            changes.Add(pInstance, ECP.ChangeSetElementState.New);
            persistenceService.CommitChangeSet(m_connection, changes);    
        }
        #region IPrimitiveCommandEvents Members
        /// <summary>
        /// called when the command is terminated.  Need to remove the items from the toolsettings
        /// dialog.
        /// </summary>
        void BCOM.IPrimitiveCommandEvents.Cleanup()
        {
            m_toolsettings.DetachFromMicroStation();
            m_toolsettings.Dispose();
        }
        /// <summary>
        /// called on the data point in a view window.
        /// this creates a line with a user data linkage that will also have EC Attributes.
        /// </summary>
        /// <param name="Point"></param>
        /// <param name="View"></param>
        void BCOM.IPrimitiveCommandEvents.DataPoint(ref BCOM.Point3d Point, BCOM.View View)
        {
            if (false == bFirstSelected)
            {
                m_firstElmSelected = m_App.CommandState.LocateElement(Point, View, true);
                bFirstSelected = true;
                return;
            }
            if (false == bSecondSelected)
            {
                m_secondElmSelected = m_App.CommandState.LocateElement(Point, View, true);
                bSecondSelected = true;
                //Optional Start Dynamics b/c we are ready to show elements 
                m_App.CommandState.StartDynamics();
                m_App.CommandState.SetDefaultCursor();
                return;
            }

            BCOM.LineElement oLine = m_App.CreateLineElement2(null, Point, Point);
            oLine.LineWeight = 5;
            BCOM.DataBlock emptyBlock = new BCOM.DataBlockClass();
            oLine.AddUserAttributeData(22848, emptyBlock);
            m_App.ActiveModelReference.AddElement(oLine);
            AttachECData(oLine);
        }
        /// <summary>
        /// called when the moust moves.
        /// </summary>
        /// <param name="Point"></param>
        /// <param name="View"></param>
        /// <param name="DrawMode"></param>
        void BCOM.IPrimitiveCommandEvents.Dynamics(ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
        {
            BCOM.LineElement oLine = m_App.CreateLineElement2(null, Point, Point);
            oLine.LineWeight = 5;
            oLine.Redraw(DrawMode);
        }
        /// <summary>
        /// called when information is key'd into the MicroStation keyin dialog.
        /// </summary>
        /// <param name="Keyin"></param>
        void BCOM.IPrimitiveCommandEvents.Keyin(string Keyin)
        {
        }
        /// <summary>
        /// called on a reset mouse click
        /// </summary>
        void BCOM.IPrimitiveCommandEvents.Reset()
        {
            bFirstSelected = false;
            bSecondSelected = false;
        }
        /// <summary>
        /// called at the start of the command.  The toolsettings is populated at this time.
        /// </summary>
        void BCOM.IPrimitiveCommandEvents.Start()
        {
            //Instantiate the Form and Tell Microstation it is a tool settings
            m_connection = WorkPackageAddin.OpenConnection();
            //Show Prompts etc.
            bSecondSelected = false;
            bFirstSelected = false;
            m_App.ShowCommand("Place Work Task Point");
            m_App.ShowPrompt("Select Root Element");
            m_App.CommandState.SetLocateCursor();
            //Enables Accusnap for this command if the user has it enabled in Microstation
            m_App.CommandState.EnableAccuSnap();
            m_toolsettings = new plcWorkTaskSettings(m_AddIn);
            // m_toolsettings.AttachAsTopLevelForm(m_AddIn, true);
            m_toolsettings.AttachToToolSettings(m_AddIn);
        }

        #endregion
    }
}
