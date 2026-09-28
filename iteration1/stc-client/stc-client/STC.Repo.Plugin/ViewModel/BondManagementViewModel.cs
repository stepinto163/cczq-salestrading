using STC.Common.Handler.Limit;
using STC.Common.Model;
using STC.Common.Model.Limit;
using STC.Common.Utils;
using STC.Repo.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Repo.Plugin.ViewModel
{
    public class BondManagementViewModel : BaseViewModel
    {
        private object _dataLock = new object();
        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        private ICommand _RefreshCommand;

        public ICommand RefreshCommand
        {
            get { return _RefreshCommand; }
            set { _RefreshCommand = value; }
        }


        private ICommand _SearchCommand;

        public ICommand SearchCommand
        {
            get { return _SearchCommand; }
            set { _SearchCommand = value; }
        }

        private string _SearchText;

        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                this.OnPropertyChanged();
            }
        }

        private string _selectedInstitution = "全部";

        public string SelectedInstitution
        {
            get { return _selectedInstitution; }
            set { 
                _selectedInstitution = value;
                this.OnPropertyChanged();
            }
        }

        private string _selectedQtpAccounts;

        public string SelectedQtpAccounts
        {
            get { return _selectedQtpAccounts; }
            set { _selectedQtpAccounts = value; }
        }



        private ObservableCollection<TotalBondItem> _totalList = new ObservableCollection<TotalBondItem>();

        public ObservableCollection<TotalBondItem> TotalList
        {
            get { return _totalList; }
            set
            {
                _totalList = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<TotalBondItem> _totalSummaryList = new ObservableCollection<TotalBondItem>();

        public ObservableCollection<TotalBondItem> TotalSummaryList
        {
            get { return _totalSummaryList; }
            set
            {
                _totalSummaryList = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<DetailBondItem> _detailList = new ObservableCollection<DetailBondItem>();

        public ObservableCollection<DetailBondItem> DetailList
        {
            get { return _detailList; }
            set
            {
                _detailList = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<DetailBondItem> _detailSummaryList = new ObservableCollection<DetailBondItem>();

        public ObservableCollection<DetailBondItem> DetailSummaryList
        {
            get { return _detailSummaryList; }
            set
            {
                _detailSummaryList = value;
                this.OnPropertyChanged();
            }
        }

        private TotalBondItem _selectTotalBond;

        public TotalBondItem SelectTotalBond
        {
            get { return _selectTotalBond; }
            set { 
                
                _selectTotalBond = value;
                this.OnPropertyChanged();

                if (value != null)
                {
                    QueryDetails(value.SecurityID, value.QtpAccout);
                }
                else
                {
                    DetailList.Clear();
                    DetailSummaryList.Clear();
                    DetailSummaryList.Add(new DetailBondItem()
                    {
                        Code = "合计",
                        CouponAmount = decimal.Zero,
                    });
                }
            }
        }

        private Dictionary<string, object> _qtpAccounts = new Dictionary<string, object>();

        public Dictionary<string, object> QtpAccounts
        {
            get { return _qtpAccounts; }
            set { _qtpAccounts = value;
            }
        }

        public BondManagementViewModel()
        {
            _qtpAccounts["account"] = "account";
            _qtpAccounts["NEW_account"] = "NEW_account";
            _qtpAccounts["TEST_account"] = "TEST_account";
            this.OnPropertyChanged("QtpAccounts");

            ViewSource.Source = TotalList;
            BindingOperations.EnableCollectionSynchronization(ViewSource.View, _dataLock);
            SetFilterDatas();
            ViewSource.View.CollectionChanged += View_CollectionChanged;

            RefreshData();
            RefreshCommand = new ComRelayCommand(RefreshData);
            SearchCommand = new ComRelayCommand(SearchData);
        }

        private void View_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                int count = 0;
                decimal total_CT0 = decimal.Zero;
                decimal total_CT1 = decimal.Zero;
                decimal total_M0 = decimal.Zero;
                decimal total_ST0 = decimal.Zero;
                decimal total_ST1 = decimal.Zero;
                foreach (TotalBondItem item in ViewSource?.View)
                {
                    count++;
                    total_CT0 += item.CT0;
                    total_CT1 += item.CT1;
                    total_M0 += item.M0;
                    total_ST0 += item.ST0;
                    total_ST1 += item.ST1;
                }

                TotalSummaryList.Clear();
                TotalSummaryList.Add(new TotalBondItem()
                {
                    SecurityID = "库存总计",
                    Symbol = $"共{count}支债券",
                    CT0 = total_CT0,
                    CT1 = total_CT1,
                    M0 = total_M0,
                    ST0 = total_ST0,
                    ST1 = total_ST1,
                    InstitutionCode = "",
                    InstitutionName = "",
                    LastQty = null,
                    QtpAccout = "",
                    DueDate = "",
                });
            }
        }

        private void SearchData()
        {
            ViewSource?.View.Refresh();

           
        }

        private void SetFilterDatas()
        {
            this.ViewSource.View.Filter = x => {
                TotalBondItem data = x as TotalBondItem;

                if (string.IsNullOrEmpty(this.SearchText) == false
                    && (data.SecurityID == null || !data.SecurityID.Contains(this.SearchText))
                    && (data.Symbol == null || !data.Symbol.Contains(this.SearchText)))
                    return false;

                if (SelectedInstitution != "全部" && SelectedInstitution != data.InstitutionCode)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(SelectedQtpAccounts))
                {
                    string[] qtpAccountArr = SelectedQtpAccounts.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                    if (!qtpAccountArr.Contains(data.QtpAccout))
                    {
                        return false;
                    }
                }

                return true;
            };
        }

        private async void RefreshData()
        {
            await Task.Run(() =>
            {
                List<string> accountList = _qtpAccounts.Keys.ToList();
                QuerySummaryReply reply = LimitHandler.Instance.QuerySummary(new QuerySummaryReq()
                {
                    accountList = accountList
                });

                if (reply != null && reply.IsSuccess())
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        TotalList.Clear();
                        TotalSummaryList.Clear();
                        int count = 0;
                        decimal total_CT0 = decimal.Zero;
                        decimal total_CT1 = decimal.Zero;
                        decimal total_M0 = decimal.Zero;
                        decimal total_ST0 = decimal.Zero;
                        decimal total_ST1 = decimal.Zero;
                        if (reply.datas != null)
                        {
                            foreach (BondSummaryInfo info in reply.datas)
                            {
                                TotalList.Add(new TotalBondItem()
                                {
                                    SecurityID = info.securityId,
                                    Symbol = info.symbol,
                                    Status = info.status,
                                    CT0 = info.t0AvailableLimit,
                                    CT1 = info.t1AvailableLimit,
                                    M0 = info.mxLimitAmount,
                                    ST0 = info.t0UsedLimit,
                                    ST1 = info.t1UsedLimit,
                                    InstitutionCode = "",
                                    InstitutionName = info.agencyName,
                                    LastQty = null,
                                    QtpAccout = info.qtpAccount,
                                    DueDate = string.IsNullOrEmpty(info.leaseMaturityDate) ? string.Empty : info.leaseMaturityDate.Substring(0, 10),
                                });

                                count++;
                                total_CT0 += info.t0AvailableLimit;
                                total_CT1 += info.t1AvailableLimit;
                                total_M0 += info.mxLimitAmount;
                                total_ST0 += info.t0UsedLimit;
                                total_ST1 += info.t1UsedLimit;
                            }


                            if (SelectTotalBond == null && TotalList.Count > 0)
                            {
                                SelectTotalBond = TotalList[0];
                            }


                            TotalSummaryList.Add(new TotalBondItem()
                            {
                                SecurityID = "库存总计",
                                Symbol = $"共{count}支债券",
                                CT0 = total_CT0,
                                CT1 = total_CT1,
                                M0 = total_M0,
                                ST0 = total_ST0,
                                ST1 = total_ST1,
                                InstitutionCode = "",
                                InstitutionName = "",
                                LastQty = null,
                                QtpAccout = "",
                                DueDate = "",
                            });
                        }
                        else
                        {
                            TotalSummaryList.Add(new TotalBondItem() {
                                SecurityID = "库存总计",
                                Symbol = "共0支债券",
                                CT0 = decimal.Zero,
                                CT1 = decimal.Zero,
                                M0 = decimal.Zero,
                                ST0 = decimal.Zero,
                                ST1 = decimal.Zero,
                                InstitutionCode = "",
                                InstitutionName = "",
                                LastQty = null,
                                QtpAccout = "",
                                DueDate = "",
                            });
                        }
                    }));
                }
            });
        }

        private async void QueryDetails(string securityID, string account)
        {
            await Task.Run(()=>{
                QueryDetailsReply reply = LimitHandler.Instance.QueryDetailSummary(new QueryDetailsReq()
                {
                    securityId = securityID,
                    account = account,
                });

                if (reply != null && reply.IsSuccess())
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() => 
                    {
                        DetailList.Clear();
                        DetailSummaryList.Clear();
                        decimal totalAmount = decimal.Zero;
                        if (reply.datas != null)
                        {
                            foreach (BondDetailInfo info in reply.datas)
                            {
                                if (info.limitStatus == "R")
                                {
                                    continue;
                                }

                                string code = string.Empty;
                                if (!string.IsNullOrEmpty(info.execid))
                                {
                                    code = info.execid;
                                }
                                else if (!string.IsNullOrEmpty(info.orderId))
                                {
                                    code = info.orderId;
                                }
                                else if (!string.IsNullOrEmpty(info.stcClOrderId))
                                {
                                    code = info.stcClOrderId;
                                }

                                totalAmount += info.usedBondAmount;

                                DetailList.Add(new DetailBondItem()
                                {
                                    Code = code,
                                    CouponAmount = info.usedBondAmount,
                                    BondStartDate = string.IsNullOrEmpty(info.firstSettleDate) ? string.Empty : info.firstSettleDate.Substring(0, 10),
                                    DueSettlementDate = string.IsNullOrEmpty(info.dueSettleDate) ? string.Empty : info.dueSettleDate.Substring(0, 10)
                                });
                            }

                            DetailSummaryList.Add(new DetailBondItem()
                            {
                                Code = "合计",
                                CouponAmount = totalAmount,
                            });
                        }
                        else
                        {
                            DetailSummaryList.Add(new DetailBondItem()
                            {
                                Code = "合计",
                                CouponAmount = decimal.Zero,
                            });
                        }
                    }));
                }
            });
        }
    }
}
