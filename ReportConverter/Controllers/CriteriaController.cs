using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReportConverter.Controllers
{
    public class CriteriaController : Controller
    {
        //
        // GET: /Criteria/
   
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProcessForm(Criterion criteria)
        {
            int ReportheaderID, CriteriaID;
            using (EDI_ReportConverterEntities entity = new EDI_ReportConverterEntities())
            {
                ReportheaderID = (int)Session["ReportheaderID"];
                criteria.ReportHeader_Id = ReportheaderID;
                entity.Criteria.Add(criteria);
                entity.SaveChanges();
                CriteriaID = criteria.Id;
            }

            Session["CriteriaID"] = CriteriaID;
            return RedirectToAction("Index", "Scheduler");
        }

    }
}
