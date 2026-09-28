using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;

namespace STC.SalesTrading.Plugin.View
{
    public sealed class QuoteEditor : UserControl
    {
        private readonly Dictionary<string, TextBox> texts = new Dictionary<string, TextBox>();
        private readonly Dictionary<string, ComboBox> choices = new Dictionary<string, ComboBox>();
        private readonly Dictionary<string, CheckBox> flags = new Dictionary<string, CheckBox>();
        private readonly FiccApiClient api;
        private readonly Action<string> showError;
        private string institutionId;
        private string contactId;
        private bool setting;

        public QuoteEditor(FiccApiClient api, Action<string> showError)
        {
            this.api = api;
            this.showError = showError;
            WrapPanel panel = new WrapPanel { Margin = new Thickness(8) };
            AddText(panel, "securityId", "债券代码", "");
            AddChoice(panel, "side", "方向", new[] { "1|Bid 买入", "2|Ofr 卖出" });
            AddChoice(panel, "quoteType", "计价方式", new[] { "1|收益率", "2|净价", "3|全价" });
            AddChoice(panel, "yieldType", "收益率类型", new[] { "1|到期", "2|行权" });
            AddText(panel, "price", "价格 / 收益率", "2.5000");
            AddText(panel, "quantity", "数量（万）", "1000");
            AddText(panel, "tradeDate", "交易日", DateTime.Today.ToString("yyyy-MM-dd"));
            AddChoice(panel, "settlType", "清算速度", new[] { "1|T+0", "2|T+1" });
            choices["settlType"].SelectedValue = "2";
            AddText(panel, "institutionName", "报价机构", "");
            panel.Children.Add(UiElements.Button("选择机构", async () => await ChooseAsync("INSTITUTION")));
            AddText(panel, "contactName", "联系人", "");
            panel.Children.Add(UiElements.Button("选择联系人", async () => await ChooseAsync("CONTACT")));
            AddText(panel, "remark", "备注", "");
            foreach (string item in new[] { "urgent|急", "recommended|荐", "whole|整", "oco|OCO", "packing|打包" })
            {
                string[] parts = item.Split('|');
                CheckBox check = new CheckBox { Content = parts[1], Foreground = Brushes.Gainsboro, Margin = new Thickness(8), VerticalAlignment = VerticalAlignment.Center };
                flags.Add(parts[0], check);
                panel.Children.Add(check);
            }
            texts["institutionName"].TextChanged += (sender, args) =>
            {
                if (!setting)
                {
                    institutionId = null;
                    contactId = null;
                }
            };
            texts["contactName"].TextChanged += (sender, args) =>
            {
                if (!setting)
                {
                    contactId = null;
                }
            };
            Content = panel;
        }

        private void AddText(Panel panel, string field, string label, string value)
        {
            TextBox input = new TextBox { Text = value, Width = 135, Margin = new Thickness(3), Padding = new Thickness(4) };
            System.Windows.Automation.AutomationProperties.SetAutomationId(input, "QuoteEditor." + field);
            texts.Add(field, input);
            StackPanel wrapper = new StackPanel { Margin = new Thickness(3) };
            wrapper.Children.Add(new TextBlock { Text = label });
            wrapper.Children.Add(input);
            panel.Children.Add(wrapper);
        }

        private void AddChoice(Panel panel, string field, string label, string[] values)
        {
            ComboBox choice = new ComboBox { Width = 125, Margin = new Thickness(3), SelectedValuePath = "Tag" };
            System.Windows.Automation.AutomationProperties.SetAutomationId(choice, "QuoteEditor." + field);
            foreach (string value in values)
            {
                string[] parts = value.Split('|');
                choice.Items.Add(new ComboBoxItem { Content = parts[1], Tag = parts[0] });
            }
            choice.SelectedIndex = 0;
            choices.Add(field, choice);
            StackPanel wrapper = new StackPanel { Margin = new Thickness(3) };
            wrapper.Children.Add(new TextBlock { Text = label });
            wrapper.Children.Add(choice);
            panel.Children.Add(wrapper);
        }

