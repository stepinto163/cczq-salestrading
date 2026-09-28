using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.PP.Plugin.Utils
{
    public static class PPClipboardHelper
    {
        public static void SetText(string text, int retryTimes = 10, int retryInterval = 200)
        {
            for (int i = 0; i < retryTimes; i++)
            {
                try
                {
                    Clipboard.SetText(text);
                    return; // 成功后退出
                }
                catch (COMException ex)
                {
                    // Framework 4.0 不支持 when 条件过滤器
                    if (ex.ErrorCode == -2147221040) // 0x800401D0
                    {
                        if (i == retryTimes - 1)
                        {
                            throw; // 最后一次重试失败则抛出异常
                        }
                        Thread.Sleep(retryInterval);
                    }
                    else
                    {
                        throw; // 其他异常直接抛出
                    }
                }
            }
        }

        public static void SetDataObject(object data, bool copy = true, int retryTimes = 10, int retryInterval = 100)
        {
            for (int i = 0; i < retryTimes; i++)
            {
                try
                {
                    Clipboard.SetDataObject(data, copy);
                    return;
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == -2147221040)
                    {
                        if (i == retryTimes - 1)
                        {
                            throw;
                        }
                        Thread.Sleep(retryInterval);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public static string GetText(int retryTimes = 10, int retryInterval = 100)
        {
            for (int i = 0; i < retryTimes; i++)
            {
                try
                {
                    return Clipboard.GetText();
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == -2147221040)
                    {
                        if (i == retryTimes - 1)
                        {
                            throw;
                        }
                        Thread.Sleep(retryInterval);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return string.Empty;
        }
    }
}
