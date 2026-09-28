using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Common.ViewModel.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Common.ViewModel.UI
{
    public class MsgAlertSettingViewModel : BaseViewModel
    {
        #region 构造初始化
        public ICommand SelectAllCommand { get { return new ComRelayCommand(OnSelectAllCommandClick); } }

        public ICommand RestoreDefaultCommand { get { return new ComRelayCommand(OnRestoreDefaultCommandClick); } }

        public ICommand CancelCommand { get { return new ComRelayCommand(OnCancelCommandClick); } }

        public ICommand ConfirmCommand { get { return new ComRelayCommand(OnConfirmCommandClick); } }


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

        public const string PARAM_KEY_MSGALERTPARAM = "STC_Msg_Notice_Setting";

        private object updlock = new object();

        private bool _PopMsgAlert = true;
        public bool PopMsgAlert
        {
            get
            {
                return _PopMsgAlert;
            }

            set
            {
                _PopMsgAlert = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// true：跳数字且弹框(1)
        /// false： 只跳数字不弹框(0)
        /// </summary>
        private bool _PopMsgMode = true;

        public bool PopMsgMode
        {
            get
            {
                return _PopMsgMode;
            }

            set
            {
                _PopMsgMode = value;
                this.OnPropertyChanged();
            }
        }

        private bool _FixMasAlert = false;
        public bool FixMasAlert
        {
            get
            {
                return _FixMasAlert;
            }

            set
            {
                _FixMasAlert = value;
                this.OnPropertyChanged();
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
        #endregion

        #endregion
        public MsgAlertSettingViewModel()
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
                GetUserSetting();
                IsInit = true;
            }
        }


        #region 查询当前用户的设置

        /// <summary>
        /// 查询
        /// </summary>
        public async void QueryGlobalPara(bool isdefault= false)
        {
            SystemParameterReq req = new SystemParameterReq();
            req.ParamKey = PARAM_KEY_MSGALERTPARAM;
            if (!isdefault)
                req.Username = LocalEnv.UserInfo.UserName;
            else
                req.Username = "*";

            TbookSystemParameterReplyPo reply = await GetReply(req);
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
                                Prompt2Seconds prompt = new Prompt2Seconds();
                                prompt.tbTitle.Text = "消息";
                                prompt.tbRefer.Text = "恢复默认值成功！";
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

        private void HideMsgAlertUI()
        {
            MsgAlertVisible = Visibility.Collapsed;
            LogHelper.Warn("未配置 消息待办提醒内容 设置！！！");
        }

        public void GetUserSetting()
        {
            UserSetting us = SystemParamHandler.Instance.GetUserSetting();
            if (us != null)
            {
                if (SystemParamHandler.Instance.GetUserSetting().inf2 != null)
                {
                    ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(SystemParamHandler.Instance.GetUserSetting().inf2);
                    if (consistencyCheckPo != null)
                    {
                        string msgpopstyle = consistencyCheckPo.MessageBellPopupStyle ?? "1";
                        PopMsgAlert = msgpopstyle == "1" ? true : false;
                        FixMasAlert = !PopMsgAlert;
                        PopMsgMode = (consistencyCheckPo.PopMsgMode ?? "1") == "1" ? true : false;
                    }
                }
            }
        }

        public async Task<TbookSystemParameterReplyPo> GetReply(SystemParameterReq req)
        {
            TbookSystemParameterReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = AdminHandler.Instance.querySystemParameter(req);
            });

            return reply;
        }

        #endregion


        #region 保存当前用户的设置

        /// <summary>
        /// 查询
        /// </summary>
        public void SaveGlobalPara(bool isShowDiaglog = true)
        {

            UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
            if (userSetting != null)
            {
                if (SystemParamHandler.Instance.GetUserSetting().inf2 != null)
                {
                    ConsistencyCheckPo consistencyCheckPo = JsonConvert.DeserializeObject<ConsistencyCheckPo>(userSetting.inf2);
                    if (consistencyCheckPo != null)
                    {
                        consistencyCheckPo.MessageBellPopupStyle = PopMsgAlert ? "1" : "0";
                        consistencyCheckPo.PopMsgMode = PopMsgMode ? "1" : "0";
                    }

                    userSetting.inf2 = JsonConvert.SerializeObject(consistencyCheckPo);

                    ThreadPoolHelper.InvokeThreadPool(GetSettingParam, userSetting);
                }
            }

            string json = JsonConvert.SerializeObject(this.GridDatas);
            SystemGlobalParamReq req = new SystemGlobalParamReq();
            req.action = "2";
            req.list = new List<TTbookSystemParameter>();
            TTbookSystemParameter param = new TTbookSystemParameter();
            param.param_key = PARAM_KEY_MSGALERTPARAM;
            param.username = LocalEnv.UserInfo.UserName;
            param.param_value = json;
            param.param_desc = "消息提醒设置";
            req.list.Add(param);

            PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
            if (reply != null)
            {
                if (reply.Code == "0000")
                {
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                Common.Data.DesignHandler.Instance.SettingChange(new Model.MM.NewSettingMsg() { key = "MessageBellPopupStyle", value = PopMsgAlert ? "1" : "0" });
                Common.Data.DesignHandler.Instance.SettingChange(new Model.MM.NewSettingMsg() { key = "PopMsgMode", value = PopMsgMode ? "1" : "0" });
            });
        }

        private void GetSettingParam(object obj)
        {
            try
            {
                UserSetting us = obj as UserSetting;
                PResult result = SystemParamHandler.Instance.UpdatesystemParas(us);
                if (result.IsSuccess())
                {
                    SystemParamHandler.Instance.CopyParam(us);
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    //    MessageBoxGUI msgBox = new MessageBoxGUI();
                    //    msgBox.setWarnMsg("保存成功", true);
                    //    msgBox.ShowDialog();
                    //}));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    SaveIsEnable = true;
                //}));
            }
        }

        #endregion

        #region Command

        /// <summary>
        /// 更新子开关
        /// </summary>
        /// <param name="mdm"></param>
        /// <param name="isCheck"></param>
        public void UpdateGridDatas(MsgAlertModel mdm, bool isCheck = false)
        {
            lock (updlock)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (MsgAlertModel mam in mdm.subList)
                    {
                        mam.ischecked = isCheck;
                        mam.isenabled = isCheck;
                    }
                });
            }
        }

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
        /// 恢复默认
        /// </summary>
        private void OnRestoreDefaultCommandClick()
        {
            QueryGlobalPara(true);
            GetUserSetting();
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void OnCancelCommandClick()
        {
            QueryGlobalPara(false);
            GetUserSetting();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.tbTitle.Text = "消息";
                prompt.tbRefer.Text = "已恢复到最后保存前的状态！";
                prompt.Show();
            });
        }

        /// <summary>
        /// 确认
        /// </summary>
        private void OnConfirmCommandClick()
        {
            SaveGlobalPara();
        }
        #endregion

        #region 初始化测试数据
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
                TempBox.Add(new MsgAlertModel() { id = "1_1", pid = "1", code = "Huijia", name = "回价", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_2", pid = "1", code = "DianJia", name = "点价", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_3", pid = "1", code = "ChengJiaoHuiBao", name = "成交回报", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_4", pid = "1", code = "JiaGePiPeiJingZhun", name = "价格匹配(精准)", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_5", pid = "1", code = "JiaGePiPeiMoHu", name = "价格匹配(模糊)", ischecked = true, visible = true });
                TempBox.Add(new MsgAlertModel() { id = "1_6", pid = "1", code = "JiaoYiCuoWuTiShi", name = "交易错误提示", ischecked = true, visible = true });
                this.GridDatas.Add(new MsgAlertDataModel() { id = "1", code = "JiaoYi", name = "交易", subList = TempBox, visible = true });

                //内部看板
                TempBox = new List<MsgAlertModel>();
                TempBox.Add(new MsgAlertModel() { id = "2_1", pid = "2", code = "DingDanDingYue", name = "订单订阅", ischecked = true, visible = true });
                this.GridDatas.Add(new MsgAlertDataModel() { id = "2", code = "NeiBuKanBan", name = "内部看板", subList = TempBox, visible = true });

                //RFQ
                TempBox = new List<MsgAlertModel>();
                TempBox.Add(new MsgAlertModel() { id = "3_1", pid = "3", code = "RfqXiaoXi", name = "RFQ消息", ischecked = true });
                this.GridDatas.Add(new MsgAlertDataModel() { id = "3", code = "Rfq", name = "RFQ", subList = TempBox, visible = true });

                //双边做市
                TempBox = new List<MsgAlertModel>();
                TempBox.Add(new MsgAlertModel() { id = "4_1", pid = "4", code = "ShuangBianZuoShiXiaoXi", name = "双边做市消息", ischecked = true });
                this.GridDatas.Add(new MsgAlertDataModel() { id = "4", code = "ShuangBianZuoShi", name = "双边做市", subList = TempBox, visible = true });

                //策略控制台
                TempBox = new List<MsgAlertModel>();
                TempBox.Add(new MsgAlertModel() { id = "5_1", pid = "5", code = "CeLueKongZhiTaiXiaoXi", name = "策略控制台", ischecked = true });
                this.GridDatas.Add(new MsgAlertDataModel() { id = "5", code = "CeLueKongZhiTai", name = "策略控制台", subList = TempBox, visible = true });

                //系统消息
                TempBox = new List<MsgAlertModel>();
                TempBox.Add(new MsgAlertModel() { id = "6_1_1", pid = "6_1", code = "Tradebook", name = "Tradebook", ischecked = true, isenabled = true });
                TempBox.Add(new MsgAlertModel() { id = "6_1_2", pid = "6_1", code = "ShuangBian", name = "双边", ischecked = true, isenabled = true });
                TempBox.Add(new MsgAlertModel() { id = "6_1_3", pid = "6_1", code = "Xbond", name = "X-Bond", ischecked = true, isenabled = true });
                TempBox.Add(new MsgAlertModel() { id = "6_1_4", pid = "6_1", code = "JingJiShang", name = "经纪商", ischecked = true, isenabled = true });


                List<MsgAlertModel> mainTemp = new List<MsgAlertModel>();
                mainTemp.Add(new MsgAlertModel() { id = "6_1", pid = "6", code = "CeLueKongZhiTaiXiaoXi", name = "交易引擎提醒", ischecked = true, isgroup = true, subList = TempBox });
                this.GridDatas.Add(new MsgAlertDataModel() { id = "6", code = "XiTongXiaoXi", name = "系统消息", subList = mainTemp, visible = true });

                //其它消息
                TempBox = new List<MsgAlertModel>();
                TempBox.Add(new MsgAlertModel() { id = "7_1_1", pid = "7_1", code = "Tradebook", name = "Tradebook", ischecked = false, isenabled = false });
                TempBox.Add(new MsgAlertModel() { id = "7_1_2", pid = "7_1", code = "ShuangBian", name = "双边", ischecked = false, isenabled = false });
                TempBox.Add(new MsgAlertModel() { id = "7_1_3", pid = "7_1", code = "Xbond", name = "X-Bond", ischecked = false, isenabled = false });
                TempBox.Add(new MsgAlertModel() { id = "7_1_3", pid = "7_1", code = "JingJiShang", name = "经纪商", ischecked = false, isenabled = false });

                mainTemp = new List<MsgAlertModel>();
                mainTemp.Add(new MsgAlertModel() { id = "7_1", pid = "7", code = "FuWuQiLianJieTiXing", name = "服务连接提醒", ischecked = false, isgroup = true, subList = TempBox });
                this.GridDatas.Add(new MsgAlertDataModel() { id = "7", code = "QiTaXiaoXi", name = "其它消息", subList = mainTemp });

                this.Mv.View.Refresh();
            }
        }
        #endregion
    }

}