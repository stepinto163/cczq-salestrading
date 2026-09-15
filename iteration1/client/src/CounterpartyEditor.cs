using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed class CounterpartyEditor : UserControl {
        readonly ComboBox institution = new ComboBox { Width = 235, DropDownWidth = 480, DropDownStyle = ComboBoxStyle.DropDown };
        readonly ComboBox contact = new ComboBox { Width = 235, DropDownWidth = 400, DropDownStyle = ComboBoxStyle.DropDown };
        readonly List<JObject> institutions = new List<JObject>();
        readonly List<JObject> contacts = new List<JObject>();
        bool loading;
        Func<string, JObject, Task<JObject>> request;
        readonly Button searchInstitution = new Button { Text = "查机构", AutoSize = true };
        readonly Button searchContact = new Button { Text = "查联系人", AutoSize = true };
        readonly Label searchStatus = new Label { AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
        int selectionVersion;
        sealed class ReferenceChoice {
            public JObject Record;
            public string Name;
            public string Id;
            public override string ToString() => Name + " [" + Id + "]";
        }
        public CounterpartyEditor(){
            Dock = DockStyle.Fill;
            Height = 40;
            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true, Padding = new Padding(4) };
            bar.Controls.AddRange(new Control[] { Caption("对手机构"), institution, searchInstitution, Caption("联系人"), contact, searchContact, searchStatus });
            Controls.Add(bar);
            institution.TextChanged += (sender, args) => {
                if (!loading) {
                    selectionVersion++;
                    RefreshContacts(true);
                }
            };
            searchInstitution.Click += async (sender, args) => await SearchAsync(false);
            searchContact.Click += async (sender, args) => await SearchAsync(true);
            searchStatus.Text = "输入名称或代码后查询";
        }
        public void ConfigureSearch(Func<string, JObject, Task<JObject>> requester) {
            request = requester;
        }
        public async Task SearchAsync(bool forContact) {
            if (request == null) {
                searchStatus.Text = "请先连接 GW";
                return;
            }
            string institutionId = Selected(institution)?.Id;
            if (forContact && institutionId == null) {
                searchStatus.Text = "请先查询并选择机构";
                return;
            }
            ComboBox input = forContact ? contact : institution;
            string originalText = input.Text;
            string text = Selected(input)?.Name ?? originalText.Trim();
            int version = selectionVersion;
            searchInstitution.Enabled = false;
            searchContact.Enabled = false;
            searchStatus.Text = "正在查询 CRM…";
            try {
                var query = new JObject { ["kind"] = forContact ? "CONTACT" : "INSTITUTION", ["text"] = text, ["limit"] = 50 };
                if (forContact) query["institutionId"] = institutionId;
                JObject data = await request("ficc.base.query", query);
                if (IsDisposed || version != selectionVersion || input.Text != originalText) return;
                var records = ((JArray)data["records"]).OfType<JObject>().ToList();
                if (forContact) {
                    contacts.Clear();
                    contacts.AddRange(records);
                    RefreshContacts(false);
                } else {
                    SetReferenceData(records, contacts.ToList());
                }
                searchStatus.Text = records.Count == 0 ? "无匹配记录，可保留名称待确认"
                    : (bool?)data["hasMore"] == true ? "显示前 50 条，请细化搜索" : "请选择名称及 ID";
                if (records.Count > 0) input.DroppedDown = true;
            } catch (Exception error) {
                searchStatus.Text = "CRM 查询失败：" + error.Message;
            } finally {
                if (!IsDisposed) {
                    searchInstitution.Enabled = true;
                    searchContact.Enabled = true;
                }
            }
        }
        static Label Caption(string text) => new Label { Text = text, AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
        static ReferenceChoice Selected(ComboBox control) {
            var selected = control.SelectedItem as ReferenceChoice;
            return selected != null && control.Text == selected.ToString() ? selected : null;
        }
        public void SetReferenceData(IEnumerable<JObject> institutionRecords, IEnumerable<JObject> contactRecords) {
            var previous = Fields();
            var newInstitutions = institutionRecords.ToList();
            var newContacts = contactRecords.ToList();
            institutions.Clear();
            institutions.AddRange(newInstitutions);
            contacts.Clear();
            contacts.AddRange(newContacts);
            loading = true;
            try {
                institution.Items.Clear();
                foreach (var record in institutions) {
                    institution.Items.Add(new ReferenceChoice { Record = record, Name = (string)record["institutionName"], Id = (string)record["institutionId"] });
                }
            } finally {
                loading = false;
            }
            LoadFields(previous);
        }
        void RefreshContacts(bool clearId){
            string previousName = Selected(contact)?.Name ?? contact.Text;
            string previousId = clearId ? null : Selected(contact)?.Id;
            string institutionId = Selected(institution)?.Id;
            loading = true;
            try {
                contact.Items.Clear();
                foreach (JObject record in contacts.Where(record => institutionId != null &&
                        ((string)record["institutionId"] == institutionId || (record["institutionIds"] as JArray)?.Values<string>().Contains(institutionId) == true))) {
                    contact.Items.Add(new ReferenceChoice { Record = record, Name = (string)record["contactName"], Id = (string)record["contactId"] });
                }
                SelectReference(contact, previousId, previousName);
            } finally {
                loading = false;
            }
        }
        static void SelectReference(ComboBox control,string id,string name){
            control.SelectedIndex = -1;
            var choice = control.Items.Cast<ReferenceChoice>().FirstOrDefault(item => id != null && item.Id == id);
            if (choice == null && id != null && !string.IsNullOrWhiteSpace(name)) {
                choice = new ReferenceChoice { Id = id, Name = name };
                control.Items.Add(choice);
            }
            if (choice != null) control.SelectedItem = choice;
            else control.Text = name ?? "";
        }
        public void LoadFields(JObject quote) {
            selectionVersion++;
            loading = true;
            try {
                SelectReference(institution, (string)quote?["institutionId"], (string)quote?["institutionName"]);
            } finally {
                loading = false;
            }
            RefreshContacts(true);
            loading = true;
            try {
                SelectReference(contact, (string)quote?["contactId"], (string)quote?["contactName"]);
            } finally {
                loading = false;
            }
        }
        public JObject Fields() {
            var selectedInstitution = Selected(institution);
            var selectedContact = Selected(contact);
            return new JObject {
                ["institutionId"] = selectedInstitution?.Id,
                ["institutionName"] = EmptyToNull(selectedInstitution?.Name ?? institution.Text),
                ["contactId"] = selectedContact?.Id,
                ["contactName"] = EmptyToNull(selectedContact?.Name ?? contact.Text)
            };
        }
        static string EmptyToNull(string value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
        public static bool NeedsMatching(JObject fields)=>(!string.IsNullOrWhiteSpace((string)fields["institutionName"])&&string.IsNullOrWhiteSpace((string)fields["institutionId"]))||(!string.IsNullOrWhiteSpace((string)fields["contactName"])&&string.IsNullOrWhiteSpace((string)fields["contactId"]));
        public JObject ConfirmSelection(IWin32Window owner){
            using(var dialog=new Form{Text="确认报价：匹配对手机构与联系人",Width=1150,Height=245,MinimumSize=new Size(900,245),StartPosition=FormStartPosition.CenterParent,Font=Font}){
                var selection = new CounterpartyEditor { Dock = DockStyle.Top };
                selection.SetReferenceData(institutions, contacts);
                selection.LoadFields(Fields());
                selection.ConfigureSearch(request);
                var info=new Label{Dock=DockStyle.Top,Height=70,Padding=new Padding(8),Text="先选择对手机构，再选择属于该机构的联系人。\r\n同名联系人可能属于不同机构，请核对 ID。原始输入名称保留在报价历史中。"};
                var error=new Label{Dock=DockStyle.Fill,Padding=new Padding(8),ForeColor=Color.DarkRed};
                var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=42,Padding=new Padding(8)};
                var accept = new Button { Text = "使用所选对手", AutoSize = true };
                var cancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel };
                bar.Controls.AddRange(new Control[] { accept, cancel });
                accept.Click += (sender, args) => {
                    if (NeedsMatching(selection.Fields())) {
                        error.Text = "已有名称必须选择对应 ID；请完成机构和联系人匹配。";
                        return;
                    }
                    dialog.DialogResult = DialogResult.OK;
                };
                dialog.Controls.Add(error);
                dialog.Controls.Add(selection);
                dialog.Controls.Add(info);
                dialog.Controls.Add(bar);
                dialog.CancelButton = cancel;
                return dialog.ShowDialog(owner)==DialogResult.OK?selection.Fields():null;
            }
        }
    }
}
