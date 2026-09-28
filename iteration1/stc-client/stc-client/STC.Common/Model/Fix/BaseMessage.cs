using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model.Fix
{
    public class BaseMessage : Dictionary<string, object>
    {
        public bool HasGroup(string groupName)
        {
            return ContainsKey(groupName);
        }

        public Dictionary<string, object> getGroup(int index, string groupName)
        {
            if (index < 0)
            {
                return null;
            }
            if (!HasGroup(groupName))
            {
                return null;
            }
            IList list = this[groupName] as IList;
            if (list == null)
            {
                return null;
            }
            if (index >= list.Count)
            {
                return null;
            }
            Object map = list[index];
            Dictionary<string, object> temp = map as Dictionary<string, object>;

            return temp;
        }

        public int GetGroupCount(string groupName)
        {
            int count = 0;
            if (!HasGroup(groupName))
            {
                return 0;
            }
            IList list = this[groupName] as IList;
            if (list != null)
            {
                count = list.Count;
            }
            return count;
        }

        public void AddGroup(Dictionary<string, object> group, string groupName)
        {
            if (!HasGroup(groupName))
            {
                this.Add(groupName, new ArrayList());
            }
            IList list = this[groupName] as IList;
            if (list != null)
            {
                list.Add(group);
            }
        }

        public bool RemoveGroup(Dictionary<string, object> group, string groupName)
        {
            bool b = true;
            if (!HasGroup(groupName))
            {
                return false;
            }
            IList list = this[groupName] as IList;
            if (list != null)
            {
                list.Remove(group);
            }
            return b;
        }

        public Dictionary<string, object> RemoveGroup(int index, string groupName)
        {
            if (index < 0)
            {
                return null;
            }
            if (!HasGroup(groupName))
            {
                return null;
            }
            IList list = this[groupName] as IList;
            if (list == null)
            {
                return null;
            }
            if (index >= list.Count)
            {
                return null;
            }
            Object map = list[index];
            Dictionary<string, object> temp = map as Dictionary<string, object>;
            if (temp != null)
            {
                list.Remove(temp);
            }

            return temp;
        }

        public IEnumerator GetAllGroup(String groupName)
        {
            if (!HasGroup(groupName))
            {
                return null;
            }
            IList list = this[groupName] as IList;
            if (list != null)
            {
                return list.GetEnumerator();
            }
            return null;
        }

        public object Get(string str)
        {
            object o = null;
            if (this.ContainsKey(str))
            {
                o = this[str];
            }
            return o;
        }

        public void Put(string str, object o)
        {

            if (!this.ContainsKey(str))
            {
                this.Add(str, o);
            }
            else
            {
                this[str] = o;
            }

        }
    }
}