        public void Set(JObject quote)
        {
            setting = true;
            try
            {
                foreach (var field in texts)
                {
                    field.Value.Text = (string)quote[field.Key] ?? "";
                }
                foreach (var field in choices)
                {
                    field.Value.SelectedValue = (string)quote[field.Key] ?? "1";
                }
                foreach (var field in flags)
                {
                    field.Value.IsChecked = (bool?)quote[field.Key] ?? false;
                }
                institutionId = (string)quote["institutionId"];
                contactId = (string)quote["contactId"];
            }
            finally
            {
                setting = false;
            }
        }

        public void SetDictionary(JObject dictionary)
        {
            JArray settlements = dictionary["dictionaries"]?["settlType"] as JArray;
            if (settlements == null || settlements.Count == 0)
            {
                throw new InvalidOperationException("服务未返回清算速度字典。");
            }
            ComboBox choice = choices["settlType"];
            string selected = (string)choice.SelectedValue;
            choice.Items.Clear();
            foreach (JObject option in settlements)
            {
                choice.Items.Add(new ComboBoxItem { Content = (string)option["label"], Tag = (string)option["value"] });
            }
            choice.SelectedValue = selected ?? "2";
        }

        public JObject Read()
        {
            JObject result = new JObject();
            foreach (var field in texts)
            {
                result[field.Key] = field.Value.Text.Trim();
            }
            foreach (var field in choices)
            {
                result[field.Key] = field.Value.SelectedValue?.ToString();
            }
            foreach (var field in flags)
            {
                result[field.Key] = field.Value.IsChecked == true;
            }
            if (!decimal.TryParse((string)result["price"], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out decimal price) || ((decimal.GetBits(price)[3] >> 16) & 0xff) > 4)
            {
                throw new InvalidOperationException("价格必须为数字，且最多四位小数。");
            }
            result["institutionId"] = institutionId;
            result["contactId"] = contactId;
            result["quantityUnit"] = "WAN";
            result["currency"] = "CNY";
            if ((string)result["quoteType"] != "1")
            {
                result["yieldType"] = null;
            }
            return result;
        }

        private async Task ChooseAsync(string kind)
        {
            try
            {
                string field = kind == "INSTITUTION" ? "institutionName" : "contactName";
                JObject request = new JObject { ["kind"] = kind, ["text"] = texts[field].Text.Trim(), ["limit"] = 30 };
                if (kind == "CONTACT" && institutionId != null)
                {
                    request["institutionId"] = institutionId;
                }
                JObject data = await api.RequestAsync("ficc.base.query", request);
                DataGrid grid = kind == "INSTITUTION"
                    ? UiElements.Grid("机构|institutionName", "ID|institutionId")
                    : UiElements.Grid("联系人|contactName", "机构|institutionName", "ID|contactId");
                grid.ItemsSource = (JArray)data["records"];
                DockPanel content = new DockPanel();
                Window picker = new Window { Title = "选择 CRM 记录（搜索前可修改输入名称）", Width = 650, Height = 420,
                    Owner = Window.GetWindow(this), Content = content, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                Button select = UiElements.Button("使用选中记录", () =>
                {
                    if (!(grid.SelectedItem is JObject record))
                    {
                        return;
                    }
                    setting = true;
                    if (kind == "INSTITUTION")
                    {
                        institutionId = (string)record["institutionId"];
                        texts[field].Text = (string)record[field];
                        contactId = null;
                        texts["contactName"].Clear();
                    }
                    else
                    {
                        contactId = (string)record["contactId"];
                        texts[field].Text = (string)record[field];
                        if (record["institutionId"] != null)
                        {
                            institutionId = (string)record["institutionId"];
                            texts["institutionName"].Text = (string)record["institutionName"] ?? texts["institutionName"].Text;
                        }
                    }
                    setting = false;
                    picker.Close();
                });
                DockPanel.SetDock(select, Dock.Bottom);
                content.Children.Add(select);
                content.Children.Add(grid);
                picker.ShowDialog();
            }
            catch (Exception error)
            {
                showError(error.Message);
            }
        }
    }
}
