using STC.Common.View.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using com.gateway.log;
using STC.Common.Utils;

namespace STC.Broker.Plugin.View.Component
{
    /// <summary>
    /// 拷贝类型
    /// </summary>
    public enum ECopyType
    {
        /// <summary>
        /// 不带单号
        /// </summary>
        Ctrl_C = 0,
        带单号 = 1,
        不带Broker = 2,
        WithContacts = 3,
        HideCounterparty,
        WithTrader = 5,

    }
    /// <summary>
    /// 自定义网关控件，用于存放明细列表，并支持复制操作
    /// </summary>
    public class CopyExDataGrid : DataGrid
    {


        //0 :ctrl-c; 1:带单号 2 不带broker
        public static ECopyType CopyType = ECopyType.Ctrl_C;
        public CopyExDataGrid()
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(CopyExDataGrid),
                new CommandBinding(ApplicationCommands.Copy,
                    new ExecutedRoutedEventHandler(OnExecutedCopy),
                    new CanExecuteRoutedEventHandler(OnCanExecuteCopy)));

        }

        private static void OnCanExecuteCopy(object target, CanExecuteRoutedEventArgs args)
        {
            ((CopyExDataGrid)target).OnCanExecuteCopy(args);
        }
        protected new void OnCanExecuteCopy(CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = ClipboardCopyMode != DataGridClipboardCopyMode.None && this.SelectedCells.Count > 0;
            args.Handled = true;
        }

        private static void OnExecutedCopy(object target, ExecutedRoutedEventArgs args)
        {
            ((CopyExDataGrid)target).OnExecutedCopy(args);
        }

        /// <summary>
        /// This virtual method is called when ApplicationCommands.Copy command is executed.
        /// </summary>
        /// <param name="args"></param>
        public new void OnExecutedCopy(ExecutedRoutedEventArgs args)
        {
            // Supported default formats: Html, Text, UnicodeText and CSV
            //Collection<string> formats = new Collection<string>(new string[] { DataFormats.UnicodeText }); //{ DataFormats.Text, DataFormats.UnicodeText, DataFormats.CommaSeparatedValue });// DataFormats.Html,
            //Dictionary<string, StringBuilder> dataGridStringBuilders = new Dictionary<string, StringBuilder>(formats.Count);
            //foreach (string format in formats)
            //{
            //    dataGridStringBuilders[format] = new StringBuilder();
            //}
            string html = "";
            string txt = "";
            // Row has a selecion
            if (SelectedCells.Count > 0)
            {
                switch (CopyType)
                {
                    case ECopyType.Ctrl_C://0:
                        for (int z = 0; z < SelectedCells.Count;)
                        {
                            PositionDetailPo p = new PositionDetailPo();
                            DataRowView a = SelectedCells[z].Item as DataRowView;
                            if (a == null)
                                p = this.SelectedCells[z].Item as PositionDetailPo;
                            else
                            {
                                DataRow b = a.Row;
                                p = CopyDetailHelper.D2E<PositionDetailPo>(b);
                            }
                            if (p != null)
                            {
                                html += p.HtmlText;

                                if (p.DataType == EDataType.汇总)
                                {
                                    if (!string.IsNullOrWhiteSpace(p.Text))
                                    {
                                        txt += string.Format("{0}\r\n", p.Text.Replace("\r", "").TrimStart());
                                    }
                                    else
                                    {
                                        txt += "\r\n";
                                    }
                                }
                                else
                                {
                                    if (p.TotalText != null)
                                        txt += string.Format("{0}\r\n", p.TotalText.Replace("\r", ""));
                                    else
                                        txt += "\r\n";
                                }
                            }
                            z++;
                        }
                        break;

                    case ECopyType.带单号://1:
                        for (int z = 0; z < SelectedCells.Count;)
                        {
                            PositionDetailPo p = new PositionDetailPo();
                            DataRowView a = SelectedCells[z].Item as DataRowView;
                            if (a == null)
                                p = this.SelectedCells[z].Item as PositionDetailPo;
                            else
                            {
                                DataRow b = a.Row;
                                p = CopyDetailHelper.D2E<PositionDetailPo>(b);
                            }
                            if (p != null)
                            {
                                html += p.CopyHtmlText;
                                //if (p.CopyText != null)
                                //    txt += string.Format("{0}\r\n", p.CopyText.Replace("\r", ""));
                                //else
                                //    txt += "\r\n";
                                if (p.DataType == EDataType.汇总)
                                {
                                    if (!string.IsNullOrWhiteSpace(p.Text))
                                    {
                                        txt += string.Format("{0}\r\n", p.Text.Replace("\r", "").TrimStart());
                                    }
                                    else
                                    {
                                        txt += "\r\n";
                                    }
                                }
                                else
                                {
                                    if (p.CopyText != null)
                                        txt += string.Format("{0}\r\n", p.CopyText.Replace("\r", ""));
                                    else
                                        txt += "\r\n";
                                }
                            }
                            z++;
                        }
                        break;
                    case ECopyType.不带Broker://2:
                        for (int z = 0; z < SelectedCells.Count;)
                        {
                            PositionDetailPo p = new PositionDetailPo();
                            DataRowView a = SelectedCells[z].Item as DataRowView;
                            if (a == null)
                                p = this.SelectedCells[z].Item as PositionDetailPo;
                            else
                            {
                                DataRow b = a.Row;
                                p = CopyDetailHelper.D2E<PositionDetailPo>(b);
                            }
                            if (p != null)
                            {
                                html += p.WithoutBrokerHtmlText;
                                //if (p.WithoutBrokerText != null)
                                //    txt += string.Format("{0}\r\n", p.WithoutBrokerText.Replace("\r", ""));
                                //else
                                //    txt += "\r\n";
                                if (p.DataType == EDataType.汇总)
                                {
                                    if (!string.IsNullOrWhiteSpace(p.Text))
                                    {
                                        txt += string.Format("{0}\r\n", p.Text.Replace("\r", "").TrimStart());
                                    }
                                    else
                                    {
                                        txt += "\r\n";
                                    }
                                }
                                else
                                {
                                    if (p.WithoutBrokerText != null)
                                        txt += string.Format("{0}\r\n", p.WithoutBrokerText.Replace("\r", ""));
                                    else
                                        txt += "\r\n";
                                }
                            }
                            z++;
                        }
                        break;
                    case ECopyType.WithContacts:
                        for (int z = 0; z < SelectedCells.Count;)
                        {
                            PositionDetailPo p = new PositionDetailPo();
                            DataRowView a = SelectedCells[z].Item as DataRowView;
                            if (a == null)
                                p = this.SelectedCells[z].Item as PositionDetailPo;
                            else
                            {
                                DataRow b = a.Row;
                                p = CopyDetailHelper.D2E<PositionDetailPo>(b);
                            }
                            if (p != null)
                            {
                                html += p.HtmlWithContactsText;
                                //if (p.WithContactsText != null)
                                //    txt += string.Format("{0}\r\n", p.WithContactsText.Replace("\r", ""));
                                //else
                                //    txt += "\r\n";
                                if (p.DataType == EDataType.汇总)
                                {
                                    if (!string.IsNullOrWhiteSpace(p.Text))
                                    {
                                        txt += string.Format("{0}\r\n", p.Text.Replace("\r", "").TrimStart());
                                    }
                                    else
                                    {
                                        txt += "\r\n";
                                    }
                                }
                                else
                                {
                                    if (p.WithContactsText != null)
                                        txt += string.Format("{0}\r\n", p.WithContactsText.Replace("\r", ""));
                                    else
                                        txt += "\r\n";
                                }
                            }
                            z++;
                        }
                        break;
                    case ECopyType.HideCounterparty:
                        for (int z = 0; z < SelectedCells.Count;)
                        {
                            PositionDetailPo p = new PositionDetailPo();
                            DataRowView a = SelectedCells[z].Item as DataRowView;
                            if (a == null)
                                p = this.SelectedCells[z].Item as PositionDetailPo;
                            else
                            {
                                DataRow b = a.Row;
                                p = CopyDetailHelper.D2E<PositionDetailPo>(b);
                            }
                            if (p != null)
                            {
                                html += p.HtmlHideCounterpartyText;
                                //if (p.WithContactsText != null)
                                //    txt += string.Format("{0}\r\n", p.WithContactsText.Replace("\r", ""));
                                //else
                                //    txt += "\r\n";
                                if (p.DataType == EDataType.汇总)
                                {
                                    if (!string.IsNullOrWhiteSpace(p.Text))
                                    {
                                        txt += string.Format("{0}\r\n", p.Text.Replace("\r", "").TrimStart());
                                    }
                                    else
                                    {
                                        txt += "\r\n";
                                    }
                                }
                                else
                                {
                                    if (p.HideCounterpartyText != null)
                                        txt += string.Format("{0}\r\n", p.HideCounterpartyText.Replace("\r", ""));
                                    else
                                        txt += "\r\n";
                                }
                            }
                            z++;
                        }
                        break;
                    case ECopyType.WithTrader:
                        for (int z = 0; z < SelectedCells.Count;)
                        {
                            PositionDetailPo p = new PositionDetailPo();
                            DataRowView a = SelectedCells[z].Item as DataRowView;
                            if (a == null)
                                p = this.SelectedCells[z].Item as PositionDetailPo;
                            else
                            {
                                DataRow b = a.Row;
                                p = CopyDetailHelper.D2E<PositionDetailPo>(b);
                            }
                            if (p != null)
                            {
                                html += p.HtmlWithTraderText;
                                if (p.DataType == EDataType.汇总)
                                {
                                    if (!string.IsNullOrWhiteSpace(p.Text))
                                    {
                                        txt += string.Format("{0}\r\n", p.Text.Replace("\r", "").TrimStart());
                                    }
                                    else
                                    {
                                        txt += "\r\n";
                                    }
                                }
                                else
                                {
                                    if (p.WithTraderText != null)
                                        txt += string.Format("{0}\r\n", p.WithTraderText.Replace("\r", ""));
                                    else
                                        txt += "\r\n";
                                }
                            }
                            z++;
                        }
                        break;
                    default:
                        break;
                }
            }
            LogHelper.Info("明细复制内容：\r\n{0}", txt);
            ClipboardHelper.CopyToClipboard(html, txt);
        }

        public CalculateQtyModel OnCalculateQty(Dictionary<DetailTrader, List<CopyDetailPo>> dicInst, Dictionary<DetailTrader, List<CopyDetailPo>> dichisInst)
        {
            CalculateQtyModel res = new CalculateQtyModel();
            decimal? buyQty = 0, sellQty = 0, qtyE = 10000;
            for (int z = 0; z < SelectedCells.Count; z++)
            {
                PositionDetailPo p = new PositionDetailPo();
                DataRowView a = SelectedCells[z].Item as DataRowView;
                if (a == null)
                    p = this.SelectedCells[z].Item as PositionDetailPo;
                else
                {
                    DataRow b = a.Row;
                    p = CopyDetailHelper.D2E<PositionDetailPo>(b);
                }
                if (p != null && p.DataType == EDataType.数据)
                {
                    CopyDetailPo order = p.IsHist ? dichisInst[p.DetailTrader].Where(_ => _.execId == p.ExecId).FirstOrDefault() :
                        dicInst[p.DetailTrader].Where(_ => _.execId == p.ExecId).FirstOrDefault();
                    if (p.SideShow == "买入")
                        buyQty += order.qty;
                    else
                        sellQty += order.qty;
                }
            }

            res.buyQty = buyQty == null ? 0 : decimal.Parse((buyQty / qtyE).ToString());
            res.sellQty = sellQty == null ? 0 : decimal.Parse((sellQty / qtyE).ToString());

            return res;
        }
    }

    public class CalculateQtyModel
    {
        public decimal buyQty = 0;
        public decimal sellQty = 0;
    }
}
