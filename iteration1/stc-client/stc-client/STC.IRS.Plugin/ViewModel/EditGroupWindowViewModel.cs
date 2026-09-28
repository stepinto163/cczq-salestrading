using Newtonsoft.Json;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Data;
using STC.IRS.Plugin.Utils;
using STC.IRS.Plugin.View;
using STC.IRS.Plugin.View.Component;
using STC.IRS.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.IRS.Plugin.ViewModel
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
            InitGroupTag();
        }

        private void InitGroupTag()
        {
            ThreadPoolHelper.InvokeThreadPool(o => {
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
                        if (group.marketindicator != "2")
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
                        item.FocusTags_Edit = item.FocusTags = group.focusTags;

                        STC.Common.Utils.CommonUtils.ProcessInf3(group.inf3, group.inf1, item);

                        //Dictionary<string, List<string>> dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(group.focusTags);
                        //item.FocusTagsDic = dic;
                        item.IfSubscribe_Edit = item.IfSubscribe = group.ifSubscribe == "1" ? true : false;
                        this.GroupList.Add(item);
                    }

                    if (GroupList != null && GroupList.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(_defaultSelectedGroupID))
                        {
                            try
                            {
                                this.SelectedItem = this.GroupList.First(item => item.GroupID == _defaultSelectedGroupID);
                            }
                            catch (InvalidOperationException)
                            {
                                this.SelectedItem = this.GroupList[0];
                            }
                        }
                        else
                        {
                            this.SelectedItem = this.GroupList[0];
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
            groupItem.FocusTags_Edit = groupItem.FocusTags = InnerBoardFilterPanel_New.DEFAULT_FOCUSTAGS;
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
                    req.focusType = "TAG";
                    req.marketindicator = "2";

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

                    if (item.IsPrice_Edit && !string.IsNullOrEmpty(item.StartPrice_Edit) || !string.IsNullOrEmpty(item.EndPrice_Edit))
                    {
                        req.priceRangeStr = $"{item.StartPrice_Edit},{item.EndPrice_Edit}";
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
                    req.securityId = item.Inf2;
                    req.securitySide = item.Inf3;
                    req.marketindicator = "2";
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

                    if (item.IsPrice_Edit && (!string.IsNullOrEmpty(item.StartPrice_Edit) || !string.IsNullOrEmpty(item.EndPrice_Edit)))
                    {
                        req.priceRangeStr = $"{item.StartPrice_Edit},{item.EndPrice_Edit}";
                        isDYNAMIC_PARAM = true;
                    }

                    if ("BOND" != req.focusType)
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

                                        STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, newItem);

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
                                        newItem.Aciton = "0";

                                        this.GroupList_Org.Add(newItem);

                                        itemList[index].GroupID = reply.Datas[0].id;
                                        itemList[index].UserName = reply.Datas[0].username;
                                        itemList[index].GroupName_Edit = itemList[index].GroupName = reply.Datas[0].groupname;
                                        itemList[index].IfSubscribe_Edit = itemList[index].IfSubscribe = reply.Datas[0].ifSubscribe == "1" ? true : false;
                                        itemList[index].FocusTags_Edit = itemList[index].FocusTags = reply.Datas[0].focusTags;
                                        STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, itemList[index]);
                                        itemList[index].Inf1 = reply.Datas[0].inf1;
                                        itemList[index].Inf2 = reply.Datas[0].inf2;
                                        itemList[index].Inf3 = reply.Datas[0].inf3;
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

                                        STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, updateItem);

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
                                        updateItem.Aciton = "0";

                                        itemList[index].UserName = reply.Datas[0].username;
                                        itemList[index].GroupName_Edit = itemList[index].GroupName = reply.Datas[0].groupname;
                                        itemList[index].IfSubscribe_Edit = itemList[index].IfSubscribe = reply.Datas[0].ifSubscribe == "1" ? true : false;
                                        itemList[index].FocusTags_Edit = itemList[index].FocusTags = reply.Datas[0].focusTags;
                                        STC.Common.Utils.CommonUtils.ProcessInf3(reply.Datas[0].inf3, reply.Datas[0].inf1, itemList[index]);
                                        itemList[index].Inf1 = reply.Datas[0].inf1;
                                        itemList[index].Inf2 = reply.Datas[0].inf2;
                                        itemList[index].Inf3 = reply.Datas[0].inf3;
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
                });
            }
            else
            {
                Common.Utils.CommonUtils.Prompt2Second("保存", "无保存数据", win);
            }

            //// to do
            //if (_selectedItem != null)
            //{
            //    Sub_Save(_selectedItem);
            //}
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
    }
}
