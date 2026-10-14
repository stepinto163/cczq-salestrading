using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Ficc.Protocol;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.Shared.Components;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    public partial class QuoteEntryDialog
    {
        private FiccApiClient liveApi;
        private JObject originalQuote;
        private QuoteConfirmRowViewModel crmParty;
        private bool busy;
        public bool IsReady { get; private set; }
        public string ErrorMessage => SubmitError.Text;

        // 编辑与历史共用产品表单；创建报价统一经过精确报价预览与确认。
        public static void ShowForQuote(Window owner, FiccApiClient api, string quoteId)
        {
            if (api == null || string.IsNullOrWhiteSpace(quoteId))
            {
                throw new ArgumentException("编辑报价必须指定已存在的报价ID和服务连接。");
            }
            var dialog = new QuoteEntryDialog { Owner = owner, liveApi = api };
            dialog.ModalShell.IsEnabled = false;
            dialog.Loaded += async (sender, args) => await dialog.LoadLiveQuoteAsync(quoteId);
            dialog.ApplyHostGeometry();
            dialog.ShowDialog();
        }

        private async Task LoadLiveQuoteAsync(string quoteId)
        {
            try
            {
                originalQuote = await liveApi.GetQuoteAsync(quoteId);
                JObject dictionary = await liveApi.RequestProjectionAsync(SalesTradingApi.QuoteDictionaryQuery,
                    new DictionaryRequest());
                SettlementBox.Items.Clear();
                SettlementBox.SelectedValuePath = "Tag";
                foreach (JObject option in (JArray)dictionary["dictionaries"]["settlType"])
                    SettlementBox.Items.Add(new ComboBoxItem { Content = (string)option["label"], Tag = (string)option["value"] });
                SettlementBox.SelectedValue = (string)originalQuote["settlType"];
                SetBondHeader((string)originalQuote["securityId"], (string)originalQuote["securityName"], null, null, null, null);
                DirBid.IsChecked = (string)originalQuote["side"] == "1";
                DirOfr.IsChecked = (string)originalQuote["side"] == "2";
                PriceBox.Text = (string)originalQuote["price"];
                VolumeBox.Text = (string)originalQuote["quantity"];
                // 编辑保留原计量单位，避免把不同单位的数值无换算改为万。
                VolumeUnitBox.Items.Clear();
                VolumeUnitBox.Items.Add(new ComboBoxItem { Content = (string)originalQuote["quantityUnit"] == "WAN" ? "万" : (string)originalQuote["quantityUnit"] });
                VolumeUnitBox.SelectedIndex = 0;
                VolumeUnitBox.IsEnabled = false;
                PriceTypeBox.SelectedIndex = (string)originalQuote["quoteType"] == "2" ? 2
                    : (string)originalQuote["quoteType"] == "3" ? 3 : (string)originalQuote["yieldType"] == "2" ? 1 : 0;
                TradeDateBox.Text = (string)originalQuote["tradeDate"];
                RemarkBox.Text = (string)originalQuote["remark"] ?? "";
                FlagUrgent.IsChecked = (bool?)originalQuote["urgent"] == true;
                FlagRec.IsChecked = (bool?)originalQuote["recommended"] == true;
                FlagWhole.IsChecked = (bool?)originalQuote["whole"] == true;
                FlagOco.IsChecked = (bool?)originalQuote["oco"] == true;
                FlagPacking.IsChecked = (bool?)originalQuote["packing"] == true;
                crmParty = new QuoteConfirmRowViewModel(new QuoteConfirmRow
                {
                    InstitutionId = (string)originalQuote["institutionId"], Institution = (string)originalQuote["institutionName"],
                    ContactId = (string)originalQuote["contactId"], Trader = (string)originalQuote["contactName"]
                });
                DataContext = crmParty;
                CrmLookup.SetDirectory(this, new DpsCrmDirectory(liveApi));
                HistoryButton.Visibility = Visibility.Visible;
                IsReady = true;
            }
            catch (Exception error)
            {
                SubmitError.Text = "加载失败：" + error.Message;
                com.gateway.log.LogHelper.Error(error);
            }
            finally
            {
                ModalShell.IsEnabled = true;
            }
        }

        private async Task SaveLiveQuoteAsync()
        {
            if (busy || !IsReady) return;
            SubmitError.Text = "";
            try
            {
                if (!decimal.TryParse(PriceBox.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out decimal price) || ((decimal.GetBits(price)[3] >> 16) & 0xff) > 4)
                    throw new InvalidOperationException("价格必须为数字，且最多四位小数。");
                if (!decimal.TryParse(VolumeBox.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal volume) || volume <= 0)
                    throw new InvalidOperationException("报量必须为正数。");
                if (!DateTime.TryParseExact(TradeDateBox.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tradeDate))
                    throw new InvalidOperationException("交易日期格式应为 yyyy-MM-dd。");
                if (SettlementBox.SelectedValue == null) throw new InvalidOperationException("请选择清算速度。");
                if (FlagOco.IsChecked == true && FlagPacking.IsChecked == true) throw new InvalidOperationException("OCO与打包不能同时选择。");
                var changes = new JObject
                {
                    ["side"] = DirBid.IsChecked == true ? "1" : "2", ["price"] = price.ToString(CultureInfo.InvariantCulture),
                    ["quantity"] = volume.ToString(CultureInfo.InvariantCulture),
                    ["quoteType"] = PriceTypeBox.SelectedIndex == 2 ? "2" : PriceTypeBox.SelectedIndex == 3 ? "3" : "1",
                    ["yieldType"] = PriceTypeBox.SelectedIndex >= 2 ? null : PriceTypeBox.SelectedIndex == 1 ? "2" : "1",
                    ["settlType"] = SettlementBox.SelectedValue.ToString(), ["tradeDate"] = tradeDate.ToString("yyyy-MM-dd"),
                    ["institutionId"] = crmParty.Row.InstitutionId, ["institutionName"] = crmParty.Institution,
                    ["contactId"] = crmParty.Row.ContactId, ["contactName"] = crmParty.Trader,
                    ["urgent"] = FlagUrgent.IsChecked == true, ["recommended"] = FlagRec.IsChecked == true,
                    ["whole"] = FlagWhole.IsChecked == true, ["oco"] = FlagOco.IsChecked == true,
                    ["packing"] = FlagPacking.IsChecked == true, ["remark"] = RemarkBox.Text
                };
                busy = true;
                IsReady = false;
                ModalShell.IsEnabled = false;
                FiccApiClient.RequireQuoteResults(await liveApi.OperateAsync("update", new[] { originalQuote }, changes));
                Close();
            }
            catch (Exception error)
            {
                SubmitError.Text = error.Message;
                com.gateway.log.LogHelper.Error(error);
            }
            finally
            {
                busy = false;
                IsReady = true;
                ModalShell.IsEnabled = true;
            }
        }

        private async void OnHistory(object sender, RoutedEventArgs args)
        {
            if (busy || originalQuote == null) return;
            try
            {
                var history = await liveApi.RequestProjectionAsync(SalesTradingApi.QuoteHistoryQuery,
                    new QuoteHistoryRequest { QuoteId = (string)originalQuote["quoteId"], Limit = 100 });
                new Window
                {
                    Owner = this, Title = "报价变更历史", Width = 800, Height = 600,
                    Content = new TextBox { Text = history.ToString(), IsReadOnly = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }
                }.ShowDialog();
            }
            catch (Exception error)
            {
                SubmitError.Text = error.Message;
                com.gateway.log.LogHelper.Error(error);
            }
        }
    }
}
