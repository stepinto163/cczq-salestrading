using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Speech.aliyun
{
    class ResulfModel
    {
        public string appkey { get; set; }
        public string token { get; set; }
        public string text { get; set; }
        public string format { get; set; }
        public int sample_rate { get; set; }
        public string voice { get; set; }
        public int volume { get; set; }
        public int speech_rate { get; set; }
    }
}
