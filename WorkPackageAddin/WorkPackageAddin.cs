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

private static WorkPackageAddin           s_addin = null;
private static BCOM.Application           s_comApp = null;
private static ECApiExampleAddSchemaToElm pSchemaListForm;
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

    //  Register reload and unload events, and show the form
    this.ReloadEvent += new ReloadEventHandler(WorkPackageAddin_ReloadEvent);
    this.UnloadedEvent += new UnloadedEventHandler(WorkPackageAddin_UnloadedEvent);

    return 0;
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
    ECSR.RepositoryConnectionService repositoryConnectionService = ECSR.RepositoryConnectionServiceFactory.GetService();
    //Bentley.Collections.IExtendedParameters params;
    repositoryConnectionService.Close(connection, null);
}

/// <summary> Open a connection to a repository </summary>
public static ECSR.RepositoryConnection OpenConnection()
{
    ECSS.ECSession ecSession = ECSS.SessionManager.CreateSession(); // the ECSession provides context for most ECService calls.

    ECSR.RepositoryConnection connection = OpenConnectionToActiveModel(ecSession);

    return connection;
}
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
    //sets the active form.  this is used to set the form reference to null when disposed.
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
/// In this example, this method isn't really accomplishing other than demonstrate how we can discover which ECSchemas are available
/// </summary>
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
public static ECOS.IECSchema LocateExampleSchema(ECSR.RepositoryConnection connection, string schemaToFind)
{
    ECP.PersistenceService persistenceService = ECP.PersistenceServiceFactory.GetService();
    object contextForSchemaLocate = persistenceService.GetSchemaContext(connection);
    ECOS.IECSchema schema = ECO.ECObjects.LocateSchema(schemaToFind, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
    //ECO.ECObjects.LocateSchema(schemaToFind, 1, 0, ECOS.SchemaMatchType.LatestCompatible, null, contextForSchemaLocate);
    Debug.Assert(null != schema);

    return schema;
}

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
/// <summary>Gets a connection to the active DGN Model</summary>
public static ECSR.RepositoryConnection OpenConnectionToActiveModel(ECSS.ECSession ecSession)
{
    ECSR.RepositoryConnectionService repositoryConnectionService = ECSR.RepositoryConnectionServiceFactory.GetService();

    string fileName = ""; // implies active file
    string modelName = ""; // implies active model
    //string loc;
    string location = BECPC.ECRepositoryConnectionHelper.BuildLocation(fileName, modelName);

    string ecPluginId = BDGNP.Constants.PluginID;

    Debug.WriteLine("location = " + location);
    Debug.WriteLine("PluginID = " + ecPluginId);

    ECSR.RepositoryConnection connection = repositoryConnectionService.Open(ecSession, ecPluginId, location, null, null);

    Debug.Assert(null != connection);

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

//public List<ElementPacket> BuildJSONFromString(string message)
//{
//   return List<ElementPacket> elements = JNS.JsonConvert.DeserializeObject<List<ElementPacket>>(message);
//}

}   // End of WorkPackageAddin

class ElementPacket
{
    public string ElementID { get; set; }
}
}   // End of Namespace
