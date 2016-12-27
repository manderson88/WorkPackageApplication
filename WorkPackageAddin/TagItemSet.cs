using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCOM = Bentley.Interop.MicroStationDGN;

namespace WorkPackageApplication
{
    public class TagItemSet:IEquatable<TagItemSet>
    {
        public TagItemSet(long mdlID, long flPos, string logical, string clsName, string clsInfo)
        {
            filePos = flPos;
            modelID = mdlID;
            logicalName = logical;
            className = clsName;
            classInfo = clsInfo;

        }
        public TagItemSet()
        {
            filePos = 0;
            modelID = 0;
            logicalName = "";
        }
       // public bool Hilite { get; set; }
        public long filePos { get; set; }
        public long modelID { get; set; }
        public string className { get; set; }
        public string classInfo { get; set; }
        public string logicalName { get; set; }

        #region IEquatable<TagItemSet> Members

        public bool Equals(TagItemSet other)
        {
            if ((this.filePos == other.filePos) && (this.modelID == other.modelID)&&(this.className == other.className)&&(this.classInfo == other.classInfo))
                return true;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return (int)filePos;
        }
        #endregion
    }
}
