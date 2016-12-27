/*--------------------------------------------------------------------------------------+
|
|     $Source: KeyinCommands.cs,v $
|    $RCSfile: Keyincommands].cs,v $
|   $Revision: 1.1 $
|
|  $Copyright: (c) 2006 Bentley Systems, Incorporated. All rights reserved. $
|
+--------------------------------------------------------------------------------------*/

using System;
using System.IO;

using BCOM=Bentley.Interop.MicroStationDGN;
using BMI=Bentley.MicroStation.InteropServices;
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
using System.Threading;
using WebSockets.Common;
using WebSockets;
using System.Text;

namespace WorkPackageApplication
{
/// <summary>Class used for running key-ins.  The key-ins
/// XML file commands.xml provides the class name and the method names.
/// </summary>
internal class KeyinCommands
{
    /// <summary>
    /// a command that will dump out the ecschema in the model.  if there is an unparsed arg it will be
    /// the only schema searched for in the model.
    /// </summary>
    /// <param name="unparsed">the schema to look for.</param>
    public static void ECApiExampleCommand (System.String unparsed)
        {
            ECSR.RepositoryConnection connection = WorkPackageAddin.OpenConnection();
            WorkPackageAddin.DiscoverWhichECSchemasAreAvailableInTheCurrentConnection(connection);

            //to find "DGNECPlugin_Test" add this as the unparsed arg to the keyin.
            if (unparsed.Length > 0)
                WorkPackageAddin.LocateExampleSchema(connection, unparsed);

            WorkPackageAddin.CloseConnection(connection); 
        }
    /// <summary>
    /// Simple keyin to import a schema to a model reference
    /// </summary>
    /// <param name="unparsed"> the full path and name of the schema to import.  Nothing would look at the ExampleSchema cfg var</param>
    public static void ECApiExampleImportSchema(System.String unparsed)
        {
            {
                string schemaName="";

                if (unparsed.Length > 0)
                    schemaName = unparsed;

                string exampleSchemaName;

                ECSR.RepositoryConnection connection = WorkPackageAddin.OpenConnection();
                ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            
                if (0 == schemaName.Length)
                    exampleSchemaName = WorkPackageAddin.ComApp.ActiveWorkspace.ExpandConfigurationVariable("$(ExampleECSchema)");
                else
                    exampleSchemaName = schemaName;

                //Debug.Assert(null != exampleSchemaName, "You must set the ExampleECSchema cfg variable");
                FileStream schemaInputFile = new FileStream(exampleSchemaName, FileMode.Open, FileAccess.Read, FileShare.Read);
                ECOS.IECSchema exampleSchema = null;
                using (ECOX.ECSchemaXmlStreamReader schemaReader = new ECOX.ECSchemaXmlStreamReader(schemaInputFile))
                {
                    exampleSchema = schemaReader.Deserialize();
                }

                object contextForSchemaLocate = persistenceService.GetSchemaContext(connection);
                try
                {
                    ECOS.IECSchema schema = WorkPackageAddin.LocateExampleSchema(connection, exampleSchema.Name);
                    if (null == schema)
                        //ECO.ECObjects.LocateSchema(schemaName, 1, 0, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
                     persistenceService.ImportSchema(connection, Bentley.DGNECPlugin.Constants.PluginID, exampleSchema, null);
                }
                catch (SystemException e)
                {
                    Debug.Print(e.ToString());
                    Debug.Print("schema Already in the model");
                }
                WorkPackageAddin.CloseConnection(connection);
            }
        }
    /// <summary>
    /// Start a placement command that wil place a cell and attach an ecinstance to the cell
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExamplePlacementCommand (System.String unparsed)
        {
          ECApiExamplePlacementCmd.StartPlacementCommand (WorkPackageAddin.MyAddin); 
        }
    /// <summary>
    /// locate command to add an ecinstance to an element.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExampleLocateCommand(System.String unparsed)
    {
        ECApiExampleLocateCmd.StartLocateCommand(WorkPackageAddin.MyAddin);
    }
    /// <summary>
    /// A command to add a set of ecinstances to an element
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExampleAddClassesToElement(System.String unparsed)
    {
        cmdAddClass.StartLocateCommand(WorkPackageAddin.MyAddin);
    }
    /// <summary>
    /// experiment on manipulating named groups.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExampleItemSet(System.String unparsed)
    {
        NamedGroupList ngList = new NamedGroupList(WorkPackageAddin.MyAddin);
        ngList.Show();
    }
    /// <summary>
    /// command for modifying a widget item
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExampleLocateWidgetCommand(System.String unparsed)
    {
        ECApiExampleModifyWidgetCmd.StartModifyCommand(WorkPackageAddin.MyAddin);
    }
    /// <summary>
    /// command for placing a work task item. This is defined as a point element with ec attributes.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExamplePlaceWorkTask(System.String unparsed)
    {
        PlaceWorkTaskId.PlaceWorkTaskCommand(WorkPackageAddin.MyAddin);
    }
    public static void ECApiExamplePlaceOfficeInfo(System.String unparsed)
    {
        PlaceOfficeInfo.PlaceOfficeInfoCommand(WorkPackageAddin.MyAddin);
    }
    /// <summary>
    /// example of pulling a schema out of a dll.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExampleUnEmbedSchema (System.String unparsed)
    {
        WorkPackageAddin.GetStoredSchema();
    }
    /// <summary>
    /// finds an instance based the class name and property value NOT USED
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExampleFindInstancesOf(System.String unparsed)
    {
        LocateClass.FindItem(unparsed);
    }
    /// <summary>
    /// ecapiexample open classinfo dialog.  this can be used to select 
    /// a schema
    /// a class
    /// a property
    /// and a value.
    /// there will be a list created.  Double click on an item and it will open 
    /// the element list dialog.  double clicking on the item in the element list
    /// will open the details and optionally locate the item.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExampleFindClassInstances(System.String unparsed)
    {
        ClassesToFind frm = new ClassesToFind(WorkPackageAddin.MyAddin);
        frm.AttachAsTopLevelForm(WorkPackageAddin.MyAddin, true);
        frm.Show();
    }
    /// <summary>
    /// ecapiexample open finder.  This can be used to find an item based on 
    /// schema
    /// class
    /// property
    /// options are to use like and to find all classes with the same property.
    /// this will open an element list dialog.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void ECApiExampleFindClassInstance(System.String unparsed)
    {
        System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.Cancel;
        System.String[] args;
        args = unparsed.Split(':');
        System.Collections.Generic.List<TagItemSet> elList = new System.Collections.Generic.List<TagItemSet>();
        if (args.Length > 2)
            elList = LocateClass.FindClassAndValue(args[0], args[1], args[2], args[3], true,false);
        else
        {
            QueryForm frm = new QueryForm(WorkPackageAddin.MyAddin);
            frm.AttachAsTopLevelForm(WorkPackageAddin.MyAddin, true);
            result = frm.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                elList = LocateClass.FindClassAndValue(frm.m_schemaName, frm.m_className, frm.m_propName, frm.m_value, frm.m_bAllClasses,frm.m_bUseLike);
        }
        if ((elList!=null) && (elList.Count > 0))
        {
            ElementList lstForm;
            lstForm = new ElementList(WorkPackageAddin.MyAddin, elList);
            lstForm.AttachAsTopLevelForm(WorkPackageAddin.MyAddin, true);
            lstForm.Show();
            try
            {
                for (int i = 0; i < elList.Count; ++i)
                {
                    BCOM.Element el;
                    if ((WorkPackageAddin.ComApp.ActiveModelReference.Attachments.Count>0)&&(elList[i].modelID != 0))
                    {
                        //BCOM.Attachment oAttach = WorkPackageAddin.ComApp.ActiveModelReference.Attachments.FindByLogicalName(elList[i].logicalName);
                        BCOM.ModelReference oAttach = WorkPackageAddin.ComApp.MdlGetModelReferenceFromModelRefP((int)elList[i].modelID);
                        el = oAttach.GetElementByID(elList[i].filePos);
                    }
                    else
                    {
                        el = WorkPackageAddin.ComApp.ActiveModelReference.GetElementByID(elList[i].filePos);
                    }
                    if (el.IsGraphical)
                    {
                        elList[i].logicalName = el.Type.ToString();
                      //  WorkPackageAddin.ComApp.ActiveModelReference.SelectElement(el, true);
                    }
                }
            }catch (System.Runtime.InteropServices.COMException exp) { Debug.WriteLine(exp.Message); }
        }
        //requeue the command to find info.
        if ((args.Length < 2)&&(result == System.Windows.Forms.DialogResult.OK))
            WorkPackageAddin.ComApp.CadInputQueue.SendKeyin("WorkPackageAddin OPEN Finder");
        return;
    }
    /// <summary>
    /// start the demo of using websockets to communicate
    /// </summary>
    /// <param name="unparsed"></param>
    public static void StartWSChat(String unparsed)
    {
        ChatForm frm = new ChatForm(WorkPackageAddin.MyAddin);
        frm.AttachAsTopLevelForm(WorkPackageAddin.MyAddin, true);
        frm.Show();
    }
    /// <summary>
    /// attempt to use the websocket to communicate to the socket server.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void SendMessage(string unparsed)
    {
        try
        {
            WebSocketLogger wsl = new WebSocketLogger(unparsed);
            WSClientApp.TestClient(wsl);
        }
        catch (Exception e) { Debug.Print(e.Message); }
    }
    /// <summary>
    /// this will collect the elements to send across the wire.
    /// 
    /// </summary>
    /// <param name="unparsed">a property to filter on</param>
    public static void GatherElements(string unparsed)
    {
        if (unparsed.Length < 1)
        {
            WorkPackageAddin.ComApp.MessageCenter.AddMessage("requires a target group", "no target name passed in to operation", BCOM.MsdMessageCenterPriority.Error, true);
            return;
        }

        WorkPackageAddin.ComApp.CadInputQueue.SendKeyin("itemset create " + unparsed);
        bool bHasData = false;
        //loop through the selection and add to the target group.
        ECSR.RepositoryConnection m_connection = WorkPackageAddin.OpenConnection();
        /// process the fenced elements.
        if (WorkPackageAddin.ComApp.ActiveDesignFile.Fence.IsDefined)
        {
            bHasData = true;
            BCOM.ElementEnumerator ee = WorkPackageAddin.ComApp.ActiveDesignFile.Fence.GetContents(true);
            while (ee.MoveNext())
            {
                string clsName = "";
                string prop = ItemSetUtilities.populateData(ee.Current, out clsName, "GUID", m_connection);
                ItemSetUtilities.MoveItemBetweenLists(unparsed, "Available", prop, "GUID");
                KeyinCommands.SendMessage("moved element " + prop + " to " + unparsed);
            }
        }

        if (WorkPackageAddin.ComApp.ActiveModelReference.AnyElementsSelected)
        {
            bHasData = true;
            BCOM.ElementEnumerator ee = WorkPackageAddin.ComApp.ActiveModelReference.GetSelectedElements();
            while (ee.MoveNext())
            {
                string clsName = "";
                string prop = ItemSetUtilities.populateData(ee.Current, out clsName, "GUID", m_connection);
                ItemSetUtilities.MoveItemBetweenLists(unparsed, "Available", prop, "GUID");
                KeyinCommands.SendMessage("moved element " + prop + " to " + unparsed);
            }
        }

        if (!bHasData)
            SelectForScopeCmd.StartScopeCommand(WorkPackageAddin.MyAddin,unparsed);
        else
            WorkPackageAddin.CloseConnection(m_connection);
    }
    /// <summary>
    /// based on the class name this will locate the schema that holds the definition.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void FindSchemaForClass(string unparsed)
    {
        string schemaName="";
        ECSR.RepositoryConnection connection = WorkPackageAddin.OpenConnection();
        if (WorkPackageAddin.FindSchemaForClassName(ref schemaName, unparsed, connection))
            Debug.WriteLine(string.Format("found the class {0} in schema {1}", unparsed, schemaName));
    }
    /// <summary>
    /// opens a working file and references back in the current file.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void OpenWorkFile(string unparsed)
    {
        ItemSetUtilities._AttachToWorkingModel(unparsed);
    }
    /// <summary>
    /// puts an element in the target collection using the GUID and
    /// the schema name.
    /// </summary>
    /// <param name="schemNameElGUID">a string with schema name:GUID</param>
    public static void AddElementToItemSet(string schemNameElGUID)
    {
        string[] args = schemNameElGUID.Split(':');
        ItemSetUtilities.AddItemToSetByID(args[0], args[1],args[2]);
    }
    public static void CloseConnection(string unparsed)
    {
        if (ItemSetUtilities.conn != null)
        {
            WorkPackageAddin.CloseConnection(ItemSetUtilities.conn);
            ItemSetUtilities.conn = null;
        }
    }
    /// <summary>
    /// moves an element from one group to another.
    /// </summary>
    /// <param name="unparsed"></param>
    public static void MoveBetweenGroups(string unparsed)
    {
        string[] args = unparsed.Split(':');
        string targetGroupName = args[1];
        string sourceGroupName = args[0];
        string propKey = args[2];
        string propValue = args[3];
        ItemSetUtilities.MoveItemBetweenLists(targetGroupName, sourceGroupName, propValue, propKey);

    }
    /// <summary>
    /// send the items to the websocket service...
    /// </summary>
    /// <param name="name">name of the list to send.</param>
    public static void GetElementsInItemSet(string unparsed)
    {
        string[] args = unparsed.Split(':');
        string name = args[0];
        string propKey = args[1];

        ItemSetUtilities.SendItemListElements(name,propKey);
    }
    public static void NewList(string unparsed)
    {
        ItemSetUtilities.AddToList("", true);
    }
    public static void BuildList(string unparsed)
    {
        ItemSetUtilities.AddToList(unparsed, false);
    }

    public static void ProcessList(string unparsed)
    {
        ItemSetUtilities.PopulateWPlist(unparsed);
    }
}  // End of KeyinCommands

}  // End of the namespace
