using STC.Common.Model.TBook;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.XBond.Plugin.Setting.ViewModel
{
    public class VenueViewModel : ViewModelBase
    {
        /// <summary>
        /// 构造函数 
        /// </summary>
        public VenueViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(this.BondView, lockObj);
        }

        private object lockObj = new object();

        #region 表格字段
        private const string SN = "Sn";
        private const string LEVEL = "Level";
        private const string NAME = "Name";
        private const string TYPE = "Type";
        private const string VENUE = "Venue";
        private const string VENUETYPEGW = "Venuetypegw";
        private const string WEIGHT = "Weight";

        private const string LEVELC = "Levelc";
        private const string NAMEC = "Namec";
        private const string TYPEC = "Typec";
        private const string VENUEC = "Venuec";
        private const string VENUETYPEGWC = "Venuetypegwc";
        private const string WEIGHTC = "Weightc";
        #endregion

        private DataView _BondView;
        public DataView BondView
        {
            get
            {
                lock (lockObj)
                {
                    if (_BondView == null)
                    {
                        _BondView = new DataTable().DefaultView;
                        _BondView.Table.Columns.Add(LEVEL, typeof(decimal));
                        _BondView.Table.Columns.Add(NAME, typeof(string));
                        _BondView.Table.Columns.Add(SN, typeof(decimal));
                        _BondView.Table.Columns.Add(TYPE, typeof(string));
                        _BondView.Table.Columns.Add(VENUE, typeof(string));
                        _BondView.Table.Columns.Add(VENUETYPEGW, typeof(string));
                        _BondView.Table.Columns.Add(WEIGHT, typeof(decimal));

                        _BondView.Table.Columns.Add(LEVELC, typeof(decimal));
                        _BondView.Table.Columns.Add(NAMEC, typeof(string));  
                        _BondView.Table.Columns.Add(TYPEC, typeof(string));
                        _BondView.Table.Columns.Add(VENUEC, typeof(string));
                        _BondView.Table.Columns.Add(VENUETYPEGWC, typeof(string));
                        _BondView.Table.Columns.Add(WEIGHTC, typeof(decimal));
                        this.OnPropertyChanged();
                    }
                    return this._BondView;
                }
            }
        }


        private ObservableCollection<Venues> _venueList = new ObservableCollection<Venues>();

        public ObservableCollection<Venues> VenueList
        {
            get
            {
                return this._venueList;
            }
            set
            {
                this._venueList = value;
                this.OnPropertyChanged("VenueList");
            }
        }
        public Dictionary<string, Venues> dicVenue = new Dictionary<string, Venues>();
        public Dictionary<int, string> diclevel = new Dictionary<int, string>();

        private Venues selected;
        public Venues SelectedMusic
        {
            get { return this.selected; }
            set
            {
                this.selected = value;
                this.OnPropertyChanged("SelectedMusic");
            }
        }
        
        public bool IsNew { get; set; }
        public void Init(UserSetting UserSetting)
        {
            #region 生成测试数据
            //this.GenerateData();
            #endregion
            diclevel.Clear();
            diclevel.Add(0, "禁止");
            diclevel.Add(1, "1档");
            diclevel.Add(2, "2档");
            dicVenue.Clear();
            foreach (Venue v in UserSetting.systemVenue)
            {
                string key = string.Format("{0}-{1}", v.type, v.venue);
                if (!dicVenue.ContainsKey(key))
                {
                   
                    Venues item = new Venues();
                    item.Sn = v.sn;
                    item.Venue = v.venue;
                    item.Name = v.name;
                    item.Level = v.level;
                    item.Type = v.type;
                    item.Weight = v.weight;
                    item.Venuetypegw = v.venuetypegw;
                    if (diclevel.ContainsKey(v.level))
                        item.Levelformat = diclevel[v.level];
                    //DataRow row = this.BondView.Table.NewRow();
                    //row[SN] = v.sn;
                    //row[VENUE] = v.venue;
                    //row[NAME] = v.name;
                    //row[LEVEL] = v.level;
                    //row[TYPE] = v.type;
                    //row[WEIGHT] = v.weight;
                    //row[VENUETYPEGW] = v.venuetypegw;


                    var l = UserSetting.systemVenue.Find(t => t.sn == v.sn && !t.type.Equals(v.type));
                    if (l != null)
                    {
                        item.Venuec = l.venue;
                        item.Namec = l.name;
                        item.Levelc = l.level;
                        item.Typec = l.type;
                        item.Weightc = l.weight;
                        item.Venuetypegwc = l.venuetypegw;
                        if (diclevel.ContainsKey(l.level))
                            item.Levelformatc = diclevel[l.level];
                        //row[VENUEC] = l.venue;
                        //row[NAMEC] = l.name;
                        //row[LEVELC] = l.level;
                        //row[TYPEC] = l.type;
                        //row[WEIGHTC] = l.weight;
                        //row[VENUETYPEGWC] = l.venuetypegw;
                    }
                    this.VenueList.Add(item);
                    //this.BondView.Table.Rows.Add(row);

                    SetKey(item);
                }
            }
        }

        private void SetKey(Venues item)
        {
            string key = string.Format("{0}-{1}", item.Type, item.Venue);
            string key1 = string.Format("{0}-{1}", item.Typec, item.Venuec);
            if (!dicVenue.ContainsKey(key))
            {
                dicVenue.Add(key, item);
            }
            if (!dicVenue.ContainsKey(key1))
            {
                dicVenue.Add(key1, item);
            }
        }

        private void GenerateData()
        {
            Venues item = new Venues();
            item.Sn = 1;
            item.Venue = "ODM";
            item.Name = "XBOND";
            item.Level = 1;
            item.Type = "BNC";

            item.Venuec = "ODM";
            item.Namec = "XBOND";
            item.Levelc = 1;
            item.Typec = "BCO";
            this.VenueList.Add(item);

            Venues item1 = new Venues();
            item1.Sn = 2;
            item1.Venue = "TPSC";
            item1.Name = "国利";
            item1.Level = 0;
            item1.Type = "BNC";

            item1.Venuec = "TPSC";
            item1.Namec = "国利";
            item1.Levelc = 0;
            item1.Typec = "BCO";
            this.VenueList.Add(item1);

            Venues item2 = new Venues();
            item2.Sn = 3;
            item2.Venue = "CTTJ";
            item2.Name = "信唐";
            item2.Level = 1;
            item2.Type = "BNC";

            item2.Venuec = "CTTJ";
            item2.Namec = "信唐";
            item2.Levelc = 1;
            item2.Typec = "BCO";
            this.VenueList.Add(item2);

            Venues item3 = new Venues();
            item3.Sn = 4;
            item3.Venue = "PTCN";
            item3.Name = "平安";
            item3.Level = 2;
            item3.Type = "BNC";

            item3.Venuec = "PTCN";
            item3.Namec = "平安";
            item3.Levelc = 2;
            item3.Typec = "BCO";
            this.VenueList.Add(item3);

            Venues item4 = new Venues();
            item4.Sn = 5;
            item4.Venue = "CCTB";
            item4.Name = "中诚";
            item4.Level = 2;
            item4.Type = "BNC";

            item4.Venuec = "CCTB";
            item4.Namec = "中诚";
            item4.Levelc = 2;
            item4.Typec = "BCO";
            this.VenueList.Add(item4);

            Venues item5 = new Venues();
            item5.Sn = 6;
            item5.Venue = "CNEX";
            item5.Name = "国际";
            item5.Level = 2;
            item5.Type = "1";

            item5.Venuec = "CNEX";
            item5.Namec = "国际";
            item5.Levelc = 2;
            item5.Typec = "1";
            this.VenueList.Add(item5);
        }
    }
}
