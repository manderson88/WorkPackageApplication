using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkPackageApplication
{
    /// <summary>
    /// a simple class for working with the datagrid.  This has two columns
    /// a property name and value that will be displayed on a datagridview.
    /// </summary>
    public class DataInfo
    {
        /// <summary>
        /// default constuctor
        /// </summary>
        public DataInfo()
        {
            PropName = "";
            PropValue = "";
        }
        /// <summary>
        /// a constructor with the name and value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public DataInfo(string name, string value)
        {
            PropName = name;
            PropValue = value;
        }
        public string PropName { get; set; }
        public string PropValue { get; set; }
    }
}
