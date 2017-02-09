/*--------------------------------------------------------------------------------------+
//----------------------------------------------------------------------------
// DOCUMENT ID:   
// LIBRARY:       
// CREATOR:       Mark Anderson
// DATE:          05-05-2016
//
// NAME:          ItemSetUtilities.cs
//
// DESCRIPTION:   Utility methods to work with ItemSets.  To abstract the
// collection of components this application uses itemsets and named groups.
//
// REFERENCES:    ECAPI.
//
// ---------------------------------------------------------------------------
// NOTICE
//    NOTICE TO ALL PERSONS HAVING ACCESS HERETO:  This document or
//    recording contains computer software or related information
//    constituting proprietary trade secrets of Black & Veatch, which
//    have been maintained in "unpublished" status under the copyright
//    laws, and which are to be treated by all persons having acdcess
//    thereto in manner to preserve the status thereof as legally
//    protectable trade secrets by neither using nor disclosing the
//    same to others except as may be expressly authorized in advance
//    by Black & Veatch.  However, it is intended that all prospective
//    rights under the copyrigtht laws in the event of future
//    "publication" of this work shall also be reserved; for which
//    purpose only, the following is included in this notice, to wit,
//    "(C) COPYRIGHT 1997 BY BLACK & VEATCH, ALL RIGHTS RESERVED"
// ---------------------------------------------------------------------------
/*
/* CHANGE LOG
 * $Archive: $
 * $Revision:  $
 * $Modtime:  $
 * $History: $
 * 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Collections;
namespace WorkPackageApplication
{
    /// <summary>
    /// a utility class to work with the itemsets.
    /// this is the  link to the external  work package application/site.
    /// </summary>
    static class ItemSetUtilities
    {
        static List<string> guidList = new List<string>();
        public static ECSR.RepositoryConnection conn { get; set; }
        //for the JSON information.
        public static string s_appID { get; set; }
        public static string s_fileName { get; set; }
        public static string s_docGUID { get; set; }
        public static string s_dsName { get; set; }
        public static string s_appUUID { get; set; }
        public static string s_projID { get; set; }
        //public static List <ItemDictionaries> s_list { get; set; }
        private static int s_dictionarySize=0;
        public static int s_itemsCount = 0;

     /*   public static void AddToList(string guidString,bool newList)
        {
            if (newList)
                guidList.Clear();

            if(guidString.Length>0)
                guidList.Add(guidString);

        }

        public static void PopulateWPlist(string name)
        {
            foreach (string guidString in guidList)
            {
                AddItemToSetByID(name, "GUID", guidString,"");
            }
        }*/
        /// <summary>
        /// this is the dictionary that will hold a dictionary of the lists.  
        /// the outer most dictionary key is the uuid for the page that is in focus.
        /// the dictionary key is tag id that is sent across as a collector for elements.
        /// the list is the ids for the elements that are in the collection.
        /// this list is populated when the file is first loaded up.
        /// </summary>
        public static Dictionary<string, Dictionary<string, List<ItemSetInfo>>> s_pageInfo;
        /// <summary>
        /// the items are the guid for the elements
        /// </summary>
        /// <param name="data">the item to add to a list</param>
        /// <returns>the list of items.</returns>
        public static List<ItemSetInfo> CreateItemsList(ItemSetInfo data)
        {
            List<ItemSetInfo> items = new List<ItemSetInfo>();
            if (null != data)
                items.Add(data);
            return items;
        }
        /// <summary>
        /// this is the collector for the lists
        /// </summary>
        /// <param name="name">the name of the collection</param>
        /// <returns></returns>
        public static Dictionary<string,List<ItemSetInfo>> CreateTagCollection(string name)
        {
            Dictionary<string,List<ItemSetInfo>> data = new Dictionary<string,List<ItemSetInfo>>();
            List <ItemSetInfo> items = CreateItemsList(null);
            data.Add(name, items);
            return data;
        }
        /// <summary>
        /// the outer most collection. the key is the uuid for the page.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string,List<ItemSetInfo>>> CreatePageInfo(string name)
        {
            Dictionary<string, Dictionary<string, List<ItemSetInfo>>> _pageInfo = new Dictionary<string, Dictionary<string, List<ItemSetInfo>>>();

            return _pageInfo;
        }
        private static Boolean AddToInnerList(List<ItemSetInfo> listing, ItemSetInfo item)
        {
            foreach(ItemSetInfo member in listing)
                if(0==member.elKey.CompareTo(item.elKey))
                    return false; //its in the list already.

            listing.Add(item);
            return true;
        }
        /// <summary>
        /// put an item in the collection of collections.
        /// </summary>
        /// <param name="name">the collection/tagid name</param>
        /// <param name="propName">the name of the field that is being used to identify</param>
        /// <param name="propKey">the value that is being searched for.</param>
        /// <param name="schemaName">the schema that is being used</param>
        public static void PopulateItemSet(string name, string propName, string propKey, string schemaName)
        {
            Dictionary<string,List<ItemSetInfo>> t;
            List <ItemSetInfo> innerList;
            ItemSetInfo s = new ItemSetInfo();
            if(null == s_pageInfo)
                s_pageInfo = CreatePageInfo(s_appID);

            if (!s_pageInfo.TryGetValue(s_appID, out t))
            {
                t = CreateTagCollection(name);
                s_pageInfo.Add(s_appID, t);
            }
            if (!t.TryGetValue(name, out innerList))
            {
                innerList = new List<ItemSetInfo>();
                t.Add(name,innerList);
            }
            
            s.name = name;
            s.propName = propName;
            s.elKey = propKey;
            s.schemaName = schemaName;

            AddToInnerList(innerList, s);

            s_dictionarySize++;
         }
        /// <summary>
        /// the command to process the current lists of elements.
        /// </summary>
        /// <returns></returns>
        public static int RunList(string dumpInfo)
        {
            int i=0;
            double processedItemsCount = 0;
            try
            {
                WorkPackageAddin.openCompletionBarDialog("starting itemset building " + s_dictionarySize + " items");

                ItemSetUtilities.conn = WorkPackageAddin.OpenConnection();
                Dictionary<string, List<ItemSetInfo>> data;
                if (s_pageInfo.TryGetValue(s_appID, out data))
                    foreach (var dataList in data)
                    {
                        if(dumpInfo.Length>0)
                        for (i = 0; i < dataList.Value.Count; i++)
                        {
                            if (dumpInfo.Length > 0)
                                Debug.WriteLine(string.Format("putting item : name = {0}, PropName = {1}, PropKey = {2}, Schema = {3}", dataList.Value[i].name, dataList.Value[i].propName, dataList.Value[i].elKey, dataList.Value[i].schemaName));
                        }
                        BCOM.NamedGroupElement ng;
                        int ngCount = 0;
                        processedItemsCount += dataList.Value.Count;
                        double percent;
                        percent = 100 * processedItemsCount / s_dictionarySize;

                        WorkPackageAddin.updateCompletionBar("adding item(s) " + dataList.Value.Count + " to groups " + processedItemsCount + " of " + s_dictionarySize + " items " ,Convert.ToInt32(percent));
                        try
                        {
                            ng = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(dataList.Value[0].name);
                            ngCount = ng.MembersCount;
                        }
                        catch (Exception e) { Console.WriteLine("no named group yet..."); }
                        if (ngCount != dataList.Value.Count)
                        {
                            AddCollectionToSetByID(dataList.Value[0].name, dataList.Value[0].propName, dataList.Value, dataList.Value[0].schemaName);
                        
                            AddToParentGroup(dataList.Value[0].name, "Available-" + ItemSetUtilities.s_appID);
                        }
                    }
                s_dictionarySize = 0;
                WorkPackageAddin.closeCompletionBar();
                WorkPackageAddin.CloseConnection(ItemSetUtilities.conn);
                ItemSetUtilities.conn = null;
            }
            catch (Exception e) {/* Console.WriteLine("error in run list " + e.Message); */ }
            return i;
        }
        /// <summary>
        /// looks to see if the element is already in the group.
        /// </summary>
        /// <param name="groupName">name of target </param>
        /// <param name="elm">element that is being processed</param>
        /// <returns></returns>
        private static Boolean IsInGroup(string groupName, BCOM.Element elm)
        {
            BCOM.NamedGroupElement[] nges = elm.GetContainingNamedGroups();
            if (nges.Length > 0)
                foreach (BCOM.NamedGroupElement nge in nges)
                    if (0 == nge.Name.CompareTo(groupName))
                        return true;

            return false;
        }
        /// <summary>
        /// adds an element set to the named group
        /// </summary>
        /// <param name="name">named group name</param>
        /// <param name="propKey">the property to query</param>
        /// <param name="elID">the list of ids to find.</param>
        /// <param name="schemaName">name of the schema to use for the query</param>
        public static Boolean AddCollectionToSetByID(string name, string propKey, List<ItemSetInfo> items, string schemaName)
        {
            BCOM.NamedGroupElement nge = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(name);
            Boolean bGlobalConn = true;
            try
            {
                if (null == nge)
                    nge = WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(name);

                nge.AllowFarReferences = true;
                nge.AllowSelectMembers = true;
                nge.AllowSelectMembers = false; //lets us select one member of the group.
                nge.Rewrite();
            }
            catch (Exception e)
            {
                return false;
            }

            //open the connect to the  repository
            if (conn == null)
            {
                conn = WorkPackageAddin.OpenConnection();
                if (null != conn)
                    bGlobalConn = false;
            }

            //get the schema if the schema is not found the exit.
            ECOS.IECSchema pSchema = null;
            pSchema = WorkPackageAddin.LocateExampleSchema(conn, schemaName);
            if (pSchema == null)
            {
                if (!bGlobalConn)
                {
                    WorkPackageAddin.CloseConnection(conn);
                    bGlobalConn = false;
                    conn = null;
                }
                return false;
            }
            //get the  element based on the property.  if no element is found then return
            //BCOM.Element elm = LocateClass.FindSingleElement(pSchema, "", propKey, elID, true, false, conn);
            int i = 0;
            string[] elids = new string[items.Count];
            //build the array that will be the list of IDs to query.
            foreach (ItemSetInfo item in items)
                elids[i++] = item.elKey;
            //this will take the array of ids and return back a list of elements
            List<BCOM.Element> elements = 
                LocateClass.GetCollectionOfElementsByProp(pSchema, "", propKey,elids, true, false, conn);
            //make sure there is a list
            if (elements == null)
            {
                if (!bGlobalConn)
                {
                    WorkPackageAddin.CloseConnection(conn);
                    conn = null;
                }
                return false;
            }
            //put each element in the named group.
            foreach(BCOM.Element el in elements)
                if(!IsInGroup(nge.Name,el))
                    try
                    {
                        nge.AddMember(el);
                    }
                    catch (Exception ex)
                        {
                        WorkPackageAddin.ComApp.MessageCenter.AddMessage("Element already in the work package",
                        "Item already in a work package", BCOM.MsdMessageCenterPriority.Info, false);
                        }
            finally
            {
               if (!bGlobalConn)
                {
                    WorkPackageAddin.CloseConnection(conn);
                    conn = null;
                }
            }
            try
            {
                nge.Rewrite();
            }
            catch (Exception rewrite) 
            { 
                WorkPackageAddin.ComApp.MessageCenter.AddMessage(
                    "Error rewriting group ", "error when updating " + 
                    nge.Name + " - " + 
                    rewrite.Message, BCOM.MsdMessageCenterPriority.Error, false); 
            }
            return true;
        }
        //-----------------------------------------------------------------------------------
        // Takes current model (which may be read-only), attaches to a separate, editable
        // working model, then switches current session to that working model.
        // This allows additional views to be packaged with a read-only PMM (project master model)
        // prior to publishing.
        //
        // Input: unparsed - The work file name....  full file path...
        //-----------------------------------------------------------------------------------
        public static void _AttachToWorkingModel(System.String unparsed)
        {
            BCOM.DesignFile pmmDgnFile = WorkPackageAddin.ComApp.ActiveDesignFile;
            BCOM.ModelReference pmmModel = WorkPackageAddin.ComApp.ActiveModelReference;
            BCOM.Workspace workSpace = WorkPackageAddin.ComApp.ActiveWorkspace;
            string workPath = workSpace.ExpandConfigurationVariable("$(MS_DEF)"); // nested vars
            string workFileName = System.IO.Path.GetFileName(unparsed);  
            string workFilePathName = workPath + "_" + workFileName;
            string seedFileName = workSpace.ConfigurationVariableValue("MS_DESIGNSEED", true); // -> "pmseed3d"
            //Debugger.Launch();
            WorkPackageAddin.ComApp.CreateDesignFile(seedFileName, workFilePathName, false);
            
            BCOM.DesignFile workFile = 
                WorkPackageAddin.ComApp.OpenDesignFileForProgram(workFilePathName, false); // opens a design file
            BCOM.ModelReference workModel = workFile.DefaultModelReference;
            BCOM.Attachment att = workModel.Attachments.AddCoincident(
                pmmDgnFile.FullName, pmmModel.Name, 
                pmmModel.Name, pmmModel.Description, true);
            
            att.DisplayAsNested = true;
            att.NestLevel = 99;
            att.Rewrite();
            workFile.Save();
            workFile.Close();
            WorkPackageAddin.ComApp.OpenDesignFile(workFilePathName, false, BCOM.MsdV7Action.UpgradeToV8); // swapping out
        }
        /// <summary>
        /// creates a named group
        /// </summary>
        /// <param name="name"></param>
        public static void BuildItemSet(string name)
        {
            BCOM.NamedGroupElement nge = WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(name);
        }

        /// <summary>
        /// adds an element to the named group
        /// </summary>
        /// <param name="name">named group name</param>
        /// <param name="propKey">the property to query</param>
        /// <param name="elID">id of the element to find.</param>
        /// <param name="schemaName">name of the schema to use for the query</param>
        public static Boolean AddItemToSetByID(string name, string propKey, string elID,string schemaName)
        {
            BCOM.NamedGroupElement nge = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(name);
            Boolean bGlobalConn = true;
            try
            {
                if (null == nge)
                    nge = WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(name);

                nge.AllowFarReferences = true;
                nge.AllowSelectMembers = true;
                nge.AllowSelectMembers = false; //lets us select one member of the group.

                nge.Rewrite();
            }
            catch (Exception e)
            {
                return false;
            }

            //open the connect to the  repository
            if (conn == null)
            {
                conn = WorkPackageAddin.OpenConnection();
                if (null != conn)
                    bGlobalConn = false;
            }
            
            //get the schema if the schema is not found the exit.
            ECOS.IECSchema pSchema = null;
            pSchema = WorkPackageAddin.LocateExampleSchema(conn, schemaName);
            if (pSchema == null)
            {
                if (!bGlobalConn)
                {
                    WorkPackageAddin.CloseConnection(conn);
                    bGlobalConn = false;
                    conn = null;
                }
                return false;
            }
            //get the  element based on the property.  if no element is found then return
            BCOM.Element elm = LocateClass.FindSingleElement(pSchema, "", propKey, elID, true, false,conn);
            if (elm == null)
            {
                if (!bGlobalConn)
                {
                    WorkPackageAddin.CloseConnection(conn);
                    conn = null;
                }
                return false;
            }
            try
            {
                nge.AddMember(elm);
                nge.Rewrite();
            }
            catch (Exception ex)
            {
                WorkPackageAddin.ComApp.MessageCenter.AddMessage("Element already in the work package", 
                    "Item already in a work package", BCOM.MsdMessageCenterPriority.Info, false);
            }
            finally
            {
                if (!bGlobalConn)
                {
                    WorkPackageAddin.CloseConnection(conn);
                    conn = null;
                }
            }
            return true;
        }
        /// <summary>
        /// populates the list of element properties for the class
        /// </summary>
        /// <param name="pElement">the source element to query</param>
        /// <param name="clsName">the class that the element is associated to</param>
        /// <param name="propKey">the property to search for</param>
        /// <param name="m_connection">the repository connection</param>
        /// <returns></returns>
        public static string populateData(BCOM.Element pElement, out string clsName, string propKey, ECSR.RepositoryConnection m_connection)
        {
            clsName = "None Found";
            string propKeyVal="";
            List<DataInfo> eList = new List<DataInfo>();

            System.Collections.Generic.IList<ECOI.IECInstance> pInstances = 
                BDGNP.DgnECPersistence.GetAllInstancesOnElement(m_connection, 
                (System.IntPtr)pElement.MdlElementRef(), 
                (System.IntPtr)pElement.ModelReference.MdlModelRefP(), 
                ECP.LoadModifiers.None, ECP.LoadModifiers.None, 1, null);

            foreach (ECOI.IECInstance pInstance in pInstances)
                if (pInstance.ContainsValues)
                {
                    clsName = pInstance.ClassDefinition.Name;
                    System.Collections.Generic.IEnumerator<ECOI.IECPropertyValue> pVals = 
                                                            pInstance.GetEnumerator(true);
                    while (pVals.MoveNext())
                        if (!pVals.Current.IsNull)
                        {
                            DataInfo dInfo = new DataInfo();
                            try
                            {
                                dInfo.PropName = pVals.Current.AccessString;
                                dInfo.PropValue = pVals.Current.XmlStringValue;
                                if (dInfo.PropName.Equals(propKey))
                                    if (dInfo.PropValue == null)
                                        propKeyVal = "NULL";
                                    else
                                        propKeyVal = dInfo.PropValue;

                                eList.Add(dInfo);
                            }
                            catch (Exception e) { WorkPackageAddin.ComApp.MessageCenter.AddMessage(
                                "error getting property value", 
                                "error trying to get " + dInfo.PropName + e.Message, 
                                BCOM.MsdMessageCenterPriority.Error, false); }
                        }
                }
            return propKeyVal;
        }

        /// <summary>
        /// put the created group into an parent set.
        /// </summary>
        /// <param name="grpName">child group to add to a parent</param>
        /// <returns>0 if successful  and -1 if the child group does not exist</returns>
        public static int AddToParentGroup(string grpName,string parentName)
        {
            BCOM.NamedGroupElement childGroup = 
                WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(grpName);
            if (childGroup == null)
                return -1;

            BCOM.NamedGroupElement parentGroup =
                WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(parentName);
            if (parentGroup == null)
                parentGroup = 
                    WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(parentName, "Available Items");
            

            parentGroup.Description = s_appID;
            parentGroup.AllowFarReferences = true;
            parentGroup.AllowSelectMembers = true;
            parentGroup.Rewrite();
            if (!IsInGroup(parentName, (BCOM.Element)childGroup))
                try
                {
                    BCOM.NamedGroupMember parented = parentGroup.AddMember((BCOM.Element)childGroup);
                    parentGroup.Rewrite();
                }
            catch (Exception e) { /*Debug.WriteLine("Exception in adding to parent group " + e.Message);*/ }
            return 0;
        }

        /// <summary>
        /// gets the groups that hold the target element.  This is used to add the  
        /// group to the parent.
        /// </summary>
        /// <param name="el">the selected element</param>
        /// <returns>the named group that holds the selected element</returns>
        private static List<BCOM.NamedGroupElement> GetGroupElement(BCOM.Element el)
        {
            List<BCOM.NamedGroupElement> namedGroups = new List<BCOM.NamedGroupElement>();
            BCOM.NamedGroupElement[] nge = el.GetContainingNamedGroups();
            for (int i = 0; i < nge.Length; i++)
            {
                if ((!nge[i].Name.StartsWith("Available"))||(!nge[i].Name.StartsWith("Available")))
                    namedGroups.Add(nge[i]);
            }

            return namedGroups;
        }
        /// <summary>
        /// build the IWP just using the group information.  
        /// </summary>
        /// <param name="ee">the collection of elements to process.</param>
        public static void BuildIWP(BCOM.ElementEnumerator ee)
        {
            string sourceList = "Available-" + s_appID;
            string targetList = "IWP-" + s_appID;
            BCOM.NamedGroupElement sourceGroup = 
                WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(sourceList);
            if (sourceGroup == null)
                return;

            BCOM.NamedGroupElement targetGroup = 
                WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(targetList);
            if (null == targetGroup)
                targetGroup = WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(targetList);

            targetGroup.Description = s_appID;
            targetGroup.AllowFarReferences = true;
            targetGroup.AllowSelectMembers = true;
            targetGroup.AllowSelectMembers = false;

            targetGroup.Rewrite();

            //BCOM.ElementEnumerator soureElements = sourceGroup.GetElements(true, BCOM.MsdMemberTraverseType.Simple, true);

            while (ee.MoveNext())
            {
                BCOM.Element elm = ee.Current;
                List<BCOM.NamedGroupElement> hostGroups = GetGroupElement(elm);
                foreach (BCOM.NamedGroupElement srcEl in hostGroups)
                {
                    AddToParentGroup(srcEl.Name, targetList);
                }
            }

        }
        /// <summary>
        /// move an item from one list to another.  needs the property name and value for the key.
        /// </summary>
        /// <param name="targetList">Target Group</param>
        /// <param name="sourceList">Source Group</param>
        /// <param name="value">Key Value</param>
        /// <param name="prop">Key Property</param>
        /// <param name="m_connection">connection to ec repository</param>
        public static void MoveItemBetweenLists(string targetList, string sourceList,string value,string prop, ECSR.RepositoryConnection m_connection)
        {
            BCOM.NamedGroupElement sourceGroup = 
                WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(sourceList);
            if (sourceGroup == null)
                return;

            BCOM.NamedGroupElement targetGroup = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(targetList);
            if (null == targetGroup)
                targetGroup = WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(targetList);

            targetGroup.Description = s_appID;
            targetGroup.AllowFarReferences = true;
            targetGroup.AllowSelectMembers = true;
            targetGroup.AllowSelectMembers = false;

            targetGroup.Rewrite();

            BCOM.Element el;
            BCOM.ElementEnumerator ee = sourceGroup.GetElements(true, BCOM.MsdMemberTraverseType.Simple, true);
            Boolean bMoved = false;
           
                while (ee.MoveNext() && !bMoved)
                {
                     try
                        {
                                string clsName = "";
                                el = ee.Current;
                                string testValue = populateData(el, out clsName, prop, m_connection);
                                
                                if (testValue.Equals(value))
                                {
                                    List<BCOM.NamedGroupElement> src = GetGroupElement(el);
                                    foreach (BCOM.NamedGroupElement srcEl in src)
                                    {
                                        AddToParentGroup(srcEl.Name, targetGroup.Name);

                                        BCOM.NamedGroupMember sourceMember = 
                                            sourceGroup.GetMember((BCOM.Element)srcEl);
                                        if (null != sourceMember)
                                        {
                                            sourceGroup.RemoveMember(sourceMember);
                                            sourceGroup.Rewrite();
                                        }
                                        bMoved = true;
                                    }
                                }
                        }
                     catch (Exception e)
                     {
                         WorkPackageAddin.ComApp.MessageCenter.AddMessage(e.Message, 
                             e.StackTrace, BCOM.MsdMessageCenterPriority.Error, false);
                     }
                }
           
            if (!bMoved)
                WorkPackageAddin.ComApp.MessageCenter.AddMessage("not an available item", 
                    "the item was not in the available item set", 
                    BCOM.MsdMessageCenterPriority.Error, false);

        }
        /// <summary>
        /// send a JSON Array of element GUIDs ? extracted from the named list.
        /// based on the property key.
        /// </summary>
        /// <param name="name">name of the list of elements</param>
        /// <param name="propKey">the property to use as a key.</param>
        /// 
        public static void SendItemListElements(string name,string propKey)
        {
           
            BCOM.NamedGroupElement nge = null;
            try
            {
                 nge = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(name);
            }
            catch (Exception e) { WorkPackageAddin.ComApp.MessageCenter.AddMessage("No IWP Created", "there is no IWP-" + s_appID + " in the file", BCOM.MsdMessageCenterPriority.Info, false); }

            string one = "\"DataFile\":{\"fileName\":\""+s_fileName+"\",";
            string two = "\"docGUID\":\""+s_docGUID+"\",";
            string three = "\"dsName\":\""+s_dsName+"\",";
            string four = "\"appUUID\":\""+s_appUUID+"\",";
            string five = "\"ProjID\":\""+s_projID+"\"},";
            string jsonHeader = one+two+three+four+five;
            //string jsonHeader = string.Format("\"DataFile\":{\"fileName\":\"{0}\",\"docGUID\":\"{1}\",\"dsName\":\"{2}\",\"appUUID\":\"{3}\",\"ProjID\":\"{4}\"},",s_fileName,s_docGUID,s_dsName,s_appUUID,s_projID);

            if (null == nge)
            {
                string emptyJSON = "{\"AppID\":\"" + s_appID + "\"," + jsonHeader + "\"Components\":[]}";
                KeyinCommands.SendMessage( emptyJSON);
                return;
            }

            ECSR.RepositoryConnection m_connection = WorkPackageAddin.OpenConnection();

            BCOM.NamedGroupMember[] mbrs = nge.GetMembers();

            string appId = nge.Description;
            
            if (appId.Length < 1)
                appId = "null";
            
            StringBuilder sb = new StringBuilder();
            //build an array of guids to send back...?
            sb.Append("{\"AppID\":\"" + appId + "\",");
            sb.Append(jsonHeader);
            sb.Append("\"Components\":[");
            
           
            //while (ee.MoveNext())
            BCOM.NamedGroupElement el;
            BCOM.ElementEnumerator tee = null;

            for (int i = 0; i < nge.MembersCount;i++ )
            {

                el = (BCOM.NamedGroupElement)mbrs[i].GetElement();
                
                if (null != tee)
                    sb.Append(",");

                tee = null;
                sb.Append("{\"TagID\":\"" + el.Name + "\",\"Elements\":");
                //build the  inner array of elements.
                sb.Append("[");
                //if(el.IsNamedGroupElement())
                //for (int j = 0; j < el.MembersCount; j++)
               tee = el.GetElements(true, BCOM.MsdMemberTraverseType.Simple, true);
                StringBuilder elemString = new StringBuilder();
                while(tee.MoveNext())
                {                    
                    string clsName;
                    string propKeyVal;
                    string propECClassName;
                    string propECSchemaName;
                    BCOM.Element xel = tee.Current;
                    // BCOM.Element el = ee.Current;
                    propKeyVal = populateData(xel, out clsName, propKey, m_connection);
                    propECClassName = populateData(xel, out clsName, "EC_CLASS_NAME", m_connection);

                    if (propECClassName.Length < 1)
                        propECClassName = " ";

                    propECSchemaName = populateData(xel, out clsName, "EC_SCHEMA_NAME", m_connection);

                    if (propECSchemaName.Length < 1)
                        propECSchemaName = " ";

                    elemString.Append("{\""+propKey+"\":\""+propKeyVal+"\", \"EC_CLASS_NAME\":\""+propECClassName+"\", \"EC_SCHEMA_NAME\":\""+propECSchemaName+"\"},");
                }
                string arrayElement = elemString.ToString().TrimEnd(',');
                sb.Append(arrayElement);
                sb.Append("]}");
                
            }
            //close the components list and data file
            sb.Append("]}");
            KeyinCommands.SendMessage(sb.ToString());

            WorkPackageAddin.CloseConnection(m_connection);
        }
    }
    /// <summary>
    /// relates to the page
    /// </summary>
    public class ItemDictionaries
    {
        public string name { get; set; }
        public List< ItemSetList> items { get; set; }
    }
    /// <summary>
    /// this is the tagid
    /// </summary>
    public class ItemSetList
    {
        public string name { get; set; }
        public Dictionary<string,List<ItemSetInfo>> items;
    }
    /// <summary>
    /// this is the actual item.
    /// </summary>
    public class ItemSetInfo
    {
        public string name { get; set; }
        public string propName { get; set; }
        public string elKey { get; set; }
        public string schemaName { get; set; }
    }
}
