using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDT.Tools.AutoUpdater.Config
{
    public class YamlConfig
    {
        public List<BIN> BIN;
        public List<PLUGIN> PLUGIN;
        public List<CLIENT> CLIENT;
        public List<CONTROL> CONTROL;
    }
    public class BIN
    {
        public string bindll { get; set; }
    }
    public class PLUGIN
    {
        public string plugindll { get; set; }
    }
    public class CLIENT
    {
        public string startexe { get; set; }
        public string exe { get; set; }
        public string file { get; set; }
    }
    public class CONTROL
    {
        public string config { get; set; }
        public string key { get; set; }
    }
}
