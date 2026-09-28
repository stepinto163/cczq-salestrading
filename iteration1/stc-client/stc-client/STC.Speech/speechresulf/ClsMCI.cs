using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Speech.speechresulf
{
    /// <summary>
    /// clsMci 的摘要说明。
    /// </summary>
    public class ClsMCI
    {
        public ClsMCI()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        //定义API函数使用的字符串变量
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        private string Name = "";
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        private string durLength = "";
        [MarshalAs(UnmanagedType.LPTStr, SizeConst = 128)]
        private string TemStr = "";
        int ilong;
        //定义播放状态枚举变量
        public enum State
        {
            mPlaying = 1,
            mPuase = 2,
            mStop = 3
        };
        //结构变量
        public struct structMCI
        {
            public bool bMut;
            public int iDur;
            public int iPos;
            public int iVol;
            public int iBal;
            public string iName;
            public State state;
        };

        public structMCI mc = new structMCI();

        //取得播放文件属性
        public string FileName
        {
            get
            {
                return mc.iName;
            }
            set
            {
                //ASCIIEncoding asc = new ASCIIEncoding();
                try
                {
                    TemStr = "";
                    TemStr = TemStr.PadLeft(127, Convert.ToChar(" "));
                    Name = Name.PadLeft(260, Convert.ToChar(" "));
                    mc.iName = value;
                    ilong = APIClass.GetShortPathName(mc.iName, Name, Name.Length);
                    Name = GetCurrPath(Name);
                    //Name = "open " + Convert.ToChar(34) + Name + Convert.ToChar(34) + " alias media";
                    Name = "open " + Convert.ToChar(34) + Name + Convert.ToChar(34) + " alias media";
                    ilong = APIClass.mciSendString("close all", TemStr, TemStr.Length, 0);
                    ilong = APIClass.mciSendString(Name, TemStr, TemStr.Length, 0);
                    ilong = APIClass.mciSendString("set media time format milliseconds", TemStr, TemStr.Length, 0);
                    mc.state = State.mStop;
                }
                catch
                {
                }
            }
        }
        bool flag = true;
        //播放  http://sosoft.cnblogs.com/
        public void play()
        {
            try
            {
                int countTime = this.Duration;
                if (countTime == 0)
                {
                    flag = false;
                }
                TemStr = "";
                TemStr = TemStr.PadLeft(127, Convert.ToChar(" "));
                APIClass.mciSendString("play media", TemStr, TemStr.Length, 0);
                mc.state = State.mPlaying;
                //文件总时间                                               // Console.WriteLine("aa:" + aa);                                      //循环获取播放时间
                while (flag)
                {
                    int currentTime = this.CurrentPosition; //文件当前播放时间
                                                            //如果总时间等于文件当前播放时间，直接进行下一个文件
                    if (countTime == currentTime)
                    {
                        Console.WriteLine("**************************");
                        flag = false;
                        return;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

        }
        //停止
        public void StopT()
        {
            TemStr = "";
            TemStr = TemStr.PadLeft(128, Convert.ToChar(" "));
            ilong = APIClass.mciSendString("close media", TemStr, 128, 0);
            ilong = APIClass.mciSendString("close all", TemStr, 128, 0);
            mc.state = State.mStop;
        }

        public void Puase()
        {
            TemStr = "";
            TemStr = TemStr.PadLeft(128, Convert.ToChar(" "));
            ilong = APIClass.mciSendString("pause media", TemStr, TemStr.Length, 0);
            mc.state = State.mPuase;
        }
        private string GetCurrPath(string name)
        {
            if (name.Length < 1) return "";
            name = name.Trim();
            name = name.Substring(0, name.Length - 1);
            return name;
        }
        //总时间
        public int Duration
        {
            get
            {
                durLength = "";
                durLength = durLength.PadLeft(128, Convert.ToChar(" "));
                APIClass.mciSendString("status media length", durLength, durLength.Length, 0);
                durLength = durLength.Trim();
                durLength = durLength.Replace("\0", "");
                if (durLength == "") return 0;
                int lent = int.Parse(durLength);
                return lent;
            }
        }

        //当前时间
        public int CurrentPosition
        {
            get
            {
                durLength = "";
                durLength = durLength.PadLeft(128, Convert.ToChar(" "));
                APIClass.mciSendString("status media position", durLength, durLength.Length, 0);
                durLength = durLength.Replace("\0", "");
                int lent = int.Parse(durLength);
                return lent;
            }
        }
    }

    public class APIClass
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(
        string lpszLongPath,
        string shortFile,
        int cchBuffer
        );

        [DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
        public static extern int mciSendString(
        string lpstrCommand,
        string lpstrReturnString,
        int uReturnLength,
        int hwndCallback
        );
    }
}
