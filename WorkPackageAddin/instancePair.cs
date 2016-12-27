using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkPackageApplication
{
    public class instancePair: IEquatable<instancePair>
    {
        public instancePair()
        {
            clsName = "";
            value = "";
        }
        public instancePair(string val)
        {
            value = val;
        }
        public instancePair(string val, string name)
        {
            value = val;
            clsName = name;
        }

        public string clsName { get; set; }
        public string value { get; set; }
        public bool Equals(instancePair p)
        {
            if ((this.value == p.value) && (this.clsName.CompareTo(p.clsName)==0))
                return true;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}
