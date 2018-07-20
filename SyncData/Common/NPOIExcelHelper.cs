using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Data.Common;
using System.IO;

namespace SyncData.Common
{
    public class NPOIExcelHelper
    {
        public static byte[] BuilderExcel(DataTable table, string dateFormat = "yyyy-MM-dd HH:mm:ss")
        {
            IWorkbook workbook = new XSSFWorkbook(); //office2007            
            ISheet sheet = workbook.CreateSheet("Sheet1");
            IRow headerRow = sheet.CreateRow(0);

            // handling header.
            foreach (DataColumn column in table.Columns)
            {
                headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
            }   

            int rowIndex = 1;
            foreach (DataRow row in table.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);
                foreach (DataColumn column in table.Columns)
                {
                    dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                }
                rowIndex++;
            }

            byte[] bys = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                memoryStream.Flush();
                bys = memoryStream.ToArray();
            }
            return bys;
        }

        // untest
        public static byte[] BuilderExcel(DbDataReader dtReader, string dateFormat = "yyyy-MM-dd HH:mm:ss")
        {
            IWorkbook workbook = new XSSFWorkbook(); //office2007            
            ISheet sheet = workbook.CreateSheet("Sheet1");
            IRow headerRow = sheet.CreateRow(0);

            // handling header.
            for(var i=0; i<dtReader.FieldCount; i++)
            {
                headerRow.CreateCell(i).SetCellValue(dtReader.GetName(i));
            }

            int rowIndex = 1;
            while (dtReader.Read())
            {
                IRow dataRow = sheet.CreateRow(rowIndex);
                for (var i= 0; i < dtReader.FieldCount; i++)
                {
                    dataRow.CreateCell(i).SetCellValue(dtReader[i].ToString());
                }
                rowIndex++;
            }

            byte[] bys = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                memoryStream.Flush();
                bys = memoryStream.ToArray();
            }
            return bys;
        }

        public static DataTable ReadExcel(System.IO.Stream stream, bool bNewExcel = true)
        {
            IWorkbook wb;
            if (bNewExcel)
            {
                wb = new XSSFWorkbook(stream); // excel2007
            }
            else
            {
                wb = new HSSFWorkbook(stream); // excel97
            }

            // 只取第一个sheet
            ISheet sheet = wb.GetSheetAt(0);
            DataTable dt = new DataTable();

            // 由第一行取标题
            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum; // 取列数
            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                var sColName = headerRow.GetCell(i).StringCellValue.Trim();
                dt.Columns.Add(new DataColumn(sColName));
            }
            
            //TODO 这里改成读列名应该会好点
            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                DataRow dataRow = dt.NewRow();
                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    ICell cell = row.GetCell(j);
                    if (cell != null)
                    {
                        //根据.CellType来区别不同类型
                        switch (cell.CellType)
                        {
                            case CellType.Numeric:
                                dataRow[j] = cell.NumericCellValue;
                                break;
                            default: 
                                // String
                                dataRow[j] = cell.StringCellValue;
                                break;
                        }
                    }
                }

                dt.Rows.Add(dataRow);
            }
            
            return dt;
        }
        public static DataTable ReadExcel(System.IO.FileInfo file)
        {
            using (System.IO.Stream stream = file.OpenRead())
            {
                var bNewExcel = (0==string.Compare(file.Extension,".xlsx", false));
                return ReadExcel(stream, bNewExcel);
            }
        }
    }
}
