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
        //
        // GET: /Home/

        //Session objects


        List<string> SegmentInitiator = new List<string>();
        List<int> SegmentLocation = new List<int>();
        List<string> FieldName = new List<string>();

        //string[][] LabelMatrix = new string[50][];

        public ActionResult Index()
        {
            return View();
        }

        
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase postedFile, ReportDetails report)
        {
            //string strDDLValue = Request.Form["Country"].ToString();

            string Ctry = report.Country;
            string RptType = report.ReportType;
            char Separator = report.Separator;
            string NewLineSeperator = report.NewLineSeparator;

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
                ViewBag.Message = "Please upload file.";
            }
            
            return View();

            //return RedirectToAction("Index");
        }

        public string[][] CreateMatrix(string[] Array, char elementSeparator)
        {
            string[][] jaggedArray = new string[Array.Length][];

            string[] LabelArray = new string[] {};

            for(int i=0; i< Array.Length;i++)
            {
                LabelArray = Array[i].Split(elementSeparator);
                jaggedArray[i] = LabelArray;
            }

            return jaggedArray;
        }

        public void newMappingParameter(string RowIndex, string ColumnIndex)
        {
            int rIndex = Convert.ToInt32(RowIndex);
            int cIndex = Convert.ToInt32(ColumnIndex);


        }


        public ActionResult Save_Mapping()
        {
            //Load from Session
            if (Session["SegmentInitiator"] != null && Session["SegmentInitiator"] != null && Session["SegmentInitiator"] != null)
            {
                SegmentInitiator = (List<string>)Session["SegmentInitiator"];
                SegmentLocation = (List<int>)Session["SegmentLocation"];
                FieldName = (List<string>)Session["FieldName"];
            }

            using (EDI_ReportConverterEntities entity = new EDI_ReportConverterEntities())
            {
                //entity.ReportHeaders.Add();
            }


            //validate if ReportHeader fields present

            ViewBag.Save_Mapping_Confirmation = "Mapping saved successfully!";

            return View("Index");
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

        //public void xEditable_json()
        //{
        //    string input = clicked_id;
        //}


        public void AddParameter(string ParamValue)
        {
            MappingParameters mp = new MappingParameters();
            mp.ParamValue = ParamValue;
        }

        public void CreateNewModule(string EDIdata, ReportDetails report)
        {
            string Ctry = report.Country;
            string RptType = report.ReportType;
            char Separator = report.Separator;
            string NewLineSeperator = report.NewLineSeparator;

            if (NewLineSeperator == "\\r\\n")
            {
                NewLineSeperator = "\r\n";
            }

            if (NewLineSeperator == "\\n")
            {
                NewLineSeperator = "\n";
            }
        }


        public void GenerateCsv(DataTable dt)
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

        public void ProcessTxtPRICAT(string EDIdata, ReportDetails report)
        {
            #region Header sDeclare
            StringBuilder sb = new StringBuilder();
            string sResult = "";
            string query = "";
            string sCountry = "";
            string CatalogNumber = "";
            string MfrpnString = "";
            string DescString = "";
            DateTime Date = new DateTime();
            int lineNum = 1;
            char seperator, newlineSeperator;
            #endregion

            #region Line sDeclare
            List<String> sLineNo = new List<String>();
            List<String> IMSku = new List<String>();
            List<String> sMaterialDescription = new List<String>();
            List<String> sQuantity = new List<String>();
            List<DateTime> DateList = new List<DateTime>();
            List<String> MFRPN = new List<String>();
            List<String> CatalogNumberList = new List<String>();
            List<String> ContractPrice = new List<String>();
            List<String> MFR_RetailPrice = new List<String>();

            #endregion

            try
            {
                string Ctry = report.Country;
                string RptType = report.ReportType;
                char Separator = report.Separator;
                string NewLineSeperator = report.NewLineSeparator;

                if (NewLineSeperator == "\\r\\n")
                {
                    NewLineSeperator = "\r\n";
                }

                if (NewLineSeperator == "\\n")
                {
                    NewLineSeperator = "\n";
                }
                

                string[] LineArray = new string[] { };
                string[] LineDetails = new string[] { };
                string[] DetailArray = new String[] { };

                sResult = EDIdata;


                LineArray = sResult.Split(new string[] { NewLineSeperator }, StringSplitOptions.None);

                for (int i = 0; i < LineArray.Length; i++)
                {
                    //Refresh variables
                    DescString = "";

                    if (LineArray[i].Contains("BCT" + Separator))
                    {
                        DetailArray = LineArray[i].Split(Separator);
                        DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        CatalogNumber = DetailArray[3];
                        string temp = DetailArray[2].Insert(4, "/").Insert(7, "/");
                        Date = Convert.ToDateTime(DetailArray[2].Insert(4, "/").Insert(7, "/"));
                    }

                    else if (LineArray[i].Contains("LIN" + Separator))
                    {
                        lineNum++;
                        CatalogNumberList.Add(CatalogNumber);
                        DateList.Add(Date);
                        DetailArray = LineArray[i].Split(Separator);
                        DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        sLineNo.Add(DetailArray[1]);
                        IMSku.Add(DetailArray[3]);
                        MFRPN.Add(DetailArray[5]);
                        MfrpnString = DetailArray[5];

                        if (LineArray[i + 3].Contains("PID" + Separator))
                        {
                            DetailArray = LineArray[i + 3].Split(Separator);
                            DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            DescString = DetailArray[2].Replace(MfrpnString, "").Replace(",", "");

                            //Multi-line Description check
                            if (LineArray[i + 4].Contains("PID" + Separator))
                            {
                                DetailArray = LineArray[i + 4].Split(Separator);
                                DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                DescString += " " + DetailArray[2].Replace(",", "");

                                DetailArray = LineArray[i + 6].Split(Separator);
                                DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                ContractPrice.Add(DetailArray[2]);
                                //check for mfr Reatil price
                                if (LineArray[i + 7].Contains("CTP" + Separator + Separator +"MSR"))
                                {
                                    DetailArray = LineArray[i + 7].Split(Separator);
                                    DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                    MFR_RetailPrice.Add(DetailArray[2]);
                                }
                                else
                                {
                                    MFR_RetailPrice.Add("");
                                }
                            }
                            else
                            {
                                DetailArray = LineArray[i + 5].Split(Separator);
                                DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                ContractPrice.Add(DetailArray[2]);
                                //check for mfr Reatil price
                                if (LineArray[i + 6].Contains("CTP" + Separator + Separator + "MSR"))
                                {
                                    DetailArray = LineArray[i + 6].Split(Separator);
                                    DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                    MFR_RetailPrice.Add(DetailArray[2]);
                                }
                                else
                                {
                                    MFR_RetailPrice.Add("");
                                }
                            }



                            sMaterialDescription.Add(DescString);

                        }
                        else
                        {
                            DetailArray = LineArray[i + 4].Split(Separator);
                            DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            DescString = DetailArray[2].Replace(MfrpnString, "").Replace(",", "");

                            //Multi-line Description check
                            if (LineArray[i + 5].Contains("PID" + Separator))
                            {
                                DetailArray = LineArray[i + 5].Split(Separator);
                                DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                DescString += " " + DetailArray[2].Replace(",", "");

                                DetailArray = LineArray[i + 7].Split(Separator);
                                DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                ContractPrice.Add(DetailArray[2]);
                                //check for mfr Reatil price
                                if (LineArray[i + 8].Contains("CTP" + Separator + Separator + "MSR"))
                                {
                                    DetailArray = LineArray[i + 8].Split(Separator);
                                    DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                    MFR_RetailPrice.Add(DetailArray[2]);
                                }
                                else
                                {
                                    MFR_RetailPrice.Add("");
                                }
                            }

                            else
                            {
                                DetailArray = LineArray[i + 6].Split(Separator);
                                DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                ContractPrice.Add(DetailArray[2]);
                                //check for mfr Reatil price
                                if (LineArray[i + 7].Contains("CTP" + Separator + Separator + "MSR"))
                                {
                                    DetailArray = LineArray[i + 7].Split(Separator);
                                    DetailArray = DetailArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                    MFR_RetailPrice.Add(DetailArray[2]);
                                }
                                else
                                {
                                    MFR_RetailPrice.Add("");
                                }
                            }


                            sMaterialDescription.Add(DescString);
                        }
                    }
                }

                DataTable dt = new DataTable();
                //Define columns
                dt.Columns.Add("Catalog_Number");
                dt.Columns.Add("Row_Number");
                dt.Columns.Add("Imsku");
                dt.Columns.Add("Mfrpn");
                dt.Columns.Add("MaterialDescription");
                dt.Columns.Add("ContractPrice");
                dt.Columns.Add("MFR_RetailPrice");
                dt.Columns.Add("Date");
                for (int i = 0; i < sLineNo.Count; i++)
                {
                    DataRow row = dt.NewRow();
                    row[0] = CatalogNumberList[i];
                    row[1] = sLineNo[i];
                    row[2] = IMSku[i];
                    row[3] = MFRPN[i];
                    row[4] = sMaterialDescription[i].Replace("'", "");
                    row[5] = ContractPrice[i];
                    row[6] = MFR_RetailPrice[i];
                    row[7] = DateList[i];
                    dt.Rows.Add(row);
                }

                GenerateCsv(dt);
            }

            catch (Exception exc)
            {
                throw exc;
            }
        }

    }
}
