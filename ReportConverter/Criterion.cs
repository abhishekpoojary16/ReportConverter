//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReportConverter
{
    using System;
    using System.Collections.Generic;
    
    public partial class Criterion
    {
        public int Id { get; set; }
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FTPDirectory { get; set; }
        public string LocalDirectory { get; set; }
        public string Filename { get; set; }
        public string SeeburgerAgreement { get; set; }
        public Nullable<int> Scheduler_Id { get; set; }
        public Nullable<int> ReportHeader_Id { get; set; }
    }
}
