using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Data
{
    public class FixedSizeCollection<T> : ObservableCollection<T>
    {
        private readonly int _maxSize;

        public FixedSizeCollection(int maxSize)
        {
            _maxSize = maxSize;
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);

            // 如果集合超过指定的最大大小，则移除最后一项
            if (this.Count > _maxSize)
            {
                this.RemoveAt(this.Count - 1);
            }
        }

        public void AddToFirst(T item)
        {
            this.Insert(0, item);
        }
    }
}
