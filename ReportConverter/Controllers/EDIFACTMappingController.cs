using ReportConverter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReportConverter.Controllers
{
    public class EDIFACTMappingController : Controller
    {
        //
        // GET: /EDIFACTMapping/

        List<string> SegmentInitiator = new List<string>();
        List<int> SegmentLocation = new List<int>();
        List<string> FieldName = new List<string>();
        List<string> SegmentIdentifier_2 = new List<string>();

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
            string SubElementSeparator = bigModel.ReportHeader.SubElementSeparator;


            //storing in session
            Session["ElementSeparator"] = Separator;
            Session["NewlineSeparator"] = NewLineSeperator;
            Session["SubElementSeparator"] = SubElementSeparator;

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



                string[][] LabelMatrix = CreateMatrix(resultArray, Separator, SubElementSeparator);

                //LabelMatrix = CreateMatrix(resultArray, Separator); //unwanted duplicate

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

        public string[][] CreateMatrix(string[] Array, string elementSeparator, string subelementSeparator)
        {
            string[][] jaggedArray = new string[Array.Length][];

            string[] LabelArray = new string[] { };

            string[] subArray = new string[] { };

            List<string> superString = new List<string> { };

            for (int i = 0; i < Array.Length; i++)
            {
                superString.Clear();
                LabelArray = Array[i].Split(new string[] { elementSeparator }, StringSplitOptions.None);
                foreach (string element_1 in LabelArray)
                {
                    //split and merge based on subelement separator
                    subArray = element_1.Split(new string[] { subelementSeparator }, StringSplitOptions.None);

                    foreach (string element_2 in subArray)
                    {
                        superString.Add(element_2);
                    }
                }
                jaggedArray[i] = superString.ToArray();
            }

            return jaggedArray;
        }

        public ActionResult Save_Mapping(BigViewModel bigModel)
        {
            ReportHeader header = new ReportHeader();
            header.ElementSeparator = (string)Session["ElementSeparator"];
            header.NewlineSeparator = (string)Session["NewlineSeparator"];
            header.SubElementSeparator = (string)Session["SubElementSeparator"];

            header.Country = bigModel.ReportHeader.Country;
            header.PartnerName = bigModel.ReportHeader.PartnerName;
            header.ReportType = bigModel.ReportHeader.ReportType;


            //Load from Session
            if (Session["SegmentInitiator"] != null && Session["SegmentLocation"] != null && Session["FieldName"] != null)
            {
                SegmentInitiator = (List<string>)Session["SegmentInitiator"];
                SegmentLocation = (List<int>)Session["SegmentLocation"];
                FieldName = (List<string>)Session["FieldName"];
                SegmentIdentifier_2 = (List<string>)Session["SegmentIdentifier_2"];
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
                    mapping.SegmentIdentifier_2 = SegmentIdentifier_2[i];

                    entity.ReportMappings.Add(mapping);
                    entity.SaveChanges();
                }
            }


            //validate if ReportHeader fields present (Make all ReportHeader fields mandatory)

            TempData["Message_Save_Mapping_Confirmation"] = "Mapping saved successfully!";


            //return View("Index");
            return RedirectToAction("Index", "EDIFACT");
        }

        //returns corresponding Identifier of the given row
        public string FetchSegment(int row_Number, int Segment_Number)
        {
            string Segment = "";
            string[][] LabelMatrix = (string[][])Session["LabelMatrix"];

            if (Segment_Number == 0)
            {
                Segment = LabelMatrix[row_Number][0];
            }
            else if (Segment_Number == 1)
            {
                Segment = LabelMatrix[row_Number][1];
            }
            return Segment;
        }

        public void xEditable(string clicked_id, string clicked_parentid, string value)
        {
            //Load from Session
            if (Session["SegmentInitiator"] != null && Session["SegmentInitiator"] != null && Session["SegmentInitiator"] != null)
            {
                SegmentInitiator = (List<string>)Session["SegmentInitiator"];
                SegmentLocation = (List<int>)Session["SegmentLocation"];
                FieldName = (List<string>)Session["FieldName"];
                SegmentIdentifier_2 = (List<string>)Session["SegmentIdentifier_2"];
            }

            SegmentInitiator.Add(FetchSegment(Convert.ToInt32(clicked_parentid),0));
            SegmentLocation.Add(Convert.ToInt32(clicked_id));
            FieldName.Add(value);
            
            //No Second Identifier if segment is present in 0th or 1st position
            if (Convert.ToInt32(clicked_id) < 2)
            {
                SegmentIdentifier_2.Add(null);
            }
            else
            {
                SegmentIdentifier_2.Add(FetchSegment(Convert.ToInt32(clicked_parentid), 1));
            }

            //Load into Session
            Session["SegmentInitiator"] = SegmentInitiator;
            Session["SegmentLocation"] = SegmentLocation;
            Session["FieldName"] = FieldName;
            Session["SegmentIdentifier_2"] = SegmentIdentifier_2;
        }
    }
}
