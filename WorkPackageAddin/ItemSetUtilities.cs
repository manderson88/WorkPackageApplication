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
                AddItemToSetByID(name, "GUID", guidString);
            }
        }
        //-----------------------------------------------------------------------------------
        // Takes current model (which may be read-only), attaches to a separate, editable
        // working model, then switches current session to that working model.
        // This allows additional views to be packaged with a read-only PMM (project master model)
        // prior to publishing.
        //
        // Input: unparsed - string value provided during keyin, currently unused
        //-----------------------------------------------------------------------------------
        public static void _AttachToWorkingModel(System.String unparsed)
        {
            // Prerequisite: Attaching PMM prior to view generation
            // Attaching reference of PMM
            BCOM.DesignFile pmmDgnFile = WorkPackageAddin.ComApp.ActiveDesignFile;
            BCOM.ModelReference pmmModel = WorkPackageAddin.ComApp.ActiveModelReference;

            //Example: lkr default expands to "C:\\Dev\\OPM_SS6_workspaces\\PWR_IMP_LOCAL\\Workfiles\\Models\\";
            BCOM.Workspace workSpace = WorkPackageAddin.ComApp.ActiveWorkspace;
            // working file extension to be <fileName>.dgn
            string workPath = workSpace.ExpandConfigurationVariable("$(MS_DEF)"); // nested vars
            string workFileName = pmmDgnFile.Name.Remove(pmmDgnFile.Name.IndexOf('.')) + ".dgn";
            string workFilePathName = workPath + "_" + workFileName;

            string seedFileName = workSpace.ConfigurationVariableValue("MS_DESIGNSEED", true); // -> "pmseed3d"
            WorkPackageAddin.ComApp.CreateDesignFile(seedFileName, workFilePathName, false);
            BCOM.DesignFile workFile = WorkPackageAddin.ComApp.OpenDesignFileForProgram(workFilePathName, false); // opens a design file
            BCOM.ModelReference workModel = workFile.DefaultModelReference;
            BCOM.Attachment att = workModel.Attachments.AddCoincident(pmmDgnFile.FullName, pmmModel.Name, pmmModel.Name, pmmModel.Description, true);
            // workModel.Attachments.AddCoincident(pmmDgnFile.FullName, pmmModel.Name, pmmModel.Name, pmmModel.Description, false);
            att.DisplayAsNested = true;
            att.NestLevel = 99;
            att.Rewrite();
            workFile.Save();
            workFile.Close();
            WorkPackageAddin.ComApp.OpenDesignFile(workFilePathName, false, BCOM.MsdV7Action.UpgradeToV8); // swapping out
            // ...now using workmodel as the active model for future operations.
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
        public static void AddItemToSetByID(string name, string propKey, string elID)
        {
            BCOM.NamedGroupElement nge = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(name);

            if (null == nge)
                nge = WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(name);

            nge.AllowFarReferences = true;
            nge.AllowSelectMembers = true;
            nge.AllowSelectMembers = false; //lets us select one member of the group.

            nge.Rewrite();
            //open the connect to the  repository
            if(conn == null)
                conn = WorkPackageAddin.OpenConnection();
            //get the schema 
            ECOS.IECSchema pSchema = null;
            pSchema = WorkPackageAddin.LocateExampleSchema(conn, "OpenPlant_3D.01.04");
            if (pSchema == null)
                return;

            BCOM.Element e = LocateClass.FindSingleElement(pSchema, "", propKey, elID, true, false,conn);
            if (e == null)
            {
               // WorkPackageAddin.CloseConnection(conn);
                return;
            }
            //WorkPackageAddin.CloseConnection(conn);

            try
            {
                nge.AddMember(e);
                nge.Rewrite();
                KeyinCommands.SendMessage("added element to " + name + " items");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                WorkPackageAddin.ComApp.MessageCenter.AddMessage("Element already in the work package", "Item already in a work package", BCOM.MsdMessageCenterPriority.Info, false);
                KeyinCommands.SendMessage("Item " + elID + " already in a work package");
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
            System.Collections.Generic.IList<ECOI.IECInstance> pInstances = BDGNP.DgnECPersistence.GetAllInstancesOnElement(m_connection, (System.IntPtr)pElement.MdlElementRef(), (System.IntPtr)pElement.ModelReference.MdlModelRefP(), ECP.LoadModifiers.None, ECP.LoadModifiers.None, 1, null);
            foreach (ECOI.IECInstance pInstance in pInstances)
                if (pInstance.ContainsValues)
                {
                    clsName = pInstance.ClassDefinition.Name;
                    System.Collections.Generic.IEnumerator<ECOI.IECPropertyValue> pVals = pInstance.GetEnumerator(true);
                    while (pVals.MoveNext())
                        if (!pVals.Current.IsNull)
                        {
                            DataInfo dInfo = new DataInfo();
                            //Debug.WriteLine(string.Format("the property is {0} is {1}", pVals.Current.AccessString, pVals.Current.XmlStringValue));
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
        /// move an item from one list to another.  needs the property name and value for the key.
        /// </summary>
        /// <param name="targetList">Target Group</param>
        /// <param name="sourceList">Source Group</param>
        /// <param name="value">Key Value</param>
        /// <param name="prop">Key Property</param>
        public static void MoveItemBetweenLists(string targetList, string sourceList,string value,string prop)
        {
            ECSR.RepositoryConnection m_connection = WorkPackageAddin.OpenConnection();

            BCOM.NamedGroupElement sourceGroup = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(sourceList);
            if (sourceGroup == null)
                return;

            BCOM.NamedGroupElement targetGroup = WorkPackageAddin.ComApp.ActiveModelReference.GetNamedGroup(targetList);
            if (null == targetGroup)
                targetGroup = WorkPackageAddin.ComApp.ActiveModelReference.AddNewNamedGroup(targetList);

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
                                    targetGroup.AddMember(el);
                                    targetGroup.Rewrite();
                                    BCOM.NamedGroupMember sourceMember = sourceGroup.GetMember(el);
                                    sourceGroup.RemoveMember(sourceMember);
                                    sourceGroup.Rewrite();
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

            if (m_connection != null)
                WorkPackageAddin.CloseConnection(m_connection);
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
            BCOM.ElementEnumerator ee = nge.GetElements(true, BCOM.MsdMemberTraverseType.Simple, false);
            StringBuilder sb = new StringBuilder();
            //build an array of guids to send back...?
            sb.Append("[");
            while (ee.MoveNext())                         
            {
                string clsName;
                string propKeyVal;
                BCOM.Element el = ee.Current;
                propKeyVal = populateData(el, out clsName,propKey,m_connection);
                sb.Append("{\"" + propKey + "\":\"" + propKeyVal + "\"},");
            }
            string arrayElement = sb.ToString().TrimEnd(',');
                        
            KeyinCommands.SendMessage(arrayElement + "]");

            WorkPackageAddin.CloseConnection(m_connection);
        }
    }
}
