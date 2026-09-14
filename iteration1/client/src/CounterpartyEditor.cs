using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    public sealed class CounterpartyEditor : UserControl {
        readonly ComboBox institution=new ComboBox{Width=235,DropDownStyle=ComboBoxStyle.DropDown};
        readonly ComboBox contact=new ComboBox{Width=235,DropDownStyle=ComboBoxStyle.DropDown};
        readonly List<JObject> institutions=new List<JObject>(),contacts=new List<JObject>();
        bool loading;
        sealed class ReferenceChoice {
            public JObject Record;public string Name,Id;
            public override string ToString()=>Name+" ["+Id+"]";
        }
        public CounterpartyEditor(){
            Dock=DockStyle.Fill;Height=40;
            var bar=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false,AutoScroll=true,Padding=new Padding(4)};
            bar.Controls.AddRange(new Control[]{Caption("对手机构"),institution,Caption("联系人"),contact,Caption("可先输入名称；确认时从下拉列表选定 ID")});Controls.Add(bar);
            institution.TextChanged+=(s,e)=>{if(!loading)RefreshContacts(true);};
        }
        static Label Caption(string text)=>new Label{Text=text,AutoSize=true,Padding=new Padding(0,6,0,0)};
        static ReferenceChoice Selected(ComboBox control){var selected=control.SelectedItem as ReferenceChoice;return selected!=null&&control.Text==selected.ToString()?selected:null;}
        public void SetReferenceData(IEnumerable<JObject> institutionRecords,IEnumerable<JObject> contactRecords){
            var previous=Fields();institutions.Clear();institutions.AddRange(institutionRecords);contacts.Clear();contacts.AddRange(contactRecords);
            loading=true;try{institution.Items.Clear();foreach(var record in institutions)institution.Items.Add(new ReferenceChoice{Record=record,Name=(string)record["institutionName"],Id=(string)record["institutionId"]});}finally{loading=false;}LoadFields(previous);
        }
        void RefreshContacts(bool clearId){
            string previousName=Selected(contact)?.Name??contact.Text;string previousId=clearId?null:Selected(contact)?.Id;string institutionId=Selected(institution)?.Id;
            loading=true;try{
                contact.Items.Clear();foreach(var record in contacts.Where(record=>institutionId!=null&&(string)record["institutionId"]==institutionId))contact.Items.Add(new ReferenceChoice{Record=record,Name=(string)record["contactName"],Id=(string)record["contactId"]});
                SelectReference(contact,previousId,previousName);
            }finally{loading=false;}
        }
        static void SelectReference(ComboBox control,string id,string name){
            control.SelectedIndex=-1;var choice=control.Items.Cast<ReferenceChoice>().FirstOrDefault(item=>id!=null&&item.Id==id);
            if(choice!=null)control.SelectedItem=choice;else control.Text=name??"";
        }
        public void LoadFields(JObject quote){
            loading=true;try{SelectReference(institution,(string)quote?["institutionId"],(string)quote?["institutionName"]);}finally{loading=false;}
            RefreshContacts(true);loading=true;try{SelectReference(contact,(string)quote?["contactId"],(string)quote?["contactName"]);}finally{loading=false;}
        }
        public JObject Fields(){
            var selectedInstitution=Selected(institution);var selectedContact=Selected(contact);
            return new JObject{["institutionId"]=selectedInstitution?.Id,["institutionName"]=EmptyToNull(selectedInstitution?.Name??institution.Text),["contactId"]=selectedContact?.Id,["contactName"]=EmptyToNull(selectedContact?.Name??contact.Text)};
        }
        static string EmptyToNull(string value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
        public static bool NeedsMatching(JObject fields)=>(!string.IsNullOrWhiteSpace((string)fields["institutionName"])&&string.IsNullOrWhiteSpace((string)fields["institutionId"]))||(!string.IsNullOrWhiteSpace((string)fields["contactName"])&&string.IsNullOrWhiteSpace((string)fields["contactId"]));
        public JObject ConfirmSelection(IWin32Window owner){
            using(var dialog=new Form{Text="确认报价：匹配对手机构与联系人",Width=900,Height=245,MinimumSize=new Size(860,245),StartPosition=FormStartPosition.CenterParent,Font=Font}){
                var selection=new CounterpartyEditor{Dock=DockStyle.Top};selection.SetReferenceData(institutions,contacts);selection.LoadFields(Fields());
                var info=new Label{Dock=DockStyle.Top,Height=70,Padding=new Padding(8),Text="先选择对手机构，再选择属于该机构的联系人。\r\n同名联系人可能属于不同机构，请核对 ID。原始输入名称保留在报价历史中。"};
                var error=new Label{Dock=DockStyle.Fill,Padding=new Padding(8),ForeColor=Color.DarkRed};
                var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=42,Padding=new Padding(8)};
                var accept=new Button{Text="使用所选对手",AutoSize=true};var cancel=new Button{Text="取消",DialogResult=DialogResult.Cancel};bar.Controls.AddRange(new Control[]{accept,cancel});
                accept.Click+=(s,e)=>{if(NeedsMatching(selection.Fields())){error.Text="已有名称必须选择对应 ID；请完成机构和联系人匹配。";return;}dialog.DialogResult=DialogResult.OK;};
                dialog.Controls.Add(error);dialog.Controls.Add(selection);dialog.Controls.Add(info);dialog.Controls.Add(bar);dialog.CancelButton=cancel;
                return dialog.ShowDialog(owner)==DialogResult.OK?selection.Fields():null;
            }
        }
    }
}
