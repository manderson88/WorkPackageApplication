using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
    /// this is various methods to locate class information based on query paramters.
    /// </summary>
    static class LocateClass
    {
        public static StatusForm m_statusForm = new StatusForm();
        /// <summary>
        /// this is some code to parse through the persistant element path (PEP).  
        /// </summary>
        /// <param name="pepString"></param>
        /// <returns>returns an array of string that are the model refs then the element id.</returns>
        private static string[] ParsePEPath(string pepString)
        {
            string[] pep;
            //if the  string is only 10 chars then rip the first two chars off and return
            //if (pepString.Length == 10)
            //{
            //    pep = new string[1];
            //    pep[0] = (pepString.Substring(2, 8));
            //}

            //length -2 mod 8?
            int count = pepString.Length/8;
            pep = new string[count];
            for (int i = 0; i < count; ++i)
            {
                pep[i] = pepString.Substring(2 + (8 * i), 8);
            }
            return pep;
        }
        /// <summary>
        /// this is an alternate method to parse the PEP into a real model reference.  not tested
        /// </summary>
        /// <param name="oAttachment"></param>
        /// <param name="targetID"></param>
        /// <param name="pep"></param>
        private static void ParseAttachPath(BCOM.Attachment oAttachment, int targetID, string[] pep)
        {
            if (oAttachment.Attachments.Count > 0)
            foreach (BCOM.Attachment oAttach in oAttachment.Attachments)
            {
                if (targetID == oAttach.ElementID)
                    Debug.WriteLine("found");
            }
        }
        /// <summary>
        /// This will take in the ECInstance and use an undocumented tool to get the model and element back
        /// </summary>
        /// <param name="pInst"></param>
        /// <returns>returns a COM element.</returns>
        private static BCOM.Element ElementFinder(ECOI.IECInstance pInst)
        {
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();

            //string instID = BDGNP.DgnECPersistence.CreateInstanceId(conn, (System.IntPtr)pElement.ModelReference.MdlModelRefP(), (ulong)pElement.ID, "");
            ulong filePos;
            System.IntPtr elPtr;
            string localKey = "";

            BDGNP.DgnECPersistence.TryGetElementInfo(conn, pInst.InstanceId, out elPtr, out filePos, out localKey);

            BCOM.ModelReference oModel = WorkPackageAddin.ComApp.MdlGetModelReferenceFromModelRefP((int)elPtr);
            BCOM.Element el;
            el = oModel.GetElementByID((long)filePos);

            WorkPackageAddin.CloseConnection(conn);

            return el;
        }
        /// <summary>
        /// this finds the element based on the ec instance id.
        /// the model  is the first segment of the id string
        /// the element is the  second half of the id string
        /// for  both the algorithm seems to be grab the part
        /// of the sting then reverse the pairs and conver to 
        /// base 10 from hex.
        /// </summary>
        /// <param name="pInstance"></param>
        public static TagItemSet FindHostElement(ECOI.IECInstance pInstance)
        {
            TagItemSet tset = new TagItemSet();

            if (pInstance != null)
            {
                BCOM.Element el = ElementFinder(pInstance);
                tset.modelID = el.ModelReference.MdlModelRefP();
                tset.filePos = el.ID;
            }

            return tset;
        }
        /// <summary>
        /// example code DO NOT USE
        /// this will get the attribute that we need to find. this is still under investigation
        /// </summary>
        /// <param name="pInstance"></param>
        /// <param name="conn"></param>
        public static void TraverseRelationship(ECOI.IECInstance pInstance, ECSR.RepositoryConnection conn)
        {
            ECOI.IECInstance result;
            String instanceId = pInstance.InstanceId;
            ECPQ.ECInstanceIdExpression idExpression = new ECPQ.ECInstanceIdExpression(instanceId);
            ECPQ.ECQuery query = new ECPQ.ECQuery(pInstance.ClassDefinition);

            query.SelectClause.SelectAllProperties = true;
            
            int depth = 2;

            ECPQ.QueryHelper.SelectAllRelatedInstances(query, true, depth);

            query.WhereClause.Add(idExpression);

            //------------
            Bentley.Collections.IExtendedParameters extendedParameters = Bentley.Collections.ExtendedParameters.Create();
            System.Collections.Generic.List<System.String> providerFilterList = new System.Collections.Generic.List<System.String>();
            providerFilterList.Add("Bentley.DGNECPlugin");
            Bentley.ECSystem.ProviderFilter.SetIn(extendedParameters, new Bentley.ECSystem.ProviderFilter(true, providerFilterList));
            //--------------------
            Bentley.EC.Persistence.PersistenceService psvc = Bentley.EC.Persistence.PersistenceServiceFactory.GetService();
            ECP.QueryResults results = psvc.ExecuteQuery(conn, query, -1, extendedParameters);

            if (results.Count > 0 /*&& results->Count == 1*/)
            {
                result = results.GetElement(0);
                Debug.WriteLine(result.ToString());
                result.Dump(Console.Out, "debug:");
                try
                {
                    FindHostElement(pInstance);
                }
                catch (Bentley.ECObjects.ECObjectsException.PropertyNotFound e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

        }
        /// <summary>
        /// this is from the finder dialog.  It will pass in the schema, class name, property in the class, and value to match.
        /// there are options to search all related classes and to use the like vs exact match.
        /// </summary>
        /// <param name="schema">the schema for the class</param>
        /// <param name="className">the class to search for</param>
        /// <param name="propName">the property name to key on.</param>
        /// <param name="value">the value to select</param>
        /// <param name="allClasses">to search all classes not just the exact one.</param>
        /// <param name="useLike">use a like comparison operation.</param>
        /// <returns></returns>
        public static List<TagItemSet> FindClassAndValue(string schema, string className, string propName, string value ,Boolean allClasses, Boolean useLike)
        {
            if (null != m_statusForm)
                m_statusForm.Show();
            else
            {
                m_statusForm = new StatusForm();
                m_statusForm.Show();
            }
            List<TagItemSet> elList = new List<TagItemSet>();
            ECOS.IECSchema pSchema = null;
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            pSchema = WorkPackageAddin.LocateExampleSchema(conn, schema);
            if (pSchema == null)
                return null;

            System.Collections.IEnumerator clsEnum = WorkPackageAddin.GetClassesInSchema(pSchema);
            
            ECPQ.ECQuery query;
            if (allClasses)
            {
                string[] names = WorkPackageAddin.GetAllClassNamesInSchema(pSchema);
                query = ECPQ.QueryHelper.CreateQuery(pSchema, names);
            }
            else
                query = ECPQ.QueryHelper.CreateQuery(pSchema,className);
            //get the class in question.  no class no try.
            ECOS.IECClass pClass;
            if (className.Length > 1)
                pClass = pSchema[className];
            else
                pClass = null;

            if (pClass == null)
            {
                m_statusForm.Hide();
                return null;
            }
            ECOS.IECProperty namedProperty = null;
            //make sure there is a property name to find.
            if(propName.Length>0)
                namedProperty = pClass[propName];

            //ECPQ.QueryHelper.SelectAllRelatedInstances(query, false, 3);
            query.SelectClause.SelectAllProperties = true;
            //if there is a named property thenn look to see if we are looking
            // for an exact match or not.
            if (namedProperty != null)
            {
                ECPQ.RelationalOperator usesLike = ECPQ.RelationalOperator.EQ;
                if (useLike)
                    usesLike = ECPQ.RelationalOperator.LIKE;

                ECPQ.QueryHelper.WherePropertyExpressions(query, namedProperty, usesLike, value);
            }

            //make the query
            ECP.PersistenceService psvc = ECP.PersistenceServiceFactory.GetService();
            ECP.QueryResults pResult = psvc.ExecuteQuery(conn, query, -1);
            //if there are any results loop through them.
            if (pResult.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("No Items Found", "Items Find", System.Windows.Forms.MessageBoxButtons.OK);
                m_statusForm.Hide();
                return null;
            }
            m_statusForm.SetStatus(string.Format("found {0} items", pResult.Count));
            for (int i = 0; i < pResult.Count; ++i)
            {
                ECOI.IECInstance iInstance;

                if (pResult.TryGetElement(i, out iInstance))
                {
                    TagItemSet tset;
                    string val = "";
                    if (namedProperty != null)
                    {
                        val = iInstance.GetAsString(propName);
                        Debug.WriteLine(val);
                    }
                    tset = FindHostElement(iInstance);
                   // LocateClass.TraverseRelationship(iInstance, conn);
                    tset.className = className;
                    if(value.Length>0)
                        tset.classInfo = val;
                    if(tset.filePos >0)
                        elList.Add(tset);
                }
            }
            WorkPackageAddin.CloseConnection(conn);
            m_statusForm.Hide();
        return elList;
        }
        /// <summary>
        /// this is connected to the Query Form dialog.  it will take in the 
        /// schema name, class name, and property name and value.  There are 
        /// options to search for all classes that have the property and use
        /// the like not equal operator.
        /// </summary>
        /// <param name="schemaName">the name of the schema</param>
        /// <param name="clsName">the class name </param>
        /// <param name="propName">the property name to query against</param>
        /// <param name="searchValue">the value to search for.</param>
        /// <param name="useLike">true to use the like operator</param>
        /// <param name="allClasses">true to use all the classes.</param>
        /// <returns>a List of strings that are the attribute being searched for.</returns>
        public static List<instancePair> FindInstanceByClassAndProperty(string schemaName, string clsName, string propName, string searchValue, int relationOp ,Boolean allClasses)
        {
            List<instancePair> pList = new List<instancePair>();
            ECOS.IECSchema pSchema = null;
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            pSchema = WorkPackageAddin.LocateExampleSchema(conn, schemaName);
            if (pSchema == null)
                return null;
            m_statusForm.Show();
            System.Collections.IEnumerator clsEnum = WorkPackageAddin.GetClassesInSchema(pSchema);
            
            ECPQ.ECQuery query;
            if (allClasses)
            {
                string[] names = WorkPackageAddin.GetAllClassNamesInSchema(pSchema);
                query = ECPQ.QueryHelper.CreateQuery(pSchema, names);
            }
            else
                query = ECPQ.QueryHelper.CreateQuery(pSchema,clsName);

            ECOS.IECClass pClass;
            pClass = pSchema[clsName];
            if (pClass == null)
                return null;

            m_statusForm.SetStatus(string.Format("setting the property to {0} ", propName));
            ECOS.IECProperty namedProperty = null;
            if (propName.Length>0)
                namedProperty = pClass[propName];

            ECPQ.RelationalOperator usesLike = ECPQ.RelationalOperator.EQ;

            switch (relationOp)
            {
                case 0:
                    usesLike = ECPQ.RelationalOperator.IN;
                    break;

                case 1:
                    usesLike = ECPQ.RelationalOperator.GT;
                    break;

                case 2:
                    usesLike = ECPQ.RelationalOperator.GTEQ;
                    break;

                case 3:
                    usesLike = ECPQ.RelationalOperator.LT;
                    break;

                case 4:
                    usesLike = ECPQ.RelationalOperator.LTEQ;
                    break;

                case 5:
                    usesLike = ECPQ.RelationalOperator.EQ;
                    break;

                case 6:
                    usesLike = ECPQ.RelationalOperator.NE;
                    break;

                case 7:
                    usesLike = ECPQ.RelationalOperator.LIKE;
                    break;

               // default:
               //     usesLike = ECPQ.RelationalOperator.EQ;
            }
            //if (namedProperty != null)
            //{
              //  if (useLike)
                //    usesLike = ECPQ.RelationalOperator.LIKE;
                //if (searchValue.CompareTo("*") == 0)
                   // query.r
                    //usesLike = ECPQ.RelationalOperator.ISNOTNULL;

                ECPQ.QueryHelper.WherePropertyExpressions(query, namedProperty, usesLike, searchValue);
            //}
                int maxDepth = 2;
            query.SelectClause.SelectAllProperties = true;
            query.SelectClause.SelectDistinctValues = true;
            ECPQ.QueryHelper.SetPolymorphic(query, true);
            ECPQ.PropertyAccessorExpression pae = new ECPQ.PropertyAccessorExpression(usesLike, propName, searchValue);
            ECPQ.OrderByCriterion obc = new ECPQ.OrderByCriterion(propName, true);
            query.OrderBy.Add(propName, true);
            ECPQ.QueryHelper.SelectAllRelatedInstances(query, true, maxDepth);
            ECP.PersistenceService psvc = ECP.PersistenceServiceFactory.GetService();
            Bentley.Collections.IExtendedParameters extendedParameters = Bentley.Collections.ExtendedParameters.Create();
            System.Collections.Generic.List<System.String> providerFilterList = new System.Collections.Generic.List<System.String>();
            providerFilterList.Add("Bentley.DGNECPlugin");
            Bentley.ECSystem.ProviderFilter.SetIn(extendedParameters, new Bentley.ECSystem.ProviderFilter(true, providerFilterList));
            //--------------------
            //ECP.LoadModifiers loadMOds =  ECP.LoadModifiers.LoadGeometryFromDGN | ECP.LoadModifiers.IncludeECBusinessKey;
            //ECP.QueryResults pResult = psvc.ExecuteQuery(conn, query, 100,loadMOds);
            query.PrepareForExecution();
            ECP.QueryResults pResult = psvc.ExecuteQuery(conn, query, -1, extendedParameters);
            
            Debug.WriteLine (string.Format("the query is {0}",query.ToString()));
            m_statusForm.SetStatus(string.Format("running query {0} ", query.ToString()));
            for (int i = 0; i < pResult.Count; ++i)
            {
                ECOI.IECInstance iInstance;
                m_statusForm.SetStatus(string.Format("processing {0} of {1} items", i, pResult.Count));
                if (pResult.TryGetElement(i, out iInstance))
                {
                    string val;
                    if (propName.Length > 0)
                    {
                        val = iInstance.GetAsString(propName);
                        Debug.WriteLine(val);
                    }
                    else
                        val = iInstance.GetType().Name;
                    
                    instancePair p = new instancePair(val,iInstance.ClassDefinition.Name);
                   
                    if (!pList.Contains(p))
                        pList.Add(p);
                }
            }

            if (pResult.Count == 0)
                System.Windows.Forms.MessageBox.Show("No Items Found", "Class Info", System.Windows.Forms.MessageBoxButtons.OK);
            WorkPackageAddin.CloseConnection(conn);
            m_statusForm.Hide();
            return pList;
        }
        /// <summary>
        /// example code.  this is hard coded to look in the openplant3d 01.06 schema only
        /// </summary>
        /// <param name="searchingString"></param>
        /// <returns></returns>
        public static Boolean FindItem(string searchingString)
        {
            Boolean retValue = false;
            ECOS.IECSchema pSchema=null;
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            m_statusForm.Show();
            m_statusForm.SetStatus("starting find item");
            pSchema = WorkPackageAddin.LocateExampleSchema (conn,"OpenPlant_3D.01.06");
            System.Collections.IEnumerator classesInSchema = WorkPackageAddin.GetClassesInSchema(pSchema);
            ECOS.IECClass pClass;
            //get one of the classes that should have the property we want to work with.
            pClass = pSchema["PIPE"];
            string [] names = WorkPackageAddin.GetAllClassNamesInSchema(pSchema);
            {
                
                ECPQ.ECQuery query = ECPQ.QueryHelper.CreateQuery(pSchema, names);
                //this seems to be a key step. 
                Bentley.ECObjects.Schema.IECProperty namePathProperty = pClass["ISO_SHEET"];
                ECPQ.QueryHelper.WherePropertyExpressions(query, namePathProperty, ECPQ.RelationalOperator.EQ, searchingString);
                Bentley.EC.Persistence.PersistenceService psvc = Bentley.EC.Persistence.PersistenceServiceFactory.GetService();
                ECP.QueryResults pResult = psvc.ExecuteQuery(conn, query, 100);
                m_statusForm.SetStatus(string.Format("found {0} items",pResult.Count));
                for (int c = 0; c < pResult.Count; c++)
                {
                    ECOI.IECInstance iInstance;

                    if (pResult.TryGetElement(c, out iInstance))
                    {
                        string val = iInstance.GetAsString("ISO_SHEET");
                        Debug.WriteLine(val);
                        FindHostElement(iInstance);
                        retValue = true;
                    }
                }
            }
            m_statusForm.Hide();
            return retValue;
        }
        /// <summary>
        /// this will find the elements that have the value for the iso sheet property.
        /// it needs a search string 
        /// </summary>
        /// <param name="searchString"> the value to look for.</param>
        /// <returns>true that it found elements.</returns>
        public static List<TagItemSet> FindClassAndValueRelated(string schema, string className, string propName, string value, Boolean allClasses, Boolean useLike)
        {
            List<TagItemSet> elList = new List<TagItemSet>();
            ECOS.IECSchema pSchema = null;
            //put in a default class name to look for
            string lastClassName="EQUIPMENT";
            //open the connect to the  repository
            ECSR.RepositoryConnection conn = WorkPackageAddin.OpenConnection();
            //get the schema 
            pSchema = WorkPackageAddin.LocateExampleSchema(conn, schema);
            if (pSchema == null)
                return null;
            //get the list of classes that are available
            System.Collections.IEnumerator clsEnum = WorkPackageAddin.GetClassesInSchema(pSchema);
            string[] names = WorkPackageAddin.GetAllClassNamesInSchema(pSchema);
            //building a query
            ECPQ.ECQuery query;
            if (allClasses)
            {
                query = ECPQ.QueryHelper.CreateQuery(pSchema, names);
                lastClassName = names[names.Length - 1];
            }
            else
                query = ECPQ.QueryHelper.CreateQuery(pSchema, className);

            ECOS.IECClass pClass;

            if (className.Length > 1)
                pClass = pSchema[className];
            else
                pClass = null;

            ECPQ.QueryHelper.SetPolymorphic(query, true);
            ECPQ.QueryHelper.SelectLogicalChildInstances(query, conn, true, 3);
            ECPQ.RelationalOperator usesLike = ECPQ.RelationalOperator.EQ;
            if (useLike)
                usesLike = ECPQ.RelationalOperator.LIKE;

            //get all the properties
            query.SelectClause.SelectAllProperties = true;
            //this was slow.
            //ECPQ.QueryHelper.SelectAllRelatedInstances(query, true, 3);
           
            //since this query is using a specific named propery we need to find
            //a class that has this property
            ECOS.IECProperty namedProperty=null;
            try
            {
                while ((namedProperty == null) && (clsEnum.MoveNext()))
                {
                    pClass = (ECOS.IECClass)clsEnum.Current;

                    namedProperty = pClass[propName];
                }
            }
            catch (Bentley.ECObjects.ECObjectsException.ArgumentError ae) { Debug.WriteLine(ae.Message); }

            if (namedProperty != null)
            //add the where propery clause.
                ECPQ.QueryHelper.WherePropertyExpressions(query, namedProperty, usesLike, value);

            //make the query
            ECP.PersistenceService psvc = ECP.PersistenceServiceFactory.GetService();
            ECP.QueryResults pResult = psvc.ExecuteQuery(conn, query, 100);
            // loop through the results.
            for (int i = 0; i < pResult.Count; ++i)
            {
                ECOI.IECInstance iInstance;

                if (pResult.TryGetElement(i, out iInstance))
                {
                    TagItemSet tset;
                    tset = FindHostElement(iInstance);
                    if (tset.filePos > 0)
                        elList.Add(tset);
                }
            }
            if (pResult.Count == 0)
                WorkPackageAddin.ComApp.MessageCenter.AddMessage("No Items found", "the query " + query.ToString(), BCOM.MsdMessageCenterPriority.Info, false);

            WorkPackageAddin.CloseConnection(conn);
            return elList;
        }
        /// <summary>
        /// get a single element
        /// </summary>
        /// <param name="schema">The schema to use</param>
        /// <param name="className">the class for the business data</param>
        /// <param name="propName">The key property</param>
        /// <param name="value">the key value </param>
        /// <param name="allClasses">use all classes</param>
        /// <param name="useLike">use a like search</param>
        /// <returns></returns>
        public static BCOM.Element FindSingleElement(ECOS.IECSchema pSchema, string className,
                                                     string propName, string value,
                                                     Boolean allClasses, Boolean useLike,
                                                     ECSR.RepositoryConnection conn)
        {
            BCOM.Element el = null;
            
            //put in a default class name to look for
            string lastClassName = "EQUIPMENT";
           
           
            //get the list of classes that are available
            System.Collections.IEnumerator clsEnum = WorkPackageAddin.GetClassesInSchema(pSchema);
            string[] names = WorkPackageAddin.GetAllClassNamesInSchema(pSchema);
            //building a query
            ECPQ.ECQuery query;
            if (allClasses)
            {
                query = ECPQ.QueryHelper.CreateQuery(pSchema, names);
                lastClassName = names[names.Length - 1];
            }
            else
                query = ECPQ.QueryHelper.CreateQuery(pSchema, className);

            ECOS.IECClass pClass;

            if (className.Length > 1)
                pClass = pSchema[className];
            else
                pClass = null;

            //ECPQ.QueryHelper.SetPolymorphic(query, true);
            //ECPQ.QueryHelper.SelectLogicalChildInstances(query, conn, true, 3);
            ECPQ.RelationalOperator usesLike = ECPQ.RelationalOperator.EQ;
            if (useLike)
                usesLike = ECPQ.RelationalOperator.LIKE;

            //get all the properties
            //query.SelectClause.SelectAllProperties = true;
            //this was slow.
            //ECPQ.QueryHelper.SelectAllRelatedInstances(query, true, 3);

            //since this query is using a specific named propery we need to find
            //a class that has this property
            ECOS.IECProperty namedProperty = null;
            try
            {
                while ((namedProperty == null) && (clsEnum.MoveNext()))
                {
                    pClass = (ECOS.IECClass)clsEnum.Current;

                    namedProperty = pClass[propName];
                }
            }
            catch (Bentley.ECObjects.ECObjectsException.ArgumentError ae) { Debug.WriteLine(ae.Message); }

            if (namedProperty != null)
                //add the where propery clause.
                ECPQ.QueryHelper.WherePropertyExpressions(query, namedProperty, usesLike, value);

            //make the query
            ECP.PersistenceService psvc = ECP.PersistenceServiceFactory.GetService();
            ECP.QueryResults pResult = psvc.ExecuteQuery(conn, query, 100);
            // loop through the results.
            for (int i = 0; i < pResult.Count; ++i)
            {
                ECOI.IECInstance iInstance;

                if (pResult.TryGetElement(i, out iInstance))
                {
                    TagItemSet tset;
                    tset = FindHostElement(iInstance);
                    if (tset.filePos > 0)
                    {
                        //get the element.
                        BCOM.ModelReference oModel = WorkPackageAddin.ComApp.MdlGetModelReferenceFromModelRefP((int)tset.modelID);
                        el = oModel.GetElementByID(tset.filePos);
                    }
                }
            }
            if (pResult.Count == 0)
                WorkPackageAddin.ComApp.MessageCenter.AddMessage("No Items found", "the query " + query.ToString(), BCOM.MsdMessageCenterPriority.Info, false);

            //WorkPackageAddin.CloseConnection(conn);
            return el;
        }
    }
}
