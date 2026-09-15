using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class ApiDebugForm {
        readonly List<JObject> subscriptionCatalog = new List<JObject>();
        readonly List<JObject> visibleSubscriptionTopics = new List<JObject>();
        readonly ComboBox subscriptionPresets = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 750,
            DropDownWidth = 1050
        };
        readonly TextBox subscriptionDescription = new TextBox {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            BackColor = SystemColors.Info
        };
        Button prepareSubscription;

        void InitializeSubscriptionPanel(TabPage page) {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "subscriptions.json");
            foreach (JObject item in JArray.Parse(File.ReadAllText(path))) {
                subscriptionCatalog.Add(item);
            }
            var panel = new TableLayoutPanel {
                Dock = DockStyle.Top,
                Height = 166,
                ColumnCount = 1,
                RowCount = 4
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            var choices = Bar();
            choices.Controls.Add(Label("当前服务的订阅 Topic"));
            choices.Controls.Add(subscriptionPresets);
            prepareSubscription = new Button { Text = "获取会话 Topic", AutoSize = true };
            prepareSubscription.Click += async (sender, args) => {
                prepareSubscription.Enabled = false;
                try {
                    await Guard(PrepareSubscriptionTopic);
                } finally {
                    prepareSubscription.Enabled = SelectedSubscriptionTopic()?["streamOpen"] != null;
                }
            };
            choices.Controls.Add(prepareSubscription);
            var actions = Bar();
            actions.Controls.Add(Label("实际 Topic"));
            actions.Controls.Add(subscribeTopic);
            actions.Controls.Add(image);
            Button(actions, "订阅", Subscribe);
            Button(actions, "退订", Unsubscribe);
            panel.Controls.Add(choices, 0, 0);
            panel.Controls.Add(actions, 0, 1);
            panel.Controls.Add(subscriptionDescription, 0, 2);
            panel.Controls.Add(Label("DPS 支持 * / **；当前 FICC GW 仅允许本会话的精确 report topic，拒绝通配及内部 topic。"), 0, 3);
            page.Controls.Add(panel);
            subscriptionPresets.SelectedIndexChanged += (sender, args) => SelectSubscriptionTopic();
            server.TextChanged += (sender, args) => RefreshSubscriptionTopics();
            RefreshSubscriptionTopics();
        }

        void RefreshSubscriptionTopics() {
            subscriptionPresets.Items.Clear();
            visibleSubscriptionTopics.Clear();
            foreach (JObject item in subscriptionCatalog.Where(item => (string)item["serverName"] == server.Text)) {
                visibleSubscriptionTopics.Add(item);
                subscriptionPresets.Items.Add($"{item["name"]} · {item["topic"]} [{item["access"]}]");
            }
            if (subscriptionPresets.Items.Count > 0) {
                subscriptionPresets.SelectedIndex = 0;
            } else {
                subscribeTopic.Clear();
                subscriptionDescription.Text = "该服务暂无本地订阅目录；可手工填写已知 topic，是否允许由目标 GW 决定。";
                prepareSubscription.Enabled = false;
                image.Checked = false;
            }
        }

        JObject SelectedSubscriptionTopic() {
            int index = subscriptionPresets.SelectedIndex;
            return index >= 0 && index < visibleSubscriptionTopics.Count ? visibleSubscriptionTopics[index] : null;
        }

        void SelectSubscriptionTopic() {
            JObject item = SelectedSubscriptionTopic();
            if (item == null) return;
            subscribeTopic.Text = (string)item["topic"];
            subscriptionDescription.Text = (string)item["description"];
            image.Checked = (bool)item["snapshot"];
            prepareSubscription.Enabled = item["streamOpen"] != null;
        }

        async Task PrepareSubscriptionTopic() {
            if (!connected) throw new InvalidOperationException("请先连接 GW。");
            JObject item = SelectedSubscriptionTopic();
            string openTopic = (string)item?["streamOpen"];
            if (openTopic == null) throw new InvalidOperationException("该 topic 不使用会话初始化；请按说明填写实际地址。");
            // Use the normal request path so the initialization is visible in history and logs.
            topic.Text = openTopic;
            request.Text = "{}";
            await Send();
            if ((string)lastReply?["Code"] != "0000" || lastReply?["Data"]?["pushTopic"] == null) {
                throw new InvalidOperationException("获取会话 Topic 失败，请查看回复。");
            }
        }

        async Task Unsubscribe() {
            string selectedTopic = subscriptionList.SelectedItem as string ?? subscribeTopic.Text;
            if (client == null) return;
            await Task.Run(() => client.Unsubscribe(selectedTopic));
            subscriptionList.Items.Remove(selectedTopic);
            status.Text = "已退订：" + selectedTopic;
        }
    }
}
