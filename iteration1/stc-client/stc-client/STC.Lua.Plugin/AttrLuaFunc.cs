using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STC.Lua.Plugin
{
    public class AttrLuaFunc : Attribute
    {
        private string FunctionName;
        private string FunctionDoc;
        private string[] FunctionParameters = null;

        public AttrLuaFunc(string strFuncName, string strFuncDoc, params String[] strParamDocs)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = strParamDocs;
        }

        public AttrLuaFunc(String strFuncName, String strFuncDoc)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
        }

        public String getFuncName()
        {
            return FunctionName;
        }

        public String getFuncDoc()
        {
            return FunctionDoc;
        }

        public String[] getFuncParams()
        {
            return FunctionParameters;
        }
    }
}
