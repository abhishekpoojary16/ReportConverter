using ReportConverter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReportConverter.Controllers
{
    public class MappingController : Controller
    {
        //
        // GET: /Mapping/
        List<string> SegmentInitiator = new List<string>();
        List<int> SegmentLocation = new List<int>();
        List<string> FieldName = new List<string>();

        public ActionResult Index()
        {
            //Clear previous session
            Session.Abandon();
            
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase postedFile, BigViewModel bigModel)
        {
            //string strDDLValue = Request.Form["Country"].ToString();

            string Separator = bigModel.ReportHeader.ElementSeparator; //report.ElementSeparator;
            string NewLineSeperator = bigModel.ReportHeader.NewlineSeparator; //report.NewlineSeparator;

            //storing in session
            Session["ElementSeparator"] = Separator;
            Session["NewlineSeparator"] = NewLineSeperator;


            //Separator input cleaning
            if (NewLineSeperator == "\\r\\n")
            {
                NewLineSeperator = "\r\n";
            }

            if (NewLineSeperator == "\\n")
            {
                NewLineSeperator = "\n";
            }

            if (postedFile != null)
            {
                string result = string.Empty;

                using (BinaryReader b = new BinaryReader(postedFile.InputStream))
                {
                    byte[] binData = b.ReadBytes(postedFile.ContentLength);
                    result = System.Text.Encoding.UTF8.GetString(binData);
                }

                string[] resultArray = result.Split(new string[] { NewLineSeperator }, StringSplitOptions.None).Take(50).ToArray();



                string[][] LabelMatrix = CreateMatrix(resultArray, Separator);

                LabelMatrix = CreateMatrix(resultArray, Separator);

                Session["LabelMatrix"] = LabelMatrix;

                ViewBag.LabelMatrix = LabelMatrix;

                ViewBag.Separator = Separator;

                //ViewBag.Preview = result.Remove(1000);    //Use this variable to display a plain preview 

                //ProcessTxtPRICAT(result, report);
            }
            else
            {
                ViewBag.NoFileMessage = "Please upload file.";
            }

            return View();
            
            //return RedirectToAction("Index");
        }

        public string[][] CreateMatrix(string[] Array, string elementSeparator)
        {
            string[][] jaggedArray = new string[Array.Length][];

            string[] LabelArray = new string[] { };

            for (int i = 0; i < Array.Length; i++)
            {
                LabelArray = Array[i].Split(new string[] { elementSeparator }, StringSplitOptions.None);
                jaggedArray[i] = LabelArray;
            }

            return jaggedArray;
        }

        public ActionResult Save_Mapping(BigViewModel bigModel)
        {
            ReportHeader header = new ReportHeader();
            header.ElementSeparator = (string)Session["ElementSeparator"];
            header.NewlineSeparator = (string)Session["NewlineSeparator"];

            header.Country = bigModel.ReportHeader.Country;
            header.PartnerName = bigModel.ReportHeader.PartnerName;
            header.ReportType = bigModel.ReportHeader.ReportType;


            //Load from Session
            if (Session["SegmentInitiator"] != null && Session["SegmentLocation"] != null && Session["FieldName"] != null)
            {
                SegmentInitiator = (List<string>)Session["SegmentInitiator"];
                SegmentLocation = (List<int>)Session["SegmentLocation"];
                FieldName = (List<string>)Session["FieldName"];
            }

            using (EDI_ReportConverterEntities entity = new EDI_ReportConverterEntities())
            {         
                entity.ReportHeaders.Add(header);
                entity.SaveChanges();
                int id = header.Id;

                for (int i = 0; i < SegmentInitiator.Count; i++)
                {
                    ReportMapping mapping = new ReportMapping();
                    mapping.ReportHeader_Id = id;
                    mapping.SegmentInitiator = SegmentInitiator[i];
                    mapping.SegmentLocation = SegmentLocation[i];
                    mapping.FieldName = FieldName[i];

                    entity.ReportMappings.Add(mapping);
                    entity.SaveChanges();
                }
            }


            //validate if ReportHeader fields present (Make all ReportHeader fields mandatory)

            TempData["Message_Save_Mapping_Confirmation"] = "Mapping saved successfully!";
            

            //return View("Index");
            return RedirectToAction("Index", "Home");
        }

        //returns first segment of the given row
        public string FetchSegment(int row_Number)
        {
            string FirstSegment = "";
            string[][] LabelMatrix = (string[][])Session["LabelMatrix"];

            FirstSegment = LabelMatrix[row_Number][0];

            return FirstSegment;
        }

        public void xEditable(string clicked_id, string clicked_parentid, string value)
        {
            //Load from Session
            if (Session["SegmentInitiator"] != null && Session["SegmentInitiator"] != null && Session["SegmentInitiator"] != null)
            {
                SegmentInitiator = (List<string>)Session["SegmentInitiator"];
                SegmentLocation = (List<int>)Session["SegmentLocation"];
                FieldName = (List<string>)Session["FieldName"];
            }

            SegmentInitiator.Add(FetchSegment(Convert.ToInt32(clicked_parentid)));
            SegmentLocation.Add(Convert.ToInt32(clicked_id));
            FieldName.Add(value);

            //Load into Session
            Session["SegmentInitiator"] = SegmentInitiator;
            Session["SegmentLocation"] = SegmentLocation;
            Session["FieldName"] = FieldName;
        }

    }
}
