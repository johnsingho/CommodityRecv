using Common;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SyncData.Common;

namespace SyncData
{
    public class CommodityRecv_DAL
    {
        private string connectionString_Baan;// = ConfigurationManager.ConnectionStrings["baan_ConnStr"].ConnectionString;
        private string connectionString_Main;// = ConfigurationManager.ConnectionStrings["main_ConnStr"].ConnectionString;

        //暂存baan数据的表
        private static readonly string TBL_BAAN = "tbl_cr_Baan";
        private static readonly string TBL_UPLOAD = "tbl_cr_Condition";

        public CommodityRecv_DAL(string sconnBann, string sconnMain)
        {
            connectionString_Main = sconnMain;
            connectionString_Baan = sconnBann;
        }

        private StringBuilder MakeSiteQuery(string sSite)
        {
            var sql = new StringBuilder();
            //在 Jinxing Kuang 的语句基础上添加了id,CompCode
            /* 2018-05-03
            sql.AppendFormat(@"
                    SELECT
                    0 as id,
                    PH.ORNO PONUMBER,
                    PH.OTBP SUPPLIERNBR,
                    PH.COTP POTYPE,
                    (SELECT TCCOM001.NAMA FROM BO_READ{0}.TCCOM001 WHERE PH.CCON=TCCOM001.EMNO) BUYER,
                    PH.REFA REFERENCEA,
                    PH.REFB REFERENCEB,
                    TRIM(PL.ITEM) ITEM,
                    (SELECT TCIBD001.DSCA FROM BO_READ{0}.TCIBD001 WHERE PL.ITEM=TCIBD001.ITEM) ITEMDSE,
                    PL.OQUA QTY,
                    PL.CUQP UNIT,
                    TO_CHAR(PL.DDTE,'YYYY-MM-dd HH24:MI:SS') RECEIVEDATE,
                    '{0}' as CompCode 
                    FROM
                    BO_READ{0}.TDPUR400 PH,
                    BO_READ{0}.TDPUR401 PL
                    WHERE
                    PH.ORNO=PL.ORNO
                    AND PH.OTBP=PL.OTBP
                    AND 
                    (PL.DDTE_LOC>=TO_DATE(TO_CHAR(SYSDATE+(16/24),'yyyy-MM-dd'),'yyyy-MM-dd') 
                    AND PL.DDTE_LOC<TO_DATE(TO_CHAR(SYSDATE+(16/24)+1,'yyyy-MM-dd'),'yyyy-MM-dd'))
                    AND PH.ORNO LIKE '{0}I%'
                    ",
                    sSite);
            */
            sql.AppendFormat(@"
                            SELECT
                            0 as id,
                            PH.ORNO PONUMBER,
                            PH.OTBP SUPPLIERNBR,
                            PH.COTP POTYPE,
                            (SELECT TCCOM001.NAMA FROM BO_READ{0}.TCCOM001 WHERE PH.CCON=TCCOM001.EMNO) BUYER,
                            PH.REFA REFERENCEA,
                            PH.REFB REFERENCEB,
                            TRIM(PL.ITEM) ITEM,
                            (SELECT TCIBD001.DSCA FROM BO_READ{0}.TCIBD001 WHERE PL.ITEM=TCIBD001.ITEM) ITEMDSE,
                            PL.OQUA QTY,
                            PL.CUQP UNIT,
                            TO_CHAR(PL.DDTE_LOC,'yyyy-MM-dd') RECEIVEDATE,
                            '{0}' as CompCode 
                            FROM
                            BO_READ{0}.TDPUR400 PH,
                            BO_READ{0}.TDPUR401 PL
                            WHERE
                            PH.ORNO=PL.ORNO
                            AND PH.OTBP=PL.OTBP
                            AND 
                            (PL.DDTE_LOC>=TO_DATE(TO_CHAR(SYSDATE+(16/24)-1,'yyyy-MM-dd'),'yyyy-MM-dd') 
                            AND 
                            PL.DDTE_LOC<TO_DATE(TO_CHAR(SYSDATE+(16/24),'yyyy-MM-dd'),'yyyy-MM-dd'))
                            AND PH.ORNO LIKE '{0}I%'
                            ",
                            sSite);
            return sql;
        }
        private DataTable GetBaanData()
        {
            var sql = new StringBuilder();
            sql.AppendFormat(@"
                {0}
                union
                {1}
                union
                {2}
                ",
                MakeSiteQuery("867"), MakeSiteQuery("878"), MakeSiteQuery("891"));

            using (var conn = new OracleConnection(connectionString_Baan))
            {
                var dt = new DataTable();
                using (OracleDataReader dr = OraClientHelper.ExecuteReader(conn, CommandType.Text, sql.ToString(), null))
                {
                    dt.Load(dr);
                }
                return dt;
            }
        }
                
        private static void WriteDBLog(SqlConnection conn, string msg)
        {
            var sDelOld = string.Format("insert into task_log values('{0}', getdate())", msg);
            SqlCommand cmm = new SqlCommand(sDelOld, conn);
            cmm.CommandType = CommandType.Text;
            cmm.ExecuteNonQuery();
        }
        
