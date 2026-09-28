using STC.Broker.Plugin.Model.Transaction.DesignModel;
using STC.Broker.Plugin.Utils;
using STC.Common.Handler.PT;
using STC.Common.Model;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.ViewModel.Setting
{
    class AddInstWinViewModel : BaseViewModel
    {
        public AddInstWinViewModel(string groupId)
        {
            GroupId = groupId;
        }

        /// <summary>
        /// 分组id
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 搜索结果绑定源
        /// </summary>
        public ObservableCollection<InstInfoViewModel> InstInfoViewModels { get; set; } = new ObservableCollection<InstInfoViewModel>();

        /// <summary>
        /// 搜索数据添加
        /// </summary>
        /// <param name="searchKey"></param>
        public void SetInstSearchData(string searchKey)
        {
            List<CrmInst> data = CommonUtils.GetInstitutionsInfoPo(searchKey, 20);
            InstInfoViewModels.Clear();
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    InstInfoViewModels.Add(new InstInfoViewModel { InstId = item.idbkey, InstShortName = item.shortnamecn, InstFullName = item.fullnamecn });
                }
            }
        }

        /// <summary>
        /// 确认添加
        /// </summary>
        public async Task<List<OperateIssueInstReply>> DoAddInst()
        {
            List<OperateIssueInstReply> replyList = new List<OperateIssueInstReply>();
            await Task.Run(() =>
            {
                // inst，需要一个一个加, 可能存在不同步风险。。。
                // 一个机构只能属于一个组
                
                if (InstInfoViewModels.Count > 0)
                {
                    bool flag = InstInfoViewModels.Any(_ => _.Selected);
                    if (flag)
                    {
                        var queryResult = GRHandler.Instance.QueryIssueGroupSetting(new QueryIssueGroupRequest { action = (int)ActionType.Query });
                        foreach (var item in InstInfoViewModels)
                        {
                            if (item.Selected)
                            {
                                if (queryResult == null || !queryResult.datas.Any(e => e.instInfos.Any(_ => _.instId == item.InstId)))
                                {
                                    List<IssueInstTrader> instIds = new List<IssueInstTrader>();
                                    
                                    List<CrmContact> traderList = CommonUtils.GetContactInfoPoByTermids(string.Empty, 2000, item.InstId);
                                    foreach (CrmContact cc in traderList)
                                    {
                                        IssueInstTrader iit = new IssueInstTrader
                                        {
                                            instId = cc.instid,
                                            instTraderId = cc.idbkey,
                                            instTraderName = cc.namecn,
                                            instIssueId = cc.idbkey,
                                            instIssueName = cc.namecn,
                                            instExchangeContactsId = cc.idbkey,
                                            instExchangeContactsName = cc.namecn,
                                            instExchangeContactsQQ = cc.qqlist,
                                        };
                                        instIds.Add(iit);
                                    }

                                    var request = new OperateIssueInstRequest
                                    {
                                        action = (int)ActionType.Add,
                                        groupId = GroupId,
                                        instId = item.InstId,
                                        instShortName = item.InstShortName,
                                        instFullName = item.InstFullName,
                                        instTraders = instIds,
                                    };
                                    OperateIssueInstReply response = GRHandler.Instance.OperateIssueInstInfos(request);

                                    if (response == null || !response.IsSuccess())
                                    {
                                        if (response == null)
                                        {
                                            replyList.Add(new OperateIssueInstReply()
                                            {
                                                Code = "9999",
                                                Msg = $"添加【{item.InstFullName}】机构错误"
                                            });
                                        }
                                        else
                                        {
                                            replyList.Add(response);
                                        }
                                    }
                                    else
                                    {
                                        replyList.Add(response);
                                    }
                                }
                                else
                                {
                                    replyList.Add(new OperateIssueInstReply() {
                                        Code="9999",
                                        Msg = $"{item.InstShortName}添加失败,已被添加分组"
                                    });
                                }
                            }
                        }
                    }
                }
            });

            return replyList;
        }


        public void CheckAllInst(bool checkAll)
        {
            foreach (var item in InstInfoViewModels)
            {
                item.Selected = checkAll;
            }
        }
    }


    /// <summary>
    /// 添加机构中绑定的机构模型
    /// </summary>
    public class InstInfoViewModel : BaseViewModel
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public string InstId { get; set; }

        /// <summary>
        /// 机构简称
        /// </summary>
        private string instShortName;
        public string InstShortName
        {
            get
            {
                return instShortName;
            }
            set
            {
                instShortName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 机构全称
        /// </summary>
        private string instFullName;
        public string InstFullName
        {
            get
            {
                return instFullName;
            }
            set
            {
                instFullName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool selected;
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                this.OnPropertyChanged();
            }
        }
    }
}
