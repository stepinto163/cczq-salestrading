using STC.Mm.Plugin.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.Mm.Plugin.ViewModel
{
    public class BatchImportConfirmViewModel : ViewModelBase
    {
        /// <summary>
        /// 本地导入债券数量
        /// </summary>
        private ObservableCollection<ImportProp> _FileImportInfo;

        public ObservableCollection<ImportProp> FileImportInfo
        {
            get
            {
                return _FileImportInfo;
            }
            set
            {
                _FileImportInfo = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 导入后债券数量
        /// </summary>
        private IOrderedEnumerable<ImportProp> _AfterImportInfo;

        public IOrderedEnumerable<ImportProp> AfterImportInfo
        {
            get
            {
                return _AfterImportInfo;
            }
            set
            {
                _AfterImportInfo = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        private ObservableCollection<ImportErrorInfo> _ErrorInfo;

        public ObservableCollection<ImportErrorInfo> ErrorInfo
        {
            get
            {
                return _ErrorInfo;
            }
            set
            {
                _ErrorInfo = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand CancelBtnCommand { get; set; }

        public ICommand ConfirmBtnCommand { get; set; }

        private Visibility _ConfirmVisible;
        public Visibility ConfirmVisible
        {
            get
            {
                return _ConfirmVisible;
            }
            set
            {
                _ConfirmVisible = value;
                this.OnPropertyChanged();
            }
        }

        public BatchImportConfirmViewModel()
        {
            if (this.CancelBtnCommand == null)
                this.CancelBtnCommand = new RelayCommand<Window>(CloseWindow);

            if (this.ConfirmBtnCommand == null)
                this.ConfirmBtnCommand = new RelayCommand<Window>(Confirm);

            ConfirmVisible = Visibility.Visible;
        }

        private void Confirm(Window win)
        {
            if (win != null)
            {
                win.DialogResult = true;
                win.Close();
            }
        }

        private void CloseWindow(Window win)
        {
            if (win != null)
            {
                win.Close();
            }
        }
    }

    public class ImportErrorInfo : ViewModelBase
    {
        /// <summary>
        /// 债券代码
        /// </summary>
        private string _groupName;

        public string GroupName
        {
            get { return _groupName; }
            set
            {
                if (_groupName != value)
                {
                    _groupName = value;
                    this.OnPropertyChanged();
                }

            }
        }
        /// <summary>
        /// 债券代码
        /// </summary>
        private string _securityID;

        public string SecurityID
        {
            get { return _securityID; }
            set {
                if (_securityID != value)
                {
                    _securityID = value;
                    this.OnPropertyChanged();
                }

            }
        }

        /// <summary>
        /// 输入键
        /// </summary>
        private string _ImportKey;
        public string ImportKey
        {
            get { return _ImportKey; }
            set
            {
                if (_ImportKey != value)
                {
                    _ImportKey = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 输入值
        /// </summary>
        private string _ImportValue;

        public string ImportValue
        {
            get { return _ImportValue; }
            set
            {
                if (_ImportValue != value)
                {
                    _ImportValue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 错误原因
        /// </summary>
        private string _ImportErrorResean;

        public string ImportErrorResean
        {
            get { return _ImportErrorResean; }
            set
            {
                if (_ImportErrorResean != value)
                {
                    _ImportErrorResean = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }

    public class ImportProp : ViewModelBase
    {
        private int _index;

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                this.OnPropertyChanged();
            }
        }

        private string _groupName;

        public string GroupName
        {
            get { return _groupName; }
            set
            {
                _groupName = value;
                this.OnPropertyChanged();
            }
        }

        private string _validCount;

        public string ValidCount
        {
            get { return _validCount; }
            set
            {
                _validCount = value;
                this.OnPropertyChanged();
            }
        }

        private string _totalCount;

        public string TotalCount
        {
            get { return _totalCount; }
            set
            {
                _totalCount = value;
                this.OnPropertyChanged();
            }
        }

    }
}
