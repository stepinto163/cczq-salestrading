using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.PT.Plugin.ViewModel
{
    public class PTInnerSettingViewModel : ViewModelBase
    {

        private static PTInnerSettingViewModel _PTInnerSettingViewModel = new PTInnerSettingViewModel();

        public static PTInnerSettingViewModel Instance { get { return _PTInnerSettingViewModel; } }

        #region 属性
        private string _SHNumber;

        public string SHNumber
        {
            get { return _SHNumber; }
            set
            {
                _SHNumber = value;
                this.OnPropertyChanged();
            }
        }

        private string _SZNumber;

        public string SZNumber
        {
            get { return _SZNumber; }
            set
            {
                _SZNumber = value;
                this.OnPropertyChanged();
            }
        }

        private string _PartyName;

        public string PartyName
        {
            get { return _PartyName; }
            set
            {
                _PartyName = value;
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
        private bool _IsTwoSpaceChk = true;

        public bool IsTwoSpaceChk
        {
            get { return _IsTwoSpaceChk; }
            set
            {
                _IsTwoSpaceChk = value;
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

        private string tabStr = "    ";

        public string TabStr
        {
            get { return tabStr; }
            set
            {
                tabStr = value;
                this.OnPropertyChanged();
            }
        }
        //private bool _IsPackSendChk;

        //public bool IsPackSendChk
        //{
        //    get { return _IsPackSendChk; }
        //    set
        //    {
        //        _IsPackSendChk = value;
        //        this.OnPropertyChanged();
        //    }
        //}
        //private bool _IsStrokeSendChk;

        //public bool IsStrokeSendChk
        //{
        //    get { return _IsStrokeSendChk; }
        //    set
        //    {
        //        _IsStrokeSendChk = value;
        //        this.OnPropertyChanged();
        //    }
        //}
        //private bool _IsOneSendChk;

        //public bool IsOneSendChk
        //{
        //    get { return _IsOneSendChk; }
        //    set
        //    {
        //        _IsOneSendChk = value;
        //        this.OnPropertyChanged();
        //    }
        //}
        private string _IsEnabledInst = "true";

        public string IsEnabledInst
        {
            get { return _IsEnabledInst; }
            set
            {
                _IsEnabledInst = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        public const string PARAM_KEY_COPY = "STC_PTInner_Setting";
        //public static event Action PTCopyInfoChange;

        public PTInnerSettingViewModel()
        {
            TUsersQuery user = AdminHandler.Instance.usersQuery();
            if (user != null && user.Datas != null && user.Datas.tbookVenueUsers != null)
            {
                foreach (TbookVenueUsers tv in user.Datas.tbookVenueUsers)
                {
                    if (!string.IsNullOrEmpty(tv.partynamecnshortname))
                    {
                        IsEnabledInst = "false";
                        PartyName = tv.partynamecnshortname;
                        PTExecOrderProcesser.Instance.partyName = tv.partynamecnshortname;
                        PTExecOrderProcesser.Instance.partyId = tv.partyid21;
                        break;
                    }
                }
            }
            queryGlobalPara();
        }

        public void getTabStr() 
        {
            queryGlobalPara();
        }

        /// <summary>
        /// 查询
        /// </summary>
        private void queryGlobalPara()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                SystemParameterReq req = new SystemParameterReq();
                req.ParamKey = PARAM_KEY_COPY;
                req.Username = LocalEnv.UserInfo.UserName;
                TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                    {
                        PTInnerSetting pTInnerSetting = JsonConvert.DeserializeObject<PTInnerSetting>(reply.Datas.tbookSystemParameter.param_value);
                        if ("true".Equals(IsEnabledInst)) 
                        {
                            PartyName = pTInnerSetting.PartyName;
                            PTExecOrderProcesser.Instance.partyName = PartyName;
                        }
                        this.SZNumber = pTInnerSetting.SZNumber;
                        PTExecOrderProcesser.Instance.SZNumber = this.SZNumber;
                        this.SHNumber = pTInnerSetting.SHNumber;
                        PTExecOrderProcesser.Instance.SHNumber = this.SHNumber;
                        switch (pTInnerSetting.TabChk)
                        {
                            case "空格":
                                IsSpaceChk = true;
                                IsTwoSpaceChk = false;
                                IsLongSpaceChk = false;
                                IsTagChk = false;
                                TabStr = " ";
                                break;
                            case "空格(2)":
                                IsSpaceChk = false;
                                IsTwoSpaceChk = true;
                                IsLongSpaceChk = false;
                                IsTagChk = false;
                                TabStr = "  ";
                                break;
                            case "长空格":
                                IsSpaceChk = false;
                                IsLongSpaceChk = true;
                                IsTwoSpaceChk = false;
                                IsTagChk = false;
                                TabStr = "    ";
                                break;
                            case "Tab制表符":
                                IsSpaceChk = false;
                                IsLongSpaceChk = false;
                                IsTwoSpaceChk = false;
                                IsTagChk = true;
                                TabStr = "\t";
                                break;
                        }
                        //switch (pTInnerSetting.SendRule)
                        //{
                        //    case "打包":
                        //        IsPackSendChk = true;
                        //        IsStrokeSendChk = false;
                        //        IsOneSendChk = false;
                        //        break;
                        //    case "逐笔":
                        //        IsPackSendChk = false;
                        //        IsStrokeSendChk = true;
                        //        IsOneSendChk = false;
                        //        break;
                        //    case "一笔":
                        //        IsPackSendChk = false;
                        //        IsStrokeSendChk = false;
                        //        IsOneSendChk = true;
                        //        break;
                        //}
                    }
                }
            });
        }


        public void SaveParam()
        {
            PTInnerSetting pTInner = new PTInnerSetting();
            pTInner.SHNumber = this.SHNumber;
            pTInner.SZNumber = this.SZNumber;
            if (IsSpaceChk)
            {
                pTInner.TabChk = "空格";
                TabStr = " ";
            }
            else if (IsTwoSpaceChk)
            {
                pTInner.TabChk = "空格(2)";
                TabStr = "  ";
            }
            else if (IsTagChk)
            {
                pTInner.TabChk = "Tab制表符";
                TabStr = "\t";
            }
            else
            {
                pTInner.TabChk = "长空格";
                TabStr = "    ";
            }
            //if (IsPackSendChk)
            //{
            //    pTInner.SendRule = "打包";
            //}
            //else if (IsStrokeSendChk)
            //{
            //    pTInner.SendRule = "逐笔";
            //}
            //else if (IsOneSendChk)
            //{
            //    pTInner.SendRule = "一笔";
            //}
            if ("true".Equals(IsEnabledInst))
            {
                pTInner.PartyName = PartyName;
            }
            string json = JsonConvert.SerializeObject(pTInner);

            SystemGlobalParamReq req = new SystemGlobalParamReq();
            req.action = "2";
            req.list = new List<TTbookSystemParameter>();
            TTbookSystemParameter param = new TTbookSystemParameter();
            param.param_key = PARAM_KEY_COPY;
            param.username = LocalEnv.UserInfo.UserName;
            param.param_value = json;
            param.param_desc = "交易后处理配置";
            req.list.Add(param);

            PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
            if (reply != null)
            {
                if (reply.Code == "0000")
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        PTExecOrderProcesser.Instance.partyName = PartyName;
                        PTExecOrderProcesser.Instance.SZNumber = SZNumber;
                        PTExecOrderProcesser.Instance.SHNumber = SHNumber;
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg("保存成功", true);
                        msgBox.ShowDialog();
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
        }

    }
}
