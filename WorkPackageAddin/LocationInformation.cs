using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkPackageApplication
{
    public class LocationInformation
    {
        public string file_name { get; set; }
        public string model_name { get; set; }
        public long   file_position { get; set; }
        public double LOCATION_X { get; set; }
        public double LOCATION_Y { get; set; }
        public double LOCATION_Z { get; set; }
    }
}
