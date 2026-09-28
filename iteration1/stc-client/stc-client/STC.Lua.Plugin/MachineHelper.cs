using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading;
namespace STC.Lua.Plugin
{
    public class MachineHelper
    {
        private static Mutex mutex = null;

        public static bool CheckProcessIsMultiple(string name)
        {
            bool newMutexCreated = false;
            try
            {
                string mutexName = "Global\\" + name;
                mutex = new Mutex(false, mutexName, out newMutexCreated);
            }
            catch
            {

            }
            return !newMutexCreated;
        }
        public static bool Is64BitProcess()
        {

            return IntPtr.Size == 8;

        }

        public static int GetOSBit()
        {

            try
            {
                string addressWidth = String.Empty;
                ConnectionOptions mConnOption = new ConnectionOptions();
                ManagementScope mMs = new ManagementScope(@"\\localhost", mConnOption);
                ObjectQuery mQuery = new ObjectQuery("select AddressWidth from Win32_Processor");
                ManagementObjectSearcher mSearcher = new ManagementObjectSearcher(mMs, mQuery);
                ManagementObjectCollection mObjectCollection = mSearcher.Get();
                foreach (ManagementObject mObject in mObjectCollection)
                {
                    addressWidth = mObject["AddressWidth"].ToString();
                }
                return Int32.Parse(addressWidth);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return 32;
            }
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
