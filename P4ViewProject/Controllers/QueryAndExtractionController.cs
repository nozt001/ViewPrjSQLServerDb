﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using P4ViewProject.Models;
using System.Data;
using System.Text;
using System.IO;
using P4ViewProject.DAL;
using System.Web.Script.Serialization;
using System.Data.OracleClient;

namespace P4ViewProject.Controllers
{
    //[Authorize]
    public class QueryAndExtractionController : Controller
    {

        private SQLServerConnClass sqlConn = new SQLServerConnClass();
        public static ViewExtractedDataWrapperModel data = new ViewExtractedDataWrapperModel();
        DataExtractionContext context = new DataExtractionContext();
        public static string select, from, where, otherClauses, queryDate;

        // Returns the query and extraction view
        [HttpGet]
        public ActionResult Index()
        {

            // this is oracle connection 
            // 
            var constr = System.Configuration.ConfigurationManager.ConnectionStrings["HR"];
            string connectionStr = constr.ConnectionString;

            OracleConnection OraConn = new OracleConnection(connectionStr);
            OraConn.Open();

            Tuple<string, Dictionary<string, List<string>>> dbNameAndTableData = sqlConn.getTablesWithDatabaseName();

            Dictionary<string, List<string>> tableDict = dbNameAndTableData.Item2;
            ViewBag.DatabaseName = dbNameAndTableData.Item1;
            ViewBag.TableDict = tableDict;

            // close the connection with oracle 
            OraConn.Close();
            OraConn.Dispose();

            return View();
        }

        // Returns analyse results view
        [HttpGet]
        public ActionResult AnalyseResults() {

            if (data.Data == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                Dictionary<string, List<string>> resultDict = new Dictionary<string, List<string>>();

                List<string> listOfAttributes = new List<string>();

                foreach (DataColumn col in data.Data.Columns) {
                    listOfAttributes.Add(col.ColumnName);
                }

                resultDict.Add("Result_Table", listOfAttributes);
                ViewBag.resultTableDict = resultDict;

                return View();
            }
        }

        [HttpPost]
        public PartialViewResult ViewData(string selectText, string fromText, string whereText, string otherClauseText, string startDate) {

            if (startDate.Equals("")) {
                startDate = DateTime.Now.ToString("M/d/yyyy");
            }

            string query = "Select " + selectText;
            query += " From " + fromText;
            query += " Where " + whereText + " ";

            List<string> fromTables = fromText.Split(',').ToList();
            bool whereTextExists = (whereText.Equals("")?false:true);
            bool flagWhereText = false;

            foreach (String tableName in fromTables) {
                if (whereTextExists || flagWhereText)
                {
                    query += "AND ";
                }
                query += getTableTemporalString(tableName.Trim(), startDate);
                flagWhereText = true;
            }

            query += " " + otherClauseText;
            query += ";";

            sqlConn.retrieveData(query);
            data.Data = sqlConn.SqlTable;

            select = selectText;
            from = fromText;
            where = whereText;
            queryDate = startDate;
            otherClauses = otherClauseText;

            return PartialView("ViewDataTable", data);
        }

        [HttpPost]
        public string CsvData(string csvData, string title, string description) {

            title = title.Trim();

            string path = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = "myCsv.csv";

            StringBuilder sb = new StringBuilder();
            sb.Append(csvData);
            using (StreamWriter outfile = new StreamWriter(path+ @"\myCsv.csv"))
            {
                outfile.Write(sb.ToString());
            }

            ExtractionData extraction = new ExtractionData();
            extraction.Name = title;
            extraction.Description = description;
            extraction.SelectBox = select;
            extraction.FromBox = from;
            extraction.WhereBox = where;
            extraction.QueryDate = queryDate;
            extraction.OtherClausesBox = otherClauses;

            context.DataExtracts.Add(extraction);
            context.SaveChanges();

            return path+fileName;
        }

        private String getTableTemporalString(string tableName, string startDate) {
            string tableTempString = "((" + tableName + ".Transaction_Date_Start <= '" + startDate + "') AND ((" +
                                      tableName + ".Transaction_Date_End IS NULL) OR (" + tableName + ".Transaction_Date_End > '" +
                                      startDate + "'))) ";
            return tableTempString;
        }

        [HttpGet]
        public int getRows() {
            try
            {
                return data.Data.Rows.Count;
            }
            catch (Exception e) {
                return 0;
            }
        }

        [HttpPost]
        public string RequestData(string colName) {

            try
            {
                List<string> resultColumn = new List<string>();
                string result = "";

                foreach (DataRow row in data.Data.Rows) {
                    resultColumn.Add(row[colName].ToString());
                }

                result = String.Join(",", resultColumn.ToArray());
                return result;
            }
            catch {
                return "";
            }
        }

        [HttpGet]
        public PartialViewResult Extractions() {
            List<ExtractionData> extractionsList = context.DataExtracts.ToList();    

            return PartialView("Extractions", extractionsList);
        }

        [HttpPost]
        public string GetExtractionInfo(string extractionName) {

            var extraction = context.DataExtracts.Where(b => b.Name == extractionName).FirstOrDefault();
            
            ExtractionData extractionInfo = (ExtractionData)extraction;

            return new JavaScriptSerializer().Serialize(extractionInfo);
        }


        [HttpGet]
        public PartialViewResult getResultsTable() {
            
            if (data.Data != null){
                return (PartialView("ViewDataTable", data));
            }

            return null;
        }

    }
}