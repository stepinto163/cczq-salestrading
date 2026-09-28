using com.gateway.log;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Setting;
using STC.Common.Handler.PT;
using STC.Common.Model;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.ViewModel.Setting
{
    public class TraderMangerWinViewModel : BaseViewModel
    {
        /// <summary>
        /// 当前交易员所属机构id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 机构id
        /// </summary>
        public string InsId { get; set; }

        public ObservableCollection<InstTraderViewModel> InstTraderViewModels { get; private set; } = new ObservableCollection<InstTraderViewModel>();


        private TraderMangerWin _traderMangerWin = null;

        public TraderMangerWinViewModel(long id, string insId, TraderMangerWin traderMangerWin)
        {
            Id = id;
            InsId = insId;
            _traderMangerWin = traderMangerWin;
            InitData();
        }

        public async Task InitData()
        {
            //List<Common.Model.CRM.CrmContact> crmContratList = CommonUtils.GetContactInfoPo(string.Empty, 2000, InsId);
            IssueInst response = await GetInfo();
            if (response == null)
            {
                return;
            }
            InstTraderViewModels.Clear();

            try
            {
                if (response.instTraders != null && response.instTraders.Count > 0)
                {
                    foreach (var findTrader in response.instTraders)
                    {
                        InstTraderViewModel trader = new InstTraderViewModel();
                        trader.ContactId = findTrader.instTraderId;
                        trader.Contact = findTrader.instTraderName;
                        trader.InstId = findTrader.instId;
                        trader.IssuerId = findTrader.instIssueId;
                        trader.Issuer = findTrader.instIssueName;
                        trader.ExchangeContact = findTrader.instExchangeContactsName;
                        trader.ExchangeContactId = findTrader.instExchangeContactsId;
                        trader.ExchangeQQ = findTrader.instExchangeContactsQQ;
                        InstTraderViewModels.Add(trader);
                    }
                }
            }
            catch (ArgumentNullException)
            {
                // ignore
            }


            //if (crmContratList != null && crmContratList.Count > 0)
            //{
            //    foreach (var item in crmContratList)
            //    {
            //        InstTraderViewModel trader = new InstTraderViewModel
            //        {
            //            ContactId = item.idbkey,
            //            Contact = item.namecn,
            //            InstId = item.instid,
            //        };

            //        try
            //        {
            //            IssueInstTrader findTrader = response.instTraders.Find(_ => _.instTraderId == item.idbkey);
            //            if (findTrader != null)
            //            {
            //                //trader.InstId = findTrader.instId;
            //                //trader.ContactId = findTrader.instTraderId;
            //                //trader.Contact = findTrader.instTraderName;
            //                trader.IssuerId = findTrader.instIssueId;
            //                trader.Issuer = findTrader.instIssueName;
            //                trader.ExchangeContact = findTrader.instExchangeContactsName;
            //                trader.ExchangeContactId = findTrader.instExchangeContactsId;
            //                trader.ExchangeQQ = findTrader.instExchangeContactsQQ;
            //            }
            //        }
            //        catch (ArgumentNullException)
            //        {
            //            // ignore
            //        }

            //        InstTraderViewModels.Add(trader);
            //    }
            //}
        }

        public async Task<OperateIssueInstReply> UpdateTrader(InstTraderViewModel instTraderView)
        {
            OperateIssueInstReply response = null;
            await Task.Run(() =>
            {
                try
                {
                    QueryIssueGroupRequest request = new QueryIssueGroupRequest { action = (int)ActionType.Query };
                    QueryIssueGroupReply result = GRHandler.Instance.QueryIssueGroupSetting(request);
                    if (result != null && result.datas != null)
                    {
                        IEnumerable<IssueGroup> groupList = result.datas.Where(item => item.instInfos.Any(subitem => subitem.id == Id));
                        foreach (IssueGroup ig in groupList)
                        {
                            IssueInst ii = ig.instInfos.Find(item => item.id == Id);
                            IssueInstTrader iit = ii.instTraders.Find(item => item.instTraderId == instTraderView.ContactId);
                            if (iit != null)
                            {
                                iit.instIssueId = instTraderView.IssuerId;
                                iit.instIssueName = instTraderView.Issuer;
                            }
                            else
                            {
                                ii.instTraders.Add(new IssueInstTrader
                                {
                                    instId = InsId,
                                    instTraderId = instTraderView.ContactId,
                                    instTraderName = instTraderView.Contact,
                                    instIssueId = instTraderView.IssuerId,
                                    instIssueName = instTraderView.Issuer,
                                });
                            }
                            if (ii != null)
                            {
                                OperateIssueInstRequest issueInstTrader = new OperateIssueInstRequest()
                                {
                                    action = (int)ActionType.Update,
                                    id = ii.id,
                                    groupId = ii.groupId,
                                    instId = ii.instId,
                                    instShortName = ii.instShortName,
                                    instFullName = ii.instFullName,
                                    instTraders = ii.instTraders,
                                    defaultRequest = ii.defaultRequest
                                };
                                response = GRHandler.Instance.OperateIssueInstInfos(issueInstTrader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            });
            return response;
        }

        /// <summary>
        /// 查询接口
        /// </summary>
        private async Task<IssueInst> GetInfo()
        {
            IssueInst result = null;
            await Task.Run(() =>
            {
                QueryIssueGroupRequest request = new QueryIssueGroupRequest { action = (int)ActionType.Query };
                QueryIssueGroupReply response = GRHandler.Instance.QueryIssueGroupSetting(request);
                if (response != null && response.datas != null)
                {
                    IEnumerable<IssueGroup> groupList = response.datas.Where(item => item.instInfos.Any(subitem => subitem.id == Id));
                    foreach (IssueGroup ig in groupList)
                    {
                        result = ig.instInfos.Find(item => item.instId == InsId);
                        break;
                    }
                }
            });
            return result;
        }
    }

    public class InstTraderViewModel : BaseViewModel
    {

        /// <summary>
        /// 机构Id
        /// </summary>
        public string InstId { get; set; }

        /// <summary>
        /// 联系人ID
        /// </summary>
        public string ContactId { get; set; }

        /// <summary>
        /// 发单人Id
        /// </summary>
        public string IssuerId { get; set; }

        /// <summary>
        /// 交易所联系人Id
        /// </summary>
        public string ExchangeContactId { get; set; }


        /// <summary>
        /// 联系人
        /// </summary>
        private string contact;
        public string Contact
        {
            get
            {
                return contact;
            }
            set
            {
                contact = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 发单人
        /// </summary>
        private string issuer;
        public string Issuer
        {
            get
            {
                return issuer;
            }
            set
            {
                issuer = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 交易所联系人
        /// </summary>
        private string exchangeContact;
        public string ExchangeContact
        {
            get
            {
                return exchangeContact;
            }
            set
            {
                exchangeContact = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 交易所联系人QQ
        /// </summary>
        private string exchangeQQ;
        public string ExchangeQQ
        {
            get
            {
                return exchangeQQ;
            }
            set
            {
                exchangeQQ = value;
                this.OnPropertyChanged();
            }
        }
    }
}
