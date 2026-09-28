using CefSharp;
using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Chrome.Plugin.ViewModel.Topo;
using STC.Common.Handler.Monitor;
using STC.Common.Model.Chrome;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Monitor;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Chrome.Plugin.ViewModel
{
    public class TopoViewModel
    {
        private object updlock = new object();
        private ChromeUserControl ChromeCtrl;
        public bool IsSubscribe = false;
        public string LocalJsonFile = AppDomain.CurrentDomain.BaseDirectory + $"bin\\HTML\\Topo\\stc_tbook_monitor_svrinfo.json";
        /// <summary>
        /// 数据库中的所有Node节点
        /// </summary>
        public Dictionary<string, nodeDataModel> LstNodeDataModel = new Dictionary<string, nodeDataModel>();
        public TopoViewModel()
        {
            
        }

        public TopoViewModel(ChromeUserControl brower)
        {
            ChromeCtrl = brower;
            ChromeCtrl.Browser.FrameLoadEnd += Browser_FrameLoadEnd;
            ChromeCtrl.Browser.Unloaded += Browser_Unloaded;
        }

        private void Browser_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            MonitorHandler.Instance.UnSubscribeMonitorInfo(OnMonitorInfoResultCallBack);
            MonitorHandler.Instance.UnSubscribeMonitorMsginfo(OnMonitorMsgInfoCallBack);
        }

        #region Load...
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            Init();
        }

        private async void Init()
        {
            await Task.Run(() =>
            {
                Thread.Sleep(100);
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync("getModel();");
                }));
            });

            if (!IsSubscribe)
            {
                MonitorHandler.Instance.SubscribeMonitorInfo(OnMonitorInfoResultCallBack);
                IsSubscribe = true;
            }
        }
        #endregion

        #region 订阅回调
        private List<linkDataArray> LstLinkData = new List<linkDataArray>();
        /// <summary>
        /// zookeeper,currentconnections 回调处理
        /// </summary>
        /// <param name="riskIndicator"></param>
        private void OnMonitorInfoResultCallBack(Dictionary<string, monitormodel> res)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (updlock)
                {
                    foreach (var k in res)
                    {
                        if (k.Key == "zookeeper")
                        {
                            monitormodel nm = k.Value;
                            if (nm != null)
                            {
                                if (!nm.desc.Contains("Stat:1"))
                                {
                                    nm.value = 0;
                                }

                                CallJSupdateNode(nm);
                            }
                        }
                        else if (k.Key == "runinfo")
                        {
                            monitormodel runinfo = k.Value;
                            if (runinfo != null)
                            {
                                CallJSupdateRuninfo(runinfo);
                            }
                        }
                        else if (k.Key == "connections")
                        {
                            monitormodel conn = k.Value;
                            if (conn != null)
                            {
                                if (!string.IsNullOrEmpty(conn.desc))
                                {
                                    string[] desc = conn.desc.Split(',');
                                    for (int i = 0; i < desc.Length; i++)
                                    {
                                        linkDataArray lda = new linkDataArray();
                                        lda.from = desc[i].Split('-')[1];
                                        lda.to = conn.serverName;
                                        lda.updatetime = conn.updatetime;
                                        lda.status = 1;
                                        CallJSaddLink(lda);
                                    }
                                }
                            }
                        }
                    }
                }
            }));
        }

        #endregion

        private void OnMonitorMsgInfoCallBack(Dictionary<string, Dictionary<string, monitormodel>> res)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (updlock)
                {
                    foreach (var ms in res)
                    {
                        string servername = ms.Key;
                        if (ms.Value.ContainsKey("connections"))
                        {
                            monitormodel connections = ms.Value["connections"];
                            if (connections != null)
                            {
                                if (!string.IsNullOrEmpty(connections.desc))
                                {
                                    string[] desc = connections.desc.Split(',');
                                    for (int i = 0; i < desc.Length; i++)
                                    {
                                        linkDataArray lda = new linkDataArray();
                                        lda.from = desc[i].Split('-')[1];
                                        lda.to = connections.serverName;
                                        lda.updatetime = connections.updatetime;
                                        lda.status = 1;
                                        CallJSaddLink(lda);
                                        //Thread.Sleep(5);
                                    }
                                }
                            }
                        }
                        if (ms.Value.ContainsKey("runinfo"))
                        {
                            monitormodel runinfo = ms.Value["runinfo"];
                            if (runinfo != null)
                            {
                                CallJSupdateRuninfo(runinfo);
                                //Thread.Sleep(5);
                            }
                        }
                        if (ms.Value.ContainsKey("zookeeper"))
                        {
                            monitormodel zookeeper = ms.Value["zookeeper"];
                            if (zookeeper != null)
                            {
                                if (zookeeper != null)
                                {
                                    if (!zookeeper.desc.Contains("Stat:1"))
                                    {
                                        zookeeper.value = 0;
                                    }

                                    CallJSupdateNode(zookeeper);
                                    //Thread.Sleep(5);
                                }
                            }
                        }
                    }
                }
            }));
        }

        #region 修改UI状态
        /// <summary>
        /// 初始化Node
        /// </summary>
        /// <param name="obj"></param>
        private void CallJSupdateNode(object obj)
        {
            string res = JsonConvert.SerializeObject(obj);
            var t = Task.Run(async () =>
            {
                if (ChromeCtrl.Browser != null)
                {
                    try
                    {
                        await ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"updateNode('{res}')");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }
            });
        }

        /// <summary>
        /// 初始化Node  CallJSupdateRuninfo
        /// </summary>
        /// <param name="obj"></param>
        private void CallJSupdateRuninfo(object obj)
        {
            string res = JsonConvert.SerializeObject(obj);
            var t = Task.Run(async () =>
            {
                if (ChromeCtrl.Browser != null)
                {
                    try
                    {
                        await ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"updateRuninfo('{res}')");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }
            });
        }

        /// <summary>
        /// 增加连接线
        /// </summary>
        /// <param name="obj"></param>
        private void CallJSaddLink(object obj)
        {
            string res = JsonConvert.SerializeObject(obj);
            var t = Task.Run(async () =>
            {
                if (ChromeCtrl.Browser != null)
                {
                    try
                    {
                        await ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"addLink('{res}')");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }
            });
        }

        /// <summary>
        /// 故障连接线
        /// </summary>
        /// <param name="obj"></param>
        private void CallJSproblemLink(object obj)
        {
            string res = JsonConvert.SerializeObject(obj);
            var t = Task.Run(async () =>
            {
                if (ChromeCtrl.Browser != null)
                {
                    try
                    {
                        await ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"problemLink('{res}')");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }
            });
        }

        /// <summary>
        /// 移除连接线
        /// </summary>
        /// <param name="obj"></param>
        private void CallJSremoveLink(object obj)
        {
            string res = JsonConvert.SerializeObject(obj);
            var t = Task.Run(async () =>
            {
                if (ChromeCtrl.Browser != null)
                {
                    try
                    {
                        await ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"removeLink('{res}')");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }
            });
        }
        #endregion

        #region 保存配置
        /// <summary>
        /// Js Call保存配置到后台
        /// </summary>
        /// <param name="code"></param>
        public string getSaveModel(string code)
        {
            string res = string.Empty;
            var t = Task.Run(async () =>
            {
                res = await CommmitManualInput(code);
            });

            Task.WaitAll(t);
            return res;

        }

        private async Task<string> CommmitManualInput(string code)
        {
            string resreply = null;
            try
            {
                int cntSuccess = 0;
                int cntFail = 0;
                int cntRecord = 0;
                //替换名称冲突的
                code = code.Replace("\"class\":\"GraphLinksModel\"", "\"Class\":\"GraphLinksModel\"");
                GraphLinksModels glm = JsonConvert.DeserializeObject<GraphLinksModels>(code);
                try
                {
                    foreach (nodeDataModel ndm in glm.nodeDataArray)
                    {
                        MonitorInfoModel er = new MonitorInfoModel();
                        bool iffound = LstNodeDataModel.ContainsKey(ndm.key.Trim());
                        //action 2 修改 3插入 4查询zookeeper 1 查询所有节点
                        er.action = iffound ? "2" : "3";
                        er.id = ndm.id;
                        er.loc = ndm.loc;
                        er.type = ndm.type;
                        er.serviceName = ndm.key.Trim();
                        er.size = ndm.size;
                        er.color = ndm.color;
                        er.figure = ndm.figure;
                        er.fill = ndm.fill;
                        er.thickness = ndm.thickness;
                        er.visible = ndm.visible;
                        er.isGroup = ndm.isGroup ? 1 : 0;
                        er.svrGroup = ndm.group;
                        PResult reply = await CommmitMonitorSvrinfo(er);

                        if (reply != null && reply.IsSuccess())
                        {
                            if (er.action == "3")
                            {
                                LstNodeDataModel.Add(ndm.key.Trim(), ndm);
                            }
                            else
                            {
                                LstNodeDataModel[ndm.key] = ndm;
                            }
                            cntSuccess++;
                        }
                        else
                        {
                            cntFail++;
                        }

                        cntRecord++;
                    }

                    //查询删除的记录
                    List<string> src = LstNodeDataModel.Keys.ToList();
                    List<string> tar = glm.nodeDataArray.Select(p => p.key).ToList();
                    List<string> exp = src.Except(tar).ToList();
                    if (exp.Count > 0)
                    {
                        for (int i = 0; i < exp.Count; i++)
                        {
                            if (LstNodeDataModel.ContainsKey(exp[i]))
                            {
                                nodeDataModel ndm = LstNodeDataModel[exp[i]];
                                MonitorInfoModel er = new MonitorInfoModel();
                                er.action = "2";
                                er.visible = 0;
                                er.id = ndm.id;
                                er.loc = ndm.loc;
                                er.type = ndm.type;
                                er.serviceName = ndm.key.Trim();
                                er.size = ndm.size;
                                er.color = ndm.color;
                                er.figure = ndm.figure;
                                er.fill = ndm.fill;
                                er.thickness = ndm.thickness;
                                 er.isGroup = ndm.isGroup ? 1 : 0;
                                er.svrGroup = ndm.group;
                                PResult reply = await CommmitMonitorSvrinfo(er);
                                if (reply != null && reply.IsSuccess())
                                {

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                { 
                
                }
                //((ManualInput)myWindow).ShowEMSMask(false);
                if ((cntSuccess == cntRecord) && cntRecord > 0)
                {
                    showSuccessMessage($"录入全部成功!\r\n成功录入{cntSuccess} 条！");
                }
                else
                {
                    if (cntRecord > 0)
                    {
                        CustomMessageBox.Error($"录入部分失败!\r\n成功录入{cntSuccess} 条!\r\n失败录入{cntFail} 条!", "提示", null);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                //((ManualInput)myWindow).ShowEMSMask(false);
                CustomMessageBox.Error($"录入失败因: {ex.Message}", "提示", null);
            }
            finally
            {
                resreply = $"保存成功！";
            }

            return resreply;
        }

        public async Task<PResult> CommmitMonitorSvrinfo(MonitorInfoModel req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = MonitorHandler.Instance.SetMonitorSvrinfo(req);
            });

            return reply;
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 读取数据库中保存的Node和布局样式
        /// </summary>
        /// <returns></returns>
        public string getGraphLinksModel()
        {
            LstNodeDataModel.Clear();
            MonitorHandler.Instance.UnSubscribeMonitorMsginfo(OnMonitorMsgInfoCallBack);

            string res = string.Empty;
            MonitorInfoReq req = new MonitorInfoReq();
            req.action = "1";
            MonitorInfoDetailResult reply = null;

            reply = MonitorHandler.Instance.QueryMonitorSvrinfo(req);
            if (reply != null)
            {
                if (reply.Code == "0000")
                {
                    if (reply.Datas != null)
                    {
                        SouceMonitorInfo = reply.Datas;
                        
                        //覆盖本地保存的json
                        string tmpres = JsonConvert.SerializeObject(SouceMonitorInfo);
                        JArray jobj = JArray.Parse(tmpres);
                        File.WriteAllText(LocalJsonFile, jobj.ToString(), Encoding.UTF8);
                        
                        res = ParseMonitorInfoModel(SouceMonitorInfo);
                    }
                }
                else
                {
                    res = getLocalGraphLinksModel();
                }
            }
            else
            {
                res = getLocalGraphLinksModel();
            }

            return res;
        }

        #region Open editor
        public void editModel()
        {
            string url = AppDomain.CurrentDomain.BaseDirectory + $"bin\\HTML\\Topo\\blockEditor.html?timestamp={new Random(10000).NextDouble()}";
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { Url = url, IsTopMost = false, IsShowAddressBar = true };
            cw.Key = "blockEditor";
            cw.Title = "拓扑监控编辑器";
            cw.ForceTargetSelf = true;
            cw.UseChromePluginObject = true;
            cw.CallBackObjectInstance.Add("cefJS|", "TopoViewModel");
            //使用sock5代理 直连为空 http代理:"127.0.0.1:8118" sock5代理: "socks5://127.0.0.1:1080"
            //cw.Proxy = "socks5://127.0.0.1:1080";
            OpenChromeInWindow(cw);
        }


        public void OpenChromeInWindow(ChromeWinModel cw)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                BorderWin myTradingPopupWin = new BorderWin();
                myTradingPopupWin.WindowState = cw.WindowState;
                myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                try
                {
                    ChromeUserControl chrom = new ChromeUserControl(cw);

                    chrom.SetTopBarVisible(cw.IsShowAddressBar);
                    chrom.SetStatusBarVisible(cw.IsShowStatusBar);
                    myTradingPopupWin.SetMainControl(chrom);
                    myTradingPopupWin.SetTitleAndKey(cw.Key, cw.Title);
                    myTradingPopupWin.Title = cw.Title;
                    chrom.SetParentWindow(myTradingPopupWin);
                    chrom.SetAddress(cw.Url);

                    myTradingPopupWin.Width = cw.Width;
                    myTradingPopupWin.Height = cw.Height;

                    if (cw.WindowState != System.Windows.WindowState.Maximized)
                    {
                        if ((cw.MaxHeight != 0) && (cw.MaxWidth != 0))
                        {
                            myTradingPopupWin.MaxHeight = cw.MaxHeight;
                            myTradingPopupWin.MaxWidth = cw.MaxWidth;
                        }
                    }

                    myTradingPopupWin.Owner = null;
                    if ((cw.PositionX != 0) && (cw.PositionY != 0))
                    {
                        myTradingPopupWin.Top = cw.PositionX;
                        myTradingPopupWin.Left = cw.PositionY;
                    }

                    WindowManager.Instance.OpenOnlyOneWindow($"{cw.Key}", myTradingPopupWin, null);
                }
                catch (Exception ex)
                {
                    LogHandler.Error(ex.Message);
                }
            }));
        }

        #endregion


        /// 读取本地保存的Node和布局样式
        /// </summary>
        /// <returns></returns>
        public string getLocalGraphLinksModel()
        {
            LstNodeDataModel.Clear();
            string res = string.Empty;
            res = LoadDataFormLocalJson();
            if (string.IsNullOrWhiteSpace(res))
            {
                res = getGraphLinksModel();
            }
            return res;
        }

        public List<MonitorInfoModel> SouceMonitorInfo = new List<MonitorInfoModel>();
        private string LoadDataFormLocalJson()
        {
            string res = string.Empty;
            if (File.Exists(LocalJsonFile))
            {
                string fncontent = File.ReadAllText(LocalJsonFile);
                SouceMonitorInfo = JsonConvert.DeserializeObject<List<MonitorInfoModel>>(fncontent);
                res = ParseMonitorInfoModel(SouceMonitorInfo);
            }

            return res;
        }

        public void SaveToLocalJson(List<nodeDataModel> nodeDataArray)
        {
            string res = string.Empty;
            foreach (nodeDataModel ndm in nodeDataArray)
            {
                MonitorInfoModel er = SouceMonitorInfo.FindLast(o => o.serviceName == ndm.key.ToString());
                if (er != null)
                {
                    er.id = ndm.id;
                    er.loc = ndm.loc;
                    er.type = ndm.type;
                    er.serviceName = ndm.key.Trim();
                    er.size = ndm.size;
                    er.color = ndm.color;
                    er.figure = ndm.figure;
                    er.fill = ndm.fill;
                    er.thickness = ndm.thickness;
                    er.visible = ndm.visible;
                    er.isGroup = ndm.isGroup ? 1 : 0;
                    er.svrGroup = ndm.group;
                }
                else
                {
                    MonitorInfoModel newer = new MonitorInfoModel();
                    newer.id = ndm.id;
                    newer.loc = ndm.loc;
                    newer.type = ndm.type;
                    newer.serviceName = ndm.key.Trim();
                    newer.size = ndm.size;
                    newer.color = ndm.color;
                    newer.figure = ndm.figure;
                    newer.fill = ndm.fill;
                    newer.thickness = ndm.thickness;
                    newer.visible = ndm.visible;
                    newer.isGroup = ndm.isGroup ? 1 : 0;
                    newer.svrGroup = ndm.group;
                    SouceMonitorInfo.Add(newer);
                }
            }

            res = JsonConvert.SerializeObject(SouceMonitorInfo);
            JArray jobj = JArray.Parse(res);
            File.WriteAllText(LocalJsonFile, jobj.ToString(), Encoding.UTF8);
        }


        /// <summary>
        /// 解析出前台UI可识别的Json
        /// </summary>
        /// <param name="resmim"></param>
        /// <returns></returns>
        public string ParseMonitorInfoModel(List<MonitorInfoModel> resmim)
        {
            List<nodeDataModel> nodeDataArray = new List<nodeDataModel>();
            //默认的 Node
            foreach (MonitorInfoModel mi in resmim)
            {
                if (mi.visible == 0 || string.IsNullOrEmpty(mi.serviceName))
                {
                    continue;
                }

                nodeDataModel ndm = new nodeDataModel();
                ndm.key = mi.serviceName;
                ndm.size = mi.size;
                ndm.loc = mi.loc;
                ndm.color = mi.color;
                ndm.figure = mi.figure;
                ndm.fill = mi.fill;
                ndm.thickness = mi.thickness;
                ndm.visible = mi.visible;
                ndm.type = mi.type;
                ndm.details = mi.host + ":" + mi.port;
                ndm.id = mi.id;
                ndm.group = string.IsNullOrEmpty(mi.svrGroup) ? "0" : mi.svrGroup;
                ndm.isGroup = mi.isGroup == 1? true :false;
                string cnname = string.IsNullOrEmpty(mi.cnServiceName) ? string.Empty : mi.cnServiceName;
                if (!ndm.isGroup)
                {
                    ndm.text = $"{cnname} {mi.serviceName}";
                }
                else
                {
                    ndm.text = $"{cnname}";
                }

                ndm.host = mi.host;
                ndm.port = mi.port;
                ndm.updatetime = mi.updatetime;

                nodeDataArray.Add(ndm);

                if (!LstNodeDataModel.ContainsKey(ndm.key))
                {
                    LstNodeDataModel.Add(ndm.key.ToString().Trim(),ndm);
                }
            }

            //默认连接线
            List<linkDataModel> linkDataArray = new List<linkDataModel>();
            //foreach (MonitorInfoModel mi in resmim)
            //{
            //    if (mi.visible == 0 || string.IsNullOrEmpty(mi.serviceName) || string.IsNullOrEmpty(mi.connectSvr))
            //    {
            //        continue;
            //    }

            //    List<string> conndesc = mi.connectSvr.Split(',').ToList();

            //    for (int i = 0; i < conndesc.Count; i++)
            //    {
            //        linkDataModel ldm = new linkDataModel();
            //        ldm.from = mi.serviceName;
            //        ldm.to = conndesc[i];
            //        linkDataArray.Add(ldm);
            //    }
            //}

            string json = JsonConvert.SerializeObject(nodeDataArray);
            json = json.Replace(",\"group\":\"0\"", "");

            string linkjson = JsonConvert.SerializeObject(linkDataArray);
            string res = "{\"class\": \"GraphLinksModel\"," + $"\"nodeDataArray\":{json},\"linkDataArray\":{linkjson}" + "}";
            //getLastZookeeper();
            MonitorHandler.Instance.SubscribeMonitorMsginfo(OnMonitorMsgInfoCallBack);
            return res;
        }



        #endregion

        #region test
        /// <summary>
        /// 通用异步函数，骆驼命名法
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<string> doTestTask(string code)
        {
            string reply = null;
            await Task.Run(() =>
            {
                //替换名称冲突的
                code = code.Replace("\"class\":\"GraphLinksModel\"", "\"Class\":\"GraphLinksModel\"");
                GraphLinksModels glm = JsonConvert.DeserializeObject<GraphLinksModels>(code);
                foreach (nodeDataModel ndm in glm.nodeDataArray)
                {

                }

                reply = $"保存成功！";
            });
            return reply;
        }
        public string getGraphLinksModelTest()
        {
            List<nodeDataModel> nodeDataArray = new List<nodeDataModel>();
            nodeDataArray.Add(new nodeDataModel() { key = "GW", text = "\u751f\u4ea7\u73af\u5883\u7f51\u5173(GW)", size = "120 50", color = "blue" });
            nodeDataArray.Add(new nodeDataModel() { key = "tbook", text = "\u4ea4\u6613\u64ae\u5408(tbook)", size = "120 50", color = "blue" });
            nodeDataArray.Add(new nodeDataModel() { key = "MD", text = "\u884c\u60c5\u6a21\u5757(MD)", size = "120 50", color = "blue" });
            nodeDataArray.Add(new nodeDataModel() { key = "mm", text = "\u91cf\u5316\u7b56\u7565\u6a21\u677f(Quant/QE)", size = "120 50", color = "blue" });
            nodeDataArray.Add(new nodeDataModel() { key = "Quant", text = "\u53cc\u8fb9\u505a\u5e02(mm)", size = "200 50", color = "blue" });

            List<linkDataModel> linkDataArray = new List<linkDataModel>();

            string json = JsonConvert.SerializeObject(nodeDataArray);
            string linkjson = JsonConvert.SerializeObject(linkDataArray);
            string res = "{\"class\": \"GraphLinksModel\"," + $"\"nodeDataArray\":{json},\"linkDataArray\":{linkjson}" + "}";

            return res;

            //{
            //    "class": "GraphLinksModel",
            //      "nodeDataArray": [
            //    { "key":"GW","loc":"500 -260","text":"\u751f\u4ea7\u73af\u5883\u7f51\u5173(GW)","size":"460 50","color":"blue","problem":"","status":0,"operation":1,"type":"S2", "thickness":1, "visible":1,"fill":"beige"},
            //    { "key":"tbook","loc":"200 -165","text":"\u4ea4\u6613\u64ae\u5408(tbook)","size":"120 50","group":7,"figure":"Rectangle","problem":"","status":1,"operation":2,"type":"S3", "thickness":1,"fill":"beige"},
            //    { "key":"MD","loc":"210 -15","text":"\u884c\u60c5\u6a21\u5757(MD)","size":"120 50","group":7,"figure":"Rectangle","fill":"lightyellow","problem":"","status":1,"operation":0,"type":"S3"},
            //    { "key":7,"isGroup":true,"text":"","color":"green","fill":"lightyellow","loc":"500 -55","problem":"","status":1,"operation":1,"type":"S3"},
            //    { "key":"mm","loc":"420 -165","text":"\u53cc\u8fb9\u505a\u5e02(mm)","size":"120 50","group":7,"figure":"Rectangle","problem":"","status":2,"operation":2, "type":"S3"},
            //    { "key":"Quant","loc":"650 -165","text":"\u91cf\u5316\u7b56\u7565\u6a21\u677f(Quant/QE)","group":7,"figure":"Rectangle","problem":"","status":2,"operation":1,"size":"200 50", "type":"S3"},
            //    { "key":"Risk","loc":"400 25","text":"\u98ce\u63a7\u6a21\u5757(Risk)","size":"120 50","group":7,"figure":"Rectangle","fill":"lightyellow","problem":"","status":1,"operation":0, "type":"S3"},
            //    { "key":"EMS","loc":"590 -75","text":"\u6267\u884c\u6a21\u5757(EMS)","group":7,"figure":"Rectangle","fill":"lightyellow","problem":"","status":2,"operation":0,"size":"120 50", "type":"S3"},
            //    { "key":"UFX","loc":"830 55","text":"UFX","size":"60 50","group":7,"figure":"Rectangle","fill":"lightyellow","problem":"","status":0,"operation":0, "type":"S3"},
            //    { "key":"Position","loc":"600 30","text":"\u5934\u5bf8\u6a21\u5757(Position)","size":"120 50","group":7,"figure":"Rectangle","fill":"lightyellow","problem":"","status":1,"operation":2, "type":"S3"},
            //    { "key":-16,"isGroup":true,"text":"","color":"green","fill":"beige","loc":"500 190","problem":"","status":2,"operation":1, "type":"S3"},
            //    { "key":"ODM","loc":"267.5 165","text":" XBond\u6a21\u5757(ODM)","details":"","problem":"","status":1,"operation":1,"size":"120 50","group":-16, "type":"S3"},
            //    { "key":"QDMESP","loc":"480 160","text":" \u53cc\u8fb9\u6a21\u5757(QDMESP)","details":"","problem":"","status":2,"operation":0,"size":"120 50","group":-16, "type":"S3"},
            //    { "key":"NDM","loc":"797.5 220","text":"NDM\u6a21\u5757","details":"","problem":"","status":1,"operation":0,"size":"120 50","group":-16, "type":"S3", "visible":0},
            //    { "key":"CTP","loc":"617.5 220","text":"CTP\u6a21\u5757","details":"","problem":"","status":2,"operation":0,"size":"120 50","group":-16, "type":"S3"},
            //    { "key":"CDH","loc":"202.5 220","text":"CDH","details":"","problem":"","status":1,"operation":0,"size":"120 50","group":-16, "type":"S3"},
            //    { "key":"CMDS","loc":"622.5 160","text":"CMDS\u6210\u4ea4\u6a21\u5757","details":"","problem":"","status":2,"operation":1,"size":"120 50","group":-16, "type":"S3"},
            //    { "key":"RDI","loc":"792.5 160","text":"RDI\u6a21\u5757(RDI)","details":"","problem":"","status":2,"operation":0,"size":"120 50","group":-16, "type":"S3"}
            //    ],
            //     "linkDataArray": [
            //    { "from":"MD","to":"Risk","problem":"","dash":[2,4],"fromSpot":"RightSide"},
            //    { "from":"EMS","to":"UFX","problem":"","dash":[2,4],"fromSpot":"BottomSide"},
            //    { "from":"EMS","to":"mm","problem":"","dash":[2,4],"fromSpot":"LeftSide"},
            //    { "from":"EMS","to":"Position","problem":"","dash":[2,4],"fromSpot":"BottomSide"},
            //    { "from":"mm","to":"Risk","problem":"","dash":[2,4],"dir":2,"fromSpot":"BottomSide"},
            //    { "from":"tbook","to":"Risk","problem":"","dash":[2,4],"fromSpot":"BottomSide"},
            //    { "from":"MD","to":"tbook","dash":[2,4],"problem":"","fromSpot":"TopSide"},
            //    { "from":"ODM","to":"MD","dash":[2,4],"problem":"","fromSpot":"TopSide"},
            //    { "from":"EMS","to":"ODM","problem":"","dash":[2,4],"fromSpot":"BottomSide"},
            //    { "from":"EMS","to":"Risk","problem":"","dash":[2,4],"dir":2, "fromSpot":"LeftSide"},
            //    { "from":"Position","to":"Risk","problem":"","dash":[2,4], "fromSpot":"LeftSide"},
            //    { "from":"QDMESP","to":"MD","problem":"","dash":[2,4],"fromSpot":"TopSide"},
            //    { "from":"EMS","to":"QDMESP","problem":"","dash":[2,4],"dir":2,"fromSpot":"BottomSide"},
            //    { "from":"GW","to":"7","problem":"","dir":2,"fromSpot":"BottomSide"}
            //    ]}
        }
        #endregion

        #region 提示
        /// <summary>
        /// 提示Warn消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.ShowDialog();
            }));
        }

        /// <summary>
        /// 提示成功消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showSuccessMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                SuccessBoxView me = new SuccessBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.ShowDialog();
            }));
        }

        #endregion
    }
}
