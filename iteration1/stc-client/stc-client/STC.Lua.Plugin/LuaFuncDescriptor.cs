using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace STC.Lua.Plugin
{
    public class LuaFuncDescriptor
    {
        private String FunctionName;
        private String FunctionDoc;
        private Dictionary<ParameterInfo, string> FunctionParameters;
       
        private String FunctionDocString;

        public LuaFuncDescriptor(String strFuncName, String strFuncDoc, Dictionary<ParameterInfo, string> pParams)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = pParams;
        

            String strFuncHeader = strFuncName + "(%params%) - " + strFuncDoc;
            String strFuncBody = "";
            String strFuncParams = "";

            Boolean bFirst = true;

            foreach (var strFuncParam in FunctionParameters)
            {
                if (!bFirst)
                    strFuncParams += ", ";

                strFuncParams += strFuncParam.Key;
                strFuncBody += " " + strFuncParam.Key.Name + ":" + strFuncParam.Value + "(" + strFuncParam.Key.ParameterType.Name + "),";

                bFirst = false;
            }
            if (strFuncBody.Length>0)
            strFuncBody = strFuncBody.Substring(0, strFuncBody.Length - 1);
            if (bFirst && strFuncBody.Length > 0)
                strFuncBody = strFuncBody.Substring(0, strFuncBody.Length - 1);

            FunctionDocString = strFuncHeader.Replace("%params%", strFuncParams) + strFuncBody;
        }

        public String getFuncName()
        {
            return FunctionName;
        }

        public String getFuncDoc()
        {
            return FunctionDoc;
        }

        

        public String getFuncHeader()
        {
            if (FunctionDocString.IndexOf("\n") == -1)
                return FunctionDocString;

            return FunctionDocString.Substring(0, FunctionDocString.IndexOf("\n"));
        }

        public String getFuncFullDoc()
        {
            return FunctionDocString;
        }
    }
}