        //保存baan数据到本地表
        private Tuple<int, string> FillBaanData(SqlConnection conn)
        {
            int nItems = 0;
            var dtNew = GetBaanData();
            if (null==dtNew || 0==dtNew.Rows.Count) {
                return new Tuple<int, string>(nItems, "没有Baan数据");
            }
            //clear first
            var sTrunc = string.Format("TRUNCATE TABLE {0}", TBL_BAAN);
            SqlServerHelper.ExecuteCommand(conn, sTrunc);

            //AddOtherCol(dtNew);
            var sErr = string.Empty;
            SqlBulkCopy bulkcopy = new SqlBulkCopy(conn);
            bulkcopy.DestinationTableName = TBL_BAAN; // dtNew.TableName;
            try
            {
                bulkcopy.WriteToServer(dtNew);
                nItems = dtNew.Rows.Count;
                var sLog = string.Format("Baan item={0}", nItems);
                Console.WriteLine(sLog);
                LogHelper.WriteInfo(typeof(CommodityRecv_DAL), sLog);
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
                LogHelper.WriteError(typeof(CommodityRecv_DAL), ex);
            }
            return new Tuple<int, string>(nItems, sErr);
        }


        private Tuple<int, string> FillUploadCond(SqlConnection conn, DataTable dt)
        {
            //清除上次上传的计划数据
            //var sTrunc = string.Format("TRUNCATE TABLE {0}", TBL_UPLOAD);
            //SqlServerHelper.ExecuteCommand(conn, sTrunc);

            int nItems = 0;
            var dtNew = dt;
            if (null == dtNew || 0 == dtNew.Rows.Count)
            {
                return new Tuple<int, string>(nItems, "没有Baan数据");
            }

            var sErr = string.Empty;
            SqlBulkCopy bulkcopy = new SqlBulkCopy(conn);
            bulkcopy.DestinationTableName = TBL_UPLOAD; // dtNew.TableName;
            try
            {
                bulkcopy.WriteToServer(dtNew);
                nItems = dtNew.Rows.Count;
                var sLog = string.Format("UploadCond item={0}", nItems);
                Console.WriteLine(sLog);
                LogHelper.WriteInfo(typeof(CommodityRecv_DAL), sLog);
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
                LogHelper.WriteError(typeof(CommodityRecv_DAL), ex);
            }
            return new Tuple<int, string>(nItems, sErr);
        }

        //private void AddOtherCol(DataTable dtNew)
        //{
        //    dtNew.Columns.Add("Version", typeof(int));
        //    int nNext = GetMaxVersion();
        //    nNext++;
        //    for(int i=0; i<dtNew.Rows.Count; i++)
        //    {
        //        var r = dtNew.Rows[i];
        //        r["Version"] = nNext;
        //    }
        //}


        //重新加载数据到本地库
        public int DoSyncData(out string sErr)
        {
            using (SqlConnection conn = new SqlConnection(connectionString_Main))
            {
                conn.Open();

                WriteDBLog(conn, "start sync");
                var task2 = new Task<Tuple<int,string>>(() => FillBaanData(conn));
                task2.Start();

                task2.Wait();
                var ret = task2.Result;
                WriteDBLog(conn, string.Format("end sync:{0}", ret.Item1));
                sErr = ret.Item2;
                return ret.Item1;
            }
        }

        //public DataTable DoCalc(DateTime dtCalc)
        //{
        //    using (SqlConnection conn = new SqlConnection(connectionString_Main))
        //    {
        //        conn.Open();

        //        var args = new List<TParamVal>();
        //        args.Add(SqlServerHelper.CreateParameter("curDate", dtCalc));
        //        DateTime oUpdated=DateTime.Now;
        //        args.Add(SqlServerHelper.CreateParameter("outUpdateHGDDatetime", oUpdated, ParameterDirection.Output));
        //        var dt = SqlServerHelper.ExecuteSPQuery(conn, "CalcMinMax_Last", args);                
        //        return dt;
        //    }
        //}

