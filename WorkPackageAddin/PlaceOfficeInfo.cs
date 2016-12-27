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
   
   

    class PlaceOfficeInfo : BCOM.IPrimitiveCommandEvents
    {
        #region "Private Variables"

		// These will get used over and over, so just get them once.
		private Bentley.MicroStation.AddIn  m_AddIn;
		private BCOM.Application m_App;
        //private string m_occupant="";
        //private string m_officeID="";
        
        private ECSR.RepositoryConnection m_connection;
        private plcOfficeInfoSettings m_toolsettings;
        private BCOM.TextNodeElement m_node = null;
        
		#endregion
        /// <summary>
        /// hide the default constructor
        /// </summary>
        private PlaceOfficeInfo (){}
        /// <summary>
        /// the constructor will take a reference to the host application
        /// </summary>
        /// <param name="addIn"></param>
        internal PlaceOfficeInfo(Bentley.MicroStation.AddIn addIn)
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
        internal static void PlaceOfficeInfoCommand (Bentley.MicroStation.AddIn addIn)
        {
           Bentley.MicroStation.AddIn pAddIn = addIn;
			BCOM.Application pApp = WorkPackageAddin.ComApp;

			//Create a PlaceRouteCommand object
            PlaceOfficeInfo poiCommand = new PlaceOfficeInfo(addIn);
            BCOM.CommandState commandState = pApp.CommandState;
			pApp.CommandState.StartPrimitive (poiCommand, false);
			// Record the name that is saved in the Undo buffer and shown as the prompt.
			pApp.CommandState.CommandName = "Place Office Info Command";
        }
        /// <summary>
        /// a method to attach the ECAttributes to the element that is being placed.
        /// </summary>
        /// <param name="pMarker"></param>
        private void AttachECData(BCOM.Element pMarker)
        {
            ECOI.IECInstance pInstance = WorkPackageAddin.CreateECInstance("BentleyDemoSpaceInfo.01.00", "SpaceInfo", m_connection);
            pInstance.SetAsString ("Occupant",m_toolsettings.txtOccupant.Text);  //access string and value string
            pInstance.SetAsString ("OfficeID",m_toolsettings.txtOfficeID.Text);
       
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
            if ((m_toolsettings.txtOccupant.Text.Length > 0) && (m_toolsettings.txtOfficeID.Text.Length > 0))
            {
                BCOM.TextNodeElement oNode;
                oNode = m_App.CreateTextNodeElement1(null, Point, View.get_Rotation());
                oNode.AddTextLine(m_toolsettings.txtOccupant.Text);
                oNode.AddTextLine(m_toolsettings.txtOfficeID.Text);
                m_App.ActiveModelReference.AddElement(oNode);

                AttachECData(oNode);
            }
        }
        /// <summary>
        /// called when the moust moves.
        /// </summary>
        /// <param name="Point"></param>
        /// <param name="View"></param>
        /// <param name="DrawMode"></param>
        void BCOM.IPrimitiveCommandEvents.Dynamics(ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
        {
           

            m_node = m_App.CreateTextNodeElement2(null, Point, View.get_Rotation(), false, false);

            if (m_toolsettings.txtOccupant.Text.Length > 0)
                m_node.AddTextLine(m_toolsettings.txtOccupant.Text);
            else
                m_node.AddTextLine("Un Occupied");
            if (m_toolsettings.txtOfficeID.Text.Length > 0)
                m_node.AddTextLine(m_toolsettings.txtOfficeID.Text);
            else
                m_node.AddTextLine("999");
         
            m_node.Redraw(DrawMode);

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
            m_App.CommandState.StopDynamics();
        }
        /// <summary>
        /// called at the start of the command.  The toolsettings is populated at this time.
        /// </summary>
        void BCOM.IPrimitiveCommandEvents.Start()
        {
            //Instantiate the Form and Tell Microstation it is a tool settings
            m_connection = WorkPackageAddin.OpenConnection();
            //Show Prompts etc.
           
            m_App.ShowCommand("Place Office Point");
            m_App.ShowPrompt("Place Text Info");
            //m_App.CommandState.SetLocateCursor();
            //Enables Accusnap for this command if the user has it enabled in Microstation
            m_App.CommandState.EnableAccuSnap();

            BCOM.TextStyle tstyle = m_App.ActiveDesignFile.TextStyles.Find("Office Owner App");
            if (null != tstyle)
                m_App.ActiveSettings.TextStyle  = tstyle;

            m_toolsettings = new plcOfficeInfoSettings(m_AddIn);
            // m_toolsettings.AttachAsTopLevelForm(m_AddIn, true);
            m_toolsettings.AttachToToolSettings(m_AddIn);
            m_node = m_App.CreateTextNodeElement2(null, m_App.Point3dZero(), m_App.Matrix3dIdentity(), false,null);
            m_node.AddTextLine("UnOccupied");
            m_node.AddTextLine("999");
            m_App.CommandState.StartDynamics();
        }

        #endregion
    }
}
