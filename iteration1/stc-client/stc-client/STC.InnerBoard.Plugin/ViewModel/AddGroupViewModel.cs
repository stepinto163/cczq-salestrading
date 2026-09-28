using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using STC.InnerBoard.Plugin.Utils;
using STC.InnerBoard.Plugin.View;
using STC.InnerBoard.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.InnerBoard.Plugin.ViewModel
{
    public class AddGroupViewModel : BaseViewModel
    {
        private string _groupName;

        public string GroupName
        {
            get { return _groupName; }
            set { 
                _groupName = value;
                this.OnPropertyChanged();
            }
        }

        private bool _ifSubscribe = true;

        public bool IfSubscribe
        {
            get { return _ifSubscribe; }
            set
            {
                _ifSubscribe = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<FilterGroupItem> GroupList { get; set; }

        public FilterGroupItem _selectItem;
        public FilterGroupItem SelectItem
        {
            get { return _selectItem; }
            set
            {
                _selectItem = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _groupCbcVisibility = Visibility.Visible;

        public Visibility GroupCbcVisibility
        {
            get { return _groupCbcVisibility; }
            set
            {
                _groupCbcVisibility = value;
                this.OnPropertyChanged();
            }
        }



        private string _focusTags;
        private bool _isAppTerm;
        private string _startTermDY;
        private string _endTermDY;
        private bool _isCoupon;
        private string _startCoupon;
        private string _endCoupon;
        private string _startPrice;
        private string _endPrice;
        private string _startChinaBondValue;
        private string _endChinaBondValue;
        private string _startBidBp;
        private string _endBidBp;
        private string _startOfrBp;
        private string _endOfrBp;

        private AddGroupWindow _myWin;

        public AddGroupViewModel(ObservableCollection<FilterGroupItem> groupList, string focusTags, AddGroupWindow win, string groupid, string startTermDY, string endTermDY, bool isAppTerm,
            string startCoupon, string endCoupon, bool isCoupon, string startPrice, string endPrice, string startChinaBond, string endChinaBond,
            string startBidBp, string endBidBp, string startOfrBp, string endOfrBp)
        {
            if (SelectItem == null && groupList != null && groupList.Count > 0)
            {
                if (!string.IsNullOrEmpty(groupid))
                {
                    try
                    {
                        this.SelectItem = groupList.First(item => item.GroupID == groupid);
                    }
                    catch (InvalidOperationException)
                    {
                        this.SelectItem = groupList[0];
                    }
                }
                else
                {
                    this.SelectItem = groupList[0];
                }
            }
            if (groupList.Count == 0)
            {
                GroupCbcVisibility = Visibility.Collapsed;
            }
            GroupList = groupList;
            _focusTags = focusTags;
            _isAppTerm = isAppTerm;
            _startTermDY = startTermDY;
            _endTermDY = endTermDY;
            _isCoupon = isCoupon;
            _startCoupon = startCoupon;
            _endCoupon = endCoupon;
            _startPrice = startPrice;
            _endPrice = endPrice;
            _startChinaBondValue = startChinaBond;
            _endChinaBondValue = endChinaBond;
            _startBidBp = startBidBp;
            _endBidBp = endBidBp;
            _startOfrBp = startOfrBp;
            _endOfrBp = endOfrBp;
            _myWin = win;
        }

        internal void UpdateOne()
        {
            // 更新
            QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
            req.action = "2";
            req.id = SelectItem.GroupID;
            req.username = req.closeby = LocalEnv.UserInfo.UserName;
            req.groupname = SelectItem.GroupName_Edit;
            req.ifSubscribe = SelectItem.IfSubscribe_Edit ? "1" : "0";
            req.focusType = "TAG";
            req.marketindicator = "4";
            try
            {
                if (!string.IsNullOrEmpty(_focusTags))
                {
                    //Dictionary<string, List<string>> dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(_focusTags);
                    //if (dic.ContainsKey(Constants.BONDTYPE))
                    //    req.bondTypes = dic[Constants.BONDTYPE];
                    //if (dic.ContainsKey(Constants.TERM))
                    //    req.terms = dic[Constants.TERM];
                    //if (dic.ContainsKey(Constants.FIRM))
                    //    req.firms = dic[Constants.FIRM];
                    //if (dic.ContainsKey(Constants.SETTLTYPE))
                    //    req.settlTypes = dic[Constants.SETTLTYPE];
                    //if (dic.ContainsKey(Constants.SIDE))
                    //    req.sides = dic[Constants.SIDE];
                    //if (dic.ContainsKey(Constants.VIP))
                    //    req.ifVip = dic[Constants.VIP];
                    //else
                    //{
                    //    req.ifVip = new List<string>();
                    //    req.ifVip.Add("0010");
                    //}
                    //if (dic.ContainsKey(Constants.ROLETYPE))
                    //    req.roleTypes = dic[Constants.ROLETYPE];
                    //if (dic.ContainsKey(Constants.ORGTYPE))
                    //    req.orgTypes = dic[Constants.ORGTYPE];
                    //if (dic.ContainsKey(Constants.FINBONDTYPE))
                    //    req.finBondTypes = dic[Constants.FINBONDTYPE];
                    //if (dic.ContainsKey(Constants.DQTYPE))
                    //    req.dqTypes = dic[Constants.DQTYPE];
                    req.focusTags = _focusTags;
                    //req.ordersn = item.Ordersn;
                    req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                    req.ip = EngineHelper.GetIp();
                    //req.remark
                    req.updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                }

                bool isDYNAMIC_PARAM = false;
                if (_isAppTerm && (!string.IsNullOrEmpty(_startTermDY) || !string.IsNullOrEmpty(_endTermDY)))
                {
                    isDYNAMIC_PARAM = true;
                    req.termRangeStr = $"{_startTermDY},{_endTermDY}";

                    string startT = string.Empty;
                    if (!string.IsNullOrEmpty(_startTermDY))
                    {
                        startT = STC.Common.Utils.CommonUtils.GetDays(_startTermDY).ToString();
                    }

                    string endT = string.Empty;
                    if (!string.IsNullOrEmpty(_startTermDY))
                    {
                        endT = STC.Common.Utils.CommonUtils.GetDays(_startTermDY).ToString();
                    }
                    req.termDays = $"{startT},{endT}";
                }

                if (_isCoupon && (!string.IsNullOrEmpty(_startCoupon) || !string.IsNullOrEmpty(_endCoupon)))
                {
                    isDYNAMIC_PARAM = true;
                    req.couponRangeStr = $"{_startCoupon},{_endCoupon}";
                }
                if (!string.IsNullOrEmpty(_startPrice) || !string.IsNullOrEmpty(_endPrice))
                {
                    isDYNAMIC_PARAM = true;
                    req.priceRangeStr = $"{_startPrice},{_endPrice}";
                }
                if (!string.IsNullOrEmpty(_startChinaBondValue) || !string.IsNullOrEmpty(_endChinaBondValue))
                {
                    isDYNAMIC_PARAM = true;
                    req.valuationRangeStr = $"{_startChinaBondValue},{_endChinaBondValue}";
                }
                if (!string.IsNullOrEmpty(_startBidBp) || !string.IsNullOrEmpty(_endBidBp))
                {
                    isDYNAMIC_PARAM = true;
                    req.bidSubValRangeStr = $"{_startBidBp},{_endBidBp}";
                }
                if (!string.IsNullOrEmpty(_startOfrBp) || !string.IsNullOrEmpty(_endOfrBp))
                {
                    isDYNAMIC_PARAM = true;
                    req.valSubOfrRangeStr = $"{_startOfrBp},{_endOfrBp}";
                }
                
                if (isDYNAMIC_PARAM)
                {
                    req.focusType = "DYNAMIC_PARAM";
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }

            //req.ordersn = SelectItem.Ordersn;
            req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            req.ip = EngineHelper.GetIp();
            //req.remark
            req.createtime = req.updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);

            if (reply != null && reply.IsSuccess())
            {
                try
                {
                    FilterGroupItem updateItem = this.GroupList.First(obj => obj.GroupID == this.SelectItem.GroupID);
                    updateItem.UserName = reply.Datas[0].username;
                    updateItem.GroupName = updateItem.GroupName_Edit = reply.Datas[0].groupname;
                    updateItem.IfSubscribe = updateItem.IfSubscribe_Edit = reply.Datas[0].ifSubscribe == "1" ? true : false;
                    updateItem.FocusTags = updateItem.FocusTags_Edit = reply.Datas[0].focusTags;
                    updateItem.Inf1 = reply.Datas[0].inf1;
                    updateItem.Inf2 = reply.Datas[0].inf2;
                    updateItem.Inf3 = reply.Datas[0].inf3;
                    updateItem.Inf4 = reply.Datas[0].inf4;
                    updateItem.UpdateTime = reply.Datas[0].updatetime;
                    updateItem.CreateTime = reply.Datas[0].createtime;
                    updateItem.Ordersn = reply.Datas[0].ordersn;
                    updateItem.IP = reply.Datas[0].ip;
                    updateItem.Terminal = reply.Datas[0].terminal;
                    updateItem.Remark = reply.Datas[0].remark;
                    updateItem.CloseBy = reply.Datas[0].closeby;

                    STC.Common.Utils.CommonUtils.ProcessInf4(reply.Datas[0].inf4, reply.Datas[0].inf1, updateItem);
                    STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, updateItem);

                    Common.Utils.CommonUtils.Prompt2Second("保存", "成功", _myWin);
                    _myWin?.AddAction?.Invoke(updateItem);
                    _myWin?.Close();
                }
                catch (System.InvalidOperationException)
                {
                    // ignore
                }
            }
            else
            {
                Common.Utils.CommonUtils.Prompt2Second("保存", (reply == null || string.IsNullOrEmpty(reply.Msg)) ? "失败" : reply.Msg, _myWin);
            }
        }

        internal void NewOne()
        {

            // 新增
            QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
            req.action = "1";
            req.username = req.closeby = LocalEnv.UserInfo.UserName;
            req.groupname = GroupName;
            req.ifSubscribe = IfSubscribe ? "1" : "0";
            req.focusType = "TAG";
            req.marketindicator = "4";
            try
            {
                if (!string.IsNullOrEmpty(_focusTags))
                {
                    //Dictionary<string, List<string>> dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(_focusTags);
                    //if (dic.ContainsKey(Constants.BONDTYPE))
                    //    req.bondTypes = dic[Constants.BONDTYPE];
                    //if (dic.ContainsKey(Constants.TERM))
                    //    req.terms = dic[Constants.TERM];
                    //if (dic.ContainsKey(Constants.FIRM))
                    //    req.firms = dic[Constants.FIRM];
                    //if (dic.ContainsKey(Constants.SETTLTYPE))
                    //    req.settlTypes = dic[Constants.SETTLTYPE];
                    //if (dic.ContainsKey(Constants.SIDE))
                    //    req.sides = dic[Constants.SIDE];
                    //if (dic.ContainsKey(Constants.VIP))
                    //    req.ifVip = dic[Constants.VIP];
                    //else
                    //{
                    //    req.ifVip = new List<string>();
                    //    req.ifVip.Add("0010");
                    //}
                    //if (dic.ContainsKey(Constants.ROLETYPE))
                    //    req.roleTypes = dic[Constants.ROLETYPE];
                    //if (dic.ContainsKey(Constants.ORGTYPE))
                    //    req.orgTypes = dic[Constants.ORGTYPE];
                    //if (dic.ContainsKey(Constants.FINBONDTYPE))
                    //    req.finBondTypes = dic[Constants.FINBONDTYPE];
                    //if (dic.ContainsKey(Constants.DQTYPE))
                    //    req.dqTypes = dic[Constants.DQTYPE];
                    req.focusTags = _focusTags;
                }
                bool isDYNAMIC_PARAM = false;
                if (_isAppTerm && (!string.IsNullOrEmpty(_startTermDY) || !string.IsNullOrEmpty(_endTermDY)))
                {
                    isDYNAMIC_PARAM = true;
                    req.termRangeStr = $"{_startTermDY},{_endTermDY}";

                    string startT = string.Empty;
                    if (!string.IsNullOrEmpty(_startTermDY))
                    {
                        startT = STC.Common.Utils.CommonUtils.GetDays(_startTermDY).ToString();
                    }

                    string endT = string.Empty;
                    if (!string.IsNullOrEmpty(_startTermDY))
                    {
                        endT = STC.Common.Utils.CommonUtils.GetDays(_startTermDY).ToString();
                    }
                    req.termDays = $"{startT},{endT}";
                }

                if (_isCoupon && (!string.IsNullOrEmpty(_startCoupon) || !string.IsNullOrEmpty(_endCoupon)))
                {
                    isDYNAMIC_PARAM = true;
                    req.couponRangeStr = $"{_startCoupon},{_endCoupon}";
                }

                if (!string.IsNullOrEmpty(_startPrice) || !string.IsNullOrEmpty(_endPrice))
                {
                    isDYNAMIC_PARAM = true;
                    req.priceRangeStr = $"{_startPrice},{_endPrice}";
                }
                if (!string.IsNullOrEmpty(_startChinaBondValue) || !string.IsNullOrEmpty(_endChinaBondValue))
                {
                    isDYNAMIC_PARAM = true;
                    req.valuationRangeStr = $"{_startChinaBondValue},{_endChinaBondValue}";
                }

                if (!string.IsNullOrEmpty(_startBidBp) || !string.IsNullOrEmpty(_endBidBp))
                {
                    isDYNAMIC_PARAM = true;
                    req.bidSubValRangeStr = $"{_startBidBp},{_endBidBp}";
                }
                if (!string.IsNullOrEmpty(_startOfrBp) || !string.IsNullOrEmpty(_endOfrBp))
                {
                    isDYNAMIC_PARAM = true;
                    req.valSubOfrRangeStr = $"{_startOfrBp},{_endOfrBp}";
                }

                if (isDYNAMIC_PARAM)
                {
                    req.focusType = "DYNAMIC_PARAM";
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }

            //req.ordersn = item.Ordersn;
            req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            req.ip = EngineHelper.GetIp();
            //req.remark
            req.createtime = req.updatetime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
            QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);
            if (reply != null && reply.IsSuccess())
            {
                try
                {
                    FilterGroupItem newItem = new FilterGroupItem();
                    newItem.GroupID = reply.Datas[0].id;
                    newItem.UserName = reply.Datas[0].username;
                    newItem.GroupName_Edit = newItem.GroupName = reply.Datas[0].groupname;
                    newItem.IfSubscribe_Edit = newItem.IfSubscribe = reply.Datas[0].ifSubscribe == "1" ? true : false;
                    newItem.FocusTags_Edit = newItem.FocusTags = reply.Datas[0].focusTags;
                    newItem.Inf1 = reply.Datas[0].inf1;
                    newItem.Inf2 = reply.Datas[0].inf2;
                    newItem.Inf3 = reply.Datas[0].inf3;
                    newItem.Inf4 = reply.Datas[0].inf4;
                    newItem.UpdateTime = reply.Datas[0].updatetime;
                    newItem.CreateTime = reply.Datas[0].createtime;
                    newItem.Ordersn = reply.Datas[0].ordersn;
                    newItem.IP = reply.Datas[0].ip;
                    newItem.Terminal = reply.Datas[0].terminal;
                    newItem.Remark = reply.Datas[0].remark;
                    newItem.CloseBy = reply.Datas[0].closeby;

                    STC.Common.Utils.CommonUtils.ProcessInf4(reply.Datas[0].inf4, reply.Datas[0].inf1, newItem);
                    STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, newItem);
                    
                    this.GroupList.Add(newItem);

                    Common.Utils.CommonUtils.Prompt2Second("保存", "成功", _myWin);
                    _myWin?.AddAction?.Invoke(newItem);
                    _myWin?.Close();
                }
                catch (System.InvalidOperationException)
                {
                    // ignore
                }

            }
            else
            {
                Common.Utils.CommonUtils.Prompt2Second("保存", (reply == null || string.IsNullOrEmpty(reply.Msg)) ? "失败" : reply.Msg, _myWin);
            }
        }
    }
}
