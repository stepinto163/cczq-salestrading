using Newtonsoft.Json;
using STC.Common.Data.DataModel;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.ViewModel.Setting
{
    public class InnerSettingViewModel : ViewModelBase
    {
        public static Action InnerCopyInfoChange;

        #region 属性
        private ObservableCollection<CopyColumnItem> _CommonList = new ObservableCollection<CopyColumnItem>();

        public ObservableCollection<CopyColumnItem> CommonList
        {
            get { return _CommonList; }
            set
            {
                _CommonList = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<CopyColumnItem> _CreditList = new ObservableCollection<CopyColumnItem>();

        public ObservableCollection<CopyColumnItem> CreditList
        {
            get { return _CreditList; }
            set
            {
                _CreditList = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<CopyColumnItem> _RateList = new ObservableCollection<CopyColumnItem>();

        public ObservableCollection<CopyColumnItem> RateList
        {
            get { return _RateList; }
            set
            {
                _RateList = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsSpaceChk;

        public bool IsSpaceChk
        {
            get { return _IsSpaceChk; }
            set
            {
                _IsSpaceChk = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsLongSpaceChk = true;

        public bool IsLongSpaceChk
        {
            get { return _IsLongSpaceChk; }
            set
            {
                _IsLongSpaceChk = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsTagChk;

        public bool IsTagChk
        {
            get { return _IsTagChk; }
            set
            {
                _IsTagChk = value;
                this.OnPropertyChanged();
            }
        }

        private string _bp1 = "0.25";

        public string BP1
        {
            get { return _bp1; }
            set
            {
                _bp1 = value;
                this.OnPropertyChanged();
            }
        }

        private string _bp2 = "0.5";

        public string BP2
        {
            get { return _bp2; }
            set
            {
                _bp2 = value;
                this.OnPropertyChanged();
            }
        }

        private string _bp3 = "1";

        public string BP3
        {
            get { return _bp3; }
            set
            {
                _bp3 = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 内部最优
        /// </summary>
        private bool _IsInnerCopyModeChk = true;

        public bool IsInnerCopyModeChk
        {
            get { return _IsInnerCopyModeChk; }
            set
            {
                _IsInnerCopyModeChk = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 经纪商行情
        /// </summary>
        private bool _IsBrokerCopyModeChk;

        public bool IsBrokerCopyModeChk
        {
            get { return _IsBrokerCopyModeChk; }
            set
            {
                _IsBrokerCopyModeChk = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 无价量
        /// </summary>
        private bool _IsNoneCopyModeChk;

        public bool IsNoneCopyModeChk
        {
            get { return _IsNoneCopyModeChk; }
            set
            {
                _IsNoneCopyModeChk = value;
                this.OnPropertyChanged();
            }
        }

        private string _recommendation = "今日好价，欢迎咨询";

        public string Recommendation
        {
            get { return _recommendation; }
            set
            {
                _recommendation = value;
                this.CurrentRecommendationLength = value.Length;
                this.OnPropertyChanged();
            }
        }

        private Visibility _creditratevisible = Visibility.Visible;
        /// <summary>
        /// 信用利率配置是否可见
        /// </summary>
        public Visibility CreditRateVisible
        {
            get { return _creditratevisible; }
            set
            {
                _creditratevisible = value;
                this.OnPropertyChanged();
            }
        }

        private string _rateischecked;
        /// <summary>
        /// 利率
        /// </summary>
        public string RateIsChecked
        {
            get { return _rateischecked; }
            set
            {
                _rateischecked = value;
                this.OnPropertyChanged();
            }
        }


        private bool rateNoRecovery;
        /// <summary>
        /// 利率 无需恢复
        /// </summary>
        public bool RateNoRecovery
        {
            get { return rateNoRecovery; }
            set
            {
                rateNoRecovery = value;
                if (value)
                {
                    RateIsChecked = "0";
                }

                this.OnPropertyChanged();
            }
        }

        private bool rateManualRecovery;
        /// <summary>
        /// 利率 手动恢复
        /// </summary>
        public bool RateManualRecovery
        {
            get { return rateManualRecovery; }
            set
            {
                rateManualRecovery = value;
                if (value)
                {
                    RateIsChecked = "1";
                }

                this.OnPropertyChanged();
            }
        }


        private bool rateAutoRecovery;
        /// <summary>
        /// 利率 自动恢复
        /// </summary>
        public bool RateAutoRecovery
        {
            get { return rateAutoRecovery; }
            set
            {
                rateAutoRecovery = value;
                if (value)
                {
                    RateIsChecked = "2";
                }

                this.OnPropertyChanged();
            }
        }


        private bool creditNoRecovery;
        /// <summary>
        /// 信用 无需恢复
        /// </summary>
        public bool CreditNoRecovery
        {
            get { return creditNoRecovery; }
            set
            {
                creditNoRecovery = value;
                if (value)
                {
                    CreditIsChecked = "0";
                }
                this.OnPropertyChanged();
            }
        }


        private bool creditManualRecovery;
        /// <summary>
        /// 信用 手动恢复
        /// </summary>
        public bool CreditManualRecovery
        {
            get { return creditManualRecovery; }
            set
            {
                creditManualRecovery = value;
                if (value)
                {
                    CreditIsChecked = "1";
                }
                this.OnPropertyChanged();
            }
        }

        private bool creditAutoRecovery;

        /// <summary>
        /// 信用 自动恢复
        /// </summary>
        public bool CreditAutoRecovery
        {
            get { return creditAutoRecovery; }
            set
            {
                creditAutoRecovery = value;
                if (value)
                {
                    CreditIsChecked = "2";
                }
                this.OnPropertyChanged();
            }
        }

        private string _creditischecked;
        /// <summary>
        /// 信用
        /// </summary>
        public string CreditIsChecked
        {
            get { return _creditischecked; }
            set
            {
                _creditischecked = value;
                this.OnPropertyChanged();
            }
        }

        private bool _INNERIsChecked;

        public bool INNERIsChecked
        {
            get { return _INNERIsChecked; }
            set
            {
                _INNERIsChecked = value;
                this.OnPropertyChanged();
            }
        }

        private bool _BrokerIsChecked;

        public bool BrokerIsChecked
        {
            get { return _BrokerIsChecked; }
            set
            {
                _BrokerIsChecked = value;
                this.OnPropertyChanged();
            }
        }

        private int _currentRecommendationLength = 0;

        public int CurrentRecommendationLength
        {
            get { return _currentRecommendationLength; }
            set
            {
                _currentRecommendationLength = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsAutoSendQQ;

        public bool IsAutoSendQQ
        {
            get { return _IsAutoSendQQ; }
            set
            {
                _IsAutoSendQQ = value;
                this.OnPropertyChanged();
            }
        }

        private string _IsCreditDPChk = "1";
        public string IsCreditDPChk
        {
            get { return _IsCreditDPChk; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _IsCreditDPChk = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _IsRateDPChk = "2";
        public string IsRateDPChk
        {
            get { return _IsRateDPChk; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _IsRateDPChk = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        public InnerSettingViewModel()
        {
            Init();
            queryGlobalPara();
            QueryUnRefSwitch();
            QuerySubMD();
            //QueryAutoSendQQParam();
            QueryDecimalPlaces();
        }

        private void Init()
        {
            this.CommonList.Add(new CopyColumnItem() { Name = "按剩余期限排序" });

            this.RateList.Add(new CopyColumnItem() { Name = "票面", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "中债估值", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "中证估值" });
            this.RateList.Add(new CopyColumnItem() { Name = "到期日" });
            this.RateList.Add(new CopyColumnItem() { Name = "中债久期" });
            this.RateList.Add(new CopyColumnItem() { Name = "债券余额" });
            this.RateList.Add(new CopyColumnItem() { Name = "发行量" });
            this.RateList.Add(new CopyColumnItem() { Name = "是否提还" });
            this.RateList.Add(new CopyColumnItem() { Name = "是否免税" });

            this.CreditList.Add(new CopyColumnItem() { Name = "票面", IsChecked = true });
            this.CreditList.Add(new CopyColumnItem() { Name = "评级" });
            this.CreditList.Add(new CopyColumnItem() { Name = "中债隐含评级" });
            this.CreditList.Add(new CopyColumnItem() { Name = "担保方" });
            this.CreditList.Add(new CopyColumnItem() { Name = "中债估值", IsChecked = true });
            this.CreditList.Add(new CopyColumnItem() { Name = "中证估值" });
            this.CreditList.Add(new CopyColumnItem() { Name = "募集类型" });
            this.CreditList.Add(new CopyColumnItem() { Name = "企业类型" });
            this.CreditList.Add(new CopyColumnItem() { Name = "地区", IsChecked = true });
            this.CreditList.Add(new CopyColumnItem() { Name = "城投级别" });
            this.CreditList.Add(new CopyColumnItem() { Name = "发行人" });
            this.CreditList.Add(new CopyColumnItem() { Name = "债券余额" });
            this.CreditList.Add(new CopyColumnItem() { Name = "发行量" });
            this.CreditList.Add(new CopyColumnItem() { Name = "到期日" });
            this.CreditList.Add(new CopyColumnItem() { Name = "是否免税" });

            //作废区查询权限
            bool rootRight = AdminHandler.Instance.CheckPermisionById("5000", "5004");
            bool noRight = AdminHandler.Instance.CheckPermission("5004", "500401");
            CreditRateVisible = rootRight ? noRight ? Visibility.Collapsed : Visibility.Visible : Visibility.Collapsed;

        }


        /// <summary>
        /// 查询
        /// </summary>
        private void queryGlobalPara()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                SystemParameterReq req = new SystemParameterReq();
                req.ParamKey = CommonUtils.PARAM_KEY_COPY;
                req.Username = LocalEnv.UserInfo.UserName;
                TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                    {
                        CommonUtils.CopyInfoJson = reply.Datas.tbookSystemParameter.param_value;

                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            foreach (KeyValuePair<string, IList<CopyColumnItem>> entry in CommonUtils.CopyDic)
                            {
                                if (entry.Key.ToUpper() == "COMMON" && !CommonUtils.isOnlyCredit)
                                {
                                    foreach (CopyColumnItem item in entry.Value)
                                    {
                                        var list = CommonList.Where(i => i.Name == item.Name);
                                        foreach (CopyColumnItem findItem in list)
                                        {
                                            findItem.IsChecked = item.IsChecked;
                                        }
                                    }
                                }
                                else if (entry.Key.ToUpper() == "CREDIT" && !CommonUtils.isOnlyRate)
                                {
                                    foreach (CopyColumnItem item in entry.Value)
                                    {
                                        var list = CreditList.Where(i => i.Name == item.Name);
                                        foreach (CopyColumnItem findItem in list)
                                        {
                                            findItem.IsChecked = item.IsChecked;
                                        }
                                    }
                                }
                                else if (entry.Key.ToUpper() == "RATE")
                                {
                                    foreach (CopyColumnItem item in entry.Value)
                                    {
                                        var list = RateList.Where(i => i.Name == item.Name);
                                        foreach (CopyColumnItem findItem in list)
                                        {
                                            findItem.IsChecked = item.IsChecked;
                                        }
                                    }
                                }
                                else if (entry.Key.ToUpper() == "TAB")
                                {
                                    foreach (CopyColumnItem item in entry.Value)
                                    {
                                        if (item.Name == "空格")
                                        {
                                            IsSpaceChk = true;
                                        }
                                        else if (item.Name == "长空格")
                                        {
                                            IsLongSpaceChk = true;
                                        }
                                        else if (item.Name == "Tab制表符")
                                        {
                                            IsTagChk = true;
                                        }
                                    }
                                }
                                else if (entry.Key.ToUpper() == "BP")
                                {
                                    foreach (CopyColumnItem item in entry.Value)
                                    {
                                        if (item.Name == "BP1")
                                        {
                                            this.BP1 = item.Value;
                                        }
                                        else if (item.Name == "BP2")
                                        {
                                            this.BP2 = item.Value;
                                        }
                                        else if (item.Name == "BP3")
                                        {
                                            this.BP3 = item.Value;
                                        }
                                    }
                                }
                                else if (entry.Key.ToUpper() == "SUGGEST")
                                {
                                    foreach (CopyColumnItem item in entry.Value)
                                    {
                                        if (item.Name == "TEXT")
                                        {
                                            this.Recommendation = item.Value;
                                        }
                                    }
                                }
                                else if (entry.Key.ToUpper() == "COPYMODE")
                                {
                                    foreach (CopyColumnItem item in entry.Value)
                                    {
                                        if (item.Value == "2")
                                        {
                                            this.IsBrokerCopyModeChk = true;
                                        }
                                        else if (item.Value == "3")
                                        {
                                            this.IsNoneCopyModeChk = true;
                                        }
                                    }
                                }
                            }
                        }));
                    }
                }
            });
        }

        public void SaveParam()
        {
            CommonUtils.CopyDic["COMMON"] = this.CommonList;
            CommonUtils.CopyDic["CREDIT"] = this.CreditList;
            CommonUtils.CopyDic["RATE"] = this.RateList;
            CommonUtils.CopyDic["TAB"] = new List<CopyColumnItem>();
            if (IsSpaceChk)
            {
                CommonUtils.CopyDic["TAB"].Add(new CopyColumnItem { Name = "空格", IsChecked = true });
            }
            else if (IsTagChk)
            {
                CommonUtils.CopyDic["TAB"].Add(new CopyColumnItem { Name = "Tab制表符", IsChecked = true });
            }
            else
            {
                CommonUtils.CopyDic["TAB"].Add(new CopyColumnItem { Name = "长空格", IsChecked = true });
            }

            CommonUtils.CopyDic["BP"] = new List<CopyColumnItem>();
            CommonUtils.CopyDic["BP"].Add(new CopyColumnItem { Name = "BP1", Value = this.BP1 });
            CommonUtils.CopyDic["BP"].Add(new CopyColumnItem { Name = "BP2", Value = this.BP2 });
            CommonUtils.CopyDic["BP"].Add(new CopyColumnItem { Name = "BP3", Value = this.BP3 });

            CommonUtils.CopyDic["SUGGEST"] = new List<CopyColumnItem>();
            CommonUtils.CopyDic["SUGGEST"].Add(new CopyColumnItem { Name = "TEXT", Value = this.Recommendation });

            string copyMode = "1";
            if (!CommonUtils.CopyDic.ContainsKey("COPYMODE"))
            {
                CommonUtils.CopyDic["COPYMODE"] = new List<CopyColumnItem>();
            }
            else
            {
                CommonUtils.CopyDic["COPYMODE"].Clear();
            }
            if (IsBrokerCopyModeChk)
            {
                copyMode = "2";

                CommonUtils.CopyDic["COPYMODE"].Add(new CopyColumnItem { Name = string.Empty, Value = copyMode });
            }
            else if (IsNoneCopyModeChk)
            {
                copyMode = "3";

                CommonUtils.CopyDic["COPYMODE"].Add(new CopyColumnItem { Name = string.Empty, Value = copyMode });
            }

            CommonUtils.CopyInfoJson = JsonConvert.SerializeObject(CommonUtils.CopyDic);

            // CopyInfoJson
            SystemGlobalParamReq req = new SystemGlobalParamReq();
            req.action = "2";
            req.list = new List<TTbookSystemParameter>();
            TTbookSystemParameter param = new TTbookSystemParameter();
            param.param_key = CommonUtils.PARAM_KEY_COPY;
            param.username = LocalEnv.UserInfo.UserName;
            param.param_value = CommonUtils.CopyInfoJson;
            param.param_desc = "内部看板复制配置";
            req.list.Add(param);

            PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
            if (reply != null)
            {
                if (reply.Code == "0000")
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg("保存成功", true);
                        msgBox.ShowDialog();

                        InnerCopyInfoChange?.Invoke();
                    }));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg(string.Format($"更新失败, 原因:{reply.Msg}."), true);
                        msgBox.ShowDialog();
                    }));
                }
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    MessageBoxGUI msgBox = new MessageBoxGUI();
                    msgBox.setWarnMsg(string.Format($"系统异常"), true);
                    msgBox.ShowDialog();
                }));
            }

            //日初订单恢复提醒个人系统设置更新
            UpdateUnRefSwitch();
            //更新订阅范围
            UpdateSubMDParam();

            //保存自动发送QQ配置
            //UpdateAutoSendQQParam();

            UpdateDecimalPlaces();
        }

        /// <summary>
        /// 查询日初订单恢复提醒个人系统设置
        /// </summary>
        private void QueryUnRefSwitch()
        {
            //QueryUnrefParamReplyPo QueryUnrefParam(QueryUnrefParamReq request)
            ThreadPool.QueueUserWorkItem(o =>
            {
                QueryUnrefParamReq req = new QueryUnrefParamReq();
                req.param_key = "STC_InnerBoard_Order_UnRef";
                req.username = LocalEnv.UserInfo.UserName;

                QueryUnrefParamReplyPo reply = TableHandler.Instance.QueryUnrefParam(req);
                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            TTbookSystemParameter item = reply.Datas;
                            Dictionary<string, List<UpdateUnrefParamModel>> dict = JsonConvert.DeserializeObject<Dictionary<string, List<UpdateUnrefParamModel>>>(item.param_value);
                            if (dict.ContainsKey("CREDIT"))
                            {
                                List<UpdateUnrefParamModel> lstCREDIT = dict["CREDIT"];
                                if (lstCREDIT.Count > 0)
                                {
                                    string tmpsw = string.Empty;
                                    if (!string.IsNullOrEmpty(lstCREDIT[0].IsChecked))
                                    {
                                        tmpsw = lstCREDIT[0].IsChecked.ToString();
                                    }

                                    switch (tmpsw.ToLower())
                                    {
                                        case "false":
                                        case "0":
                                            CreditNoRecovery = true;
                                            break;
                                        case "true":
                                        case "1":
                                            CreditManualRecovery = true;
                                            break;
                                        case "2":
                                            CreditAutoRecovery = true;
                                            break;
                                    }
                                }
                            }
                            if (dict.ContainsKey("CREDIT"))
                            {
                                List<UpdateUnrefParamModel> lstRATE = dict["RATE"];
                                if (lstRATE.Count > 0)
                                {
                                    string tmpsw = string.Empty;

                                    if (!string.IsNullOrEmpty(lstRATE[0].IsChecked))
                                        tmpsw = lstRATE[0].IsChecked.ToString();

                                    switch (tmpsw.ToLower())
                                    {
                                        case "false":
                                        case "0":
                                            RateNoRecovery = true;
                                            break;
                                        case "true":
                                        case "1":
                                            RateManualRecovery = true;
                                            break;
                                        case "2":
                                            RateAutoRecovery = true;
                                            break;
                                    }
                                }
                            }
                        }));
                    }
                }
            });

        }

        /// <summary>
        /// 日初订单恢复提醒个人系统设置更新
        /// </summary>
        public void UpdateUnRefSwitch()
        {
            UpdateUnrefParamReq req = new UpdateUnrefParamReq();
            req.param_key = "STC_InnerBoard_Order_UnRef";
            req.param_desc = "日初订单恢复提醒";
            req.username = LocalEnv.UserInfo.UserName;
            Dictionary<string, List<UpdateUnrefParamModel>> dict = new Dictionary<string, List<UpdateUnrefParamModel>>();
            UpdateUnrefParamModel creditmodel = new UpdateUnrefParamModel() { Name = "8:00提醒恢复", IsChecked = CreditIsChecked };
            dict.Add("CREDIT", new List<UpdateUnrefParamModel>() { creditmodel });
            UpdateUnrefParamModel ratemodel = new UpdateUnrefParamModel() { Name = "8:00提醒恢复", IsChecked = RateIsChecked };
            dict.Add("RATE", new List<UpdateUnrefParamModel>() { ratemodel });
            req.param_value = JsonConvert.SerializeObject(dict);

            PResult reply = TableHandler.Instance.UpdateUnrefParam(req);
            if (reply != null && reply.IsSuccess())
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    MessageBoxGUI msgBox = new MessageBoxGUI();
                //    msgBox.setWarnMsg("保存日初订单恢复提醒参数成功", true);
                //    msgBox.ShowDialog();

                //    InnerCopyInfoChange?.Invoke();
                //}));
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    string error = reply == null ? string.Empty : $", 原因:{ reply.Msg}";
                    MessageBoxGUI msgBox = new MessageBoxGUI();
                    msgBox.setWarnMsg(string.Format($"更新日初订单恢复提醒参数失败{error}."), true);
                    msgBox.ShowDialog();
                }));
            }
        }

        /// <summary>
        /// 查询订阅范围
        /// </summary>
        private void QuerySubMD()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                QueryUnrefParamReq req = new QueryUnrefParamReq();
                req.param_key = "STC_InnerBoard_Sub_MD";
                req.username = LocalEnv.UserInfo.UserName;

                QueryUnrefParamReplyPo reply = TableHandler.Instance.QueryUnrefParam(req);
                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            TTbookSystemParameter item = reply.Datas;
                            Dictionary<string, bool> dict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(item.param_value);
                            if (dict.ContainsKey("INNER"))
                            {
                                bool lstCREDIT = dict["INNER"];
                                INNERIsChecked = lstCREDIT;
                            }
                            if (dict.ContainsKey("CDH_MD"))
                            {
                                bool lstCREDIT = dict["CDH_MD"];
                                BrokerIsChecked = lstCREDIT;
                            }
                        }));
                    }
                }
            });

        }
        /// <summary>
        /// 更新订阅范围
        /// </summary>
        public void UpdateSubMDParam()
        {
            UpdateUnrefParamReq req = new UpdateUnrefParamReq();
            req.param_key = "STC_InnerBoard_Sub_MD";
            req.param_desc = "订阅范围";
            req.username = LocalEnv.UserInfo.UserName;
            Dictionary<string, bool> dict = new Dictionary<string, bool>();
            dict.Add("INNER", INNERIsChecked);
            dict.Add("CDH_MD", BrokerIsChecked);
            req.param_value = JsonConvert.SerializeObject(dict);

            PResult reply = TableHandler.Instance.UpdateUnrefParam(req);
            if (reply.Code == "0000")
            {
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    MessageBoxGUI msgBox = new MessageBoxGUI();
                    msgBox.setWarnMsg(string.Format($"更新订阅范围失败, 原因:{reply.Msg}."), true);
                    msgBox.ShowDialog();
                }));
            }
        }

        //private void QueryAutoSendQQParam()
        //{
        //    ThreadPoolHelper.InvokeThreadPool(o => {
        //        CommonUtils.QueryAutoSendQQParam();
        //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //        {
        //            IsAutoSendQQ = CommonUtils.AutoSendQQParam == "1";
        //        }));
        //    });
        //}

        //public void UpdateAutoSendQQParam()
        //{
        //    UpdateUnrefParamReq req = new UpdateUnrefParamReq();
        //    req.param_key = "STC_AutoSendQQParam";
        //    req.param_desc = "双击自动发送QQ信息";
        //    req.username = LocalEnv.UserInfo.UserName;
        //    req.param_value = this.IsAutoSendQQ ? "1" : "0";

        //    PResult reply = TableHandler.Instance.UpdateUnrefParam(req);
        //    if (reply.Code == "0000")
        //    {
        //    }
        //    else
        //    {
        //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //        {
        //            MessageBoxGUI msgBox = new MessageBoxGUI();
        //            msgBox.setWarnMsg(string.Format($"更新双击自动发送QQ消息设置失败, 原因:{reply.Msg}."), true);
        //            msgBox.ShowDialog();
        //        }));
        //    }
        //}


        /// <summary>
        /// 查询小数位数系统设置
        /// </summary>
        private void QueryDecimalPlaces()
        {
            //QueryUnrefParamReplyPo QueryUnrefParam(QueryUnrefParamReq request)
            ThreadPool.QueueUserWorkItem(o =>
            {
                SystemParameterReq req = new SystemParameterReq();
                req.ParamKey = "STC_Intention_DecimalPlaces";
                req.Username = LocalEnv.UserInfo.UserName;
                TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);

                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                            {
                                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(reply.Datas.tbookSystemParameter.param_value);
                                if (dict.ContainsKey("CREDIT"))
                                {
                                    IsCreditDPChk = STC.Common.Utils.CommonUtils.CreditDP = string.IsNullOrEmpty(dict["CREDIT"]) ? "1" : dict["CREDIT"];

                                }
                                if (dict.ContainsKey("RATE"))
                                {
                                    IsRateDPChk = STC.Common.Utils.CommonUtils.RateDP = string.IsNullOrEmpty(dict["RATE"]) ? "1" : dict["RATE"];
                                }
                            }
                        }));
                    }
                }
            });

        }

        /// <summary>
        /// 小数位数系统系统设置更新
        /// </summary>
        public void UpdateDecimalPlaces()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["CREDIT"] = IsCreditDPChk;
            dic["RATE"] = IsRateDPChk;

            SystemGlobalParamReq req = new SystemGlobalParamReq();
            req.action = "2";
            req.list = new List<TTbookSystemParameter>();
            TTbookSystemParameter param = new TTbookSystemParameter();
            param.param_key = "STC_Intention_DecimalPlaces";
            param.username = LocalEnv.UserInfo.UserName;
            param.param_value = JsonConvert.SerializeObject(dic);
            param.param_desc = "批量识别报价精度";
            req.list.Add(param);

            PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
            if (reply.Code == "0000")
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    //MessageBoxGUI msgBox = new MessageBoxGUI();
                    //msgBox.setWarnMsg("小数位数系统参数成功", true);
                    //msgBox.ShowDialog();

                    //InnerCopyInfoChange?.Invoke();
                    STC.Common.Utils.CommonUtils.CreditDP = IsCreditDPChk;
                    STC.Common.Utils.CommonUtils.RateDP = IsRateDPChk;
                }));
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    MessageBoxGUI msgBox = new MessageBoxGUI();
                    msgBox.setWarnMsg(string.Format($"更新批量识别报价精度参数失败, 原因:{reply.Msg}."), true);
                    msgBox.ShowDialog();
                }));
            }
        }
    }

    public class UpdateUnrefParamModel
    {
        public string Name;//参数key		

        public string IsChecked;//用户名		
    }

}
