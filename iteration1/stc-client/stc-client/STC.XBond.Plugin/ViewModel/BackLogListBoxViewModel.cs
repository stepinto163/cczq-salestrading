using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// BackLogListBoxView的数据内容
    /// </summary>
    public class BackLogListBoxViewModel : ViewModelBase
    {
        private ObservableCollection<BackLogItem> _backLogs = new ObservableCollection<BackLogItem>();

        public ObservableCollection<BackLogItem> BackLogs
        {
            get
            {
                return this._backLogs;
            }
        }

        //private ThreadQueue<Common.Model.Fix.Message.ExecutionReport> _ProcessMarketExecutioinsThreadQueue;
        
        public void Init()
        {
            #region 生成测试数据
            this.GenerateData();
            #endregion
        }

        private void GenerateData()
        {
            BackLogItem item = new BackLogItem();
            item.Message = string.Format($"[成交回报]{Environment.NewLine}170215 KTN 3.9400 5000万");
            item.LogTime = "10:32:21";
            this.BackLogs.Add(item);

            BackLogItem item1 = new BackLogItem();
            item1.Message = string.Format($"[询价RFQ]{Environment.NewLine}李晓图（成都农商行）{Environment.NewLine}170007 Ofr 待确认价格 2000万");
            item1.LogTime = "10:32:21";
            item1.Status = "正常";
            item1.StatusDescritption = "有效时间 3分20秒";
            this.BackLogs.Add(item1);

            BackLogItem item2 = new BackLogItem();
            item2.Message = string.Format($"[询价RFQ]{Environment.NewLine}李晓图（成都农商行）{Environment.NewLine}170007 Ofr 待确认价格 2000万");
            item2.LogTime = "10:32:21";
            item2.StatusDescritption = "已过期";
            this.BackLogs.Add(item2);

            BackLogItem item3 = new BackLogItem();
            item3.LogTime = "10:32:21";
            item3.Message = string.Format($"[成交回报]{Environment.NewLine}170215 KTN 3.9400 5000万");
            this.BackLogs.Add(item3);
        }
    }
}
