/*--------------------------------------------------------------------------------------+
|
|     $Source: WorkPackageAddin.cs,v $
|    $RCSfile: WorkPackageAddin.cs,v $
|   $Revision: 1.1 $
|       $Date: 2006/06/07 13:13:57 $
|
|  $Copyright: (c) 2006 Bentley Systems, Incorporated. All rights reserved. $
|
+--------------------------------------------------------------------------------------*/
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
//  Provides access to adapters needed to use forms and controls
//  from System.Windows.Forms in MicroStation
using BMW=Bentley.MicroStation.WinForms;
//  Provides access to classes used to make forms dockable in MicroStation
using BWW=Bentley.Windowing.WindowManager;
//  The Primary Interop Assembley (PIA) for MicroStation's COM object
//  model uses the namespace Bentley.Interop.MicroStationDGN
using BCOM=Bentley.Interop.MicroStationDGN;
//  The InteropServices namespace contains utilities to simplify using 
//  COM object model.
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
using SRI = System.Runtime.InteropServices;
using JNS = Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace WorkPackageApplication
{
 
/// <summary>When loading an AddIn MicroStation looks for a class
/// derived from AddIn.</summary>

[Bentley.MicroStation.AddInAttribute(MdlTaskID="WorkPackageAddin", KeyinTree="WorkPackageApplication.commands.xml")]
internal sealed class WorkPackageAddin : Bentley.MicroStation.AddIn
{
    [SRI.DllImport("stdmdlbltin.dll",EntryPoint="mdlLocate_hilite",
        CharSet=SRI.CharSet.Ansi,
        CallingConvention=SRI.CallingConvention.StdCall)]
    internal static extern void mdlLocate_hilite(
        long elRef,
        long modelRef);

    [SRI.DllImport("NamedGroupExp.dll", EntryPoint = "NamedGroupExplr_buildGroup",
        CharSet = SRI.CharSet.Ansi,
        CallingConvention = SRI.CallingConvention.Cdecl)]
    internal static extern void matCreate_createMaterial(
        string materialName);
    /// <summary>
    /// wrap up the completion bar api.  the open method to start the dialog.
    /// </summary>
    /// <param name="message"></param>
    [SRI.DllImport("WPAHelper.dll", EntryPoint = "openCompletionBar",
        CharSet = SRI.CharSet.Ansi,
        CallingConvention = SRI.CallingConvention.Cdecl)]
    internal static extern void openCompletionBar(string message);
    /// <summary>
    /// update call to update the completion bar.
    /// </summary>
    /// <param name="message">text to print</param>
    /// <param name="percent">to show how complete.</param>
    [SRI.DllImport("WPAHelper.dll", EntryPoint = "updateCompletionBar",
        CharSet = SRI.CharSet.Ansi,
        CallingConvention = SRI.CallingConvention.Cdecl)]
    internal static extern void updateCompletionBar(string message, int percent);
    /// <summary>
    /// close the completion bar.
    /// </summary>
    [SRI.DllImport("WPAHelper.dll", EntryPoint = "closeCompletionBar",
        CharSet = SRI.CharSet.Ansi,
        CallingConvention = SRI.CallingConvention.Cdecl)]
    internal static extern void closeCompletionBar();

    [SRI.DllImport("WPAHelper.dll", EntryPoint = "openCompletionBarDialog",
        CharSet = SRI.CharSet.Ansi,
        CallingConvention = SRI.CallingConvention.Cdecl)]
    internal static extern void openCompletionBarDialog(string message);


private static WorkPackageAddin           s_addin = null;
private static BCOM.Application           s_comApp = null;
private static ECApiExampleAddSchemaToElm pSchemaListForm;
private static Boolean m_inWorkFile;

/// <summary>Private constructor required for all AddIn classes derived from 
/// Bentley.MicroStation.AddIn.</summary>
private         WorkPackageAddin
(
System.IntPtr mdlDesc
) : base (mdlDesc)
    {
    s_addin = this;
    }

/// <summary>The AddIn loader creates an instance of a class 
/// derived from Bentley.MicroStation.AddIn and invokes Run.
/// </summary>
protected override int             Run
(
System.String[] commandLine
)
    {
    s_comApp = BMI.Utilities.ComApp;
    //Debugger.Launch();
    //  Register reload and unload events, and show the form
    this.ReloadEvent += new ReloadEventHandler(WorkPackageAddin_ReloadEvent);
    this.UnloadedEvent += new UnloadedEventHandler(WorkPackageAddin_UnloadedEvent);
    this.NewDesignFileEvent += new NewDesignFileEventHandler(WorkPackageAddin_NewFileEvent);
    this.ReferenceAttachedEvent += new ReferenceAttachedEventHandler(WorkPackageAddin_ReferenceAttachedEvent);
    this.ModelChangedEvent += new ModelChangedEventHandler(WorkPackageAddin_ModelChangedEventHandler);
    return 0;
    }
    /// <summary>
    /// handle when the model changes so we can queue up the open message.
    /// </summary>
    /// <param name="senderIn"></param>
    /// <param name="events"></param>
private void WorkPackageAddin_ModelChangedEventHandler(Bentley.MicroStation.AddIn senderIn, ModelChangedEventArgs events)
{
    if (events.Change == ModelChangedEventArgs.ChangeType.Active)
    {
        ComApp.CadInputQueue.SendKeyin("wpaddin chat send model Opened " + WorkPackageAddin.ComApp.ActiveDesignFile.Name);
       
       /* if(WorkPackageAddin.ComApp.ActiveDesignFile.Name.StartsWith("_"))
        {
            WorkPackageAddin.ComApp.CadInputQueue.SendKeyin("wpaddin itemset runlist");
        }*/
    }

}
    /// <summary>
    /// handle the reference file attachment.  since we attach the PMM as an attachment
    /// we do some things based on the completion.
    /// </summary>
    /// <param name="senderIn"></param>
    /// <param name="eventArgsIn"></param>
private void WorkPackageAddin_ReferenceAttachedEvent(Bentley.MicroStation.AddIn senderIn, ReferenceAttachedEventArgs eventArgsIn)
{
   // Debugger.Launch();
    if((int)eventArgsIn.Cause == 7)
    {
        WorkPackageAddin.ComApp.CadInputQueue.SendKeyin("wpaddin chat send wps>qget");
        //KeyinCommands.SendMessage("WPS>QGET");//should be the JSON for the IWP elements//
    }
}
    /// <summary>
    /// called when a new file opens.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
private void WorkPackageAddin_NewFileEvent(Bentley.MicroStation.AddIn sender, NewDesignFileEventArgs args)
{
    if (args.WhenCode == NewDesignFileEventArgs.When.AfterDesignFileOpen)
    {
        //Debugger.Launch();
        string shortName = Path.GetFileName(args.Name);
        if ((!m_inWorkFile)&&(!shortName.StartsWith("_")))
        {
            WorkPackageAddin.ComApp.CadInputQueue.SendKeyin("wpaddin chat send WPS>QGET");
            Debug.Print("in file " + WorkPackageAddin.ComApp.ActiveDesignFile.Name);
            m_inWorkFile = true;
             //KeyinCommands.SendMessage("WPS>QGET");//make this a work file.        
        }
       // if ((shortName.StartsWith("_")) && (m_inWorkFile))
       
    }
    if (args.WhenCode == NewDesignFileEventArgs.When.BeforeDesignFileClose)
    {
     
        WorkPackageAddin.CloseConnection(ItemSetUtilities.conn);
        ItemSetUtilities.conn = null;
        if (m_inWorkFile)
            WorkPackageAddin.ComApp.CadInputQueue.SendKeyin("in work file " + args.Name);
        if(m_inWorkFile)
            m_inWorkFile = false;
        
    }
}

/// <summary>Static property that the rest of the application uses 
/// get the reference to the AddIn.</summary>
internal static WorkPackageAddin MyAddin
    {
    get { return s_addin; }
    }

/// <summary>Static property that the rest of the application uses to
/// get the reference to the COM object model's main application object.</summary>
internal static BCOM.Application ComApp
    {
    get { return s_comApp; }
    }

/// <summary>Handles MDL LOAD requests after the application has been loaded.
/// </summary>
private void WorkPackageAddin_ReloadEvent(Bentley.MicroStation.AddIn sender, ReloadEventArgs eventArgs)
    {
    //  CellControl.ShowForm (this);
    }

private void WorkPackageAddin_UnloadedEvent(Bentley.MicroStation.AddIn sender, UnloadedEventArgs eventArgs)
    {
    }

protected override void OnUnloading(UnloadingEventArgs eventArgs)
    {
    //  CellControl.CloseForm ();
    base.OnUnloading (eventArgs);
    }

/// <summary>Closes a connection. Always close the connection before opening a new file.</summary>
public static void CloseConnection(ECSR.RepositoryConnection connection)
{
    try
    {
        ECSR.RepositoryConnectionService repositoryConnectionService = ECSR.RepositoryConnectionServiceFactory.GetService();
        //Bentley.Collections.IExtendedParameters params;
        repositoryConnectionService.Close(connection, null);
    }
    catch (Exception e) { Console.WriteLine("closing connection exception" + e.Message); }
    connection = null;
}

/// <summary> Open a connection to a repository </summary>
public static ECSR.RepositoryConnection OpenConnection()
{
    ECSR.RepositoryConnection connection = null;
    ECSS.ECSession ecSession = ECSS.SessionManager.CreateSession(); // the ECSession provides context for most ECService calls.

    if (null != WorkPackageAddin.ComApp.ActiveModelReference)
       connection = OpenConnectionToActiveModel(ecSession);

    return connection;
}
    /// <summary>
    /// gets all the classes in the model.
    /// </summary>
    /// <param name="pSchema"></param>
    /// <returns></returns>
public static string[] GetAllClassNamesInSchema(ECOS.IECSchema pSchema)
{
    ECOS.IECClass[] pClasses = pSchema.GetClasses();
    string []names = new string [pClasses.Length];
    for (int i = 0; i < pClasses.Length; ++i)
        names[i] = pClasses[i].Name;

    return names;
}
/// <summary>
/// gets the classes that are defined in the specified schema.
/// </summary>
/// <param name="pSchema"></param>
/// <returns></returns>
public static System.Collections.IEnumerator GetClassesInSchema(ECOS.IECSchema pSchema)
{
    ECOS.IECClass[] pClasses = pSchema.GetClasses();
    return pClasses.GetEnumerator();
}
    /// <summary>
    /// gets the static reference to the schema list form.
    /// </summary>
    /// <returns></returns>
public static ECApiExampleAddSchemaToElm GetForm()
{
    return pSchemaListForm;
}
/// <summary>
/// sets the active form.  this is used to set the form reference to null when disposed.
/// </summary>
/// <param name="pForm"></param>
public static void SetForm(ECApiExampleAddSchemaToElm pForm)
{
    pSchemaListForm = pForm;
}
/// <summary>
/// build a list of the schema that are not managed by the plugin that are in the file.
/// </summary>
/// <param name="connection">The connection to the dgn file</param>
/// <returns></returns>
public static bool ReportEbededSchema(ECSR.RepositoryConnection connection)
{
    bool retStatus = false;
    System.Collections.Generic.IList<ECOS.IECSchema> schemaList = new System.Collections.Generic.List<ECOS.IECSchema>();
    ECP.PersistenceService pSerrvice = ECP.PersistenceServiceFactory.GetService();
    string[] schemaNames = pSerrvice.GetSchemaFullNames(connection);
    object contextForSchema = pSerrvice.GetSchemaContext(connection);
    foreach (string fullSchemaName in schemaNames)
    {
        ECOS.IECSchema schema = ECO.ECObjects.LocateSchema(fullSchemaName, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchema);
        BDGNP.PersistenceStrategy pStrategy = BDGNP.DgnECPersistence.GetPersistenceStrategiesForSchema(connection, schema);
        if ((pStrategy == null) || (!pStrategy.SupportsSchema(schema)))
            schemaList.Add(schema);
    }
    return retStatus;
}
/// <summary>
/// In this example, this method isn't really accomplishing other than 
/// demonstrate how we can discover which ECSchemas are available
/// </summary>
/// <param name="connection"></param>
public static void DiscoverWhichECSchemasAreAvailableInTheCurrentConnection
(
ECSR.RepositoryConnection connection
)
{
    ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
    ECSR.RepositoryIdentifier ri = connection.RepositoryIdentifier;
    ECSR.ConnectionInfo ci = connection.ConnectionInfo;

    if (pSchemaListForm == null)
    {
        pSchemaListForm = new ECApiExampleAddSchemaToElm(s_addin);
        // Get the names of all ECSchemas available in the current RepositoryConnection
        string[] schemaNames = persistenceService.GetSchemaFullNames(connection);
        
        object contextForSchemaLocate = persistenceService.GetSchemaContext(connection);

        foreach (string fullSchemaName in schemaNames)
        {
            ECOS.IECSchema schema = ECO.ECObjects.LocateSchema(fullSchemaName, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
            System.Collections.Generic.IList <ECOS.IECSchemaLocater> pLocations = ECO.ECObjects.FindSchemaLocater(schema.GetType());
            for (int i = 0; i < pLocations.Count; ++i)
            {
                ECOS.IECSchema locatedSchema = pLocations[i].LocateSchema(schema.Name, 0, 0, ECOS.SchemaMatchType.Latest, schema, null);
                Debug.WriteLine("found ");
            }
            BDGNP.PersistenceStrategy ps = BDGNP.DgnECPersistence.GetPersistenceStrategiesForSchema(connection, schema);
            if  ((ps==null)||(!ps.SupportsSchema(schema)))
                pSchemaListForm.AddToSchemaList(schema.FullName);
            //Debug.WriteLine("ECSchema " + schema.FullName + " is availble.");
        }
    }
    pSchemaListForm.Show();
}
    /// <summary>
    /// 
    /// </summary>
    /// <param name="classInst"></param>
    /// <returns></returns>
private static string[] GetPropertyNames(ECOS.IECClass classInst)
{
   
    System.Collections.Generic.IEnumerator<ECOS.IECProperty> props = classInst.GetEnumerator();

    int i = 0;
    while (props.MoveNext())
    {
        i++;
    }
    props.Reset();
    string[] names = new string[i];
    i = 0;
    while (props.MoveNext())
        names[i++] = props.Current.Name;
    return names;
}
/// <summary>
/// 
/// </summary>
/// <param name="schemaName"></param>
/// <param name="className"></param>
/// <param name="conn"></param>
/// <returns></returns>
public static Boolean FindSchemaForClassName(ref string schemaName, string className, ECSR.RepositoryConnection conn)
{
    ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
    string[] schemaNames = persistenceService.GetSchemaFullNames(conn);
    schemaName = "";
    Boolean retstatus = false;

        object contextForSchemaLocate = persistenceService.GetSchemaContext(conn);

        foreach (string fullSchemaName in schemaNames)
        {
            ECOS.IECSchema schema = ECO.ECObjects.LocateSchema(fullSchemaName, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
            string[] classNames = GetAllClassNamesInSchema(schema);
            foreach (string cname in classNames)
            {
                if (cname.Contains(className))
                {
                    schemaName = fullSchemaName;
                    retstatus = true;
                }
            }
            ECOS.IECClass[] pClasses = schema.GetClasses();
            foreach (ECOS.IECClass pClass in pClasses)
            {
                string[] propNames = GetPropertyNames(pClass);
                foreach (string propname in propNames)
                {
                    if (propname.Contains(className))
                        Debug.WriteLine(string.Format("found prop {0} in class {1}",propname,pClass.Name));
                }
            }
        }
        return retstatus;
}
/// <summary>
/// This method assumes that this AddIn is configured so that a particular example ECSchema is always configured...
/// In general, you may need to use a technique like that of DiscoverWhichECSchemasAreAvailableInTheCurrentConnection
/// to find an ECSchema, or use persistenceService.ImportSchema to import a particular ECSchema
/// </summary>
/// <param name="connection"></param>
/// <param name="schemaToFind"></param>
/// <returns></returns>
public static ECOS.IECSchema LocateExampleSchema(ECSR.RepositoryConnection connection, string schemaToFind)
{
    ECOS.IECSchema schema = null;

    if (!connection.Session.IsValid)
        connection = OpenConnection();

    try
    {
        ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
        object contextForSchemaLocate = persistenceService.GetSchemaContext(connection);
        schema = ECO.ECObjects.LocateSchema(schemaToFind, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
        //ECO.ECObjects.LocateSchema(schemaToFind, 1, 0, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
        //Debug.Assert(null != schema);
    }
    catch (Exception e) 
    {
        Console.WriteLine("Exception in Locate Schema" + e.Message);
        try
        {
           // Debugger.Launch();
            connection = WorkPackageAddin.OpenConnection();
            ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
            object contextForSchemaLocate = persistenceService.GetSchemaContext(connection);
            schema = ECO.ECObjects.LocateSchema(schemaToFind, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
        }
        catch (Exception ee) { Console.WriteLine("inner try on the schema find." + ee.Message); }
    }
    return schema;
}
/// <summary>
/// 
/// </summary>
/// <param name="connection"></param>
/// <param name="schemaToTry"></param>
/// <returns></returns>
public static ECOS.IECSchema LocateSchemaForWPS(ECSR.RepositoryConnection connection, string schemaToTry)
{
    ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
    object contextForSchemaLocate = persistenceService.GetSchemaContext(connection);
    ECOS.IECSchema schema = null;

    if(schemaToTry.Length > 0)
        schema = ECO.ECObjects.LocateSchema(schemaToTry, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
    
    if (schema == null)
    {
        string schemaToFind = "BuildingDataGroup.01.00";
        try
        {
            schema = ECO.ECObjects.LocateSchema(schemaToFind, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
        }
        catch (Exception e)
        {
            Debug.Print(e.Message);
        }
    }
    if (schema == null)
    {
        string schemaToFind = "OpenPlant_3D.01.04";
        try
        {
            schema = ECO.ECObjects.LocateSchema(schemaToFind, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
        }
        catch (Exception e)
        {
            Debug.Print(e.Message);
        }
    }


    //ECO.ECObjects.LocateSchema(schemaToFind, 1, 0, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
    Debug.Assert(null != schema);

    return schema;

}
    /// <summary>
    ///  Creates a single instance to append to an element.
    /// </summary>
    /// <param name="schema"> the schema to work with</param>
    /// <param name="_tag"> the tag for the item</param>
    /// <param name="_mfgName">the manufacturer name</param>
    /// <returns>null for a fail </returns>
public static ECOI.IECInstance CreateECInstance(string schema, string clsName, string _tag, string _mfgName,ECSR.RepositoryConnection connection)
{
    ECOI.IECInstance pInstance = null;
    ECOS.IECClass pClass;
    ECOS.IECSchema pSchema = LocateExampleSchema(connection, schema);
    pClass = pSchema[clsName];
    pInstance = pClass.CreateInstance();
    pInstance.SetAsString("Tag", _tag);
    pInstance.SetAsString("WidgetManufacturer",_mfgName);
    return pInstance;
}
    /// <summary>
    /// this will add a blank instance of any selected class to the element.
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="clsName"></param>
    /// <param name="connection"></param>
    /// <returns></returns>
public static ECOI.IECInstance CreateECInstance(string schema, string clsName, ECSR.RepositoryConnection connection)
{
    ECOI.IECInstance pInstance = null;
    ECOS.IECClass pClass;
    ECOS.IECSchema pSchema = LocateExampleSchema(connection, schema);
    pClass = pSchema[clsName];
    try
    {
        pInstance = pClass.CreateInstance();
    }
    catch (Exception e)
    {
        s_comApp.MessageCenter.AddMessage(e.Message, e.Message, BCOM.MsdMessageCenterPriority.Error, true);
    }
    return pInstance;
}
    /// <summary>
    /// 
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="clsName"></param>
    /// <param name="numRelations"></param>
    /// <param name="connection"></param>
    /// <returns></returns>
public static ECOI.IECRelationshipInstance CreateRelationshipECInstance (string schema,string clsName,int numRelations, ECSR.RepositoryConnection connection)
{
    ECOI.IECRelationshipInstance pInstance = null;
    ECOI.IECInstance pi;
    ECOI.ECRelationshipInstance p=null;
    ECOS.IECRelationshipClass[] pClass;
    ECOS.IECSchema pSchema = LocateExampleSchema(connection, schema);
    pClass = pSchema.GetRelationshipClasses();
    try
    {
        for (int i=0;i<pClass.Length;++i)
        {
            s_comApp.MessageCenter.AddMessage(pClass[i].Name, pClass[i].Name, BCOM.MsdMessageCenterPriority.Info, false);
            pi = pClass[i].CreateInstance();
            p = new ECOI.ECRelationshipInstance(pClass[i]);
            pi.GetRelationshipInstances();
            //pInstance = (ECOI.IECRelationshipInstance)pClass[i].CreateInstance(numRelations);
            pInstance = p;
            for (int j=0;j<numRelations;++j)
            {
                
            }
        }
    }
    catch (Exception e)
    {
        s_comApp.MessageCenter.AddMessage(e.Message, e.Message, BCOM.MsdMessageCenterPriority.Error, true);
    }
    return p;
}
/// <summary>
/// Gets a connection to the active DGN Model
/// </summary>
/// <param name="ecSession"></param>
/// <returns></returns>
public static ECSR.RepositoryConnection OpenConnectionToActiveModel(ECSS.ECSession ecSession)
{
    ECSR.RepositoryConnectionService repositoryConnectionService = ECSR.RepositoryConnectionServiceFactory.GetService();
    ECSR.RepositoryConnection connection=null;
    string fileName = ""; // implies active file
    string modelName = ""; // implies active model
    //string loc;
    string location = BECPC.ECRepositoryConnectionHelper.BuildLocation(fileName, modelName);

    string ecPluginId = BDGNP.Constants.PluginID;
    Debug.WriteLine("trying active model");
    Debug.WriteLine("location = " + location);
    Debug.WriteLine("PluginID = " + ecPluginId);
    try
    {
        connection = repositoryConnectionService.Open(ecSession, ecPluginId, location, null, null);
    }
    catch (Exception connectionEx) { Console.WriteLine("error connecting " + connectionEx.Message); }
    Debug.Assert(null != connection);

    return connection;
}
/// <summary>
/// opena a connection to a specific model
/// </summary>
/// <param name="modelName"></param>
/// <param name="fileName"></param>
/// <param name="session"></param>
/// <returns></returns>
    public static ECSR.RepositoryConnection OpenConnectionToSourceModel(string modelName, string fileName, ECSS.ECSession session)
{
    ECSR.RepositoryConnectionService repositoryConnService = ECSR.RepositoryConnectionServiceFactory.GetService();

    string location = BECPC.ECRepositoryConnectionHelper.BuildLocation(fileName, modelName);
    string ecPluginID = BDGNP.Constants.PluginID;
    ECSR.RepositoryConnection connection = repositoryConnService.Open(session, ecPluginID, location, null, null);
    return connection;
}
/// <summary>
/// a simple method to write a schema to a DGN file
/// </summary>
/// <param name="fileName"></param>
public static void WriteToDGN (String fileName)
    {
            string schemaName="";

            if (fileName.Length > 0)
                schemaName = fileName;

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
/// <summary>
/// An example of taking a schema that is embedded in the assembly and then loading it to the DGN file.
/// </summary>
public static void GetStoredSchema ()
{
    Assembly _assembly;
    StreamReader _textStreamReader;
    String tempFile; 
    FileInfo fileInfo;
    try
    {
        _assembly = Assembly.GetExecutingAssembly();
        _textStreamReader = new StreamReader(_assembly.GetManifestResourceStream("WorkPackageAddin.SWAWorkTask.01.00.ecschema.xml"));
        tempFile = System.IO.Path.GetTempPath();
        tempFile += "SWAWorkTask.01.00.ecschema.xml";
        fileInfo = new FileInfo(tempFile);
        //not sure yet on this.  It could be deleting before I get to use the file.
        //fileInfo.Attributes = FileAttributes.Temporary;
   
        StreamWriter sw = new StreamWriter(tempFile);
        sw.Write(_textStreamReader.ReadToEnd());
        sw.Flush();
        sw.Close();
        WriteToDGN(tempFile);
        if (File.Exists (tempFile))
        {
            File.Delete(tempFile);
        }
    }
    catch (Exception e)
    {
        MessageBox.Show(e.Message);
    }
        
}
/// <summary>
/// Utility method to display the exception information
/// </summary>
/// <param name="e"></param>
   
public void ShowException(Exception e)
{
    // In this simple example, we just show you any exceptions in the debugger, but allow the program to continue running
    string message = "An exception occurred during example: " + e.Message + "\n" + e.StackTrace +
    "\n--------------------------------------------------------------------";
    if (e.InnerException != null)
        message += "\nInnerException: " + e.InnerException.Message + "\n" + e.InnerException.StackTrace +
        "\n--------------------------------------------------------------------";

    if (Debugger.IsAttached)
        Debug.Fail(message);
    else
        MessageBox.Show(message, "An exception occured during the example.");
}
}   // End of WorkPackageAddin

class ElementPacket
{
    public string ElementID { get; set; }
}
}   // End of Namespace
