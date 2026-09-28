using com.gateway.log;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.XBond.Plugin.ViewModel.HomePage
{
    public class BatchAddBondsWindowViewModel : ViewModelBase
    {
        private int _checkedCount = 0;

        public int CheckedCount
        {
            get
            {
                return _checkedCount;
            }

            set
            {
                if (_checkedCount != value)
                {
                    _checkedCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int _sumCount = 0;

        public int SumCount
        {
            get
            {
                return _sumCount;
            }

            set
            {
                if (_sumCount != value)
                {
                    _sumCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public const string ISCHECK_FIELD = "IsCheck";
        public const string SECURITYID_FIELD = "SecurityID";
        public const string SYMBOL_FIELD = "Symbol";
        public const string REMAININGDATE_FIELD = "RemainingDate";

        private object lockObj = new object();

        private DataView _ImportDV;
        public DataView ImportDv
        {
            get
            {
                lock (lockObj)
                {
                    if (_ImportDV == null)
                    {
                        _ImportDV = new DataTable().DefaultView;
                        _ImportDV.Table.Columns.Add(ISCHECK_FIELD, typeof(bool));
                        _ImportDV.Table.Columns.Add(SECURITYID_FIELD, typeof(string));
                        _ImportDV.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _ImportDV.Table.Columns.Add(REMAININGDATE_FIELD, typeof(string));


                        _ImportDV.Table.PrimaryKey = new DataColumn[] {
                            _ImportDV.Table.Columns[SECURITYID_FIELD]
                        };
                        this.OnPropertyChanged();
                    }
                    return this._ImportDV;
                }
            }
        }

        private string[] splitStrArr = new string[] { "\n" };
        public void BindBonds(string importStr)
        {
            importStr = importStr.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            string[] bonds = importStr.Split(splitStrArr, StringSplitOptions.RemoveEmptyEntries);
            foreach (string bond in bonds)
            {
                try
                {
                    BondsInfoPo bondInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(bond.ToUpper());
                    //如果合约代码用的是小写
                    if (bondInfoPo == null)
                    {
                        bondInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(bond.ToLower());
                    }

                    String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                    if (bondInfoPo == null || today.CompareTo(bondInfoPo.Maturity_Date.ToString()) > 0)
                    {
                        continue;
                    }
                    DataRow dr = this.ImportDv.Table.Rows.Find(bondInfoPo.Bond_ID);
                    if (dr == null)
                    {
                        dr = this.ImportDv.Table.NewRow();
                        dr[ISCHECK_FIELD] = true;
                        dr[SECURITYID_FIELD] = bondInfoPo.Bond_ID;
                        dr[SYMBOL_FIELD] = bondInfoPo.Short_Name;
                        dr[REMAININGDATE_FIELD] = bondInfoPo.RemaingingTime;
                        this.ImportDv.Table.Rows.Add(dr);
                    }
                }
                catch(Exception e)
                {
                    LogHelper.Error($"识别失败:{bond}",e);
                }
            }
        }


        public void CalCheckedCount()
        {
            this.SumCount = this.ImportDv.Count;

            int sumCount = 0;
            foreach(DataRowView drv in this.ImportDv)
            {
                if ((bool)drv[ISCHECK_FIELD] == true)
                {
                    sumCount++;
                }
            }
            this.CheckedCount = sumCount;
        }

        public void BatchImportBonds()
        {
            BtnOK3Enabled = false;
            if (ImportDv != null && ImportDv.Count > 0)
            {
                List<HomeGroupSecurityidsUpdateReq> reqList = new List<HomeGroupSecurityidsUpdateReq>();
                foreach (DataRowView drv in ImportDv)
                {
                    if ((bool)drv[BatchAddBondsWindowViewModel.ISCHECK_FIELD] == true)
                    {
                        HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                        request.securityid = CommonUtils.ConvertToString(drv[BatchAddBondsWindowViewModel.SECURITYID_FIELD]);
                        request.symbol = CommonUtils.ConvertToString(drv[BatchAddBondsWindowViewModel.SYMBOL_FIELD]);
                        request.action = "1"; // 添加
                        request.remark = $"{request.securityid}  {request.symbol}  {CommonUtils.ConvertToString(drv[BatchAddBondsWindowViewModel.REMAININGDATE_FIELD])}";
                        request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                        request.type = CommonConstants.HOME_Type;
                        reqList.Add(request);
                    }
                }
                if (reqList.Count > 0)
                {
                    ImportBonds(reqList);
                }
            }
        }

        private string groupID = string.Empty;

        public string GroupID
        {
            get
            {
                return this.groupID;
            }
            set
            {
                this.groupID = value;
                this.OnPropertyChanged();
            }
        }

        private string groupName = string.Empty;

        public string GroupName
        {
            get
            {
                return this.groupName;
            }
            set
            {
                this.groupName = value;
                this.OnPropertyChanged();
            }
        }


        private volatile int successCount = 0;

        public int SuccessCount
        {
            get
            {
                return successCount;
            }

            set
            {
                successCount = value;
                this.OnPropertyChanged();
            }
        }

        private volatile int failCount = 0;

        public int FailCount
        {
            get
            {
                return failCount;
            }

            set
            {
                failCount = value;
                this.OnPropertyChanged();
            }
        }


        private ConcurrentDictionary<HomeGroupSecurityidsUpdateReq, PResult> resultDic = new ConcurrentDictionary<HomeGroupSecurityidsUpdateReq, PResult>();


        private volatile string resultStr = string.Empty;

        public string ResultStr
        {
            get
            {
                return resultStr;
            }
            set
            {
                resultStr = value;
                this.OnPropertyChanged();
            }
        }

        private volatile bool btnOK3Enabled = false;

        public bool BtnOK3Enabled
        {
            get
            {
                return btnOK3Enabled;
            }
            set
            {
                btnOK3Enabled = value;
                this.OnPropertyChanged();
            }
        }

        private int totalCount = 0;
        private int cancleCount = 0;

        public void ImportBonds(List<HomeGroupSecurityidsUpdateReq> reqList)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {

                try
                {
                    int index = 0;
                    foreach (HomeGroupSecurityidsUpdateReq req in reqList)
                    {
                       
                        ThreadPool.QueueUserWorkItem(act =>
                        {
                            Interlocked.Increment(ref totalCount);
                            if (_isCanceled)
                            {
                                Interlocked.Increment(ref cancleCount);
                                return;
                            }
                            
                            req.groupid = GroupID;
                            PResult result = HomePageDataHandler.UpdateSecurityids(req);
                            resultDic[req] = result;
                            if (result != null && result.Code == "0000")
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    SuccessCount = successCount + 1;
                                    ResultStr += req.remark + "\r\n";
                                }));
                            }
                            else
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    FailCount = FailCount + 1;
                                    ResultStr += result.Msg + "\r\n";
                                }));
                            }
                        });
                    }


                    while (true)
                    {
                        if (totalCount == reqList.Count 
                        && (reqList.Count - cancleCount) == resultDic.Count)
                        {
                            BtnOK3Enabled = true;
                            break;
                        }
                        Thread.Sleep(500);
                    }

                    HomePageDataHandler.RequestGroups(true);

                    HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.BatchImport, this.groupID,  string.Empty);

                    foreach (KeyValuePair<HomeGroupSecurityidsUpdateReq, PResult> pair in resultDic)
                    {
                        if (pair.Value != null && pair.Value.Code == "0000")
                        {
                            HomePageDataHandler.Notify(HomePageDataHandler.FavoriteTypeEnum.AddBond, this.groupID, pair.Key.securityid);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        private volatile int callbackCount = 0;

        public int CallbackCount
        {
            get
            {
                return callbackCount;
            }

            set
            {
                callbackCount = value;
                this.OnPropertyChanged();
            }
        }

        private volatile bool _isCanceled = false;

        public void CancelOpt()
        {
            _isCanceled = true;
        }

        public bool FinishOpt()
        {
            return resultDic.Count == ImportDv.Count;
        }
    }
}
