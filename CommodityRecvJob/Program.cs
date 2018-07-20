using SyncData;
using System;

namespace MinMaxPullJob
{
    class Program
    {
        static void Main(string[] args)
        {
            var dtNow = DateTime.Now;
            CommodityRecv_DAL dal = new CommodityRecv_DAL(CustomConfig.ConnStrBaan, CustomConfig.ConnStrMain);            
            bool bRun = true;
            bool bSync = CustomConfig.PullBaanData;  //PullBaanData=false用于测试
            if (bSync)
            {
                var sErr = string.Empty;
                bRun = dal.DoSyncData(out sErr) > 0;
            }
            if(bRun)
            {
                var dt = dal.DoCalc();
                var sFileTemp = SyncData.Common.PathHelper.GetFile("", "email.template");
                dal.SendMail(dt, sFileTemp, dtNow);
            }
        }

    }
}


