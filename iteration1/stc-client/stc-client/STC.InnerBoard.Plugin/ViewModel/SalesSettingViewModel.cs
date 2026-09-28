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
using STC.Common.ViewModel.Setting;
using STC.InnerBoard.Plugin.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.InnerBoard.Plugin.ViewModel
{
    public class SalesSettingViewModel : ViewModelBase
    {
        public static event Action DefaultCopyInfoChange;

        #region 金融Q
        private string _isInnerGroupChatType_FQ = "1";

        public string IsInnerGroupChatType_FQ
        {
            get { return _isInnerGroupChatType_FQ; }
            set
            {
                _isInnerGroupChatType_FQ = value;
                this.OnPropertyChanged();
            }
        }

        private string _isGroupChatType_FQ; //= "0";

        public string IsGroupChatType_FQ
        {
            get { return _isGroupChatType_FQ; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _isGroupChatType_FQ = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _isPrivateChatType_FQ; //= "2";

        public string IsPrivateChatType_FQ
        {
            get { return _isPrivateChatType_FQ; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _isPrivateChatType_FQ = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _isAutoType_FQ = "1";

        public string IsAutoType_FQ
        {
            get { return _isAutoType_FQ; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _isAutoType_FQ = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isChecked_FQ;

        public bool IsChecked_FQ
        {
            get { return _isChecked_FQ; }
            set
            {
                _isChecked_FQ = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _bpValue_FQ;

        public decimal? BpValue_FQ
        {
            get { return _bpValue_FQ; }
            set
            {
                _bpValue_FQ = value;
                this.OnPropertyChanged();
            }
        }

        private string _if_Intention_WhiteList = "1"; 

        public string If_Intention_WhiteList
        {
            get { return _if_Intention_WhiteList; }
            set
            {
                _if_Intention_WhiteList = value;
                this.OnPropertyChanged();
            }
        }

        private string _if_Auto_Insert = "0";

        public string If_Auto_Insert
        {
            get { return _if_Auto_Insert; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _if_Auto_Insert = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isChecked_Broker_Spread;

        public bool IsChecked_Broker_Spread
        {
            get { return _isChecked_Broker_Spread; }
            set
            {
                _isChecked_Broker_Spread = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _broker_Spread;

        public decimal? Broker_Spread
        {
            get { return _broker_Spread; }
            set
            {
                _broker_Spread = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region 企业微信
        private string _isAutoType_WX = "1";

        public string IsAutoType_WX
        {
            get { return _isAutoType_WX; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _isAutoType_WX = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isChecked_WX;

        public bool IsChecked_WX
        {
            get { return _isChecked_WX; }
            set
            {
                _isChecked_WX = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _bpValue_WX;

        public decimal? BpValue_WX
        {
            get { return _bpValue_WX; }
            set
            {
                _bpValue_WX = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region 对客端
        private string _isAutoType = "1";

        public string IsAutoType
        {
            get { return _isAutoType; }
            set
            {
                _isAutoType = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _bpValue;

        public decimal? BpValue
        {
            get { return _bpValue; }
            set
            {
                _bpValue = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region 事件
        public ICommand ListManagementCommand { get { return new ComRelayCommand(ListManagementClick); } }
        public volatile bool IsEdit;//{ get; set; }
        public bool IsPassChange { get; set; }
        #endregion

        #region 复制
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

        ///// <summary>
        ///// 经纪商行情
        ///// </summary>
        //private bool _IsBrokerCopyModeChk;

        //public bool IsBrokerCopyModeChk
        //{
        //    get { return _IsBrokerCopyModeChk; }
        //    set
        //    {
        //        _IsBrokerCopyModeChk = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        ///// <summary>
        ///// 无价量
        ///// </summary>
        //private bool _IsNoneCopyModeChk;

        //public bool IsNoneCopyModeChk
        //{
        //    get { return _IsNoneCopyModeChk; }
        //    set
        //    {
        //        _IsNoneCopyModeChk = value;
        //        this.OnPropertyChanged();
        //    }
        //}

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
        #endregion

        private void ListManagementClick()
        {
            ListManagementUI addGroupWindow = new ListManagementUI();
            addGroupWindow.Owner = Application.Current.MainWindow;
            addGroupWindow.ShowDialog();
        }

        public SalesSettingViewModel()
        {
            bool isIntentionPermission = AdminHandler.Instance.CheckPermisionById("4000", "4008");
            bool isPermission = AdminHandler.Instance.CheckPermisionById("4000", "4013");
            if (isPermission == false && isIntentionPermission)
            {
                Init();
                queryGlobalPara();
            }
            QueryParam();
        }
        private void Init()
        {
            this.CommonList.Add(new CopyColumnItem() { Name = "按剩余期限排序" });

            this.RateList.Add(new CopyColumnItem() { Name = "方向", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "剩余期限", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "债券代码", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "债券简称", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "报量", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "报价", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "票面", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "中债估值", IsChecked = true });
            this.RateList.Add(new CopyColumnItem() { Name = "中证估值" });
            this.RateList.Add(new CopyColumnItem() { Name = "到期日" });
            this.RateList.Add(new CopyColumnItem() { Name = "中债久期" });
            this.RateList.Add(new CopyColumnItem() { Name = "债券余额" });
            this.RateList.Add(new CopyColumnItem() { Name = "发行量" });
            this.RateList.Add(new CopyColumnItem() { Name = "是否提还" });
            this.RateList.Add(new CopyColumnItem() { Name = "是否免税" });

            this.CreditList.Add(new CopyColumnItem() { Name = "方向", IsChecked = true });
            this.CreditList.Add(new CopyColumnItem() { Name = "剩余期限", IsChecked = true });
            this.CreditList.Add(new CopyColumnItem() { Name = "债券代码", IsChecked = true });
            this.CreditList.Add(new CopyColumnItem() { Name = "债券简称", IsChecked = true });
            this.CreditList.Add(new CopyColumnItem() { Name = "报量", IsChecked = true });
            this.CreditList.Add(new CopyColumnItem() { Name = "报价", IsChecked = true });
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
        }

        public void SaveParam()
        {
            bool isIntentionPermission = AdminHandler.Instance.CheckPermisionById("4000", "4008");
            bool isPermission = AdminHandler.Instance.CheckPermisionById("4000", "4013");
            if (isPermission == false && isIntentionPermission)
            {
                SaveGlobalParam();
            }

            //IsAuto聊天信息对接,1:自动确认; 0:弹窗人工确认。IsBp是否使用点差,1:使用点差;0:不使用点差。Bp点差,点差数值。后缀FQ：金融Q；WX:微信；无：对客端
            SystemGlobalParamReq req = new SystemGlobalParamReq();
            req.action = "2";
            req.list = new List<TTbookSystemParameter>();
            TTbookSystemParameter param = new TTbookSystemParameter();
            param.param_key = CommonUtils.STC_SALE_INTENTION;
            param.username = LocalEnv.UserInfo.UserName;
            param.param_desc = "需求管理";
            req.list.Add(param);

            Dictionary<string, string> dic = new Dictionary<string, string>();
            //金融Q
            dic["IsInnerGroupChat_FQ"] = IsInnerGroupChatType_FQ;

            dic["IsGroupChat_FQ"] = IsGroupChatType_FQ;
            dic["IsPrivateChat_FQ"] = IsPrivateChatType_FQ;

            dic["IsAuto_FQ"] = IsAutoType_FQ;
            dic["IsBp_FQ"] = IsChecked_FQ ? "1" : "0";
            dic["Bp_FQ"] = BpValue_FQ.HasValue ? BpValue_FQ.ToString() : string.Empty;

            // 个需是否使用白名单 
            dic["If_Intention_WhiteList"] = If_Intention_WhiteList;

            // 个需聊天信息对接
            dic["If_Auto_Insert"] = If_Auto_Insert;
            dic["IsChecked_Broker_Spread"] = IsChecked_Broker_Spread ? "1" : "0";
            dic["Broker_Spread"] = IsChecked_Broker_Spread == true && Broker_Spread.HasValue ? Broker_Spread.ToString() : string.Empty;

            //微信
            dic["IsAuto_WX"] = IsAutoType_WX;
            dic["IsBp_WX"] = IsChecked_WX ? "1" : "0";
            dic["Bp_WX"] = BpValue_WX.HasValue ? BpValue_WX.ToString() : string.Empty;
            //对客端
            dic["IsAuto"] = IsAutoType;
            dic["IsBp"] = IsChecked ? "1" : "0";
            dic["Bp"] = BpValue.HasValue ? BpValue.ToString() : string.Empty;
            CommonUtils.SaleInfoJson = JsonConvert.SerializeObject(dic);
            param.param_value = CommonUtils.SaleInfoJson;

            PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
            if (reply == null || !reply.IsSuccess())
            {
                string errorMsg = (reply == null || string.IsNullOrEmpty(reply.Msg)) ? "需求管理保存失败" : reply.Msg;
                CommonUtils.Prompt2Second("需求管理", errorMsg);
                return;
            }
            else
            {
                CommonUtils.Prompt2Second("需求管理", "保存成功");
            }
        }

        public void QueryParam()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                SystemParameterReq req = new SystemParameterReq();
                req.ParamKey = CommonUtils.STC_SALE_INTENTION;
                req.Username = LocalEnv.UserInfo.UserName;
                TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && JsonSplit.IsJson(reply.Datas.tbookSystemParameter.param_value))
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(reply.Datas.tbookSystemParameter.param_value);
                            //金融Q
                            if (dict.ContainsKey("IsGroupChat_FQ"))
                            {
                                this.IsGroupChatType_FQ = dict["IsGroupChat_FQ"];
                            }
                            else
                                this.IsGroupChatType_FQ = "0";
                            if (dict.ContainsKey("IsInnerGroupChat_FQ"))
                            {
                                this.IsInnerGroupChatType_FQ = dict["IsInnerGroupChat_FQ"];
                            }
                            if (dict.ContainsKey("IsPrivateChat_FQ"))
                            {
                                this.IsPrivateChatType_FQ = dict["IsPrivateChat_FQ"];
                            }
                            else
                                this.IsPrivateChatType_FQ = "2";
                            if (dict.ContainsKey("IsAuto_FQ"))
                            {
                                this.IsAutoType_FQ = dict["IsAuto_FQ"];
                            }
                            if (dict.ContainsKey("IsBp_FQ"))
                            {
                                this.IsChecked_FQ = dict["IsBp_FQ"] == "1" ? true : false;
                            }
                            if (dict.ContainsKey("Bp_FQ"))
                            {
                                this.BpValue_FQ = dict["Bp_FQ"].ConvertToDecimal();
                            }
                            if (dict.ContainsKey("If_Intention_WhiteList"))
                            {
                                this.If_Intention_WhiteList = dict["If_Intention_WhiteList"];
                            }
                            if (dict.ContainsKey("If_Auto_Insert"))
                            {
                                this.If_Auto_Insert = dict["If_Auto_Insert"];
                            }
                            if (dict.ContainsKey("IsChecked_Broker_Spread"))
                            {
                                this.IsChecked_Broker_Spread = dict["IsChecked_Broker_Spread"] == "1" ? true : false;
                            }
                            if (dict.ContainsKey("Broker_Spread"))
                            {
                                this.Broker_Spread = dict["Broker_Spread"].ConvertToDecimal();
                            }
                            //微信
                            if (dict.ContainsKey("IsAuto_WX"))
                            {
                                this.IsAutoType_WX = dict["IsAuto_WX"];
                            }
                            if (dict.ContainsKey("IsBp_FQ"))
                            {
                                this.IsChecked_WX = dict["IsBp_WX"] == "1" ? true : false;
                            }
                            if (dict.ContainsKey("Bp_WX"))
                            {
                                this.BpValue_WX = dict["Bp_WX"].ConvertToDecimal();
                            }
                            //对客端
                            if (dict.ContainsKey("IsAuto"))
                            {
                                this.IsAutoType = dict["IsAuto"];
                            }
                            if (dict.ContainsKey("IsBp"))
                            {
                                this.IsChecked = dict["IsBp"] == "1" ? true : false;
                            }
                            if (dict.ContainsKey("Bp"))
                            {
                                this.BpValue = dict["Bp"].ConvertToDecimal();
                            }
                        }));
                    }
                    else
                    {
                        this.IsPrivateChatType_FQ = "2";
                        this.IsGroupChatType_FQ = "0";
                    }
                }
            });

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
                                //else if (entry.Key.ToUpper() == "COPYMODE")
                                //{
                                //    foreach (CopyColumnItem item in entry.Value)
                                //    {
                                //        if (item.Value == "2")
                                //        {
                                //            this.IsBrokerCopyModeChk = true;
                                //        }
                                //        else if (item.Value == "3")
                                //        {
                                //            this.IsNoneCopyModeChk = true;
                                //        }
                                //    }
                                //}
                            }
                        }));
                    }
                }
            });
        }


        public void SaveGlobalParam()
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
            //if (IsBrokerCopyModeChk)
            //{
            //    copyMode = "2";

            //    CommonUtils.CopyDic["COPYMODE"].Add(new CopyColumnItem { Name = string.Empty, Value = copyMode });
            //}
            //else if (IsNoneCopyModeChk)
            //{
            //    copyMode = "3";

            //    CommonUtils.CopyDic["COPYMODE"].Add(new CopyColumnItem { Name = string.Empty, Value = copyMode });
            //}


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
                    //CommonUtils.Prompt2Second("需求管理", "保存成功");

                    InnerSettingViewModel.InnerCopyInfoChange?.Invoke();
                }
                else
                {
                    CommonUtils.Prompt2Second("需求管理", $"更新失败, 原因:{reply.Msg}.");
                }
            }
            else
            {
                CommonUtils.Prompt2Second("需求管理", "系统异常");
            }
        }
    }
}
