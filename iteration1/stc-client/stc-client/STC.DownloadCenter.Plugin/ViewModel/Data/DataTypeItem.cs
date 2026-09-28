using STC.Common.Model;
using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.DownloadCenter.Plugin.ViewModel.Data
{
    public class DataTypeItem : BaseViewModel
    {
        private  string _dataTypeCode;

        public  string DataTypeCode
        {
            get { return _dataTypeCode; }
            set { _dataTypeCode = value;
                this.OnPropertyChanged();
            }
        }


        private string _dataTypeName;

        public string DataTypeName
        {
            get { return _dataTypeName; }
            set { _dataTypeName = value;
                this.OnPropertyChanged();
            }
        }
    }
}
