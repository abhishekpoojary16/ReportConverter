using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReportConverter.Models;
using System.Web.Mvc;

namespace ReportConverter.Models
{
    public class BigViewModel
    {
        public List<SelectListItem> PartnerList { get; set; }
        public List<SelectListItem> CountryList { get; set; }
        public List<SelectListItem> ReportTypeList { get; set; }
        public ReportHeader ReportHeader { get; set; }
        public ReportMapping ReportMapping { get; set; }
    }
}