using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.MarketExec
{
    public class CapacityList<T> : List<T>
    {
        private readonly int MaximumCapacity;
        private IComparer<T> comparable;
        public CapacityList(int maximumCapacity, IComparer<T> comparer)
        {
            MaximumCapacity = maximumCapacity;
            this.comparable = comparer;
        }

        public new void Add(T t)
        {
            base.Add(t);
            if (this.Count > MaximumCapacity)
            {
                this.Sort(comparable);
                this.RemoveAt(0); // Removes the oldest item.
            }
        }


    }
}
