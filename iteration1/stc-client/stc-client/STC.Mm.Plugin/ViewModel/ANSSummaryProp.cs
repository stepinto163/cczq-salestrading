using STC.Common.Model.Ans;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Mm.Plugin.ViewModel
{
    public class ANSSummaryProp : ViewModelBase
    {
        // 状态，1：正常，2：需关注，3：已达标，4：失败
        private string _status;
        public string status
        {
            get { return _status; }
            set
            {
                _status = value;
                this.OnPropertyChanged();
            }
        }

        // 最低做市义务警告，1：需警告，0：无需警告
        private string _warnStatus;
        public string warnStatus
        {
            get { return _warnStatus; }
            set
            {
                _warnStatus = value;
                OnPropertyChanged();
            }
        }

        // 检查时间 HH:mm
        private string _checkTime;
        public string checkTime
        {
            get { return _checkTime; }
            set
            {
                _checkTime = value;
                OnPropertyChanged();
            }
        }

        // 剩余可报时间 HH:mm
        private string _remainTime;
        public string remainTime
        {
            get { return _remainTime; }
            set
            {
                _remainTime = value;
                OnPropertyChanged();
            }
        }

        // 利率债有效报价
        private string _bncSecurityCount;
        public string bncSecurityCount
        {
            get { return _bncSecurityCount; }
            set
            {
                _bncSecurityCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债品种分布
        private string _bncSecurityTypeCount;
        public string bncSecurityTypeCount
        {
            get { return _bncSecurityTypeCount; }
            set
            {
                _bncSecurityTypeCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债剩余期限
        private string _bncTimetomaturityCount;
        public string  bncTimetomaturityCount
        {
            get { return _bncTimetomaturityCount; }
            set
            {
                _bncTimetomaturityCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债国债数量
        private string _bncNDCount;
        public string bncNDCount
        {
            get { return _bncNDCount; }
            set
            {
                _bncNDCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债新发国债期限
        private string _bncNewNDCount;
        public string  bncNewNDCount
        {
            get { return _bncNewNDCount; }
            set
            {
                _bncNewNDCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债cfets有效报价
        private string _bncCfetsSecurityCount;
        public string bncCfetsSecurityCount
        {
            get { return _bncCfetsSecurityCount; }
            set
            {
                _bncCfetsSecurityCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债cfets品种分布
        private string _bncCfetsSecurityTypeCount;
        public string bncCfetsSecurityTypeCount
        {
            get { return _bncCfetsSecurityTypeCount; }
            set
            {
                _bncCfetsSecurityTypeCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债cfets剩余期限
        private string _bncCfetsTimetomaturityCount;
        public string bncCfetsTimetomaturityCount
        {
            get { return _bncCfetsTimetomaturityCount; }
            set
            {
                _bncCfetsTimetomaturityCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债cfets国债数量
        private string _bncCfetsNDCount;
        public string bncCfetsNDCount
        {
            get { return _bncCfetsNDCount; }
            set
            {
                _bncCfetsNDCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债cfets新发国债期限
        private string _bncCfetsNewNDCount;
        public string bncCfetsNewNDCount
        {
            get { return _bncCfetsNewNDCount; }
            set
            {
                _bncCfetsNewNDCount = value;
                OnPropertyChanged();
            }
        }

        // 信用债有效报价
        private string _bcoSecurityCount;
        public string bcoSecurityCount
        {
            get { return _bcoSecurityCount; }
            set
            {
                _bcoSecurityCount = value;
                OnPropertyChanged();
            }
        }

        // 信用债品种分布
        private string _bcoSecurityTypeCount;
        public string bcoSecurityTypeCount
        {
            get { return _bcoSecurityTypeCount; }
            set
            {
                _bcoSecurityTypeCount = value;
                OnPropertyChanged();
            }
        }

        // 信用债发行人覆盖
        private string _bcoIssuerCount;
        public string bcoIssuerCount
        {
            get { return _bcoIssuerCount; }
            set
            {
                _bcoIssuerCount = value;
                OnPropertyChanged();
            }
        }

        // 信用债发行人覆盖是否可见
        private string _bcoIssuerVisible;
        public string bcoIssuerVisible
        {
            get { return _bcoIssuerVisible; }
            set
            {
                _bcoIssuerVisible = value;
                OnPropertyChanged();
            }
        }

        // 信用债cfets有效报价
        private string _bcoCfetsSecurityCount;
        public string bcoCfetsSecurityCount
        {
            get { return _bcoCfetsSecurityCount; }
            set
            {
                _bcoCfetsSecurityCount = value;
                OnPropertyChanged();
            }
        }

        // 信用债cfets品种分布
        private string _bcoCfetsSecurityTypeCount;
        public string bcoCfetsSecurityTypeCount
        {
            get { return _bcoCfetsSecurityTypeCount; }
            set
            {
                _bcoCfetsSecurityTypeCount = value;
                OnPropertyChanged();
            }
        }

        // 信用债cfets发行人覆盖
        private string _bcoCfetsIssuerCount;
        public string bcoCfetsIssuerCount
        {
            get { return _bcoCfetsIssuerCount; }
            set
            {
                _bcoCfetsIssuerCount = value;
                OnPropertyChanged();
            }
        }

        // 利率债期限统计
        private List<SecurityTermProp> _securityTermLt;
        public List<SecurityTermProp> securityTermLt
        {
            get { return _securityTermLt; }
            set
            {
                _securityTermLt = value;
                OnPropertyChanged();
            }
        }

        //信用债分类统计
        private List<SecurityTypeProp> _securityTypeLt;
        public List<SecurityTypeProp> securityTypeLt
        {
            get { return _securityTypeLt; }
            set
            {
                _securityTypeLt = value;
                OnPropertyChanged();
            }
        }

        //新发国债view
        private CollectionViewSource _NewBondList = new CollectionViewSource();
        public CollectionViewSource NewBondList
        {
            get { return _NewBondList; }
            set
            {
                _NewBondList = value;
                OnPropertyChanged();
            }
        }

        //非新发国债view
        private CollectionViewSource _OtherBondList = new CollectionViewSource();
        public CollectionViewSource OtherBondList
        {
            get { return _OtherBondList; }
            set
            {
                _OtherBondList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public  ANSSummaryProp()
        {
            //ANSSummaryProp.InitViewSource(this);
        }

        public static void  InitViewSource(ANSSummaryProp prop)
        {
            Task.Run(() => {
                prop._NewBondList.Source = prop.securityTermLt;
                prop._NewBondList.IsLiveFilteringRequested = true;
                prop._NewBondList.Filter += prop._NewBondList_Filter;

                prop._OtherBondList.Source = prop.securityTermLt;
                prop._OtherBondList.IsLiveFilteringRequested = true;
                prop._OtherBondList.Filter += prop._OtherBondList_Filter;

                if (prop._NewBondList.View != null) { prop._NewBondList.View.Refresh(); }
                if (prop._OtherBondList.View != null) { prop._OtherBondList.View.Refresh(); }
            });         
        }

        public void _OtherBondList_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = false;
            if (e.Item != null)
            {
                SecurityTermProp item = e.Item as SecurityTermProp;

                //if (item.securityDec != "新发国债")
                if (item.newBondSign == "0")
                {
                    e.Accepted = true;
                }
            }
        }

        public void _NewBondList_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = false;
            if (e.Item != null)
            {
                SecurityTermProp item = e.Item as SecurityTermProp;

                //if (item.securityDec == "新发国债")
                if (item.newBondSign == "1")
                {
                    e.Accepted = true;
                }
            }
        }

        public static ANSSummaryProp ANSSummary2Prop(ANSSummary original)
        {
            ANSSummaryProp dest = new ANSSummaryProp();
            if (original != null)
            {
                dest = new ANSSummaryProp();

                dest.status = original.status;
                dest.warnStatus = original.warnStatus;
                dest.checkTime = original.checkTime;
                dest.remainTime = original.remainTime;
                dest.bncSecurityCount = original.bncSecurityCount;
                dest.bncSecurityTypeCount = original.bncSecurityTypeCount;
                dest.bncTimetomaturityCount = original.bncTimetomaturityCount;
                dest.bncNDCount = original.bncNDCount;
                dest.bncNewNDCount = original.bncNewNDCount;
                dest.bncCfetsSecurityCount = original.bncCfetsSecurityCount;
                dest.bncCfetsSecurityTypeCount = original.bncCfetsSecurityTypeCount;
                dest.bncCfetsTimetomaturityCount = original.bncCfetsTimetomaturityCount;
                dest.bncCfetsNDCount = original.bncCfetsNDCount;
                dest.bncCfetsNewNDCount = original.bncCfetsNewNDCount;
                dest.bcoSecurityCount = original.bcoSecurityCount;
                dest.bcoSecurityTypeCount = original.bcoSecurityTypeCount;
                dest.bcoIssuerCount = original.bcoIssuerCount;
                dest.bcoIssuerVisible = "";
                if (!string.IsNullOrEmpty(original.bcoIssuerCount) &&
                    "0".Equals(original.bcoIssuerCount.Substring(original.bcoIssuerCount.IndexOf("/") + 1)))
                {
                    dest.bcoIssuerVisible = "Hidden";
                }
                dest.bcoCfetsSecurityCount = original.bcoCfetsSecurityCount;
                dest.bcoCfetsSecurityTypeCount = original.bcoCfetsSecurityTypeCount;
                dest.bcoCfetsIssuerCount = original.bcoCfetsIssuerCount;

                dest.securityTermLt = new List<SecurityTermProp>();
                foreach (var item1 in original.securityTermLt)
                {
                    SecurityTermProp newTerm = new SecurityTermProp();
                    newTerm.securityDec = item1.securityDec;
                    newTerm.newBondSign = item1.newBondSign;
                    newTerm.countSafe = item1.countSafe;
                    newTerm.termCountList = new List<int>();
                    foreach(var term in item1.termCountList)
                    {
                        newTerm.termCountList.Add(term);
                    }
                    dest.securityTermLt.Add(newTerm);
                }

                dest.securityTypeLt = new List<SecurityTypeProp>();
                foreach (var type in original.securityTypeLt)
                {
                    dest.securityTypeLt.Add(new SecurityTypeProp()
                    {
                        securityDec = type.securityDec,
                        countSafe = type.countSafe
                    }) ;
                }

                //dest.NewBondList.View.Refresh();
                //dest.OtherBondList.View.Refresh();
             
            }

            return dest;
        }
    }

    public class SecurityTermProp : ViewModelBase
    {
        //债券分类名称
        private string _securityDec;
        public string securityDec
        {
            get { return _securityDec; }
            set
            {
                _securityDec = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 新发国债标识 0-不是新发国债，1-是新发国债
        /// </summary>
        private string _newBondSign;
        public string newBondSign
        {
            get { return _newBondSign; }
            set
            {
                _newBondSign = value;
                OnPropertyChanged();
            }
        }

        // 安全只数
        private int _countSafe;
        public int countSafe
        {
            get { return _countSafe; }
            set
            {
                _countSafe = value;
                OnPropertyChanged();
            }
        }

        //    期限集合，例如：[0，1，0，2，4，0]，分别对应0-1Y，1-3Y，3-5Y，5-7Y，7-10Y，>10Y的个数，注意：当securityDec为[新发国债] 时，对应原始期限的个数
        private List<int> _termCountList;
        public List<int> termCountList
        {
            get { return _termCountList; }
            set
            {
                _termCountList = value;
                OnPropertyChanged();
            }
        }
    }

    public class SecurityTypeProp : ViewModelBase
    {
        // 债券分类名称
        private string _securityDec;
        public string securityDec
        {
            get { return _securityDec; }
            set
            {
                _securityDec = value;
                OnPropertyChanged();
            }
        }

        // 安全只数
        private int _countSafe;
        public int countSafe
        {
            get { return _countSafe; }
            set
            {
                _countSafe = value;
                OnPropertyChanged();
            }
        }
    }

}
