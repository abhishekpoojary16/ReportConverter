using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportConverter.Models
{
    public class MappingParameters
    {
        public string ColumnName { get; set; }
        public string ParamValue { get; set; }
        public string SegmentName { get; set;}
        public string ArrayPosition { get; set; }
    }
}