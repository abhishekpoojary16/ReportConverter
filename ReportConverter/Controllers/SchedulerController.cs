using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReportConverter.Controllers
{
    public class SchedulerController : Controller
    {
        //
        // GET: /Scheduler/
        
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult ProcessForm(string timeString, string retryCounter)
        //{
        //    int CriteriaID, schedulerID;
        //    CriteriaID = (int)Session["CriteriaID"];
        //    using (EDI_ReportConverterEntities entity = new EDI_ReportConverterEntities())
        //    {
        //        Scheduler schedule = new Scheduler();
        //        schedule.Time = timeString;
        //        schedule.Retry_Count = Convert.ToInt32(retryCounter);
        //        entity.Schedulers.Add(schedule);
        //        entity.SaveChanges();
        //        schedulerID = schedule.Id;

        //        Criterion updatedCriteria = (from c in entity.Criteria
        //                                    where c.Id == CriteriaID
        //                                    select c).FirstOrDefault();

        //        updatedCriteria.Scheduler_Id = schedulerID;
        //        entity.SaveChanges();
                
        //    }
        //    Session["schedulerID"] = schedulerID;
        //    //return RedirectToAction("Index","Forwarding");
        //    return Json(Url.Action("Index", "Forwarding"));
        //}

        public ActionResult ProcessForm(Scheduler schedule)
        {
            int CriteriaID, schedulerID;
            CriteriaID = (int)Session["CriteriaID"];
            using (EDI_ReportConverterEntities entity = new EDI_ReportConverterEntities())
            {
                entity.Schedulers.Add(schedule);
                entity.SaveChanges();
                schedulerID = schedule.Id;

                Criterion updatedCriteria = (from c in entity.Criteria
                                             where c.Id == CriteriaID
                                             select c).FirstOrDefault();

                updatedCriteria.Scheduler_Id = schedulerID;
                entity.SaveChanges();

            }
            Session["schedulerID"] = schedulerID;
            return RedirectToAction("Index","Forwarding");
        }

    }
}
