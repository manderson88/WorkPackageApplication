using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using BM = Bentley.MicroStation;
using BCOM = Bentley.Interop.MicroStationDGN;
namespace WorkPackageApplication
{
    class SelectForScopeCmd : BCOM.ILocateCommandEvents
    {
        public string m_targetGroupName{get;set;}
        ECSR.RepositoryConnection m_connection;
        internal static void StartScopeCommand(BM.AddIn _addIn, string targetGroupName)
        {
            SelectForScopeCmd command = new SelectForScopeCmd();
            WorkPackageAddin.ComApp.CommandState.StartLocate(command);
           command.m_targetGroupName = targetGroupName;
        }

        #region ILocateCommandEvents Members

        public void Accept(BCOM.Element Element, ref BCOM.Point3d Point, BCOM.View View)
        {
            try
            {
               
                string clsName = "";
                string prop = ItemSetUtilities.populateData(Element, out clsName, "GUID", m_connection);
                string message = "[ {strElmID:" + prop + "} ]";
                ItemSetUtilities.MoveItemBetweenLists(m_targetGroupName, "Available-" + ItemSetUtilities.s_appID, prop, "GUID", m_connection);
               // KeyinCommands.SendMessage(message);
               
            }
            catch (Exception e) { WorkPackageAddin.ComApp.MessageCenter.AddMessage("error",e.Message, BCOM.MsdMessageCenterPriority.Error, false); }
        }

        public void Cleanup()
        {
            WorkPackageAddin.CloseConnection(m_connection);
        }

        public void Dynamics(ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
        {
            
        }

        public void LocateFailed()
        {
            
        }

        public void LocateFilter(BCOM.Element Element, ref BCOM.Point3d Point, ref bool Accepted)
        {
            Element.Redraw(BCOM.MsdDrawingMode.Hilite);
        }

        public void LocateReset()
        {
            
        }

        public void Start()
        {
            BCOM.LocateCriteria lc = WorkPackageAddin.ComApp.CommandState.CreateLocateCriteria(false);
            m_connection = WorkPackageAddin.OpenConnection();
            WorkPackageAddin.ComApp.CommandState.SetLocateCriteria(lc);
            WorkPackageAddin.ComApp.ShowCommand("Send to Work Package");
            WorkPackageAddin.ComApp.ShowPrompt("Select Component");
        }

        #endregion
    }
}
