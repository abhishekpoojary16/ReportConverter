using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReportConverter.Models
{
    public class ReportDetails
    {
        public string Partner { get; set; }
        public string Country { get; set; }
        public string ReportType { get; set; }
        public char Separator { get; set; }
        public string NewLineSeparator { get; set; }
    }
}