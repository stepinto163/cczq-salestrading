using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Frame.Utils
{
    /// <summary>
    /// 通过时间生成序列号
    /// </summary>
    public class DataTimeSequence
    {
        private const string sequenceFormat = "{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}{6:000}";
        private long _count = 0;
        private int _number = 2;
        private string _numberFormat = "{7:0000}";

        private static DataTimeSequence instance = new DataTimeSequence();

        public static DataTimeSequence Instance {  get { return instance; } }
       

        private DataTimeSequence()
        {
        }


        public int Number
        {
            get { return _number; }
            set
            {
                if (value >= 1)
                {
                    _number = value;
                }
                string str = "";
                for (int i = 0; i < _number; i++)
                {
                    str += "0";
                }
                _numberFormat = "{7:" + str + "}";
            }
        }

        public string GetId()
        {
            DateTime now = DateTime.Now;
            long count = Interlocked.Increment(ref _count);
            return string.Format(sequenceFormat + _numberFormat, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, count % 9);
        }
    }
}
