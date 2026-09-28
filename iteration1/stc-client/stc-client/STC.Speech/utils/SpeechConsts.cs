using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Speech.utils
{
    public class SpeechConsts
    {
        //存储音频路径
        public static readonly string Data_File = "./data/audio/";
        //软件图标位置
        public static readonly string IcoUrl = "pack://application:,,,/Images/favicon.ico";

        public static readonly string Server = "EMSSvr";
        //信用债
        public static readonly string SecurityType = "BNC";
        //订阅topic
        public static readonly string Topic = "stc.ems.e.**";


        public static string username = "";
        public static string password = "";
        public static string host = "";
        public static string port = "";
        public static string voiceMale = "";
        public static string speedMale = "";
        public static string voiceFemale = "";
        public static string speedFemale = "";
        public static string speaker = "";
        public static string token = "";
        public static string appkey = "";
        public static string logLevel = "";
        public static string logAddress = "";
        public static string accessKeyID = "";
        public static string accessKeySecret = "";
        public static string schJob = "";
        public static string gzip = "";
        public static string gwOrCeda = "";
    }
}
