using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Common.ViewModel.Component
{
    public class EditGroupWindowViewModel : BaseViewModel
    {
        public ICommand DelCommand { get; set; }

        private static FilterGroupItem _defaultFilterGroupItem = new FilterGroupItem();

        private FilterGroupItem _selectedItem = _defaultFilterGroupItem;

        public FilterGroupItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    if (value != null)
                    {
                        if (!string.IsNullOrEmpty(value.StartTermDY_Edit) || !string.IsNullOrEmpty(value.EndTermDY_Edit))
                        {
                            IsAppTerm = true;
                        }
                        else
                        {

                            IsAppTerm = false;

                        }
                        _selectedItem = value;
                        int count = 0;
                        foreach (DataRowView drv in GroupView)
                        {
                            if (!string.IsNullOrEmpty(value.FocusGroupId_Edit) && value.FocusGroupId_Edit.Contains(drv[G_Id].ToString()))
                            {
                                drv[G_IsChecked] = true;
                                count++;
                            }
                            else
                            {
                                drv[G_IsChecked] = false;
                            }
                        }
                        value.CheckCount = count;
                    }
                    else
                    {
                        IsAppTerm = false;
                        _selectedItem = _defaultFilterGroupItem;
                    }

                }
                this.OnPropertyChanged();
            }
        }

        private bool _IsAppTerm;

        public bool IsAppTerm
        {
            get { return _IsAppTerm; }
            set
            {
                _IsAppTerm = value;
                this.OnPropertyChanged();
            }
        }

        private object _dataLock = new object();
        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        //private CustomCollection<FilterGroupItem> _groupList = new Common.Utils.CustomCollection<FilterGroupItem>();
        //public CustomCollection<FilterGroupItem> GroupList
        //{
        //    get { return _groupList; }
        //    set { _groupList = value; }
        //}

        public ObservableCollection<FilterGroupItem> GroupList_Org { get; set; }
        public ObservableCollection<FilterGroupItem> GroupList { get; set; }

        private string _defaultSelectedGroupID = string.Empty;

        public EditGroupWindowViewModel(ObservableCollection<FilterGroupItem> groupList, string groupid)
        {
            DelCommand = new ComRelayCommand(DelOne);

            this.GroupList_Org = groupList;
            _defaultSelectedGroupID = groupid;

            this.GroupList = new ObservableCollection<FilterGroupItem>();
            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            ViewSource.Source = this.GroupList;
            ViewSource.View.SortDescriptions.Add(new SortDescription("Inf1", ListSortDirection.Descending));
            ViewSource.View.SortDescriptions.Add(new SortDescription("Ordersn", ListSortDirection.Ascending));
            
            this.ViewSource.View.Filter = x =>
            {
                return ((FilterGroupItem)x).Aciton == "3" ? false : true;
            };

            DesignHandler.Instance.OnNotifyUpdateGroup += Instance_OnNotifyUpdateGroup;

            BindingOperations.EnableCollectionSynchronization(this.GroupView, this.LockObj);
            InitGroupTag();

            
            this.GroupManagerCommand = new ComRelayCommand(this.GroupManager);


            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
        }

        public void UnsubscribeAppMsg()
        {
            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.UnSubscribeAppMsg(OnAppMsg);
        }

        /// <summary>
        /// 应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "refreshClientBondGroup" && obj.Msg == "*")
            {
                clearGroup();
                initGroup();
            }
        }


        private void InitGroupTag()
        {
            ThreadPoolHelper.InvokeThreadPool(o => {
                initGroup();

                Query();
            });
        }

        private void Query()
        {
            int ordersn = 0;
            QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
            req.action = "0";
            req.username = LocalEnv.UserInfo.UserName;
            QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (reply != null && reply.IsSuccess())
                {
                    foreach (QuoteTagGroup group in reply.Datas)
                    {
                        if (group.marketindicator != "4" || group.inf1== "PPGroup")
                        {
                            continue;
                        }

                        FilterGroupItem item = new FilterGroupItem();
                        item.GroupID = group.id;
                        item.GroupName_Edit = item.GroupName = group.groupname;
                        item.Ordersn = ordersn++;
                        item.UserName = group.username;
                        item.Terminal = group.terminal;
                        item.IP = group.ip;
                        item.CloseBy = group.closeby;
                        item.Remark = group.remark;
                        item.CreateTime = group.createtime;
                        item.UpdateTime = group.updatetime;
                        item.Inf1 = group.inf1;
                        item.Inf2 = group.inf2;
                        item.Inf3 = group.inf3;
                        item.Inf4 = group.inf4;
                        item.FocusTags_Edit = item.FocusTags = group.focusTags;

                        STC.Common.Utils.CommonUtils.ProcessInf4(group.inf4, group.inf1, item);
                        STC.Common.Utils.CommonUtils.ProcessInf3(group.inf3, group.inf1, item);

                        //Dictionary<string, List<string>> dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(group.focusTags);
                        //item.FocusTagsDic = dic;
                        item.IfSubscribe_Edit = item.IfSubscribe = group.ifSubscribe == "1" ? true : false;
                        this.GroupList.Add(item);
                    }

                    if (GroupList != null && GroupList.Count > 0)
                    {
                        var sortGroupList = this.GroupList.OrderByDescending(x=>x.Inf1).ThenBy(x => x.Ordersn);
                        if (!string.IsNullOrEmpty(_defaultSelectedGroupID))
                        {
                            try
                            {
                                this.SelectedItem = this.GroupList.First(item => item.GroupID == _defaultSelectedGroupID);
                            }
                            catch (InvalidOperationException)
                            {
                                this.SelectedItem = sortGroupList.First();
                            }
                        }
                        else
                        {
                            this.SelectedItem = sortGroupList.First();
                        }
                    }
                }
            }));
        }


        internal void NewOne()
        {
            FilterGroupItem groupItem = new FilterGroupItem();
            groupItem.GroupName_Edit = groupItem.GroupName = "新建组";
            groupItem.Aciton = "1";//新增
            groupItem.Inf1 = "TAG";
            groupItem.IfSubscribe_Edit = true;
            groupItem.FocusTags_Edit = groupItem.FocusTags = string.Empty;
            groupItem.CreateTime = groupItem.UpdateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
            this.GroupList.Add(groupItem);
            this.SelectedItem = groupItem;
        }

        internal void DelOne()
        {
            bool result = CustomMessageBox.Question("是否删除组？", "组管理");

            if (result == false)
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.SelectedItem.GroupID))
            {
                QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
                req.action = "3";
                req.id = this.SelectedItem.GroupID;
                QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);

                if (reply != null && reply.IsSuccess())
                {
                    try
                    {
                        FilterGroupItem removeItem = this.GroupList_Org.First(item => item.GroupID == this.SelectedItem.GroupID);
                        this.GroupList_Org.Remove(removeItem);
                    }
                    catch (System.InvalidOperationException)
                    {
                        // ignore
                    }

                    this.GroupList.Remove(this.SelectedItem);
                    // Msg 为 * 表示需要先更新订阅组列表，然后再通知网页端订阅组更新
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "UpdateQuoteTagGroup", Msg = "*" });
                }
            }
            else
            {
                this.GroupList.Remove(this.SelectedItem);
            }
        }

        //internal void DelOne()
        //{
        //    if (!string.IsNullOrEmpty(this.SelectedItem.GroupID))
        //    {
        //        this.SelectedItem.Aciton = "3";// 删除
        //        ViewSource.View?.Refresh();
        //    }
        //    else
        //    {
        //        this.GroupList.Remove(this.SelectedItem);
        //    }
        //}

        static List<string> allList = new List<string>() { "0000" }; 
        internal void Save(EditGroupWindow win)
        {
            List<QueryQuoteTagGroupRequest> opsList = new List<QueryQuoteTagGroupRequest>();
            List<FilterGroupItem> itemList = new List<FilterGroupItem>();
            foreach(FilterGroupItem item in this.GroupList)
            {
                if (item.Aciton == "1") //新增
                {
                    QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
                    req.action = "1";
                    req.username = req.closeby = LocalEnv.UserInfo.UserName;
                    req.groupname = item.GroupName_Edit;
                    req.ifSubscribe = item.IfSubscribe_Edit ? "1" : "0";
                    req.marketindicator = "4";

                    if (item.IsGroup_Edit)
                    {
                        req.focusType = "GROUP";
                    }
                    else
                    {
                        req.focusType = "TAG";
                    }

                    req.focusGroupId = item.FocusGroupId_Edit;
                    bool isDYNAMIC_PARAM = false;
                    req.focusTags = item.FocusTags_Edit;

                    if ((!string.IsNullOrEmpty(item.StartTermDY_Edit) || !string.IsNullOrEmpty(item.EndTermDY_Edit)))
                    {
                        req.termRangeStr = $"{item.StartTermDY_Edit},{item.EndTermDY_Edit}";

                        string startT = string.Empty;
                        if (!string.IsNullOrEmpty(item.StartTermDY_Edit))
                        {
                            startT = STC.Common.Utils.CommonUtils.GetDays(item.StartTermDY_Edit).ToString();
                        }

                        string endT = string.Empty;
                        if (!string.IsNullOrEmpty(item.EndTermDY_Edit))
                        {
                            endT = STC.Common.Utils.CommonUtils.GetDays(item.EndTermDY_Edit).ToString();
                        }
                        req.termDays = $"{startT},{endT}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (item.IsCoupon_Edit && !string.IsNullOrEmpty(item.StartCoupon_Edit) || !string.IsNullOrEmpty(item.EndCoupon_Edit))
                    {
                        req.couponRangeStr = $"{item.StartCoupon_Edit},{item.EndCoupon_Edit}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (!string.IsNullOrEmpty(item.StartPrice_Edit) || !string.IsNullOrEmpty(item.EndPrice_Edit))
                    {
                        req.priceRangeStr = $"{item.StartPrice_Edit},{item.EndPrice_Edit}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (!string.IsNullOrEmpty(item.StartChinaBond) || !string.IsNullOrEmpty(item.EndChinaBond))
                    {
                        req.valuationRangeStr = $"{item.StartChinaBond},{item.EndChinaBond}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (!string.IsNullOrEmpty(item.StartBidBp) || !string.IsNullOrEmpty(item.EndBidBp))
                    {
                        req.bidSubValRangeStr = $"{item.StartBidBp},{item.EndBidBp}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (!string.IsNullOrEmpty(item.StartOfrBp) || !string.IsNullOrEmpty(item.EndOfrBp))
                    {
                        req.valSubOfrRangeStr = $"{item.StartOfrBp},{item.EndOfrBp}";
                        isDYNAMIC_PARAM = true; 
                    }

                    if (isDYNAMIC_PARAM)
                    {
                        req.focusType = "DYNAMIC_PARAM";
                    }

                    req.ordersn = item.Ordersn;
                    req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                    req.ip = EngineHelper.GetIp();
                    //req.remark
                    req.createtime = req.updatetime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                    opsList.Add(req);
                    itemList.Add(item);
                }
                else if (item.Aciton == "2")//更新
                {
                    QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
                    req.action = "2";
                    req.id = item.GroupID;
                    req.username = req.closeby = LocalEnv.UserInfo.UserName;
                    req.groupname = item.GroupName_Edit;
                    req.ifSubscribe = item.IfSubscribe_Edit ? "1" : "0";
                    req.focusType = item.Inf1;
                    req.focusGroupId = item.FocusGroupId_Edit;
                    req.securityId = item.Inf2;
                    req.securitySide = item.Inf3;
                    req.marketindicator = "4";
                    //if (item.FocusTagsDic_Edit != null)
                    //{
                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.BONDTYPE))
                    //        req.bondTypes = item.FocusTagsDic_Edit[Constants.BONDTYPE];

                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.TERM))
                    //        req.terms = item.FocusTagsDic_Edit[Constants.TERM];

                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.FIRM))
                    //        req.firms = item.FocusTagsDic_Edit[Constants.FIRM];

                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.SETTLTYPE))
                    //        req.settlTypes = item.FocusTagsDic_Edit[Constants.SETTLTYPE];

                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.SIDE))
                    //        req.sides = item.FocusTagsDic_Edit[Constants.SIDE];

                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.VIP))
                    //        req.ifVip = item.FocusTagsDic_Edit[Constants.VIP];

                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.ROLETYPE))
                    //        req.roleTypes = item.FocusTagsDic_Edit[Constants.ROLETYPE];

                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.ORGTYPE))
                    //        req.orgTypes = item.FocusTagsDic_Edit[Constants.ORGTYPE];
                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.FINBONDTYPE))
                    //        req.finBondTypes = item.FocusTagsDic_Edit[Constants.FINBONDTYPE];
                    //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.DQTYPE))
                    //        req.dqTypes = item.FocusTagsDic_Edit[Constants.DQTYPE];
                    //}

                    req.focusTags = item.FocusTags_Edit;
                    bool isDYNAMIC_PARAM = false;
                    if ((!string.IsNullOrEmpty(item.StartTermDY_Edit) || !string.IsNullOrEmpty(item.EndTermDY_Edit)))
                    {
                        req.termRangeStr = $"{item.StartTermDY_Edit},{item.EndTermDY_Edit}";

                        string startT = string.Empty;
                        if (!string.IsNullOrEmpty(item.StartTermDY_Edit))
                        {
                            startT = STC.Common.Utils.CommonUtils.GetDays(item.StartTermDY_Edit).ToString();
                        }

                        string endT = string.Empty;
                        if (!string.IsNullOrEmpty(item.EndTermDY_Edit))
                        {
                            endT = STC.Common.Utils.CommonUtils.GetDays(item.EndTermDY_Edit).ToString();
                        }
                        req.termDays = $"{startT},{endT}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (item.IsCoupon_Edit && (!string.IsNullOrEmpty(item.StartCoupon_Edit) || !string.IsNullOrEmpty(item.EndCoupon_Edit)))
                    {
                        req.couponRangeStr = $"{item.StartCoupon_Edit},{item.EndCoupon_Edit}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (!string.IsNullOrEmpty(item.StartPrice_Edit) || !string.IsNullOrEmpty(item.EndPrice_Edit))
                    {
                        req.priceRangeStr = $"{item.StartPrice_Edit},{item.EndPrice_Edit}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (!string.IsNullOrEmpty(item.StartChinaBond_Edit) || !string.IsNullOrEmpty(item.EndChinaBond_Edit))
                    {
                        req.valuationRangeStr = $"{item.StartChinaBond_Edit},{item.EndChinaBond_Edit}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (!string.IsNullOrEmpty(item.StartBidBp_Edit) || !string.IsNullOrEmpty(item.EndBidBp_Edit))
                    {
                        req.bidSubValRangeStr = $"{item.StartBidBp_Edit},{item.EndBidBp_Edit}";
                        isDYNAMIC_PARAM = true;
                    }

                    if (!string.IsNullOrEmpty(item.StartOfrBp_Edit) || !string.IsNullOrEmpty(item.EndOfrBp_Edit))
                    {

                        req.valSubOfrRangeStr = $"{item.StartOfrBp_Edit},{item.EndOfrBp_Edit}";
                        isDYNAMIC_PARAM = true;
                    }

                    if ("BOND" != req.focusType)
                    {
                        if (item.IsGroup_Edit)
                        {
                            req.focusType = "GROUP";
                        }
                        else
                        {
                            if (isDYNAMIC_PARAM)
                            {
                                req.focusType = "DYNAMIC_PARAM";
                            }
                            else
                            {
                                req.focusType = "TAG";
                            }
                        }
                    }

                    req.ordersn = item.Ordersn;
                    req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                    req.ip = EngineHelper.GetIp();
                    req.remark = item.Remark;
                    req.updatetime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                    opsList.Add(req);
                    itemList.Add(item);
                }
                else if (item.Aciton == "3")// 删除
                {
                    QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
                    req.action = "3";
                    req.id = item.GroupID;
                    opsList.Add(req);
                    itemList.Add(item);
                }
            }

            if (opsList.Count > 0)
            {
                Task.Run(() => {
                    int index = 0;
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach(QueryQuoteTagGroupRequest req in opsList)
                    {
                        QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);
                        if (reply != null && reply.IsSuccess())
                        {
                            Application.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                try
                                {
                                    if (req.action == "1")
                                    {
                                        FilterGroupItem newItem = new FilterGroupItem();
                                        newItem.GroupID = reply.Datas[0].id;
                                        newItem.UserName = reply.Datas[0].username;
                                        newItem.GroupName = newItem.GroupName_Edit = reply.Datas[0].groupname;
                                        newItem.IfSubscribe = newItem.IfSubscribe_Edit = reply.Datas[0].ifSubscribe == "1" ? true : false;
                                        newItem.FocusTags = newItem.FocusTags_Edit = reply.Datas[0].focusTags;

                                        STC.Common.Utils.CommonUtils.ProcessInf4(reply.Datas[0].inf4, reply.Datas[0].inf1, newItem);
                                        STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, newItem);

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
                                        newItem.Aciton = "0";

                                        this.GroupList_Org.Add(newItem);

                                        itemList[index].GroupID = reply.Datas[0].id;
                                        itemList[index].UserName = reply.Datas[0].username;
                                        itemList[index].GroupName_Edit = itemList[index].GroupName = reply.Datas[0].groupname;
                                        itemList[index].IfSubscribe_Edit = itemList[index].IfSubscribe = reply.Datas[0].ifSubscribe == "1" ? true : false;
                                        itemList[index].FocusTags_Edit = itemList[index].FocusTags = reply.Datas[0].focusTags;
                                        STC.Common.Utils.CommonUtils.ProcessInf4(reply.Datas[0].inf4, reply.Datas[0].inf1, itemList[index]);
                                        STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, itemList[index]);
                                        itemList[index].Inf1 = reply.Datas[0].inf1;
                                        itemList[index].Inf2 = reply.Datas[0].inf2;
                                        itemList[index].Inf3 = reply.Datas[0].inf3;
                                        itemList[index].Inf4 = reply.Datas[0].inf4;
                                        itemList[index].UpdateTime = reply.Datas[0].updatetime;
                                        itemList[index].CreateTime = reply.Datas[0].createtime;
                                        itemList[index].Ordersn = reply.Datas[0].ordersn;
                                        itemList[index].IP = reply.Datas[0].ip;
                                        itemList[index].Terminal = reply.Datas[0].terminal;
                                        itemList[index].Remark = reply.Datas[0].remark;
                                        itemList[index].CloseBy = reply.Datas[0].closeby;
                                        itemList[index].Aciton = "0";
                                    }
                                    else if (req.action == "2")
                                    {
                                        FilterGroupItem updateItem = this.GroupList_Org.First(obj => obj.GroupID == req.id);
                                        updateItem.UserName = reply.Datas[0].username;
                                        updateItem.GroupName = updateItem.GroupName_Edit = reply.Datas[0].groupname;
                                        updateItem.IfSubscribe = updateItem.IfSubscribe_Edit = reply.Datas[0].ifSubscribe == "1" ? true : false;
                                        updateItem.FocusTags = updateItem.FocusTags_Edit = reply.Datas[0].focusTags;

                                        STC.Common.Utils.CommonUtils.ProcessInf4(reply.Datas[0].inf4, reply.Datas[0].inf1, updateItem);
                                        STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, updateItem);

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
                                        updateItem.Aciton = "0";

                                        itemList[index].UserName = reply.Datas[0].username;
                                        itemList[index].GroupName_Edit = itemList[index].GroupName = reply.Datas[0].groupname;
                                        itemList[index].IfSubscribe_Edit = itemList[index].IfSubscribe = reply.Datas[0].ifSubscribe == "1" ? true : false;
                                        itemList[index].FocusTags_Edit = itemList[index].FocusTags = reply.Datas[0].focusTags;
                                        STC.Common.Utils.CommonUtils.ProcessInf4(reply.Datas[0].inf4, reply.Datas[0].inf1, itemList[index]);
                                        STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, itemList[index]);
                                        itemList[index].Inf1 = reply.Datas[0].inf1;
                                        itemList[index].Inf2 = reply.Datas[0].inf2;
                                        itemList[index].Inf3 = reply.Datas[0].inf3;
                                        itemList[index].Inf4 = reply.Datas[0].inf4;
                                        itemList[index].UpdateTime = reply.Datas[0].updatetime;
                                        itemList[index].CreateTime = reply.Datas[0].createtime;
                                        itemList[index].Ordersn = reply.Datas[0].ordersn;
                                        itemList[index].IP = reply.Datas[0].ip;
                                        itemList[index].Terminal = reply.Datas[0].terminal;
                                        itemList[index].Remark = reply.Datas[0].remark;
                                        itemList[index].CloseBy = reply.Datas[0].closeby;
                                        itemList[index].Aciton = "0";
                                    }
                                    else if (req.action == "3")
                                    {
                                        FilterGroupItem removeItem = this.GroupList_Org.First(obj => obj.GroupID == req.id);
                                        this.GroupList_Org.Remove(removeItem);

                                        this.GroupList.Remove(itemList[index]);
                                    }
                                }
                                catch (System.InvalidOperationException)
                                {
                                    // ignore
                                }
                            }));
                            index++;

                            //Common.Utils.CommonUtils.Prompt2Second("保存", "成功");
                        }
                        else
                        {
                            string errMsg = (reply == null || string.IsNullOrEmpty(reply.Msg)) ? "失败" : reply.Msg;
                            stringBuilder.AppendLine($"{req.groupname}: {errMsg}");
                        }
                    }
                    if (stringBuilder.Length > 0)
                    {
                        Common.Utils.CommonUtils.Prompt2Second("保存", stringBuilder.ToString(), win);
                        win.EditAction?.Invoke();
                    }
                    else
                    {
                        
                        win.EditAction?.Invoke();
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            win.Close();
                        }));
                        Common.Utils.CommonUtils.Prompt2Second("保存", "保存成功");
                    }
                    // Msg 为 * 表示需要先更新订阅组列表，然后再通知网页端订阅组更新
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new Common.Model.Fix.Message.PResult() { Code = "UpdateQuoteTagGroup", Msg = "*"});
                });
            }
            else
            {
                Common.Utils.CommonUtils.Prompt2Second("保存", "无保存数据", win);
            }

            
        }

        private void Sub_Save(FilterGroupItem item)
        {
            if (string.IsNullOrEmpty(item.GroupID))
            {
                // 新增
                QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
                req.action = "1";
                req.username = req.closeby = LocalEnv.UserInfo.UserName;
                req.groupname = item.GroupName_Edit;
                req.ifSubscribe = item.IfSubscribe_Edit ? "1" : "0";
                req.focusType = "TAG";
                req.marketindicator = "4";
                //if(item.FocusTagsDic_Edit != null)
                //{
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.BONDTYPE))
                //        req.bondTypes = item.FocusTagsDic_Edit[Constants.BONDTYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.TERM))
                //        req.terms = item.FocusTagsDic_Edit[Constants.TERM];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.FIRM))
                //        req.firms = item.FocusTagsDic_Edit[Constants.FIRM];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.SETTLTYPE))
                //        req.settlTypes = item.FocusTagsDic_Edit[Constants.SETTLTYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.SIDE))
                //        req.sides = item.FocusTagsDic_Edit[Constants.SIDE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.VIP))
                //        req.ifVip = item.FocusTagsDic_Edit[Constants.VIP];
                //    else
                //    {
                //        req.ifVip = new List<string>();
                //        req.ifVip.Add("0010");
                //    }
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.ROLETYPE))
                //        req.roleTypes = item.FocusTagsDic_Edit[Constants.ROLETYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.ORGTYPE))
                //        req.orgTypes = item.FocusTagsDic_Edit[Constants.ORGTYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.FINBONDTYPE))
                //        req.finBondTypes = item.FocusTagsDic_Edit[Constants.FINBONDTYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.DQTYPE))
                //        req.dqTypes = item.FocusTagsDic_Edit[Constants.DQTYPE];
                //}    

                req.focusTags = item.FocusTags_Edit;
                req.ordersn = item.Ordersn;
                req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                req.ip = EngineHelper.GetIp();
                //req.remark
                req.createtime = req.updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);

                if (reply != null && reply.IsSuccess())
                {
                    try
                    {
                        FilterGroupItem newItem = new FilterGroupItem();
                        newItem.GroupID = reply.Datas[0].id;
                        newItem.UserName = reply.Datas[0].username;
                        newItem.GroupName = newItem.GroupName_Edit = reply.Datas[0].groupname;
                        newItem.IfSubscribe = newItem.IfSubscribe_Edit = reply.Datas[0].ifSubscribe == "1" ? true : false;
                        newItem.FocusTags = newItem.FocusTags_Edit = reply.Datas[0].focusTags;
                        newItem.Inf1 = reply.Datas[0].inf1;
                        newItem.Inf2 = reply.Datas[0].inf2;
                        newItem.Inf3 = reply.Datas[0].inf3;
                        newItem.UpdateTime = reply.Datas[0].updatetime;
                        newItem.CreateTime = reply.Datas[0].createtime;
                        newItem.Ordersn = reply.Datas[0].ordersn;
                        newItem.IP = reply.Datas[0].ip;
                        newItem.Terminal = reply.Datas[0].terminal;
                        newItem.Remark = reply.Datas[0].remark;
                        newItem.CloseBy = reply.Datas[0].closeby;

                        this.GroupList_Org.Add(newItem);
                    }
                    catch (System.InvalidOperationException)
                    {
                        // ignore
                    }

                    item.GroupID = reply.Datas[0].id;
                    item.UserName = reply.Datas[0].username;
                    item.GroupName = item.GroupName_Edit = reply.Datas[0].groupname;
                    item.IfSubscribe = item.IfSubscribe_Edit = reply.Datas[0].ifSubscribe == "1" ? true : false;
                    item.FocusTags = item.FocusTags_Edit = reply.Datas[0].focusTags;
                    item.Inf1 = reply.Datas[0].inf1;
                    item.Inf2 = reply.Datas[0].inf2;
                    item.Inf3 = reply.Datas[0].inf3;
                    item.UpdateTime = reply.Datas[0].updatetime;
                    item.CreateTime = reply.Datas[0].createtime;
                    item.Ordersn = reply.Datas[0].ordersn;
                    item.IP = reply.Datas[0].ip;
                    item.Terminal = reply.Datas[0].terminal;
                    item.Remark = reply.Datas[0].remark;
                    item.CloseBy = reply.Datas[0].closeby;

                    Common.Utils.CommonUtils.Prompt2Second("保存", "成功");
                }
                else
                {
                    Common.Utils.CommonUtils.Prompt2Second("保存", (reply == null || string.IsNullOrEmpty(reply.Msg)) ? "失败" : reply.Msg);
                }
            }
            else
            {
                // 更新
                QueryQuoteTagGroupRequest req = new QueryQuoteTagGroupRequest();
                req.action = "2";
                req.id = item.GroupID;
                req.username = req.closeby = LocalEnv.UserInfo.UserName;
                req.groupname = item.GroupName_Edit;
                req.ifSubscribe = item.IfSubscribe_Edit ? "1" : "0";
                req.focusType = "TAG";
                req.marketindicator = "4";
                //if (item.FocusTagsDic_Edit != null)
                //{
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.BONDTYPE))
                //        req.bondTypes = item.FocusTagsDic_Edit[Constants.BONDTYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.TERM))
                //        req.terms = item.FocusTagsDic_Edit[Constants.TERM];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.FIRM))
                //        req.firms = item.FocusTagsDic_Edit[Constants.FIRM];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.SETTLTYPE))
                //        req.settlTypes = item.FocusTagsDic_Edit[Constants.SETTLTYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.SIDE))
                //        req.sides = item.FocusTagsDic_Edit[Constants.SIDE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.VIP))
                //        req.ifVip = item.FocusTagsDic_Edit[Constants.VIP];
                //    else
                //    {
                //        req.ifVip = new List<string>();
                //        req.ifVip.Add("0010");
                //    }
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.ROLETYPE))
                //        req.roleTypes = item.FocusTagsDic_Edit[Constants.ROLETYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.ORGTYPE))
                //        req.orgTypes = item.FocusTagsDic_Edit[Constants.ORGTYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.FINBONDTYPE))
                //        req.finBondTypes = item.FocusTagsDic_Edit[Constants.FINBONDTYPE];
                //    if (item.FocusTagsDic_Edit.ContainsKey(Constants.DQTYPE))
                //        req.dqTypes = item.FocusTagsDic_Edit[Constants.DQTYPE];
                //}
                req.focusTags = item.FocusTags_Edit;
                req.ordersn = item.Ordersn;
                req.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                req.ip = EngineHelper.GetIp();
                //req.remark
                req.createtime = req.updatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                QueryQuoteTagGroupReplyPo reply = TableOldHandler.Instance.QueryQuoteTagGroup(req);

                if (reply != null && reply.IsSuccess())
                {
                    try
                    {
                        FilterGroupItem updateItem = this.GroupList_Org.First(obj => obj.GroupID == this.SelectedItem.GroupID);
                        updateItem.UserName = reply.Datas[0].username;
                        updateItem.GroupName = updateItem.GroupName_Edit = reply.Datas[0].groupname;
                        updateItem.IfSubscribe = updateItem.IfSubscribe_Edit = reply.Datas[0].ifSubscribe == "1" ? true : false;
                        updateItem.FocusTags = updateItem.FocusTags_Edit = reply.Datas[0].focusTags;
                        updateItem.Inf1 = reply.Datas[0].inf1;
                        updateItem.Inf2 = reply.Datas[0].inf2;
                        updateItem.Inf3 = reply.Datas[0].inf3;
                        updateItem.UpdateTime = reply.Datas[0].updatetime;
                        updateItem.CreateTime = reply.Datas[0].createtime;
                        updateItem.Ordersn = reply.Datas[0].ordersn;
                        updateItem.IP = reply.Datas[0].ip;
                        updateItem.Terminal = reply.Datas[0].terminal;
                        updateItem.Remark = reply.Datas[0].remark;
                        updateItem.CloseBy = reply.Datas[0].closeby;
                    }
                    catch (System.InvalidOperationException)
                    {
                        // ignore
                    }

                    item.UserName = reply.Datas[0].username;
                    item.GroupName = item.GroupName_Edit = reply.Datas[0].groupname;
                    item.IfSubscribe = item.IfSubscribe_Edit = reply.Datas[0].ifSubscribe == "1" ? true : false;
                    item.FocusTags = item.FocusTags_Edit = reply.Datas[0].focusTags;
                    item.Inf1 = reply.Datas[0].inf1;
                    item.Inf2 = reply.Datas[0].inf2;
                    item.Inf3 = reply.Datas[0].inf3;
                    item.UpdateTime = reply.Datas[0].updatetime;
                    item.CreateTime = reply.Datas[0].createtime;
                    item.Ordersn = reply.Datas[0].ordersn;
                    item.IP = reply.Datas[0].ip;
                    item.Terminal = reply.Datas[0].terminal;
                    item.Remark = reply.Datas[0].remark;
                    item.CloseBy = reply.Datas[0].closeby;

                    Common.Utils.CommonUtils.Prompt2Second("保存", "成功");
                }
                else
                {
                    Common.Utils.CommonUtils.Prompt2Second("保存", (reply == null || string.IsNullOrEmpty(reply.Msg)) ? "失败" : reply.Msg);
                }
            }
        }

        internal bool IsNeedUpdate()
        {
            foreach (FilterGroupItem item in this.GroupList)
            {
                if (item.Aciton != "0")
                {
                    return true;
                }
            }
            return false;
        }

        #region 券池

        public ICommand GroupManagerCommand { get; set; }

        //策略债券组
        public const string G_Id = "Id";
        public const string G_Groupname = "Groupname";
        public const string G_BondNum = "BondNum";
        public const string G_BondCode = "BondCode";
        public const string G_updatetime = "updatetime";
        public const string G_GroupTextVisible = "GroupTextVisible";
        public const string G_GroupTextVisibleNo = "GroupTextVisibleNo";
        public const string G_IsChecked = "IsChecked";
        public const string CLOSEBY_FIELD = "Closeby";
        public const string USERNAME_FIELD = "Username";

        /// <summary>
        /// 资源锁
        /// </summary>
        private object _lockObj = new object();
        public object LockObj
        {
            get
            {
                return _lockObj;
            }
        }


        private DataRowView _selectedGroupItem;
        public DataRowView SelectedGroupItem
        {
            get
            {
                return _selectedGroupItem;
            }

            set
            {
                _selectedGroupItem = value;
                this.OnPropertyChanged();
            }
        }

        private DataView _GroupView;
        public DataView GroupView
        {
            get
            {
                lock (this.LockObj)
                {
                    if (_GroupView == null)
                    {
                        _GroupView = new DataTable().DefaultView;
                        _GroupView.Table.Columns.Add(G_IsChecked, typeof(bool));
                        _GroupView.Table.Columns[G_IsChecked].DefaultValue = false;
                        _GroupView.Table.Columns.Add(G_Id, typeof(string));
                        _GroupView.Table.Columns.Add(G_Groupname, typeof(string));
                        _GroupView.Table.Columns.Add(G_BondNum, typeof(string));
                        _GroupView.Table.Columns.Add(G_BondCode, typeof(string));
                        _GroupView.Table.Columns.Add(G_updatetime, typeof(string));
                        _GroupView.Table.Columns.Add(CLOSEBY_FIELD, typeof(string));
                        _GroupView.Table.Columns.Add(USERNAME_FIELD, typeof(string));
                        _GroupView.Table.Columns.Add(G_GroupTextVisible, typeof(string));
                        _GroupView.Table.Columns.Add(G_GroupTextVisibleNo, typeof(string));
                        _GroupView.Table.PrimaryKey = new DataColumn[] {
                        _GroupView.Table.Columns[G_Id]
                    };
                        this.OnPropertyChanged();
                    }
                    return this._GroupView;
                }
            }
        }

        public const string _ModelId = "4007";
        public string user = ApiManager.Instance.GetDefaultApi().UserName;

        public void initGroup()
        {
            List<ModulesAction> Action = AdminHandler.Instance.GetModulesActionById(_ModelId);
            if (Action != null && Action.Count > 0)
            {
                foreach (ModulesAction m in Action)
                {
                    if ("400702".Equals(m.id))//管理自己&查看全部
                    {
                        user = "*";
                    }
                    else if ("400703".Equals(m.id))//管理全部
                    {
                        user = "*";
                    }
                    else
                    {//400701管理自己
                    }
                    break;
                }
            }

            HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
            //request.type = "Quant";
            //request.username = "*".Equals(user) ? "" : user;
            if ("*".Equals(AdminHandler.Instance.getSecurityGroupUser()))
            {
                request.username = "";
            }
            else
            {
                request.username = LocalEnv.UserInfo.UserName;
            }
            HomeGroupSecurityidsReplyPo replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery2(request);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (replyPo != null)
                {
                    if (replyPo != null && replyPo.Code == "0000" && replyPo.Datas != null)
                    {
                        var templist = replyPo.Datas.OrderByDescending(a => a.Value.updatetime).ToList();
                        foreach (KeyValuePair<string, HomeGroupInfo> pair in templist)
                        {
                            HomeGroupInfo homeGroupInfo = pair.Value;
                            if (!string.IsNullOrWhiteSpace(homeGroupInfo.inf2))
                            {
                                GroupMsgPo groupMsgPo = JsonConvert.DeserializeObject<GroupMsgPo>(homeGroupInfo.inf2);
                                Dictionary<string, string> tagTempDic = new Dictionary<string, string>();
                                if (!string.IsNullOrWhiteSpace(groupMsgPo.Tag))
                                {
                                    string[] splits = groupMsgPo.Tag.Split(' ');
                                    foreach (string tag in splits)
                                    {
                                        if (Consts.TagDic.ContainsValue(tag))
                                        {
                                            string key = Consts.TagDic.FirstOrDefault(q => q.Value.Equals(tag)).Key;
                                            tagTempDic.Add(key, tag);
                                        }
                                    }
                                }
                                homeGroupInfo.tagDic = tagTempDic;
                                homeGroupInfo.groupMsgPo = groupMsgPo;
                            }
                            if (homeGroupInfo.tagDic != null && homeGroupInfo.tagDic.ContainsKey(CommonConstants.INNER_Type))
                            {
                                UpdateGroupList(homeGroupInfo);
                            }
                        }
                        if (this.SelectedGroupItem == null)
                            this.SelectedGroupItem = this.GroupView.Count > 0 ? this.GroupView[0] : null;
                    }
                }

                if (this.SelectedItem != null)
                {
                    foreach (DataRowView drv in GroupView)
                    {
                        if (!string.IsNullOrEmpty(SelectedItem.FocusGroupId_Edit) && SelectedItem.FocusGroupId_Edit.Contains(drv[G_Id].ToString()))
                        {
                            drv[G_IsChecked] = true;
                        }
                        else
                        {
                            drv[G_IsChecked] = false;
                        }
                    }
                }
            }));
        }

        private void UpdateGroupList(HomeGroupInfo obj)
        {
            try
            {
                lock (LockObj)
                {
                    DataRow Item = this.GroupView.Table.Rows.Find(new object[] { obj.id });
                    if (Item == null)
                    {
                        Item = this.GroupView.Table.NewRow();
                        Item[G_Id] = obj.id;
                        Item[G_Groupname] = obj.groupname;
                        if (obj != null && obj.securityids != null && obj.securityids.Count > 0)
                        {
                            StringBuilder str = new StringBuilder();
                            foreach (HomeGroupSecurityIDsInfo homeSecurtity in obj.securityids)
                            {
                                str.Append(homeSecurtity.securityid);
                                str.Append(",");
                            }
                            str.Remove(str.Length - 1, 1);
                            Item[G_BondNum] = obj.securityids.Count;
                            Item[G_BondCode] = str;
                        }
                        else
                            Item[G_BondNum] = 0;
                        Item[CLOSEBY_FIELD] = obj.closeby;
                        Item[USERNAME_FIELD] = obj.username;
                        Item[G_updatetime] = obj.updatetime;
                        Item[G_GroupTextVisible] = "1";
                        Item[G_GroupTextVisibleNo] = "0";
                        this.GroupView.Table.Rows.Add(Item);
                    }
                    this.GroupView.Table.AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public void clearGroup()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    lock (LockObj)
                    {
                        this.GroupView.Table.Rows.Clear();
                        this.GroupView.Table.AcceptChanges();
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }

        private void GroupManager()
        {
            DesignHandler.Instance.ShowStrategyGroupManagementWin(null);
        }

        internal void SetCurrentGroupInf2()
        {
            //if (this.SelectedItem.IsGroup_Edit)
            {
                int count = 0;
                StringBuilder inf2_sb = new StringBuilder();
                lock(LockObj)
                {
                    foreach (DataRowView drv in this.GroupView)
                    {
                        if ((bool)drv[G_IsChecked] == true)
                        {
                            count++;
                            string id = drv[G_Id].ToString();
                            if (inf2_sb.Length > 0)
                            {
                                inf2_sb.Append(CommonConstants.COMMA_CHAR);
                            }
                            inf2_sb.Append(id);
                        }
                    }
                }
                this.SelectedItem.FocusGroupId_Edit = inf2_sb.ToString();
                this.SelectedItem.CheckCount = count;
            }
        }
        private void Instance_OnNotifyUpdateGroup(object obj)
        {
            clearGroup();
            initGroup();
        }

        #endregion
    }
}
