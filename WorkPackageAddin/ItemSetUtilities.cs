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

        public static void AddToList(string guidString,bool newList)
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
        public static void AddItemToSetByID(string name, string propKey, string elID,string schemaName)
        {
            BCOM.NamedGroupElement nge = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(name);
            Boolean bGlobalConn = true;

            if (null == nge)
                nge = WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(name);

            nge.AllowFarReferences = true;
            nge.AllowSelectMembers = true;
            nge.AllowSelectMembers = false; //lets us select one member of the group.

            nge.Rewrite();
            
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
                    WorkPackageAddin.CloseConnection(conn);
                return;
            }
            //get the  element based on the property.  if no element is found then return
            BCOM.Element e = LocateClass.FindSingleElement(pSchema, "", propKey, elID, true, false,conn);
            if (e == null)
            {
                if (!bGlobalConn)
                    WorkPackageAddin.CloseConnection(conn);
                return;
            }
            try
            {
                nge.AddMember(e);
                nge.Rewrite();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                WorkPackageAddin.ComApp.MessageCenter.AddMessage("Element already in the work package", 
                    "Item already in a work package", BCOM.MsdMessageCenterPriority.Info, false);
                KeyinCommands.SendMessage("Item " + elID + " already in a work package");
            }
            finally
            {
                if (!bGlobalConn)
                    WorkPackageAddin.CloseConnection(conn);
            }
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
                    System.Collections.Generic.IEnumerator<ECOI.IECPropertyValue> pVals = pInstance.GetEnumerator(true);
                    while (pVals.MoveNext())
                        if (!pVals.Current.IsNull)
                        {
                            DataInfo dInfo = new DataInfo();
                            dInfo.PropName = pVals.Current.AccessString;
                            dInfo.PropValue = pVals.Current.XmlStringValue;
                            if (dInfo.PropName.Equals(propKey))
                                propKeyVal = dInfo.PropValue;

                            eList.Add(dInfo);
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
            BCOM.NamedGroupElement childGroup = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(grpName);
            if (childGroup == null)
                return -1;

            BCOM.NamedGroupElement parentGroup = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(parentName);
            if (parentGroup == null)
                parentGroup = WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(parentName, "List of Components");

            parentGroup.Description = s_appID;
            parentGroup.AllowFarReferences = true;
            parentGroup.AllowSelectMembers = true;
            parentGroup.Rewrite();
            BCOM.NamedGroupMember parented = parentGroup.AddMember((BCOM.Element)childGroup);
            parentGroup.Rewrite();

            return 0;
        }

        /// <summary>
        /// gets the group that holds the target element.  This is used to add the  
        /// group to the parent.
        /// </summary>
        /// <param name="el">the selected element</param>
        /// <returns>the named group that holds the selected element</returns>
        private static BCOM.NamedGroupElement GetGroupElement(BCOM.Element el)
        {
            BCOM.NamedGroupElement[] nge = el.GetContainingNamedGroups();
            for (int i = 0; i < nge.Length; i++)
            {
                if (0 != nge[i].Name.CompareTo("Available"))
                    return nge[i];
            }

            return null;
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
            BCOM.NamedGroupElement sourceGroup = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(sourceList);
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
                                    BCOM.NamedGroupElement src = GetGroupElement(el);
                                    AddToParentGroup(src.Name,targetGroup.Name);

                                    BCOM.NamedGroupMember sourceMember = sourceGroup.GetMember((BCOM.Element)src);
                                    if (null != sourceMember)
                                    {
                                        sourceGroup.RemoveMember(sourceMember);
                                        sourceGroup.Rewrite();
                                    }
                                    bMoved = true;
                                    break;
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
            ECSR.RepositoryConnection m_connection = WorkPackageAddin.OpenConnection();
            BCOM.NamedGroupElement nge = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(name);

            string jsonHeader = string.Format("\"DataFile\":{\"fileName\":\"{1}\",\"docGUID\":\"{2}\",\"dsName\":\"{3}\",\"appUUID\":\"{4}\",\"ProjID\":\"{5}\"},",s_fileName,s_docGUID,s_dsName,s_appUUID,s_projID);

            if (null == nge)
            {
                string emptyJSON = string.Format("{\"AppID\":\"{0}{1}\"Components\":[]}",s_appID,jsonHeader);
                KeyinCommands.SendMessage( emptyJSON);
                return;
            }
                
            BCOM.NamedGroupMember[] mbrs = nge.GetMembers();

            string appId = nge.Description;
            
            if (appId.Length < 1)
                appId = "null";
            
            //BCOM.ElementEnumerator ee = nge.GetElements(true, BCOM.MsdMemberTraverseType.Simple, false);
            StringBuilder sb = new StringBuilder();
            //build an array of guids to send back...?
            sb.Append(string.Format("{\"AppID\":\"{0}\",",appId));
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
                sb.Append(string.Format("{\"TagID\":\"{0}\",\"Elements\":",el.Name));
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

                    elemString.Append(string.Format("{\"{0}\":\"{1}\", \"EC_CLASS_NAME\":\"{2}\", \"EC_SCHEMA_NAME\":\"{3}\"},",propKey,propKeyVal,propECClassName,propECSchemaName));
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
}
