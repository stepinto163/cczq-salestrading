using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace STC.Broker.Plugin.Utils.wpf.Utils
{
    public class TypeUtilities
    {
        public static IEnumerable CreateDynamicallyTypedList(IEnumerable source)
        {
            Type commonBaseClass = GetCommonBaseClass(source);
            Type type = typeof(List<>).MakeGenericType(commonBaseClass);
            MethodInfo method = type.GetMethod("Add");
            object obj = type.GetConstructor(Type.EmptyTypes).Invoke(null);
            foreach (object item in source)
            {
                method.Invoke(obj, new object[1]
                {
                    item
                });
            }

            return (IEnumerable)obj;
        }

        public static Type GetCommonBaseClass(IEnumerable e)
        {
            Type[] types = (from object o in e
                            select o.GetType()).ToArray();
            return GetCommonBaseClass(types);
        }

        public static Type GetCommonBaseClass(Type[] types)
        {
            if (types.Length == 0)
            {
                return typeof(object);
            }

            Type type = types[0];
            for (int i = 1; i < types.Length; i++)
            {
                if (types[i].IsAssignableFrom(type))
                {
                    type = types[i];
                    continue;
                }

                while (!type.IsAssignableFrom(types[i]))
                {
                    type = type.BaseType;
                }
            }

            return type;
        }
    }
}
