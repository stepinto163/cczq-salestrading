using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Chrome.Plugin.ViewModel.Topo
{
    public class GraphLinksModels
    {
        public string Class;
        public List<nodeDataModel> nodeDataArray;
        public List<linkDataModel> linkDataArray;
    }

    public class nodeDataModel
    {
        public string key;
        public string loc;
        public string text;
        public string size;
        public string color="black";
        public string type = "";
        public string figure = "Rectangle";
        public int status = 0;
        public int operation = 0;
        public int thickness = 1;
        public int visible = 1;
        public string problem = "";
        //IP + Port
        public string details = "";
        public string fill = "beige";
        public int id=0;
        public bool isGroup = false;
        public string group = "0";
        public string host = "";
        public string port = "";
        /// <summary>
        /// 服务资源信息
        /// </summary>
        public string desc = "";
        public string updatetime = "";
    }

    public class linkDataModel
    {
        public string from;
        public string to;
        public string problem = "";
        public List<int> dash = new List<int>() { 2, 4 };
        //public string fromSpot = "BottomSide"; // RightSide LeftSide TopSide BottomSide 
        public int dir = 1; //2双向
    }

    public class zookeeperHostInfo
    {
        /// <summary>
        /// 服务名    stc_tbook_monitor_svrinfo
        /// </summary>
        public string serviceName;
        
        /// <summary>
        /// 主机名
        /// </summary>
        public string host;
        
        /// <summary>
        /// 端口
        /// </summary>
        public string port;
        
        /// <summary>
        /// 中文服务名
        /// </summary>
        public string cnServiceName;

        /// <summary>
        /// 显示位置
        /// </summary>
        public string loc;

        /// <summary>
        /// 填充背景
        /// </summary>
        public string fill;

        /// <summary>
        /// 字体前景色
        /// </summary>
        public string color;

        /// <summary>
        /// 边框粗细
        /// </summary>
        public int thickness;

        /// <summary>
        /// 图形类型
        /// </summary>
        public string figure;

        /// <summary>
        /// 分组
        /// </summary>
        public string group;

        /// <summary>
        /// 是否可见
        /// </summary>
        public string visible;

        /// <summary>
        /// 故障提示
        /// </summary>
        public string problem;

        /// <summary>
        /// 图形类型
        /// </summary>
        public string type;

        /// <summary>
        /// 资源明细
        /// </summary>
        public string desc;
    }
}
