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
using STC.PP.Plugin.View;
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
    public class PPColumnsSetViewModel : ViewModelBase
    {
        #region 属性


        public bool IsInit = false;

        private ObservableCollection<MsgAlertDataModel> _gridDatas = new ObservableCollection<MsgAlertDataModel>();

        /// <summary>
        /// 列表
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
        /// 持仓显示参数设置 Key
        /// </summary>
        public const string PARAM_KEY_MSGALERTPARAM = "STC_PAPostion_ShowField_Setting";
        /// <summary>
        /// 持仓显示参数设置说明
        /// </summary>
        public const string PARAM_KEY_MSGALERTMEMO= "平安养老持仓显示字段参数设置";

        private object updlock = new object();
        #endregion

        #region Command
        public ICommand ConfirmCommand { get { return new ComRelayCommand(OnConfirmCommandClick); } }

        public ICommand CancelCommand { get { return new ComRelayCommand(OnCancelCommandClick); } }
        
        public ICommand RestoreDefaultCommand { get { return new ComRelayCommand(OnRestoreDefaultCommandClick); } }

        #endregion

        public PPColumnsSetView mywin;

        public PPColumnsSetViewModel(PPColumnsSetView win)
        {
            this.Mv.Source = GridDatas;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            mywin = win;
            Init();
        }


        public void Init()
        {
            if (!IsInit)
            {
                //查询当前用户的设置
                ////QueryGlobalPara();
                IsInit = true;
            }
        }

        public MsgAlertDataModel GetGridColumnSetFromConfig()
        {
            MsgAlertDataModel savedConfig = DataGridConfigManager.Instance.LoadAllConfigs();
            return savedConfig;
        }

        public void SetGridDatas(MsgAlertDataModel dgc)
        {
            GridDatas = new ObservableCollection<MsgAlertDataModel> { dgc };
        }

        public MsgAlertDataModel FullDataGridConfig { get; set; }


        #region 查询当前用户的设置

        /// <summary>
        /// 查询
        /// </summary>
        public async void QueryGlobalPara(bool isdefault = false)
        {
            ////SystemParameterReq req = new SystemParameterReq();
            ////req.ParamKey = PARAM_KEY_MSGALERTPARAM;
            ////if (!isdefault)
            ////    req.Username = LocalEnv.UserInfo.UserName;
            ////else
            ////    req.Username = "*";

            ////TbookSystemParameterReplyPo reply = await PAPositionHandler.Instance.GetReply(req);
            ////if (reply != null && reply.IsSuccess())
            ////{
            ////    if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
            ////    {
            ////        string s = reply.Datas.tbookSystemParameter.param_value;
            ////        if (JsonSplit.IsJson(s))
            ////        {
            ////            List<MsgAlertDataModel> lstMDM = JsonConvert.DeserializeObject<List<MsgAlertDataModel>>(s);
            ////            this.GridDatas.Clear();
            ////            foreach (MsgAlertDataModel res in lstMDM)
            ////            {
            ////                if (res.subList[0].subList != null)
            ////                {
            ////                    res.subList[0].isgroup = true;
            ////                }
    
            ////                this.GridDatas.Add(res);
            ////            }

            ////            if (isdefault)
            ////            {
            ////                SaveGlobalPara(false);

            ////                Application.Current.Dispatcher.Invoke(() =>
            ////                {
            ////                    STC.Common.View.Component.Prompt2Seconds prompt = new STC.Common.View.Component.Prompt2Seconds();
            ////                    prompt.PromptTitle = "消息";
            ////                    prompt.PromptContext = "恢复默认值成功！";
            ////                    prompt.Show();
            ////                });
            ////            }

            ////            if (lstMDM.Count == 0)
            ////            {
            ////                HideMsgAlertUI();
            ////            }
            ////        }
            ////        else
            ////        {
            ////            HideMsgAlertUI();
            ////        }
            ////    }
            ////    else
            ////    {
            ////        HideMsgAlertUI();
            ////    }
            ////}
        }

        #endregion

        #region Command
        /// <summary>
        /// 确认
        /// </summary>
        private void OnConfirmCommandClick()
        {
            //SaveGlobalPara();
            MsgAlertDataModel dg = GridDatas.ToList()[0];

            DataGridConfigManager.Instance.SaveAllConfigs(dg);
            mywin.Close();
        }

        /// <summary>
        /// 恢复默认
        /// </summary>
        private void OnRestoreDefaultCommandClick()
        {
            lock (updlock)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GridDatas = new ObservableCollection<MsgAlertDataModel> { FullDataGridConfig };
                    List<MsgAlertModel> lstmam = GridDatas[0].subList;
                    foreach (MsgAlertModel rs in lstmam)
                    {
                        if (!rs.isgroup && rs.isenabled)
                        {
                            rs.ischecked = true;
                        }
                    }

                    int i = 0;

                    MsgAlertDataModel temlist = GridDatas[0];
                    foreach(MsgAlertModel x in temlist.subList)
                    {
                        x.code = i++.ToString();
                    }

                    DataGridConfigManager.Instance.SaveAllConfigs(temlist);
                    mywin.Close();
                });
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        public void OnCancelCommandClick()
        {
            mywin.Close();
        }

        #endregion
    }
}
