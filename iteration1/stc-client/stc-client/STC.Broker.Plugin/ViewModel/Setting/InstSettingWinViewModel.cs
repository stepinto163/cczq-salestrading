using STC.Broker.Plugin.View.Setting;
using STC.Common.Handler.PT;
using STC.Common.Model;
using STC.Common.Model.PT;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.ViewModel.Setting
{
    public class InstSettingWinViewModel : BaseViewModel
    {
        /// <summary>
        /// key groupId; value IssueGroup
        /// </summary>
        private Dictionary<string, IssueGroup> issueGroupDic = new Dictionary<string, IssueGroup>();

        /// <summary>
        /// key groupId; value List IssueInst
        /// </summary>
        private Dictionary<string, List<IssueInst>> issueInstDic = new Dictionary<string, List<IssueInst>>();

        /// <summary>
        /// key instId; value List IssueInstTrader
        /// </summary>
        private Dictionary<string, List<IssueInstTrader>> issueInstTraderDic = new Dictionary<string, List<IssueInstTrader>>();


        /// <summary>
        /// 绑定分组
        /// </summary>
        public ObservableCollection<IssueGroupViewModel> IssueGroupViewModels { get; set; } = new ObservableCollection<IssueGroupViewModel>();


        /// <summary>
        /// 搜索定位的groupid
        /// </summary>
        public string CurrentGroupId = "";


        private bool isAllSelect;
        /// <summary>
        /// 全选
        /// </summary>
        public bool IsAllSelect
        {
            get
            {
                return isAllSelect;
            }
            set
            {
                isAllSelect = value;
                this.OnPropertyChanged();
            }
        }


        private bool isEnableButton;
        /// <summary>
        /// 按钮是否可用
        /// </summary>
        public bool IsEnableButton
        {
            get
            {
                return isEnableButton;
            }
            set
            {
                isEnableButton = value;
                this.OnPropertyChanged();
            }
        }


        private bool isAllRequestSelect;
        /// <summary>
        /// 全选
        /// </summary>
        public bool IsAllRequestSelect
        {
            get
            {
                return isAllRequestSelect;
            }
            set
            {
                isAllRequestSelect = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 绑定分组下机构
        /// </summary>
        public ObservableCollection<IssueInstViewModel> IssueInstViewModels { get; set; } = new ObservableCollection<IssueInstViewModel>();


        private Dictionary<string, int> oldPriorityDic = new Dictionary<string, int>();


        private bool defualtRequest;
        public bool DefualtRequest
        {
            get
            {
                return defualtRequest;
            }
            set
            {
                defualtRequest = value;
                this.OnPropertyChanged();
            }
        }

        private InstSettingWin mainWin;


        public InstSettingWinViewModel(InstSettingWin win)
        {
            mainWin = win;

            IssueGroupViewModels.CollectionChanged += IssueGroupViewModels_CollectionChanged;
            InitData();
        }

        public async void InitData()
        {
            // 查询分组
            await GetData(QueryType.GroupSearch, null, null);

            // 查询分组下机构
            if (issueGroupDic.Count > 0)
            {
                var defaultGroup = issueGroupDic.First().Value;
                await GetData(QueryType.InstSearch, defaultGroup.groupId, null);

                // 绑定默认分组机构
                SetInstData(defaultGroup.groupId);
                mainWin.GroupList.SelectedIndex = 0;
            }
            else
            {
                mainWin.tb_InstCount.Text = "0家";
            }

        }

        //获取选中行

        private IssueGroupViewModel oldGroup;
        private async void IssueGroupViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count == 1 && e.OldItems[0] is IssueGroupViewModel)
            {
                IssueGroupViewModel changed = e.OldItems[0] as IssueGroupViewModel;
                oldGroup = changed;
            }
            else if (e.Action == NotifyCollectionChangedAction.Add
                && oldGroup != null
                && e.NewItems.Count == 1
                && e.NewItems[0] is IssueGroupViewModel
                )
            {
                IssueGroupViewModel newGroup = e.NewItems[0] as IssueGroupViewModel;
                if (oldGroup.GroupId == newGroup.GroupId)
                {
                    oldGroup = null;
                    await UpdateGroupPriority(newGroup);
                }
            }
        }
        
        public void AddInst(string groupid, List<IssueInst> addInstList)
        {
            if (!issueInstDic.ContainsKey(groupid))
            {
                issueInstDic[groupid] = new List<IssueInst>();
            }
            foreach (IssueInst issueInst in addInstList)
            {
                if (!issueInstDic[groupid].Any(_ => _.id == issueInst.id))
                {
                    issueInstDic[groupid].Add(issueInst);
                }
            }
            issueGroupDic[groupid].instInfos = issueInstDic[groupid];
        }

        /// <summary>
        /// 绑定机构数据
        /// </summary>
        /// <param name="groupId"></param>
        public void SetInstData(string groupId)
        {
            DefualtRequest = issueGroupDic[groupId].defaultRequest == "1";
            var instList = issueInstDic[groupId];
            IssueInstViewModels.Clear();
            IsAllSelect = false;
            IsAllRequestSelect = false;
            bool isSelected = true;
            if (instList != null && instList.Count > 0)
            {
                foreach (var item in instList)
                {
                    if (isSelected)
                    {
                        if (item.defaultRequest != "1")
                        {
                            isSelected = false;
                        }
                    }
                    IssueInstViewModels.Add(new IssueInstViewModel(item));
                }
                IsAllRequestSelect = isSelected;
                mainWin.tb_InstCount.Text = IssueInstViewModels.Count.ToString() + "家";
            }
            else
            {
                IsAllRequestSelect = false;
                mainWin.tb_InstCount.Text = "0家";
            }
        }


        /// <summary>
        /// 查询接口
        /// </summary>
        public async static Task<QueryIssueGroupReply> GetInfo(QueryIssueGroupRequest request = null)
        {
            QueryIssueGroupReply result = null;
            await Task.Run(() =>
            {
                if (request == null)
                {
                    request = new QueryIssueGroupRequest { action = (int)ActionType.Query };
                }

                result = GRHandler.Instance.QueryIssueGroupSetting(request);

                if (result != null && result.datas != null && (!string.IsNullOrEmpty(request.groupId) || !string.IsNullOrEmpty(request.instId)))
                {
                    List<IssueGroup> removeList = new List<IssueGroup>();
                    foreach(IssueGroup ig in result.datas)
                    {
                        bool delFlag = false;
                        if (!string.IsNullOrEmpty(request.groupId))
                        {
                            delFlag = ig.groupId != request.groupId;
                        }
                        if (!delFlag && !string.IsNullOrEmpty(request.instId))
                        {
                            delFlag = !ig.instInfos.Any(item => item.instId != request.instId);
                        }
                        if (delFlag)
                        {
                            removeList.Add(ig);
                        }
                    }
                    foreach(IssueGroup ig in removeList)
                    {
                        result.datas.Remove(ig);
                    }
                }
            });
            return result;
        }

        /// <summary>
        /// 查询接口
        /// </summary>
        /// <param name="queryType"></param>
        /// <param name="groupId"></param>
        /// <param name="instId"></param>
        public async Task GetData(QueryType queryType, string groupId, string instId)
        {
            switch (queryType)
            {
                case QueryType.GroupSearch:
                    {
                        var response = await GetInfo();
                        if (response.IsSuccess())
                        {
                            IssueGroupViewModels.Clear();
                            IEnumerable<IssueGroup> sortedList = response.datas.OrderBy(item => item.priority);
                            foreach (var item in sortedList)
                            {
                                issueGroupDic[item.groupId] = item;
                                IssueGroupViewModels.Add(new IssueGroupViewModel(item));
                                oldPriorityDic[item.groupId] = item.priority;
                            }

                            if (IssueGroupViewModels.Count >= 10)
                            {
                                mainWin.btnUpdate.IsEnabled = false;
                            }
                            else
                            {
                                mainWin.btnUpdate.IsEnabled = true;
                            }
                        }
                        else
                        {
                            mainWin.btnUpdate.IsEnabled = true;
                        }
                        break;
                    }
                case QueryType.InstSearch:
                    {
                        var response = await GetInfo(new QueryIssueGroupRequest { action = (int)ActionType.Query, groupId = groupId });
                        if (response != null && response.datas.Count() > 0)
                        {
                            issueInstDic[groupId] = response.datas[0].instInfos;
                        }
                        break;
                    }
                case QueryType.InstTraderSearch:
                    {
                        var response = await GetInfo(new QueryIssueGroupRequest { action = (int)ActionType.Query, instId = instId });
                        if (response != null && response.datas.Count() > 0 && response.datas[0].instInfos.Count > 0)
                        {
                            issueInstTraderDic[instId] = response.datas[0].instInfos[0].instTraders;
                        }
                        break;
                    }
                case QueryType.InstOfGroup:
                    {
                        var response = await GetInfo(new QueryIssueGroupRequest { action = (int)ActionType.Query, instId = instId });
                        if (response.IsSuccess())
                        {
                            CurrentGroupId = response.datas[0].groupId;
                            if (string.IsNullOrWhiteSpace(CurrentGroupId)
                                && response.datas[0].instInfos != null && response.datas[0].instInfos.Count > 0)
                            {
                                CurrentGroupId = response.datas[0].instInfos[0].groupId;
                            }

                            if (!string.IsNullOrWhiteSpace(CurrentGroupId))
                            {
                                issueInstDic[CurrentGroupId] = response.datas[0].instInfos;
                            }
                        }
                        break;
                    }
            }
        }


        public async Task<BaseReplyPo> UpdateData(OperateType operateType, List<IssueGroup> groups)
        {
            BaseReplyPo reuslt = null;
            await Task.Run(() =>
            {
                reuslt = GRHandler.Instance.UpdateIssueGroupSetting(new IssueGroupSettingRequest { action = (int)operateType, groupInfos = groups });
            });
            return reuslt;
        }

        /// <summary>
        /// 请求后台，添加分组
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="priority"></param>
        public async Task<bool> AddGroup(string groupName, int priority)
        {
            bool isSuccess = false;
            var oldIndex = mainWin.GroupList.SelectedIndex;
            var response = await UpdateData(OperateType.Add, new List<IssueGroup> { new IssueGroup { groupName = groupName, priority = priority } });
            if (response.IsSuccess())
            {
                isSuccess = true;
                // 再刷新一次分组
                await GetData(QueryType.GroupSearch, null, null);

                mainWin.GroupList.SelectedIndex = oldIndex;
            }
            else
            {
                isSuccess = false;
                SysWarnGUI warn = new SysWarnGUI();
                warn.Owner = mainWin;
                warn.setWarnMsg(response.Msg);
                warn.ShowDialog();
            }

            return isSuccess;

        }

        /// <summary>
        /// 请求后台，删除分组
        /// </summary>
        /// <param name="groupId"></param>
        public async Task DeleteGroup(string groupId)
        {
            var group = IssueGroupViewModels.FirstOrDefault(x => x.GroupId.Equals(groupId));
            if (group != null)
            {
                var index = IssueGroupViewModels.IndexOf(group);
                var response = await UpdateData(OperateType.Delete, new List<IssueGroup> { new IssueGroup { groupId = groupId, oldPriority = group.Priority } });

                if (response.IsSuccess())
                {
                    // 再刷新一次分组
                    await GetData(QueryType.GroupSearch, null, null);
                    if (IssueGroupViewModels.Count > 0)
                    {
                        mainWin.GroupList.SelectedIndex = IssueGroupViewModels.Count <= index ? IssueGroupViewModels.Count - 1 : index;
                        IssueGroupViewModel issueGroupView = mainWin.GroupList.Items[mainWin.GroupList.SelectedIndex] as IssueGroupViewModel;
                        //再次刷新机构列表
                        await GetData(QueryType.InstSearch, issueGroupView.GroupId, null);
                        SetInstData(issueGroupView.GroupId);
                    }
                    else
                    {
                        IssueInstViewModels.Clear();
                        mainWin.tb_InstCount.Text = IssueInstViewModels.Count.ToString() + "家";
                    }
                }
                else
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    warn.Owner = mainWin;
                    warn.setWarnMsg(response.Msg);
                    warn.ShowDialog();
                }
            }
        }

        /// <summary>
        /// 修改组名，默认请求选项
        /// </summary>
        public async Task<bool> UpdateGroupInfo(string groupId, string groupName)
        {
            bool isSuccess = false;
            var group = IssueGroupViewModels.FirstOrDefault(x => x.GroupId.Equals(groupId));
            if (group != null)
            {
                var index = IssueGroupViewModels.IndexOf(group);
                var response = await UpdateData(OperateType.UpdateGroupInfo, new List<IssueGroup> { new IssueGroup { groupName = groupName, groupId = groupId, defaultRequest = DefualtRequest ? "1" : "2" } });
                if (response.IsSuccess())
                {
                    isSuccess = true;
                    // 再刷新一次分组
                    await GetData(QueryType.GroupSearch, null, null);

                    // 根据分组名字设置选中项
                    var addedGroup = IssueGroupViewModels.FirstOrDefault(x => x.GroupName.Equals(groupName));
                    if (addedGroup != null)
                    {
                        mainWin.GroupList.SelectedIndex = index;
                    }
                }
                else
                {
                    isSuccess = false;
                    SysWarnGUI warn = new SysWarnGUI();
                    warn.Owner = mainWin;
                    warn.setWarnMsg(response.Msg);
                    warn.ShowDialog();
                    // 再刷新一次分组
                    await GetData(QueryType.GroupSearch, null, null);

                    // 根据分组名字设置选中项
                    var addedGroup = IssueGroupViewModels.FirstOrDefault(x => x.GroupName.Equals(groupName));
                    if (addedGroup != null)
                    {
                        mainWin.GroupList.SelectedIndex = index;
                    }
                }
            }
            return isSuccess;
        }

        /// <summary>
        /// 修改分组优先级
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="priority"></param>
        /// <param name="oldPriority"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateGroupPriority(IssueGroupViewModel newGroup)
        {
            var isSuccess = false;
            var list = new List<IssueGroup>();
            int newPriority = 1;
            foreach (var item in IssueGroupViewModels)
            {
                list.Add(new IssueGroup { groupId = item.GroupId, priority = newPriority++, oldPriority = oldPriorityDic[item.GroupId] });
            }

            var response = await UpdateData(OperateType.UpdatePriority, list);
            if (response.IsSuccess())
            {
                isSuccess = true;
                // 再刷新一次分组
                await GetData(QueryType.GroupSearch, null, null);

                // 根据分组名字设置选中项
                mainWin.GroupList.SelectedIndex = IssueGroupViewModels.IndexOf(IssueGroupViewModels.FirstOrDefault(x => x.GroupId == newGroup.GroupId));
            }
            else
            {
                isSuccess = false;
                SysWarnGUI warn = new SysWarnGUI();
                warn.Owner = mainWin;
                warn.setWarnMsg(response.Msg);
                warn.ShowDialog();
            }

            return isSuccess;
        }

        /// <summary>
        /// 全选/取消全选机构
        /// </summary>
        /// <param name="checkAll"></param>
        public void CheckAllInst(bool checkAll)
        {
            foreach (var item in IssueInstViewModels)
            {
                item.IsSelect = checkAll;
            }
            // mainWin.delete.IsEnabled = checkAll;
        }

        /// <summary>
        /// 删除机构
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task DeleteInsts(string groupId)
        {
            if (IssueInstViewModels.Count > 0)
            {
                var Ids = new List<long>(IssueInstViewModels.Count);
                foreach (var item in IssueInstViewModels)
                {
                    if (item.IsSelect)
                    {
                        Ids.Add(item.Id);
                    }
                }

                StringBuilder errorMsg = new StringBuilder();
                await Task.Run(() => {
                    // 一条一条删
                    foreach (long delId in Ids)
                    {
                        var request = new OperateIssueInstRequest
                        {
                            action = (int)ActionType.Delete,
                            id = delId,
                        };
                        var response = GRHandler.Instance.OperateIssueInstInfos(request);

                        if (response == null)
                        {
                            errorMsg.AppendLine($"删除机构错误");
                        }
                        else if (!response.IsSuccess())
                        {
                            errorMsg.AppendLine(response.Msg);
                        }
                        else
                        {
                            issueInstDic[groupId].RemoveAll(_ => _.id == delId);
                        }
                    }
                });

                if (errorMsg.Length == 0)
                {
                    //await GetData(QueryType.InstSearch, groupId, null);
                    SetInstData(groupId);
                }
                else
                {
                    SysWarnGUI warn = new SysWarnGUI();
                    warn.Owner = mainWin;
                    warn.setWarnMsg(errorMsg.ToString());
                    warn.ShowDialog();
                }
            }
        }

        /// <summary>
        /// 修改机构信息
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="instList"></param>
        /// <param name="isRequest"></param>
        /// <returns></returns>
        public async Task UpdateInsts(string groupId, List<long> instList, bool isRequest)
        {
            StringBuilder errorSB = new StringBuilder();
            //await GetData(QueryType.GroupSearch, null,  null);
            await Task.Run(() => {
                IssueGroup ig = issueGroupDic[groupId];

                // 一条一条更新
                foreach (long updateId in instList)
                {
                    IssueInst ii = ig.instInfos.Find(item => item.id == updateId);
                    if (ii != null)
                    {
                        var request = new OperateIssueInstRequest
                        {
                            action = (int)ActionType.Update,
                            id = ii.id,
                            groupId = ii.groupId,
                            instId = ii.instId,
                            instShortName = ii.instShortName,
                            instFullName = ii.instFullName,
                            instTraders = ii.instTraders,
                            defaultRequest = isRequest ? "1" : "2",
                        };

                        OperateIssueInstReply response = GRHandler.Instance.OperateIssueInstInfos(request);
                        if (response == null)
                        {
                            errorSB.AppendLine($"‘{ii.instShortName}’默认勾选失败");
                        }
                        else
                        {
                            if (!response.IsSuccess())
                            {
                                if (string.IsNullOrEmpty(response.Msg))
                                {
                                    errorSB.AppendLine($"‘{ii.instShortName}’默认勾选失败");
                                }
                                else
                                {
                                    errorSB.AppendLine(response.Msg);
                                }
                            }
                            else
                            {
                                IssueInst updateII = issueInstDic[groupId].Find(_ => _.id == updateId);
                                if (updateII != null && response.datas != null && response.datas.Count > 0)
                                {
                                    updateII.defaultRequest = response.datas[0].defaultRequest;
                                }
                            }
                        }
                    }
                }
            });
            if (errorSB.Length == 0)
            {
                //await GetData(QueryType.InstSearch, groupId, null);
                SetInstData(groupId);
            }
            else
            {
                SysWarnGUI warn = new SysWarnGUI();
                warn.Owner = mainWin;
                warn.setWarnMsg(errorSB.ToString());
                warn.ShowDialog();
            }
        }
    }

    public enum QueryType
    {
        GroupSearch = 1,
        InstSearch,
        InstTraderSearch,
        InstOfGroup = 4
    }

    public enum ActionType
    {
        Query = 0,
        Add = 1,
        Update,
        Delete,
    }

    public enum OperateType
    {
        Add = 1,
        Delete,
        UpdateGroupInfo,
        UpdatePriority
    }
}
