using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace STC.Common.Utils
{
    public class QBShowBondUtil
    {
		const int WM_COPYDATA = 0x004A;

		[DllImport("user32.dll")]
		private extern static IntPtr SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, ref CopyDataStruct lParam);

		[DllImport("user32.dll")]
		private extern static IntPtr FindWindow(string windowClassName, string windowTitle);

		public static void SendMessage(String bondcode, IntPtr handle) 
		{
			ShowBondPo po = new ShowBondPo();
			po.app = "STC";
			po.bondcode = bondcode;
            po.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            po.token = WindowsIdentity.GetCurrent().Token.ToString();
            string strTotal = po.app + po.token + po.timestamp;
            po.crc = getcrc(strTotal);
            byte[] message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(po));
			SendMessage(handle, message);
		}
		private static string getcrc(string strTotal) 
		{
			uint nCrc = 0;
			const uint prime = 16777619;
			uint hash = 263132298;
			for (int i = 0; i < strTotal.Length; i++)
			{
				hash = (hash ^ strTotal[i]) * prime;
			}
			hash += hash << 13;
			hash ^= hash >> 7;
			hash += hash << 3;
			hash ^= hash >> 17;
			hash += hash << 5;

			nCrc = hash;
			return nCrc.ToString();
		}
		private static void SendMessage(IntPtr receiverApplicationId, byte[] message)
		{
			try
			{
				IntPtr hwnd = FindWindow("#32770", "qeubee");
				if (hwnd == IntPtr.Zero)
				{
					LogHandler.Info("QBShowBondUtil QB not found");
					return;
				}
				IntPtr messageBuffer = Marshal.AllocHGlobal(message.Length);
				Marshal.Copy(message, 0, messageBuffer, message.Length);
				CopyDataStruct cds = new CopyDataStruct();
				cds.cbData = message.Length;
				cds.lpData = messageBuffer;
				IntPtr result = SendMessage(hwnd, WM_COPYDATA, receiverApplicationId, ref cds);
				if (result == IntPtr.Zero) 
				{
					LogHandler.Info("QBShowBondUtil SendMessage fail");
				}
			}
			catch (Exception ex)
			{
				LogHandler.Error("SendMessage error:{0}", ex);
			}
		}
		[StructLayout(LayoutKind.Sequential)]
		struct CopyDataStruct
		{
			public IntPtr dwData;
			public int cbData;
			public IntPtr lpData;
		}
	}
	public class ShowBondPo 
	{
		public string func = "ipc";
		public string type = "showbonddetail";
		public string app = "appname";// Application.Current.GetType().Assembly.GetName().Name;
		public string token = "0A03OB0T80234GOB0T498";// Marshal.PtrToStringAnsi(Process.GetCurrentProcess().Handle);
		public string timestamp = "1720165991";
		public string crc = "859811997";
		public string bondcode = "Z0003002023GOVBGB12.CIB";
	}
}
