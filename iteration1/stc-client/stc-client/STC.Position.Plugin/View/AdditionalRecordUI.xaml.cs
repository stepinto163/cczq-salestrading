using STC.Common.Handler.Admin;
using STC.Common.Handler.PT;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Position.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// AdditionalRecordUI.xaml 的交互逻辑
    /// </summary>
    public partial class AdditionalRecordUI : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        //public bool Result { get; private set; }

        public List<PositionCheckData> positionCheckData { get; set; }
        string[] trdType2Str = { "20", "62", "112", "现券卖出", "分销卖出", "托管转出", "远期卖出" };
        string[] trdType1Str = { "10", "61", "111", "现券买入", "分销买入", "托管转入", "远期买入" };

        public AdditionalRecordUI()
        {
            InitializeComponent(); 
            this.Loaded += AdditionalRecordUI_Loaded;
        }

        private void AdditionalRecordUI_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> LstBookList = new Dictionary<string, object>();
            Task.Run(() =>
            {
                List<TTbookBook> list = AdminHandler.Instance.GetChildTTbookByID(positionCheckData[0].bookid);
                if (list != null && list.Count > 0)
                {
                    foreach (TTbookBook pbm in list)
                    {
                        if (!LstBookList.ContainsKey(pbm.Id))
                        {
                            LstBookList.Add(pbm.Id, pbm.Bookname);//$"{pbm.Id}-{pbm.Bookname}");
                        }
                    }
                    //设置用户有权限查看的组下拉列表
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.comBookId.ItemsSource = LstBookList;
                        comBookId.SelectedValue = "";
                    }));
                }
                else
                {
                    LstBookList.Add(positionCheckData[0].bookid, positionCheckData[0].bookname);//$"{positionCheckData.bookid}-{positionCheckData.bookname}");

                    //设置用户有权限查看的组下拉列表
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.comBookId.ItemsSource = LstBookList;
                        comBookId.SelectedValue = positionCheckData[0].bookid;
                    }));
                }
               
            });
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(comBookId.Text))
            {
                showMessage("未选择组合!", false);
                return;
            }
            StringBuilder str = IsCheck();
            if (str != null && str.Length > 0)
            {
                string s = string.Format("补录失败! 原因:{0}", str.ToString());
                showMessage(s, false);
                return;
            }
            List<PtExecOrderSaveReq> list = new List<PtExecOrderSaveReq>();
            foreach (PositionCheckData position in positionCheckData)
            {
                PtExecOrderSaveReq req = new PtExecOrderSaveReq();
                req.securityid = position.securityid;
                List<SimpleBondsInfoPo> result = CommonUtils.GetBondsInfoPo(req.securityid, 1, null);
                req.symbol = (result != null && result.Count > 0) ? result[0].Short_Name : position.symbol;
                //req.symboltype = positionCheckData.sy;
                req.yield = position.yield;
                req.price = position.yield;
                //decimal amunt = Convert.ToDecimal(position.amount) / 1000;
                //req.qty = amunt + "";
                req.qty = position.amount;
                req.netprice = position.netprice;
                if (trdType2Str.Contains(position.side))
                    req.side = "2";
                else if (trdType1Str.Contains(position.side))
                    req.side = "1";

                //req.venues = "";
                //req.venuetypegw = "";
                //req.institutionid = "";
                //req.institutionname = "";
                //req.traderid = "";
                //req.tradername = "";
                req.quotemethod = "";
                req.name = LocalEnv.UserInfo.FullName;
                req.username = LocalEnv.UserInfo.UserName;
                //req.userid = LocalEnv.UserInfo.UserId;
                //req.remark = txCommnet.Text;
                req.accountid = "";
                req.accountname = "";
                req.partysubname = LocalEnv.UserInfo.FullName;
                req.partysubid = LocalEnv.UserInfo.UserId;
                req.bookid = comBookId.SelectedValue + "";
                req.bookname = comBookId.Text;
                req.venueexecid = position.venueExecId;
                req.comment = string.Format("{0}-{1}", "交易核对补录", position.htexecId);
                req.tradedate = position.traderdate;
                req.settldate = position.settlDate;
                if (req.tradedate.Equals(req.settldate))
                    req.settltype = "+0";//"1";
                else
                    req.settltype = "+1";//"2";
                list.Add(req);
            }
            
            ThreadPool.QueueUserWorkItem(o =>
            {
                PResult result = PTHandler.Instance.ExecOrderBatchSave(list);
                if (result != null && "0000".Equals(result.Code))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.DialogResult = true;
                        this.Close();
                        e.Handled = true;
                        showMessage("补录成功!", true);
                    }));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {

                        showMessage(result.Msg, false);
                    }));
                }
            });
        }

        private StringBuilder IsCheck()
        {
            StringBuilder result = new StringBuilder();
            foreach (PositionCheckData p in positionCheckData)
            {
                StringBuilder pmsg = new StringBuilder();
                string s = "";
                if (string.IsNullOrEmpty(p.securityid))
                {
                    pmsg.Append("债券代码 ");
                }
                if (string.IsNullOrEmpty(p.side))
                {
                    pmsg.Append("方向 ");
                }
                if (string.IsNullOrEmpty(p.yield))
                {
                    pmsg.Append("收益率 ");
                }
                else
                {
                    bool isZero = Convert.ToDecimal(p.yield) == 0;
                    if (isZero)
                        pmsg.Append("收益率 ");
                }
                if (string.IsNullOrEmpty(p.netprice))
                {
                    pmsg.Append("净价 ");
                }
                else
                {
                    bool isZero = Convert.ToDecimal(p.netprice) == 0;
                    if (isZero)
                        pmsg.Append("净价 ");
                }
                if (string.IsNullOrEmpty(p.amount))
                {
                    pmsg.Append("量 ");
                }
                if (string.IsNullOrEmpty(p.traderdate))
                {
                    pmsg.Append("交易日 ");
                }
                if (string.IsNullOrEmpty(p.settlDate))
                {
                    pmsg.Append("结算日 ");
                }
                if (string.IsNullOrEmpty(p.htexecId))
                {
                    pmsg.Append("交易编号 ");
                }
                if (pmsg != null && pmsg.Length > 0)
                {
                    s = string.Format("编号:{0},交易要素缺少:{1}", p.htexecId, pmsg.ToString());
                    result.Append(s);
                }
            }
            return result;
        }

        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message, bool autoClose)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.HideCancelButton();
            me.setWarnMsg(message, autoClose);
            me.ShowDialog();
        }
    }
}
