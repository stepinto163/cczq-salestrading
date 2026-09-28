using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ColumnSetting
{
    public class ColumnsInfo : ViewModelBase
    {
        private bool _IsVisible = true;

        public bool IsVisible
        {
            get
            {
                return _IsVisible;
            }
            set
            {
                _IsVisible = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsEnable = true;

        public bool IsEnable
        {

            get
            {
                return _IsEnable;
            }
            set
            {
                _IsEnable = value;
                this.OnPropertyChanged();
            }
        }

        private string _ColumnName = string.Empty;

        public string ColumnName
        {
            get
            {
                return _ColumnName;
            }
            set
            {
                _ColumnName = value;
                this.OnPropertyChanged();
            }
        }

        private double _ColumnWidth;

        public double ColumnWidth
        {
            get
            {
                return _ColumnWidth;
            }
            set
            {
                _ColumnWidth = value;
                this.OnPropertyChanged();
            }
        }

        private int _displayIndex;

        public int DisplayIndex
        {
            get { return _displayIndex; }
            set
            {
                _displayIndex = value;
                this.OnPropertyChanged();
            }
        }

    }
}
