using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;
using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Model.Fix.Message;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Utils;
using STC.Common.Model.PT;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign.Reserve
{
    /// <summary>
    /// HisReocrdWin.xaml 的交互逻辑
    /// </summary>
    public partial class HisReocrdWin : Window
    {
        AutoResetEvent are = new AutoResetEvent(false);
        public List<HisRecordPo> HisData;
        //新的加载方式 逐条加载 20200430
        private ObservableCollection<HisRecordPo> HisDataEx = new ObservableCollection<HisRecordPo>();
        public HisReocrdWin()
        {
            InitializeComponent();
            //this.Icon = LocalEnv.ServerInfo.AppImage;
        }




        public void RefreshDataEx(List<BridgeHisPo> data)
        {
            List<HisRecordPo> datas = GetHisRecord(data);
            HisDataEx.Clear();
            //初次加载数量
            int initNum = 15;
            if (datas.Count() > initNum)
            {
                for (int i = 0; i < initNum; i++)
                {
                    HisDataEx.Add(datas[i]);
                }
                this.DataContext = HisDataEx;
                for (int i = initNum; i < datas.Count(); i++)
                {
                    //使用中间变量x，避免for循环中编译器对变量i重复修改。
                    int x = i;
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                     new Action(delegate () { HisDataEx.Add(datas[x]); }));
                }
            }
            else
            {
                for (int i = 0; i < datas.Count(); i++)
                {
                    HisDataEx.Add(datas[i]);
                }
                this.DataContext = HisDataEx;
            }



        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        /// <summary>
        /// 设置某列某行的背景色
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="item"></param>
        private void setColor(int[] arr, OrderDetailPo item)
        {
            foreach (int a in arr)
            {
                (dgRecords.Columns[a].GetCellContent(item) as TextBlock).Background = (Brush)FindResource("ColorBrush.Yellow");
                (dgRecords.Columns[a].GetCellContent(item) as TextBlock).Foreground = (Brush)FindResource("ColorBrush.Black");
            }

        }


        /// <summary>
        /// 获取历史操作数据，新旧对比，用于设置背景色
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public List<HisRecordPo> GetHisRecord(List<BridgeHisPo> datas)
        {
            datas = datas.OrderByDescending(o => o.updateTime).ToList();
            List<HisRecordPo> hisdatas = new List<HisRecordPo>();
            foreach (var data in datas)
            {
                HisRecordPo hisRecord = new HisRecordPo();

                hisRecord.Operator = data.updateBy;
                hisRecord.Time = data.updateTime;
                hisRecord.BridgeStatus = data.afterStateEnum.GetDescription();
                //操作类型
                hisRecord.OperateType = data.operateMsg;
                hisRecord.BidBridgeStatus = data.afterBidStateEnum.GetDescription();
                hisRecord.OfrBridgeStatus = data.afterOfrStateEnum.GetDescription();
                //hisRecord.OfrBackgroundColor = GetBackColorValue(data.StateChangeRange,true);
                //hisRecord.OfrForegroundColor = GetForeColorValue(data.StateChangeRange, true);
                //hisRecord.BidBackgroundColor = GetBackColorValue(data.StateChangeRange, false);
                //hisRecord.BidForegroundColor = GetForeColorValue(data.StateChangeRange, false);
                hisRecord.StateChangeRange = data.StateChangeRange;
                hisdatas.Add(hisRecord);
            }
            return hisdatas;
        }


        //private string GetBackColorValue(int flag,bool  bidOrOfr) {
        //    string backGround = "#090C0C";
        //    if (bidOrOfr) {
        //        if (flag==1|| flag == 3) {
        //            backGround = "#FF9300";
        //        }
        //    }
        //    else {
        //        if (flag == 2 || flag == 3) {
        //            backGround = "#FF9300";
        //        }
        //    }
        //    return backGround;
        //}

        //private string GetForeColorValue(int flag, bool bidOrOfr) {
        //    string backGround = "#FFEBC8";
        //    if (bidOrOfr) {
        //        if (flag == 1 || flag == 3) {
        //            backGround = "#000000";
        //        }
        //    }
        //    else {
        //        if (flag == 2 || flag == 3) {
        //            backGround = "#000000";
        //        }
        //    }
        //    return backGround;
        //}


        private void BackGround_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }

    public class HisRecordPo
    {
        public string Operator { get; set; }
        public string Time { get; set; }
        public string OperateType { get; set; }
        public string BridgeStatus { get; set; }

        public string BidBridgeStatus { get; set; }
        public string OfrBridgeStatus { get; set; }
        public int StateChangeRange {
            get; set;
        }

        /// <summary>
        /// 状态背景色
        /// </summary>
        public string BidBackgroundColor { get; set; }
        /// <summary>
        /// 状态字体色
        /// </summary>
        public string BidForegroundColor { get; set; }
        /// <summary>
        /// 状态背景色
        /// </summary>
        public string OfrBackgroundColor { get; set; }
        /// <summary>
        /// 状态字体色
        /// </summary>
        public string OfrForegroundColor { get; set; }
    }
}
