using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable, IDataErrorInfo
    {
        /// <summary>
        /// Finalizes an instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        ~BaseViewModel()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(false);
        }

        /// <summary>
        /// PropertyChanged事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> 是否被释放处理. </summary>
        public bool IsDisposed { get; protected set; }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (this.PropertyChanged != null)
                    {
                        // 删除所有PropertyChange事件
                        foreach (PropertyChangedEventHandler handler in this.PropertyChanged.GetInvocationList())
                        {
                            this.PropertyChanged -= handler;
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// 如果已更新了依赖性属性，则必须调用.
        /// </summary>
        /// <param name="propertyName">属性名称.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string error;
        public string Error
        {
            get
            {
                return error;
            }
            set
            {
                if (this.error != value)
                {
                    this.error = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string this[string columnName]
        {
            get
            {
                var vc = new ValidationContext(this, null, null);
                vc.MemberName = columnName;
                var rs = new List<ValidationResult>();
                var result = Validator.TryValidateProperty(this.GetType().GetProperty(columnName).GetValue(this, null), vc, rs);
                if (rs.Count > 0)
                {
                    string xx = string.Join(Environment.NewLine, rs.Select(r => r.ErrorMessage).ToArray());
                    return string.Join(Environment.NewLine, rs.Select(r => r.ErrorMessage).ToArray());
                }
                return string.Empty;
            }
        }
    }
}
