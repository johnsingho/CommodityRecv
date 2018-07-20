using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace SyncData
{
    public class CsvWriter
    {
        private static string GetTempFileName(string sDirBase)
        {
            return sDirBase + @"\MinMaxPull" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
        }

        public static string PutMinMaxData(DataTable dt, string sDirBase)
        {            
            const string sFmt = "{0},,{1},{2},{3},1";
            StringBuilder msb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                msb.AppendFormat(sFmt,
                                    r["Warehouse"] as string,
                                    r["Item"] as string,
                                    (int)r["MaxValue"],
                                    (int)r["MinValue"]
                                    );
                msb.AppendLine();
            }

            try
            {
                var fn = GetTempFileName(sDirBase);
                File.WriteAllText(fn, msb.ToString());
                return fn;
            }
            catch(Exception ex)
            {
                LogHelper.WriteError(typeof(CsvWriter), ex);
                throw;
                //Console.WriteLine("WriteFile Error: {0}", ex.Message);
            }
            //return string.Empty;
        }
        
    }
}
