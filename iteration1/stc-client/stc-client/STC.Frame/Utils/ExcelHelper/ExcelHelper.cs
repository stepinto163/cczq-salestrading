using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Utils.ExcelHelper
{
    public class ExcelHelper : IDisposable
    {
        private string fileName = null; //文件名
        private IWorkbook workbook = null;
        private FileStream fs = null;
        private bool disposed;

        public ExcelHelper(string fileName)
        {
            this.fileName = fileName;
            disposed = false;
        }

        /// <summary>
        /// 将dataDic数据导入到excel中
        /// </summary>
        /// <param name="dataDic">要导入的数据; key:sheet的名称 ;value:DataTable的列名是否要导入 </param>
        /// <param name="isColumnWritten">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public Dictionary<string, int> DataTableToExcel(Dictionary<string, DataTable> dataDic, bool isColumnWritten)
        {
            if (dataDic == null) return null;
            Dictionary<string, int> countDic = new Dictionary<string, int>();
            ISheet sheet;

            fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                workbook = new XSSFWorkbook();
            else if (fileName.IndexOf(".xls") > 0) // 2003版本
                workbook = new HSSFWorkbook();

            try
            {
                foreach(string sheetName in dataDic.Keys)
                {
                    countDic[sheetName] = -1;
                }

                foreach(KeyValuePair<string, DataTable> pair in dataDic)
                {
                    int count = 0;
                    string sheetName = pair.Key;
                    DataTable data = pair.Value;
                    if (workbook != null)
                    {
                        sheet = workbook.CreateSheet(sheetName);
                    }
                    else
                    {
                        continue;
                    }

                    if (isColumnWritten == true) //写入DataTable的列名
                    {
                        IRow row = sheet.CreateRow(0);
                        for (int j = 0; j < data.Columns.Count; ++j)
                        {
                            row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                        }
                        count = 1;
                    }
                    else
                    {
                        count = 0;
                    }

                    for (int i = 0; i < data.Rows.Count; ++i)
                    {
                        IRow row = sheet.CreateRow(count);
                        for (int j = 0; j < data.Columns.Count; ++j)
                        {
                            row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                        }
                        ++count;
                    }
                   
                    countDic[sheetName] = count;
                }
                workbook.Write(fs); //写入到excel
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            return countDic;
        }

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        ///


        public Dictionary<int, string> ReturnSheetList()
        {
            Dictionary<int, string> t = new Dictionary<int, string>();
            ISheet sheet = null;
            DataTable data = new DataTable();
            //int startRow = 0;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);
                int count = workbook.NumberOfSheets; //获取所有SheetName
                for (int i = 0; i < count; i++)
                {
                    sheet = workbook.GetSheetAt(i);
                    //if (sheet.LastRowNum > 0)
                    //{
                        t.Add(i, workbook.GetSheetAt(i).SheetName);
                    //}
                }
                return t;
            }
            catch(NPOI.HSSF.OldExcelFormatException oldExcelFormatException)
            {
                if (oldExcelFormatException.Message.Contains("POI only supports BIFF8 format (from Excel versions 97/2000/XP/2003)"))
                {
                    LogHandler.Error(oldExcelFormatException);
                    t = ReturnSheetList95(fileName);
                    return t;
                }
                else
                {
                    throw oldExcelFormatException;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        ///index excel的第几个sheet
        public DataTable ExcelToDataTable(int index)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            int startRow = 0;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);
                //int coutnts = workbook.NumberOfSheets;

                sheet = workbook.GetSheetAt(index);
                //string names= sheet.SheetName;
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int firstCellIndex = 0;
                    int cellCount = 0;
                    if  (firstRow != null)
                    {
                        firstCellIndex = firstRow.FirstCellNum;
                        cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数
                    }
                                            
                    for (int i = firstCellIndex; i < cellCount; ++i)
                    {
                        ICell cell = firstRow.GetCell(i);
                        CellType c = cell.CellType;
                        if (cell != null)
                        {
                            string cellValue = cell.StringCellValue;
                            if (cellValue != null)
                            {
                                DataColumn column = new DataColumn(cellValue);
                                data.Columns.Add(column);
                            }
                        }
                    }
                    startRow = sheet.FirstRowNum + 1;


                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                                dataRow[j] = row.GetCell(j).ToString();
                        }
                        if(CheckRowIsValid(dataRow) == true)
                        {
                            data.Rows.Add(dataRow);
                        }
                       else
                        {
                            dataRow.Delete();
                        }
                    }
                }

                return data;
            }
            catch (NPOI.HSSF.OldExcelFormatException oldExcelFormatException)
            {
                if (oldExcelFormatException.Message.Contains("POI only supports BIFF8 format (from Excel versions 97/2000/XP/2003)"))
                {
                    LogHandler.Error(oldExcelFormatException);
                    return ExcelToDataTable95(fileName, index);
                }
                else
                {
                    throw oldExcelFormatException;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<int, string> ReturnSheetList95(string Path)
        {
            OleDbConnection conn = null;
            try
            {
                Dictionary<int, string> t = new Dictionary<int, string>();
                string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
                conn = new OleDbConnection(strConn);
                conn.Open();
                DataTable schemaTable = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);

                int index = 0;
                if (schemaTable != null && schemaTable.Rows.Count > 0)
                {
                    foreach(DataRow dr in schemaTable.Rows)
                    {
                        t.Add(index, dr[2].ToString().Replace("$","").TrimStart(new char[] { '\''}).TrimEnd(new char[] { '\'' }));
                        index++;
                    }
                }

                return t;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Dispose();
                }
            }
        }

        public DataTable ExcelToDataTable95(string Path, int index)
        {
            OleDbConnection conn = null;
            DataSet ds = null;
            try
            {
                string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
                conn = new OleDbConnection(strConn);
                conn.Open();
                string strExcel = "";
                OleDbDataAdapter myCommand = null;

                DataTable schemaTable = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
                string tableName = schemaTable.Rows[index][2].ToString().Trim();

                strExcel = $"select * from [{tableName}]";
                myCommand = new OleDbDataAdapter(strExcel, strConn);
                ds = new DataSet();
                myCommand.Fill(ds, "table1");
                return ds.Tables[0];
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Dispose();
                }
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (fs != null)
                        fs.Close();
                }

                fs = null;
                disposed = true;
            }
        }

        #region 验证文件是否被占用
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
        const int OF_READWRITE = 2;
        const int OF_SHARE_DENY_NONE = 0x40;
        static readonly IntPtr HFILE_ERROR = new IntPtr(-1);
        /// <summary> 文件是否被占用
        /// </summary>
        /// <param name="importFile">文件路径</param>
        /// <returns>是否占用</returns>
        public static bool CheckFileIsLock(string importFile)
        {
            IntPtr vHandle = _lopen(importFile, OF_READWRITE | OF_SHARE_DENY_NONE);
            var result = (vHandle == HFILE_ERROR);
            CloseHandle(vHandle);
            return result;
        }
        #endregion

        public static bool CheckRowIsValid(DataRow dr)
        {
            foreach(DataColumn column in dr.Table.Columns)
            {
                if (!(dr[column.ColumnName] is System.DBNull)
                    && dr[column.ColumnName] != null
                    && dr[column.ColumnName].ToString().Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void deleteFile() 
        {
            if (File.Exists(fileName)) 
            {
                File.Delete(fileName);
            }
        }
    }
}
