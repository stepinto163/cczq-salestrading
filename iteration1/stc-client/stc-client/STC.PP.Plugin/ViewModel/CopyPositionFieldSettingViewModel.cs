using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.UI;
using STC.PP.Plugin.Handler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.PP.Plugin.ViewModel
{
    public class CopyPositionFieldSettingViewModel : ViewModelBase
    {
        #region 属性
        private List<MsgAlertModel> _TempBox = new List<MsgAlertModel>();

        /// <summary>
        /// Temp
        /// </summary>
        public List<MsgAlertModel> TempBox
        {
            get { return _TempBox; }
            set
            {
                _TempBox = value;
                this.OnPropertyChanged();
            }
        }


        public bool IsInit = false;

        private ObservableCollection<MsgAlertDataModel> _gridDatas = new ObservableCollection<MsgAlertDataModel>();

        /// <summary>
        /// 回价列表
        /// </summary>
        public ObservableCollection<MsgAlertDataModel> GridDatas
        {
            get
            {
                if (_gridDatas == null)
                    _gridDatas = new ObservableCollection<MsgAlertDataModel>();

                return _gridDatas;
            }

            set
            {
                if (value != null)
                {
                    _gridDatas = value;
                    this.OnPropertyChanged(nameof(GridDatas));
                }
            }
        }

        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }

        private Visibility _MsgAlertVisible = Visibility.Visible;
        /// <summary>
        /// 消息待办提醒内容 是否可见
        /// </summary>
        public Visibility MsgAlertVisible
        {
            get
            {
                return _MsgAlertVisible;
            }

            set
            {
                _MsgAlertVisible = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 复制持仓参数设置 Key
        /// </summary>
        public const string PARAM_KEY_MSGALERTPARAM = "STC_PAPostion_CopyMsg_Setting";
        /// <summary>
        /// 复制持仓参数设置说明
        /// </summary>
        public const string PARAM_KEY_MSGALERTMEMO= "持仓管理复制参数设置";

        private object updlock = new object();
        #endregion

        #region Command
        
        public ICommand SelectAllCommand { get { return new ComRelayCommand(OnSelectAllCommandClick); } }

        public ICommand ConfirmCommand { get { return new ComRelayCommand(OnConfirmCommandClick); } }

        public ICommand CancelCommand { get { return new ComRelayCommand(OnCancelCommandClick); } }

        public ICommand RestoreDefaultCommand { get { return new ComRelayCommand(OnRestoreDefaultCommandClick); } }

        #endregion


        public CopyPositionFieldSettingViewModel()
        {
            this.Mv.Source = GridDatas;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            Init();
        }


        public void Init()
        {
            if (!IsInit)
            {
                //查询当前用户的设置
                QueryGlobalPara();
                IsInit = true;
            }
        }


        #region 查询当前用户的设置

        /// <summary>
        /// 查询
        /// </summary>
        public async void QueryGlobalPara(bool isdefault = false)
        {
            SystemParameterReq req = new SystemParameterReq();
            req.ParamKey = PARAM_KEY_MSGALERTPARAM;
            if (!isdefault)
                req.Username = LocalEnv.UserInfo.UserName;
            else
                req.Username = "*";

            TbookSystemParameterReplyPo reply = await PAPositionHandler.Instance.GetReply(req);
            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                {
                    string s = reply.Datas.tbookSystemParameter.param_value;
                    if (JsonSplit.IsJson(s))
                    {
                        List<MsgAlertDataModel> lstMDM = JsonConvert.DeserializeObject<List<MsgAlertDataModel>>(s);
                        this.GridDatas.Clear();
                        foreach (MsgAlertDataModel res in lstMDM)
                        {
                            if (res.subList[0].subList != null)
                            {
                                res.subList[0].isgroup = true;
                            }
    
                            this.GridDatas.Add(res);
                        }

                        if (isdefault)
                        {
                            SaveGlobalPara(false);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                STC.Common.View.Component.Prompt2Seconds prompt = new STC.Common.View.Component.Prompt2Seconds();
                                prompt.PromptTitle = "消息";
                                prompt.PromptContext = "恢复默认值成功！";
                                prompt.Show();
                            });
                        }

                        if (lstMDM.Count == 0)
                        {
                            HideMsgAlertUI();
                        }
                    }
                    else
                    {
                        HideMsgAlertUI();
                    }
                }
                else
                {
                    HideMsgAlertUI();
                }
            }
        }

        #endregion

        #region Command
        /// <summary>
        /// 全选
        /// </summary>
        private void OnSelectAllCommandClick()
        {
            lock (updlock)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (MsgAlertDataModel mam in GridDatas)
                    {
                        if (mam.visible)
                        {
                            List<MsgAlertModel> lstmam = mam.subList;
                            foreach (MsgAlertModel rs in lstmam)
                            {
                                if (!rs.isgroup && rs.isenabled)
                                {
                                    rs.ischecked = true;
                                }
                                else if (rs.isgroup && rs.isenabled)
                                {
                                    rs.ischecked = true;
                                    foreach (MsgAlertModel ds in rs.subList)
                                    {
                                        ds.ischecked = true;
                                        ds.isenabled = true;
                                    }
                                }
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 确认
        /// </summary>
        private void OnConfirmCommandClick()
        {
            SaveGlobalPara();
        }

        /// <summary>
        /// 恢复默认
        /// </summary>
        private void OnRestoreDefaultCommandClick()
        {
            QueryGlobalPara(true);
            GetUserSetting();
        }

        /// <summary>
        /// 取消恢复之前的状态
        /// </summary>
        private void OnCancelCommandClick()
        {
            QueryGlobalPara(false);
            GetUserSetting();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.PromptTitle = "消息";
                prompt.PromptContext = "已恢复到最后保存前的状态！";
                prompt.Show();
            });
        }


        public void GetUserSetting()
        {
            UserSetting us = SystemParamHandler.Instance.GetUserSetting();
            if (us != null)
            {
                if (SystemParamHandler.Instance.GetUserSetting().inf2 != null)
                {
                    ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);
                }
            }
        }
        #endregion

        #region 保存当前用户的设置

        /// <summary>
        /// 查询
        /// </summary>
        public void SaveGlobalPara(bool isShowDiaglog = true)
        {
            string json = JsonConvert.SerializeObject(this.GridDatas);
            SystemGlobalParamReq req = new SystemGlobalParamReq();
            req.action = "2";
            req.list = new List<TTbookSystemParameter>();
            TTbookSystemParameter param = new TTbookSystemParameter();
            param.param_key = PARAM_KEY_MSGALERTPARAM;
            param.username = LocalEnv.UserInfo.UserName;
            param.param_value = json;
            param.param_desc = PARAM_KEY_MSGALERTMEMO;
            req.list.Add(param);

            PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
            if (reply != null)
            {
                if (reply.Code == "0000")
                {
                    //更新成功
                    lock (updlock)
                    {
                        PAPositionHandler.Instance.PAPostionCopyParams = this.GridDatas[0].subList;
                    }

                    if (isShowDiaglog)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBoxGUI msgBox = new MessageBoxGUI();
                            msgBox.setWarnMsg("保存成功", true);
                            msgBox.ShowDialog();
                        });
                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg(string.Format($"更新失败, 原因:{reply.Msg}."), true);
                        msgBox.ShowDialog();
                    });
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBoxGUI msgBox = new MessageBoxGUI();
                    msgBox.setWarnMsg(string.Format($"系统异常"), true);
                    msgBox.ShowDialog();
                });
            }

            //如果是设计模式下，直接更新设计模式的设置
            ////Application.Current.Dispatcher.Invoke(() =>
            ////{
            ////    Common.Data.DesignHandler.Instance.SettingChange(new Model.MM.NewSettingMsg() { key = "MessageBellPopupStyle", value = PopMsgAlert ? "1" : "0" });
            ////    Common.Data.DesignHandler.Instance.SettingChange(new Model.MM.NewSettingMsg() { key = "PopMsgMode", value = PopMsgMode ? "1" : "0" });
            ////});
        }


        private void HideMsgAlertUI()
        {
            MsgAlertVisible = Visibility.Collapsed;
            LogHelper.Warn("未配置 持仓管理全局复制开关 设置！！！");
            //InitDefaultSetting();
        }
        #endregion


        #region 初始化模拟测试数据
        /// <summary>
        /// 获取默认值
        /// </summary>
        public void InitDefaultSetting()
        {
            lock (new object())
            {
                this.GridDatas.Clear();
                //交易
                TempBox = new List<MsgAlertModel>();
                TempBox.Add(new MsgAlertModel() { id = "1_1", pid = "1", code = "RemaingingTime", name = "剩余期限", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_2", pid = "1", code = "SecurityId", name = "债券代码", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_3", pid = "1", code = "SymbolName", name = "债券名称", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_35", pid = "1", code = "PortfolioNo", name = "组合代码", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_4", pid = "1", code = "PortfolioName", name = "组合名称", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_50", pid = "1", code = "TradeAccount", name = "交易账户代码", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_5", pid = "1", code = "TradeName", name = "交易账户名称", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_6", pid = "1", code = "HsProductId", name = "恒生基金编号", ischecked = false, visible = true }); 
                TempBox.Add(new MsgAlertModel() { id = "1_7", pid = "1", code = "FundAccount", name = "绩效经理", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_8", pid = "1", code = "Issuer", name = "发行人", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_9", pid = "1", code = "TotalQty", name = "总持仓量(万)", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_10", pid = "1", code = "AvailablePosition", name = "可用持仓量(万)", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_11", pid = "1", code = "StcInSalesVolume", name = "STC在途卖量(万)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_12", pid = "1", code = "StcAvailablePosition", name = "STC可用持仓量(万)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_13", pid = "1", code = "StcInBuyVolume", name = "STC在途买量(万)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_14", pid = "1", code = "StcAvailableLimit", name = "STC可用额度(万)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_15", pid = "1", code = "Quotalimit", name = "主体剩余额度(万)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_015", pid = "1", code = "SingleBondAmount", name = "单券剩余额度(万)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_16", pid = "1", code = "OutStandingAmount", name = "单券发行量(万)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_17", pid = "1", code = "Investmentratio", name = "可投比例(%)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_18", pid = "1", code = "Rate", name = "内部主体评级", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_19", pid = "1", code = "Bonddrating", name = "内部债券评级", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_20", pid = "1", code = "Netprice", name = "成本净价(元)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_21", pid = "1", code = "ValuationYield", name = "中债估值(元)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_22", pid = "1", code = "ValuationYield2", name = "中债估值(%)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_23", pid = "1", code = "CsiBondValuation", name = "中证估值(%)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_24", pid = "1", code = "ExClose", name = "交易所收盘价(元)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_25", pid = "1", code = "ExClose2", name = "交易所收盘价(%)", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_26", pid = "1", code = "IssueType", name = "募集类型", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_27", pid = "1", code = "Guarantor", name = "担保人", ischecked = false, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_28", pid = "1", code = "GuarantorRating", name = "担保人内部评级", ischecked = false, visible = true });

                this.GridDatas.Add(new MsgAlertDataModel() { id = "1", code = "CopyPositionField", name = "", subList = TempBox, visible = true });
                this.Mv.View.Refresh();
            }
        }
        #endregion
    }
}
