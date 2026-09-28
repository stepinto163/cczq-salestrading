using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Monitor
{
    public class monitormodel
    {
        public string desc;
        public string key;
        public string serverName;
        public string type;
        public string updatetime;
        public int value;
    }

    public class linkDataArray
    {
        public string from;
        public string to;
        public string problem;
        public string updatetime;
        public int status = 0;
        public List<int> dash = new List<int> { 2, 4 };
    }
    
}
