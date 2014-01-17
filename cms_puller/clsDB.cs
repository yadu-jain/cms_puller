using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
//using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.HtmlControls;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Data.SqlClient;

/// <summary>
/// Summary description for clsDB
/// </summary>
public class clsDB
{
    SqlCommand cmd;
    string strConnString;

    public clsDB()
    {
        strConnString = ConfigurationSettings.AppSettings["ConnStr"].ToString();
        StartNewCommand();
    }

    public void StartNewCommand()
    {
        cmd = new SqlCommand();
    }

    public void AddParameter(string strParamName, string strParamValue, int intParamSize)
    {
        cmd.Parameters.Add("@" + strParamName, SqlDbType.VarChar, intParamSize).Value = strParamValue;
    }
    public void AddParameter(string strParamName, int intParamValue)
    {
        cmd.Parameters.Add("@" + strParamName, SqlDbType.Int).Value = intParamValue;
    }
    public void AddParameter(string strParamName, bool blnParamValue)
    {
        cmd.Parameters.Add("@" + strParamName, SqlDbType.Bit).Value = blnParamValue;
    }
    public void AddParameter(string strParamName, DateTime dtParamValue)
    {
        cmd.Parameters.Add("@" + strParamName, SqlDbType.DateTime).Value = dtParamValue;
    }
    public void AddParameter(string strParamName, decimal dclParamValue)
    {
        cmd.Parameters.Add("@" + strParamName, SqlDbType.Decimal).Value = dclParamValue;
    }
    public void AddParameter(string strParamName, double dblParamValue)
    {
        cmd.Parameters.Add("@" + strParamName, SqlDbType.Decimal).Value = dblParamValue;
    }
    public void AddParameter(string strParamName, DataTable tablePara)
    {
        cmd.Parameters.Add("@" + strParamName, SqlDbType.Structured).Value = tablePara;
    }
    public DataTable FilterDataTable(DataTable dt, string strFilterQuery, string strSortBy)
    {
        DataTable dtTemp;
        DataRow[] dr;
        dr = dt.Select(strFilterQuery, strSortBy);
        dtTemp = dt.Clone();

        foreach (DataRow r in dr)
        {
            dtTemp.ImportRow(r);
        }
        return dtTemp;
    }

    public int ExecuteDML(string strSQL, CommandType cmdType, int intTimeout)
    {
        int status = 0;
        SqlConnection conn = new SqlConnection(strConnString);

        try
        {
            cmd.CommandText = strSQL;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = intTimeout;
            conn.Open();
            cmd.Connection = conn;
            status = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
        }
        catch (System.Exception ex)
        {
            status = -1;
        }
        finally
        {
            cmd.Cancel();
            conn.Close();
            conn.Dispose();
        }
        return status;
    }

    public DataSet ExecuteSelect(string strSQL, CommandType cmdType, int intTimeout)
    {
        DataSet ds = new DataSet();
        SqlDataAdapter adp = new SqlDataAdapter();
        SqlConnection conn = new SqlConnection(strConnString);
        try
        {
            cmd.CommandText = strSQL;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = intTimeout;
            adp.SelectCommand = cmd;
            conn.Open();
            cmd.Connection = conn;
            adp.Fill(ds);
            cmd.Parameters.Clear();
        }
        catch (System.Exception e)
        {
            ds = null;
            //prateek- line below should be uncommented.
            //throw e;
        }
        finally
        {
            adp.Dispose();
            cmd.Cancel();
            conn.Close();
            conn.Dispose();
        }
        return ds;
    }
}