        public DataTable DoCalc()
        {
            using (SqlConnection conn = new SqlConnection(connectionString_Main))
            {
                conn.Open();

                var sql = new StringBuilder();
                sql.Append(@"
                            select * from v_CommodityRecvCmp 
                            order by PONUMBER,ITEM,RECEIVEDATE
                            ");
                var dt = SqlServerHelper.ExecuteQuery(conn, sql.ToString(), null);
                return dt;
            }
        }

        #region send email

        public bool SendMail(DataTable dt, string sFileTempl, DateTime dtGen)
        {
            if (CustomConfig.bSendMail)
            {
                var sMailTitle = "Commodity Receive Dashboard";
                var sMailReceiver = string.Empty;
                var sMailCC = string.Empty;
                LoadMailReceiver(out sMailReceiver, out sMailCC);
                if (string.IsNullOrEmpty(sMailReceiver))
                {
                    LogHelper.WriteError(typeof(CommodityRecv_DAL), "Have no mail receiver!", null);
                    return false;
                }
                //send mail
                var mailSender = new MailSender(CustomConfig.SmtpConnStr, sMailReceiver, sMailCC, sMailTitle);
                var sBody = new StringBuilder();
                try
                {
                    sBody.Append(System.IO.File.ReadAllText(sFileTempl));
                    sBody.Replace("{title}", sMailTitle);
                    var sContent = string.Format("Automatic Email for {0} {1}",
                            "Commodity Receive Data",
                            dtGen.ToString("yyyy-MM-dd HH:mm")
                            );
                    sBody.Replace("{content}", sContent);
                    sBody.Replace("{tabContent}", MakeTabContent(dt));
                }
                catch (Exception ex)
                {
                    LogHelper.WriteError(typeof(CommodityRecv_DAL), ex);
                    return false;
                }

                return mailSender.SendEmail(sBody.ToString(), "");
            }
            return false;
        }

        private void LoadMailReceiver(out string recvs, out string ccs)
        {
            recvs = string.Empty;
            ccs = string.Empty;
            using (SqlConnection conn = new SqlConnection(connectionString_Main))
            {
                conn.Open();

                var sql = new StringBuilder();
                sql.Append(@"
                            select * from tbl_cr_mailReceiver
                            where isValid>0
                            order by mailAddrType
                            ");
                var dt = SqlServerHelper.ExecuteQuery(conn, sql.ToString(), null);
                if (null == dt) { return; }
                var lstRecv = new List<string>();
                var lstCc = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    int nType = (int)row["mailAddrType"];
                    if (0 == nType)
                    {
                        lstRecv.Add(row["mailAddr"] as string);
                    }
                    else{
                        lstCc.Add(row["mailAddr"] as string);
                    }
                }
                if (lstRecv.Count == 0)
                {
                    recvs = string.Join(";", lstCc);
                }
                else
                {
                    recvs = string.Join(";", lstRecv);
                    ccs = string.Join(";", lstCc);
                }
            }
        }

        // DataTable ---> <Table>
        private static string MakeTabContent(DataTable dt)
        {
            if (null == dt) { return string.Empty; }
            var sTab = new StringBuilder();
            sTab.Append(@"<table border='0' cellspacing='0' cellpadding='0' class='myTab'>");
            sTab.Append(@"<thead><tr>");
            foreach (DataColumn col in dt.Columns)
            {
                sTab.AppendFormat(@"<th><p>{0}</p></th>", col.ColumnName);
            }
            sTab.Append(@"</thead></tr>");

            sTab.Append(@"<tbody>");
            foreach (DataRow row in dt.Rows)
            {
                sTab.Append(@"<tr>");
                for (var j = 0; j < dt.Columns.Count; j++)
                {
                    if(0 == string.Compare(dt.Columns[j].ColumnName, "RECEIVEDATE", true))
                    {
                        var dat = (DateTime)row[j];
                        sTab.AppendFormat(@"<td>{0}</td>", dat.ToString("yyyy-MM-dd"));
                    }
                    else
                    {
                        sTab.AppendFormat(@"<td>{0}</td>", row[j]);
                    }
                    
                }
                sTab.Append(@"</tr>");
            }
            sTab.Append(@"</tbody>");

            sTab.Append(@"</table");
            return sTab.ToString();
        }
        #endregion

        private DataTable ReadUpload(FileInfo xlsxFile)
        {
            var dt = NPOIExcelHelper.ReadExcel(xlsxFile);
            if (dt == null)
            {
                return null;
            }

            //convert 
            var dtNew = new DataTable(TBL_UPLOAD);
            dtNew.Columns.Add("id", typeof(int));
            dtNew.Columns.Add("CommodityCode", typeof(string));
            dtNew.Columns.Add("CostItemNumber", typeof(string));
            dtNew.Columns.Add("CommodityCodeDescription", typeof(string));

            var dtUploadTime = DateTime.Now;
            foreach (DataRow row in dt.Rows)
            {
                var drNew = dtNew.NewRow();
                drNew["CommodityCode"] = row["Commodity Code"];
                var sKey = row["Cost Item Number"] as string;
                if (sKey != null) { sKey = sKey.Trim(); }                
                //skip null
                if (string.IsNullOrEmpty(sKey))
                {
                    continue;
                }
                drNew["CostItemNumber"] = sKey;
                drNew["CommodityCodeDescription"] = row["Commodity Code Description"];
                dtNew.Rows.Add(drNew);
            }
            return dtNew;
        }
        public int DoImpUpload(FileInfo xlsxFile, out string sErr)
        {
            sErr = string.Empty;
            DataTable dtPlan = ReadUpload(xlsxFile);
            using (var conn = new SqlConnection(connectionString_Main))
            {
                conn.Open();

                WriteDBLog(conn, "start Upload");
                var task2 = new Task<Tuple<int,string>>(() => FillUploadCond(conn, dtPlan));
                task2.Start();

                task2.Wait();
                var res = task2.Result;
                WriteDBLog(conn, string.Format("end Upload:{0}", res.Item1));
                sErr = res.Item2;
                return res.Item1;
            }
        }
    }
}
