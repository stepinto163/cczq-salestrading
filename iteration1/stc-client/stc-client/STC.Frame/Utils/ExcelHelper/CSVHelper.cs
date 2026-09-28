using com.gateway.log;
using STC.Common.Model.Base;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Utils.ExcelHelper
{    /// <summary>
     /// 导出CSV工具类
     /// </summary>
    public class CSVHelper
    {
        private static CSVHelper hepler = new CSVHelper();
        public static CSVHelper Instance { get { return hepler; } }

        private string msg = "";

        public string Msg
        {
            get
            {
                return msg;
            }

            set
            {
                msg = value;
            }
        }
        private Dictionary<string, int> columnDic = new Dictionary<string, int>();

        public StreamWriter createStream(string fileName)
        {
            FileStream fs;
            StreamWriter sw;
            try
            {
                string savePath = Path.GetDirectoryName(fileName);
                string saveFile = fileName;
                //创建路径和文件
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                if (!File.Exists(saveFile))
                {
                    fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
                }
                else
                {
                    File.Delete(saveFile);
                    fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
                }
                sw = new StreamWriter(fs, Encoding.UTF8);
                return sw;
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return null;
        }
        public Boolean TableToCsv(DataTable dt, string fileName, string uiName)
        {
            FileStream fs;
            StreamWriter sw;
            string savePath = Path.GetDirectoryName(fileName);
            string saveFile = fileName;
            //创建路径和文件
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            if (!File.Exists(saveFile))
            {
                fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
            }
            else
            {
                File.Delete(saveFile);
                fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
            }
            sw = new StreamWriter(fs, Encoding.UTF8);
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                int colTotal = 0;
                if (dt.Columns.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in dic)
                    {
                        dt.Columns[kvp.Key].ColumnName = kvp.Value;
                    }
                }
                Dictionary<int, int> colNum = new Dictionary<int, int>();
                int settlIndex = -1;
                int colcount = 0;
                //写标题
                int iColCount = dt.Columns.Count;
                for (int i = 0; i < iColCount; i++)
                {
                    if (dic.ContainsValue(dt.Columns[i].ColumnName))
                    {
                        sw.Write(dt.Columns[i]);
                        if (colcount < colTotal - 1)
                        {
                            sw.Write(",");
                        }
                        colNum.Add(i, i);
                        if (dt.Columns[i].ColumnName.Equals("清算类型"))
                        {
                            settlIndex = i;
                        }
                        colcount++;
                    }
                }
                sw.Write(sw.NewLine);
                //写内容
                foreach (DataRow dr in dt.Rows)
                {
                    colcount = 0;
                    foreach (KeyValuePair<int, int> kpv in colNum)
                    {
                        int i = kpv.Key;
                        if (colNum.ContainsKey(i))
                        {
                            if (!Convert.IsDBNull(dr[i]))
                            {

                                if (settlIndex > 0 && i == settlIndex)
                                {
                                    string text = dr[i].ToString();
                                    if (text.Contains(","))
                                    {
                                        text = text.Replace(",", "\\");
                                        sw.Write(text);
                                    }
                                    else
                                    {
                                        sw.Write(text);
                                    }
                                }
                                else
                                {
                                    string coltext = dr[i].ToString();
                                    string writeText = "";
                                    string conumer = dt.Columns[i].ColumnName;
                                   

                                        sw.Write(coltext);
                                }
                            }
                            else
                                sw.Write("\"\"");
                            if (colcount < colTotal - 1)
                            {
                                sw.Write(",");
                            }
                            colcount++;
                        }
                    }
                    sw.Write(sw.NewLine);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                return false;
            }
            finally
            {
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            return true;
        }
        #region common

        private SimpleBondsInfoPo getBondsInfo(String securityid)
        {
            List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
            result = CommonUtils.GetODMBondsInfoPo(securityid, 1);
            if (result.Count > 0)
            {
                return result.ToArray()[0];
            }
            return null;
        }
        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型
        /// <param name="FILE_NAME">文件路径</param>
        /// <returns>文件的编码类型</returns>

        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            System.IO.FileStream fs = new System.IO.FileStream(FILE_NAME, System.IO.FileMode.Open,
            System.IO.FileAccess.Read);
            System.Text.Encoding r = GetType(fs);
            fs.Close();
            return r;
        }

        /// 通过给定的文件流，判断文件的编码类型
        /// <param name="fs">文件流</param>
        /// <returns>文件的编码类型</returns>
        public static System.Text.Encoding GetType(System.IO.FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            System.Text.Encoding reVal = System.Text.Encoding.Default;

            System.IO.BinaryReader r = new System.IO.BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = System.Text.Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = System.Text.Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = System.Text.Encoding.Unicode;
            }
            r.Close();
            return reVal;
        }

        /// 判断是否是不带 BOM 的 UTF8 格式
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
        /// <summary>
        /// 修改文件名称
        /// 我们需要保存历史数据 或者实时的知道那个文件被修改 可以通过改变文件的名称 如加上当天的日期等等。
        /// </summary>
        /// <param name="OldPath"></param>
        /// <param name="NewPath"></param>
        /// <returns></returns>
        public static bool ChangeFileName(string OldPath, string NewPath)
        {
            bool re = false;
            try
            {
                if (File.Exists(OldPath))
                {
                    File.Move(OldPath, NewPath);
                    re = true;
                }
            }
            catch
            {
                re = false;
            }
            return re;
        }

        /// <summary>
        /// CSV文件的数据写入
        /// 直接在网页表单提交数据保存在csv文件中 直接写入文件
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static bool SaveCSV(string fullPath, string Data)
        {
            bool re = true;
            try
            {
                FileStream FileStream = new FileStream(fullPath, FileMode.Append);
                StreamWriter sw = new StreamWriter(FileStream, System.Text.Encoding.UTF8);
                sw.WriteLine(Data);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                FileStream.Close();
            }
            catch
            {
                re = false;
            }
            return re;
        }

        /// <summary>
        /// 将CSV文件的数据读取到DataTable中
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        /// <returns>返回读取了CSV数据的DataTable</returns>
        public DataTable ReadCSV(string filePath)
        {
            try
            {
                Encoding encoding = GetType(filePath); //Encoding.ASCII;//
                DataTable dt = new DataTable();
                FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                StreamReader sr = new StreamReader(fs, encoding);
                //string fileContent = sr.ReadToEnd();
                //encoding = sr.CurrentEncoding;
                //记录每次读取的一行记录
                string strLine = "";
                //记录每行记录中的各字段内容
                string[] aryLine = null;
                string[] tableHead = null;
                //标示列数
                int columnCount = 0;
                //标示是否是读取的第一行
                bool IsFirst = true;
                //逐行读取CSV中的数据
                while ((strLine = sr.ReadLine()) != null)
                {
                    //strLine = Common.ConvertStringUTF8(strLine, encoding);
                    //strLine = Common.ConvertStringUTF8(strLine);

                    if (IsFirst == true)
                    {
                        tableHead = strLine.Split(',');
                        IsFirst = false;
                        columnCount = tableHead.Length;
                        //创建列
                        for (int i = 0; i < columnCount; i++)
                        {
                            DataColumn dc = new DataColumn(tableHead[i]);
                            dt.Columns.Add(dc);
                        }
                    }
                    else
                    {
                        aryLine = strLine.Split(',');
                        DataRow dr = dt.NewRow();
                        for (int j = 0; j < columnCount; j++)
                        {
                            dr[j] = aryLine[j];
                        }
                        dt.Rows.Add(dr);
                    }
                }

                sr.Close();
                fs.Close();
                return dt;
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return null;
        }
        #endregion
    }
}
