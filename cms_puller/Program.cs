using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;

using System.Data;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Configuration;
//using Newtonsoft.Json;
//using System.IO;
//using System.Data.SqlClient;


namespace cms_puller
{
    class Program {
        static void Main(string[] args) 
        {
            clsDB dbObj = new clsDB();
            DataSet ds = dbObj.ExecuteSelect("GET_NEW_TRIPS_FOR_CRS_INFO_PULL", CommandType.StoredProcedure, 160);
            
            int cnt = 0;
            string[] arrTrip_info = new string[ds.Tables[0].Rows.Count];
            int[] arrTrip_ids = new int[ds.Tables[0].Rows.Count];            
            int intTrip_id;
            foreach (DataRow dataRow in ds.Tables[0].Rows)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string webUrl = ConfigurationSettings.AppSettings["WebserviceURL"].ToString();
                        intTrip_id = Convert.ToInt32(dataRow["TRIP_ID"]);
                        webUrl += "&KeyCode=''&intTripID=" + intTrip_id.ToString();
                        webUrl += "&dtChartDate=" + ((DateTime)dataRow["CHART_DATE"]).ToString("yyyy-MMM-dd").ToString();
                        byte[] obj = client.DownloadData(webUrl);
                        string json = Encoding.ASCII.GetString(obj);
                        arrTrip_info[cnt] = json;
                        arrTrip_ids[cnt] = intTrip_id;
                    }
                }
                catch (System.Exception es){}
                cnt++;
            }

            int maxRow = 100;
            DataTable dt = new DataTable();
            //dt.TableName = "TRIP_INFO";
            dt.Columns.Add("TRIP_ID", typeof(int));
            dt.Columns.Add("ROUTE_INFO", typeof(string));
            dt.Columns.Add("DEPARTURE_TIME", typeof(DateTime));
            for (int i = 0; i < arrTrip_info.Length; i++)
            {
                if (arrTrip_info[i].Length > 0)
                {
                    string ctName = "";
                    string deprTime = "";
                    try
                    {
                        JObject tripInfo = JObject.Parse(arrTrip_info[i]);
                        var cts = tripInfo["APITripsSummaryResult"].Children();
                        foreach (var ct in cts)
                        {
                            ctName += ct["CityName"].ToString().Trim('"') + "-";
                            if (ct["Position"].ToString().Trim('"') == "1")
                                deprTime = ct["DepartureTime"].ToString().Trim('"');
                        }
                    }
                    catch (System.Exception ex){}
                    
                    DataRow dr = dt.NewRow();
                    dr["TRIP_ID"] = arrTrip_ids[i];
                    dr["ROUTE_INFO"] = ctName.Remove(ctName.Length - 1);
                    dr["DEPARTURE_TIME"] = Convert.ToDateTime(deprTime);
                    dt.Rows.Add(dr);

                    if (dt.Rows.Count == maxRow)
                    {
                        try
                        {
                            dbObj = null;
                            dbObj = new clsDB();
                            dbObj.AddParameter("TRIP_INFO", dt);
                            dbObj.ExecuteDML("UPDATE_TRIP_INFO", CommandType.StoredProcedure, 180);
                            dt.Rows.Clear();
                        }
                        catch (System.Exception ex){}
                    }
                }
            }
            
            if (dt.Rows.Count > 0 && dt.Rows.Count < maxRow)
            {
                try
                {
                    dbObj = null;
                    dbObj = new clsDB();
                    dbObj.AddParameter("TRIP_INFO", dt);
                    dbObj.ExecuteDML("UPDATE_TRIP_INFO", CommandType.StoredProcedure, 180);
                    dt.Rows.Clear();
                }
                catch (System.Exception ex) { }
            }
        }
    }

}
