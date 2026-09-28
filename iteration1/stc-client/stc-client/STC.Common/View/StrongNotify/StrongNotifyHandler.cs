using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace STC.Common.View.StrongNotify
{
    /// <summary>
    /// 强提醒处理Handler公共代码和接口
    /// </summary>
    public class StrongNotifyHandler
    {
        #region GR强提醒构造
        private static StrongNotifyHandler instance = new StrongNotifyHandler();
        public static StrongNotifyHandler Instance
        {
            get { return instance; }
        }

        private StrongNotifyManager _managerinstance = new StrongNotifyManager();
        public StrongNotifyManager ManagerInstance
        {
            get { return _managerinstance; }
        }

        public StrongNotifyHandler()
        {

        }
        #endregion

        #region Function
        /// <summary>
        /// 创建强提醒主窗口
        /// </summary>
        public void InitScreen()
        {
            ManagerInstance.InitStrongNotifyScreen();
        }

        /// <summary>
        /// 返回强提醒主窗口
        /// </summary>
        /// <returns></returns>
        public StrongNotifyMainWindow GetStrongNotifyMainWindow()
        {
            return this.ManagerInstance._window;
        }
        /// <summary>
        /// 读取属性
        /// </summary>
        /// <param name="area"></param>
        /// <param name="propertyname"></param>
        /// <returns></returns>
        public string GetPropertyValueFromControl(object area, string propertyname)
        {
            string key = string.Empty;

            // 通过反射检查 MsgId 属性
            var property = area.GetType().GetProperty(propertyname);
            if (property != null && property.PropertyType == typeof(string))
            {
                var value = property.GetValue(area) as string;
                key = value ?? string.Empty;
            }

            if (property != null && property.PropertyType == typeof(double))
            {
                var value = (double)property.GetValue(area);
                return value.ToString();
            }

            return key;
        }


        /// <summary>
        /// 使用反射设置对象的属性值  SetPropertyValueFromControl(person, "Name", "John"); SetPropertyValueFromControl(person, "Age", 30);
        /// </summary>
        /// <param name="obj">要设置属性的对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="val">要设置的属性值</param>
        public void SetPropertyValueFromControl(object obj, string propertyName, object val)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "目标对象不能为空。");

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("属性名不能为空或仅包含空白字符。", nameof(propertyName));

            // 获取对象类型
            Type type = obj.GetType();

            // 获取属性
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (property == null)
                throw new ArgumentException($"属性 '{propertyName}' 在类型 '{type.FullName}' 中不存在。");

            if (!property.CanWrite)
                throw new InvalidOperationException($"属性 '{propertyName}' 是只读属性，无法设置值。");

            // 转换属性值类型并设置值
            object convertedValue = Convert.ChangeType(val, property.PropertyType);
            property.SetValue(obj, convertedValue);
        }

        /// <summary>
        /// 利用反射调用函数  ReflectionHelper.InvokeMethod(program, "SayHello", new object[] { "John" });
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object InvokeMethodFromControl(object obj, string methodName, object[] parameters)
        {
            // 获取方法信息
            MethodInfo methodInfo = obj.GetType().GetMethod(methodName);

            if (methodInfo == null)
            {
                throw new Exception($"方法 {methodName} 不存在.");
            }

            // 调用方法
            return methodInfo?.Invoke(obj, parameters);
        }


        public static T FindVisualParent<T>(DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = System.Windows.Media.VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        /// <summary>
        /// 通用方法：查找视觉树中的第一个指定类型的子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public T FindFirstChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                T foundChild = child as T;
                if (foundChild != null)
                {
                    return foundChild;
                }

                var result = FindFirstChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        #endregion
    }
}
