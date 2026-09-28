using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace STC.Lua.Plugin
{
    public class LuaHelper
    {

        public static ILuaEngine CreateLuaEngine()
        {
            ILuaEngine luaEngine = new LuaEngine();
            return luaEngine;
        }
         
    }
}
