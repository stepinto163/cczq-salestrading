using com.gateway.log;
using Microsoft.Win32;
using STC.Common.Handler.Quant;
using STC.Common.Model;
using STC.Common.Model.Quant;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.quant.plugin.Utils;
using STC.quant.plugin.Utils.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.quant.plugin.ViewModel
{
    public class QuantLogViewModel : BaseViewModel
    {
        private object _lockObj = new object();
        public const string Createtime = "Createtime";
        public const string Content = "Content";
        public const string Id = "Id";
        public const string Txt = "Txt";
        public string quantID = "";

        private AsyncThreadQueue<TTbookQuantLog> _ProcessQuantLogThreadQueue;
        public ICommand DownLoadCommand { get { return new RelayCommand(OnDownLoadClick); } }
        public QuantLogViewModel(DataGrid dataGrid)
        {
            BindingOperations.EnableCollectionSynchronization(LogDT, _lockObj);
            if (_ProcessQuantLogThreadQueue == null)
            {
                _ProcessQuantLogThreadQueue = new AsyncThreadQueue<TTbookQuantLog>(OnQuantLog);
                QuantGroupHandler.Instance.QuantLog += Instance_QuantLog;
            }
            this.dataGrid = dataGrid;
        }

        public DataGrid dataGrid { get; set; }
        /// <summary>
        /// 策略日志数据源
        /// </summary>
        private DataView _LogDT;
        public DataView LogDT
        {
            get
            {
                lock (_lockObj)
                {
                    if (_LogDT == null)
                    {
                        _LogDT = new DataTable().DefaultView;
                        _LogDT.Table.Columns.Add(Createtime, typeof(string));
                        _LogDT.Table.Columns.Add(Content, typeof(string));
                        _LogDT.Table.Columns.Add(Id, typeof(string));
                        _LogDT.Table.Columns.Add(Txt, typeof(string));
                        _LogDT.Table.PrimaryKey = new DataColumn[] { _LogDT.Table.Columns[Id] };
                        this.OnPropertyChanged();
                    }
                    return this._LogDT;
                }
            }
        }

        private void OnQuantLog(TTbookQuantLog log)
        {
            DataRow dataRow = _LogDT.Table.NewRow();
            dataRow[Createtime] = log.Createtime;
            dataRow[Content] = log.Content;
            dataRow[Id] = log.Id;
            dataRow[Txt] = string.Format("{0}   {1}", log.Createtime, log.Content);
            this._LogDT.Table.Rows.Add(dataRow);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this._LogDT.Table.AcceptChanges();
                ScrollToEnd();
            }));
        }

        public void init(string quantID, string id, DataGrid dt)
        {
            this.quantID = quantID;
            //this.dataGrid = dt;
            ThreadPool.QueueUserWorkItem(o =>
            {
                // QuantGroupHandler.Instance.InitLog(quantID,id);
                QuantLogSelReq req = new QuantLogSelReq();
                req.pageNum = 0;
                req.pageSize = 5000;
                req.quantid = quantID;
                req.id = id;
                TbookQuantLogReplyPo reply = QuantHandler.Instance.QuantLogQuery(req);
                if (reply != null)
                {
                    if (reply != null && reply.Code == "0000" && reply.Datas != null)
                    {
                        foreach (TTbookQuantLog log in reply.Datas)
                        {
                            DataRow dataRow = _LogDT.Table.NewRow();
                            dataRow[Createtime] = log.Createtime;
                            dataRow[Content] = log.Content;
                            dataRow[Id] = log.Id;
                            dataRow[Txt] = string.Format("{0}   {1}", log.Createtime, log.Content);
                            this._LogDT.Table.Rows.Add(dataRow);//.InsertAt(dataRow, 0);
                        }

                        //int j = 1;
                        //for (int i = 0; i < 50; i++)
                        //{
                        //    foreach (TTbookQuantLog log in reply.Datas)
                        //    {
                        //        DataRow dataRow = _LogDT.Table.NewRow();
                        //        dataRow[Createtime] = log.Createtime;
                        //        dataRow[Content] = log.Content;
                        //        dataRow[Id] = log.Id + j;
                        //        dataRow[Txt] = string.Format("{0}   {1} ---{2}", log.Createtime, log.Content,j);
                        //        this._LogDT.Table.Rows.Add(dataRow);//.InsertAt(dataRow, 0);

                        //        j++;
                        //    }
                        //}

                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this._LogDT.Table.AcceptChanges();
                            ScrollToEnd();
                        }));
                    }
                }
            });
        }

        public void Instance_QuantLog(TTbookQuantLog obj)
        {
            if (obj.Quantid == this.quantID)
            {
                if (_ProcessQuantLogThreadQueue != null)
                {
                    _ProcessQuantLogThreadQueue.Enqueue(obj);
                }
            }
        }

        private void ScrollToEnd()
        {
            DataGridAutomationPeer lvap = new DataGridAutomationPeer(dataGrid);
            var svap = lvap.GetPattern(PatternInterface.Scroll) as ScrollViewerAutomationPeer;
            if (svap != null)
            {
                var scroll = svap.Owner as ScrollViewer;
                scroll.ScrollToEnd();
            }
        }

        public void Clear()
        {
            if (_ProcessQuantLogThreadQueue != null)
            {
                _ProcessQuantLogThreadQueue.Clear();
            }
        }

        #region 下载txt
        private DataTable ExportDV;
        private string fileName = "";
        AutoResetEvent autoEvent = new AutoResetEvent(false);
        private void OnDownLoadClick()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(文本文件*.txt)|*.txt";
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_"+quantID;
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            if ((bool)(saveFileDialog.ShowDialog()))
            {
                fileName = saveFileDialog.FileName;
                ExportDV = this.LogDT.Table.Clone();
                ThreadPool.QueueUserWorkItem(new WaitCallback(RequestExportMethod), autoEvent);
            }
        }
        private void RequestExportMethod(object state)
        {
            lock (ExportDV.Rows.SyncRoot)
            {
                RequestExportOrders();
            }
            autoEvent.Set();
        }

        public void RequestExportOrders()
        {
            StreamWriter sw = null;
            try
            {
                LogHelper.Info("fileName:" + fileName);

                bool isOk = TableToCsv(LogDT.ToTable(), fileName, "");
                if (isOk)
                {

                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        showMessage("导出失败");
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestExportOrders", e);
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    showMessage("导出失败");
                }));
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
            }
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
                foreach (DataRow dr in dt.Rows)
                {
                    string text = dr[3].ToString();
                    sw.WriteLine(text);
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

        private void showMessage(string message)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.setWarnMsg(message);
            me.ShowDialog();
        }
        #endregion
    }
}
