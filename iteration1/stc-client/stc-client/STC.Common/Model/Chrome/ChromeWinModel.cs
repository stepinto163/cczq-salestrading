using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Chrome
{
    public class ChromeWinModel
    {
        public bool IsTopMost = false;
        public double Height = 600;
        public double Width = 800;
        public double PositionX = 0;
        public double PositionY = 0;
        public double MaxHeight = 1080;
        public double MaxWidth = 1920;
        /// <summary>
        /// URL必须输入
        /// </summary>
        public string Url = string.Empty;

        /// <summary>
        /// 窗口Key 必须输入，用于定位和查找chrome
        /// </summary>
        public string Key = "Chrome";

        /// <summary>
        /// 窗口标题 必须输入
        /// </summary>
        public string Title = "Chrome";

        /// <summary>
        /// 窗口状态
        /// </summary>
        public System.Windows.WindowState WindowState = System.Windows.WindowState.Normal;

        /// <summary>
        /// 第一次打开市初始位置，以后会自动记忆
        /// </summary>
        public System.Windows.WindowStartupLocation  WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        
        /// <summary>
        /// 是否打开Tabpage添加到首页，false 独立窗口
        /// </summary>
        public bool IsOpenInMainTabPage = false;

        /// <summary>
        /// 是否显示地址栏
        /// </summary>
        public bool IsShowAddressBar = false;

        /// <summary>
        /// 是否显示状态栏
        /// </summary>
        public bool IsShowStatusBar = false;

        /// <summary>
        /// Js调用C# 用的类名，骆驼命名法第一个字符必须小写，key:  csharpTest 如果在key最后加上|说明是异步调用类  value：CallBackObjectInstance
        /// </summary>
        public Dictionary<string, object> CallBackObjectInstance = new Dictionary<string, object>();

        /// <summary>
        /// 是否使用插件内的类，如果设为true，则只需 CallBackObject传字符串，由ChromePlugin.cs处理
        /// </summary>
        public bool UseChromePluginObject = true;

        /// <summary>
        /// 设置代理 直连为空 http代理:"127.0.0.1:8118" sock5代理: "socks5://127.0.0.1:1080"
        /// </summary>
        public string Proxy = "";

        /// <summary>
        /// target="_blank" 强制在同一窗口打开,等于 target="_self" 
        /// </summary>
        public bool ForceTargetSelf = true;

        /// <summary>
        /// 强制在打开新的TabPage,等于 target="_self" 
        /// </summary>
        public bool ForceTargetOtherPage = false;

        /// <summary>
        /// 债券ID
        /// </summary>
        public string SecurityId = "";

        /// <summary>
        /// 是否显示右键菜单
        /// </summary>
        public bool IsShowMenuHandler = true;

        /// <summary>
        /// 是否显示地址查看源代码开发工具
        /// </summary>
        public bool IsShowAddressViewSourceDevTools = true;

        /// <summary>
        /// 是否允许最大化和最小化
        /// </summary>
        public bool IsEnableMaxMin = true;

        /// <summary>
        /// 初始数据，传递给URL
        /// </summary>
        public string ActionData = "{}";

        /// <summary>
        /// change_bond 
        /// </summary>
        public string ChangeBondData = "{}";
    }
}
