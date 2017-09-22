using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReportConverter.Controllers
{
    public class ForwardingController : Controller
    {
        //
        // GET: /Forwarding/
        
        public ActionResult Index()
        {    
            return View();
        }

        public ActionResult ProcessForm(Forwarding forwarding)
        {
            int schedulerID, forwardingID;
            using (EDI_ReportConverterEntities entity = new EDI_ReportConverterEntities())
            {
                schedulerID = (int)Session["schedulerID"];
                entity.Forwardings.Add(forwarding);
                entity.SaveChanges();
                forwardingID = forwarding.Id;

                Scheduler updatedScheduler = (from c in entity.Schedulers
                                              where c.Id == schedulerID
                                             select c).FirstOrDefault();

                updatedScheduler.Forwarding_Id = forwardingID;
                entity.SaveChanges();

            }

            return RedirectToAction("Index", "Landing");
        }

    }
}
