using STC.Frame.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace STC.Lua.Plugin
{
    public class LuaEngine : ILuaEngine
    {
        private const string X64_DLL = "bin/LuaInterface.x64.dll";
        private const string X86_DLL = "bin/LuaInterface.x86.dll";
        private const string luaType = "LuaInterface.Lua";

        private object luavm = null;

        public LuaEngine()
        {
            Initialize();
        }

        private MethodInfo luaDispose = null;
        private MethodInfo luaDoFile = null;
        private MethodInfo luaDoString = null;
        private MethodInfo luaGetFunction = null;
        private MethodInfo luaRegisterFunction = null;
        private MethodInfo luaIsLic = null;
        public void Initialize()
        {
            try
            {
                string assemblyName = X86_DLL;
                if (MachineHelper.Is64BitProcess())
                {
                    assemblyName = X64_DLL;
                }
                
               Assembly assembly = Assembly.LoadFrom(assemblyName);
                Type t=assembly.GetType(luaType);
              
                
                //luaIsLic = ReflectionHelper.GetMethodInfo(t, "isLic");
                int lic= (int) t.InvokeMember("isLic", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null,
                                              null, null);
                LogHelper.Debug("Lic:"+lic);
                if (lic == 1)
                {
                    luavm = Activator.CreateInstance(t);
                    luaDispose = ReflectionHelper.GetMethodInfo(t, "Dispose");
                    luaDoFile = ReflectionHelper.GetMethodInfo(t, "DoFile");
                    luaDoString = ReflectionHelper.GetMethodInfo(t, "DoString");
                    luaGetFunction = ReflectionHelper.GetMethodInfo(t, "GetFunction");
                    luaRegisterFunction = ReflectionHelper.GetMethodInfo(t, "RegisterFunction");
                }
                else
                {                 
                    LogHelper.Error("Licence验证错误,请重新申请");
                   Environment.Exit(0);
                }

            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }


        public void Dispose()
        {
            luaDispose.Invoke(luavm, null);
        }


        public void BindLuaFunctions(object luaAPIClass)
        {
            registerLuaFunctions(luaAPIClass);
        }

        public object[] DoFile(string luaFileName)
        {

            return luaDoFile.Invoke(luavm, new object[] { luaFileName }) as object[];
        }

        public object[] DoString(string luaStr)
        {
            return luaDoString.Invoke(luavm, new object[] { luaStr }) as object[];
        }
       

        public object[] Invoke(string luaFunction, params object[] args)
        {
            object[] os = null;

            object o = luaGetFunction.Invoke(luavm, new object[] { luaFunction });
            Type t = o.GetType();
            MethodInfo methodInfo = ReflectionHelper.GetMethodInfo(t, "Call");
            if (methodInfo != null)
            {
                os = methodInfo.Invoke(o,new object[]{ args}) as object[];
            }
            return os;
        }

        public Dictionary<string, LuaFuncDescriptor> pLuaFuncs = new Dictionary<string, LuaFuncDescriptor>();

        public void registerLuaFunctions(Object pTarget)
        {
            // Sanity checks
            if (luavm == null || pLuaFuncs == null)
                return;

            // Get the target type
            Type pTrgType = pTarget.GetType();

            // ... and simply iterate through all it's methods
            foreach (MethodInfo mInfo in pTrgType.GetMethods())
            {
                // ... then through all this method's attributes
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    // and if they happen to be one of our AttrLuaFunc attributes
                    if (attr.GetType() == typeof(AttrLuaFunc))
                    {
                        AttrLuaFunc pAttr = (AttrLuaFunc)attr;
                        Dictionary<ParameterInfo, string> pParams = new Dictionary<ParameterInfo, string>();

                        // Get the desired function name and doc string, along with parameter info
                        string strFName = pAttr.getFuncName();
                        string strFDoc = pAttr.getFuncDoc();
                        string[] pPrmDocs = pAttr.getFuncParams();

                        // Now get the expected parameters from the MethodInfo object
                        ParameterInfo[] pPrmInfo = mInfo.GetParameters();

                        // If they don't match, someone forgot to add some documentation to the
                        // attribute, complain and go to the next method
                        if (pPrmDocs != null && (pPrmInfo.Length != pPrmDocs.Length))
                        {
                            Console.WriteLine("Function " + mInfo.Name + " (exported as " +
                                              strFName + ") argument number mismatch. Declared " +
                                              pPrmDocs.Length + " but requires " +
                                              pPrmInfo.Length + ".");
                            break;
                        }

                        // Build a parameter <-> parameter doc hashtable
                        for (int i = 0; i < pPrmInfo.Length; i++)
                        {
                            pParams.Add(pPrmInfo[i], pPrmDocs[i]);
                        }

                        // Get a new function descriptor from this information
                        LuaFuncDescriptor pDesc = new LuaFuncDescriptor(strFName, strFDoc, pParams);

                        // Add it to the global hashtable
                        pLuaFuncs.Add(strFName, pDesc);

                        // And tell the VM to register it.
                        luaRegisterFunction.Invoke(luavm, new object[] { strFName, pTarget, mInfo });

                    }
                }
            }
        }

        public override string ToString()
        {
            string str = "\n\r";
            foreach (KeyValuePair<string, LuaFuncDescriptor> luaFuncDescriptor in pLuaFuncs)
            {
                str += luaFuncDescriptor.Value.getFuncFullDoc() + "\n\r";
            }
            return str;
        }
    }

}
