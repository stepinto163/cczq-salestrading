using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.PT.Plugin.Model
{
   public class PTInnerSetting : ViewModelBase
    {
        private string _SHNumber;

        public string SHNumber
        {
            get { return _SHNumber; }
            set
            {
                _SHNumber = value;
                this.OnPropertyChanged();
            }
        }

        private string _SZNumber;

        public string SZNumber
        {
            get { return _SZNumber; }
            set
            {
                _SZNumber = value;
                this.OnPropertyChanged();
            }
        }

        private string _TabChk;

        public string TabChk
        {
            get { return _TabChk; }
            set
            {
                _TabChk = value;
                this.OnPropertyChanged();
            }
        }
        private string _PartyName;//本机构名称

        public string PartyName
        {
            get { return _PartyName; }
            set
            {
                _PartyName = value;
                this.OnPropertyChanged();
            }
        }
        //private string _SendRule;

        //public string SendRule
        //{
        //    get { return _SendRule; }
        //    set
        //    {
        //        _SendRule = value;
        //        this.OnPropertyChanged();
        //    }
        //}

    }
}
