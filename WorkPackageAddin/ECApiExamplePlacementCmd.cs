/*--------------------------------------------------------------------------------------+
|
|     $Source: ECApiExamplePlacementcmd.cs,v $
|    $RCSfile: ECApiExamplePlacementCmd.cs,v $
|   $Revision: 1.1 $
|
|  $Copyright: (c) 2006 Bentley Systems, Incorporated. All rights reserved. $
|
+--------------------------------------------------------------------------------------*/
#region "System Namespaces"
using System;
using System.Drawing;
using System.Collections;
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
#endregion

namespace WorkPackageApplication
{
	/// <summary>
	/// </summary>
	internal class ECApiExamplePlacementCmd : BCOM.IPrimitiveCommandEvents
	{

		#region "Private Variables"

		// These will get used over and over, so just get them once.
		private Bentley.MicroStation.AddIn  m_AddIn;
		private BCOM.Application m_App;
        private string strLastTag="";
        private string strMfgName="";
        private ECSR.RepositoryConnection m_connection;
        private plcWidgetSettings m_toolsettings;
        private BCOM.CellElement m_tempCell = null;
		#endregion

		#region "Constructors"
		
		private ECApiExamplePlacementCmd()
		{
			//  Make sure only the IDE uses this.
		}
		
		internal ECApiExamplePlacementCmd(Bentley.MicroStation.AddIn addIn)
		{
			//Initialize class variables
			m_AddIn = addIn;
			m_App = WorkPackageAddin.ComApp;
			//  Set the controls to the values from active settings.
			BCOM.Settings settings = m_App.ActiveSettings;
			//  Initialize to active settings
		}
		
		#endregion

		/// <summary>
		/// Starts the primitive command to place the Route
		/// </summary>
		internal static void StartPlacementCommand (Bentley.MicroStation.AddIn addIn)
		{
			//These are needed because it is a static method
			Bentley.MicroStation.AddIn pAddIn = addIn;
			BCOM.Application pApp = WorkPackageAddin.ComApp;

			//Create a PlaceRouteCommand object
			ECApiExamplePlacementCmd command = new ECApiExamplePlacementCmd (pAddIn);
			BCOM.CommandState commandState = pApp.CommandState;
			pApp.CommandState.StartPrimitive (command, false);
			// Record the name that is saved in the Undo buffer and shown as the prompt.
			pApp.CommandState.CommandName = "Placement Command";
			//Optional Start Dynamics b/c we are ready to show elements 
			//pApp.CommandState.StartDynamics();
		}



		/* --------------------------------------------------------------
		 * The goal of this command is to:
		 * 
		 * ---------------------------------------------------------------------*/
		#region "IPrimitiveCommandEvents"
		public void Start()
		{
			//Instantiate the Form and Tell Microstation it is a tool settings
            m_connection = WorkPackageAddin.OpenConnection();
			//Show Prompts etc.
			m_App.ShowCommand("Place First Point");
			m_App.ShowPrompt("Enter Point");

			//Enables Accusnap for this command if the user has it enabled in Microstation
			m_App.CommandState.EnableAccuSnap();
            m_toolsettings = new plcWidgetSettings(m_AddIn);
           // m_toolsettings.AttachAsTopLevelForm(m_AddIn, true);
            m_toolsettings.AttachToToolSettings(m_AddIn);
            if (m_toolsettings.mfgName.Length > 0)
            {
                m_App.ActiveSettings.CellName = m_toolsettings.mfgName;
                m_App.CommandState.StartDynamics();
            }
            else
            {
                m_toolsettings.mfgName = m_App.ActiveSettings.CellName;
                m_App.CommandState.StartDynamics();
            }
		}


		public void DataPoint(ref BCOM.Point3d Point, BCOM.View View)
		{
            strLastTag = m_toolsettings.tagInfo;
			/*--------------------------------------------------------
			 * ------------------------------------------------------*/
            if ((strMfgName.Length>0) && (strLastTag.Length >0)){
                BCOM.Point3d pScale = m_App.Point3dOne();
                BCOM.Matrix3d pMatrix = View.get_Rotation();
            BCOM.CellElement pCell = m_App.CreateCellElement2(strMfgName, ref Point, ref pScale,true,ref pMatrix);
            m_App.ActiveModelReference.AddElement(pCell);
            //here is where to add the ecdata...
            ECOI.IECInstance pInstance = WorkPackageAddin.CreateECInstance("DgnECPluginBasics.01.00", "Widget", strMfgName, strLastTag, m_connection);
            pInstance.InstanceId = BDGNP.DgnECPersistence.CreatePartialInstanceId(m_connection, (IntPtr)m_App.ActiveModelReference.MdlModelRefP(), (ulong)pCell.ID);
            
            // Get a reference to the PersistenceService, which is the ECFramework persistence API
            ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            ECP.ChangeSet changes = new ECP.ChangeSet();
            changes.Add(pInstance, ECP.ChangeSetElementState.New);
            persistenceService.CommitChangeSet(m_connection, changes); 
            }
            else
                MessageBox.Show ("Missing Tag Information");
		}
	
		
		public void Keyin(string Keyin)
		{
			//Do Nothing
			strMfgName = Keyin;
		}

			
		public void Dynamics(ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
		{
			/*--------------------------------------------------------
			 *  
			 * -------------------------------------------------------*/
            if ((m_toolsettings.mfgName==null)&&(m_toolsettings.mfgName.Length == 0))
                return;
            else
            {
                if ((strMfgName==null) ||(strMfgName.Length == 0))
                    strMfgName = m_toolsettings.mfgName;
                else if (!strMfgName.Equals(m_toolsettings.mfgName))
                    strMfgName = m_toolsettings.mfgName;

                BCOM.Point3d pt = m_App.Point3dZero();

                if (m_tempCell == null)
                    m_tempCell = m_App.CreateCellElement3(strMfgName,ref pt, true);
                else if (!strMfgName.Equals(m_tempCell.Name))
                    m_tempCell = m_App.CreateCellElement3(strMfgName, ref pt, true);
                BCOM.Matrix3d pMatrix = View.get_Rotation();
                BCOM.Transform3d trans = m_App.Transform3dFromMatrix3dPoint3d (ref pMatrix ,ref Point);
                m_tempCell.Transform(ref trans);
                m_tempCell.Redraw(DrawMode);
               
            }
		}


		public void Reset()
		{
         			m_App.CommandState.StartDefaultCommand();
		}


        public void Cleanup()
        {
            m_tempCell = null;
            strMfgName = "";
            WorkPackageAddin.CloseConnection(m_connection);
            m_toolsettings.DetachFromMicroStation();
            m_toolsettings.Dispose();
        }

		#endregion


	}
}