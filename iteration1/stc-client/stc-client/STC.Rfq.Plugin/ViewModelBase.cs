using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace STC.Rfq.Plugin
{
    /// <summary>
    /// 基础ViewModel
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Finalizes an instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        ~ViewModelBase()
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
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
