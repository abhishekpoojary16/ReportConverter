using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReportConverter.Models;
using System.Data;
using System.Text;

namespace ReportConverter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            BigViewModel bigModel = new BigViewModel();
            bigModel.PartnerList = PopulateList("Partner");
            bigModel.CountryList = PopulateList("Country");
            bigModel.ReportTypeList = PopulateList("ReportType");


            return View(bigModel);
        }

        private static List<SelectListItem> PopulateList(string attribute)
        {
            EDI_ReportConverterEntities entity = new EDI_ReportConverterEntities();

            List<String> DropdownList = new List<String>();

            if (attribute == "Partner")
            {
                DropdownList = entity.ReportHeaders.Select(c => c.PartnerName).ToList();
            }
            else if (attribute == "Country")
            {
                DropdownList = entity.ReportHeaders.Select(c => c.Country).ToList();
            }
            else if (attribute == "ReportType")
            {
               DropdownList = entity.ReportHeaders.Select(c => c.ReportType).ToList();
            }

            //Removing null values from list

            DropdownList.RemoveAll(c => c == null);

            List<SelectListItem> item = DropdownList.ConvertAll(a =>
            {
                return new SelectListItem()
                {
                    Text = a.ToString(),
                    Value = a.ToString(),
                    Selected = false
                };
            });
            return item;
        }


        [HttpPost]
        public ActionResult Index(HttpPostedFileBase postedFile, BigViewModel report)
        {
            string P = report.ReportHeader.PartnerName;
            string C = report.ReportHeader.Country;
            string R = report.ReportHeader.ReportType;

            //Fetch Separator and NewLineSeparator based on P,C,R
            EDI_ReportConverterEntities entity = new EDI_ReportConverterEntities();

            ReportHeader reportHeader = entity.ReportHeaders.Where(x => x.PartnerName == P)
                .Where(x => x.Country == C)
                .Where(x => x.ReportType == R)
                .Single();

            string Separator = reportHeader.ElementSeparator;
            string NewLineSeperator = reportHeader.NewlineSeparator;
            int header_id = reportHeader.Id;

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

                
                //Get mapping rows from dbo.ReportMapping

                var mappingRows = entity.ReportMappings.Where(x => x.ReportHeader_Id == header_id);

                List<string> SegmentInitiator = new List<string>();
                List<int> SegmentLocation = new List<int>();
                List<string> FieldName = new List<string>();


                foreach (ReportMapping row in mappingRows)
                {
                    SegmentInitiator.Add(row.SegmentInitiator);
                    SegmentLocation.Add((int)row.SegmentLocation);
                    FieldName.Add(row.FieldName);
                }

                string[] resultArray = result.Split(new string[] { NewLineSeperator }, StringSplitOptions.None);

                Dictionary<string, List<string>> ListDict = Create_ListDictionary(resultArray, Separator, SegmentInitiator, SegmentLocation, FieldName);

                DataTable dt = Create_Datatable(ListDict);

                Create_CSV(dt);
            }
            else
            {
                
                TempData["Message_NoFile"] = "Please upload file";
            }

            //return View();

            return RedirectToAction("Index");
        }


        public Dictionary<string, List<string>> Create_ListDictionary(string[] resultArray, string Separator, List<string> SegmentInitiator_List, List<int> SegmentLocation_List, List<string> FieldName_List)
        {
            //maintain lists in dictionary
            Dictionary<string, List<string>> ListDict = new Dictionary<string, List<string>>();

            foreach (string field in FieldName_List)
            {
                ListDict.Add(field, new List<string>());
            }

            for (int i = 0; i < resultArray.Length; i++)
            {
                string SegmentInitiator = resultArray[i].Split(new string[] { Separator }, StringSplitOptions.None)[0];
                bool b = SegmentInitiator_List.Any(SegmentInitiator.StartsWith);

                if (b == true)
                {
                    //int index = SegmentInitiator_List.IndexOf(SegmentInitiator);

                    var result = Enumerable.Range(0, SegmentInitiator_List.Count)
             .Where(x => SegmentInitiator_List[x] == SegmentInitiator)
             .ToList();

                    foreach (int index in result)
                    {
                        int SegmentLocation = SegmentLocation_List[index];

                        string FieldName = FieldName_List[index];

                        ListDict[FieldName].Add(resultArray[i].Split(new string[] { Separator }, StringSplitOptions.None)[SegmentLocation]);
                    }
                }
            }

            return ListDict;
        }

        public DataTable Create_Datatable(Dictionary<string, List<string>> ListDict)
        {
            DataTable dt = new DataTable();

            foreach (string columnName in ListDict.Keys)
            {
                dt.Columns.Add(columnName);
            }

            for (int i = 0; i < ListDict[dt.Columns[0].ColumnName].Count; i++)
            {
                DataRow row = dt.NewRow();
             
                int j=0;
                foreach (List<string> field in ListDict.Values)
                {
                    try
                    {
                        row[j] = field[i];
                        j++;
                    }
                    catch
                    {
                        row[j] = "";
                        j++;
                    }
                }
                dt.Rows.Add(row);
            }

            return dt;
        }

        public void Create_CSV(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            string[] columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName).
                                              ToArray();
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                string[] fields = row.ItemArray.Select(field => field.ToString()).
                                                ToArray();
                sb.AppendLine(string.Join(",", fields));
            }

            string strResult = "";

            //StreamReader sr = new StreamReader(rsp.GetResponseStream());
            strResult = sb.ToString();
            //    sr.ReadToEnd().Trim();
            //sr.Close();

            Response.Clear();
            Response.ClearHeaders();

            Response.AddHeader("Content-Length", strResult.Length.ToString());
            Response.ContentType = "text/plain";
            Response.AppendHeader("content-disposition", "attachment;filename=\"" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff").Replace(":", "").Replace(".", "").Replace(" ", "").Trim() + ".csv" + "\"");

            Response.Write(strResult);
            Response.End();

            //File.WriteAllText(CsvPath + @"\" + filename.Replace(".txt", ".csv"), sb.ToString());

        }

        

    }
}
