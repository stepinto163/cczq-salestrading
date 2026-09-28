using IDC.Matching.Plugin.Utils;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Frame.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STC.XBond.Plugin.Setting.ViewModel
{
    public class RoleTreeModel : ViewModelBase
    {
        public List<Roles> RolesSt = new List<Roles>();
        public RoleTreeModel()
        {
            RolesSt = getRoles();
        }
        private ObservableCollection<Nodes> _initialNodeList = new ObservableCollection<Nodes>();
        public Dictionary<string, List<ModulesAction>> dicModulesAction = new Dictionary<string, List<ModulesAction>>();
        public Dictionary<string, List<ModulesAction>> dicDefaultModulesAction = new Dictionary<string, List<ModulesAction>>();
        private string RoleId;

        public ObservableCollection<Nodes> InitialNodeList
        {
            get
            {
                return this._initialNodeList;
            }
            set
            {
                this._initialNodeList = value;
                this.OnPropertyChanged("InitialNodeList");
            }
        }
        public void Init()
        {
            string ps = LocalEnv.UserInfo.Password;
            _initialNodeList.Clear();
            dicModulesAction.Clear();
            #region Action
            TUsersQuery usersQuery = AdminHandler.Instance.usersQuery();
            if (usersQuery != null && usersQuery.Datas != null)
            {
                RoleModulesActionQueryReq req = new RoleModulesActionQueryReq();
                req.roleid = "682233810082856960";//usersQuery.Msg.roleid;
                RoleId = "682233810082856960";//usersQuery.Msg.roleid;
                TRoleModulesActionQuery RoleModulesAction = AdminHandler.Instance.roleModulesActionQuery(req);
                if (RoleModulesAction != null && RoleModulesAction.Datas != null)
                {
                    //List<ModulesAction> idlist = RoleModulesAction.Msg.moduleaction.Where((x, i) => RoleModulesAction.Msg.moduleaction.FindIndex(z => z.moduleid == x.moduleid) == i).ToList();//////////去重
                    //foreach (ModulesAction ac in RoleModulesAction.Msg.moduleaction)
                    //{
                    //    if (!dicModulesAction.ContainsKey(ac.moduleid))
                    //    {
                    //        List<ModulesAction> temp = new List<ModulesAction>();
                    //        temp.Add(ac);
                    //        dicModulesAction.Add(ac.moduleid, temp);
                    //    }
                    //    else
                    //    {
                    //        dicModulesAction[ac.moduleid].Add(ac);
                    //    }
                    //}
                    foreach (Module a in RoleModulesAction.Datas)
                    {
                        List<ModulesAction> temp = new List<ModulesAction>();
                        foreach (ModulesAction b in a.moduleaction)
                        {
                            temp.Add(b);
                        }
                        if (!dicModulesAction.ContainsKey(a.id))
                        {
                            dicModulesAction.Add(a.id, temp);
                        }
                    }
                }
            }
            #endregion 

            #region 默认action（静态数据）
            List<ModulesAction> molesaction = getModulesAction();
            foreach (ModulesAction mo in molesaction)
            {
                //if (!dicModulesAction.ContainsKey(mo.moduleid))
                //{
                if (!dicDefaultModulesAction.ContainsKey(mo.moduleid))
                {
                    List<ModulesAction> temp = new List<ModulesAction>();
                    temp.Add(mo);
                    dicDefaultModulesAction.Add(mo.moduleid, temp);
                }
                else
                {
                    dicDefaultModulesAction[mo.moduleid].Add(mo);
                }
                //}
            }
            #endregion

            List<Modules> moles = getModules();
            Dictionary<string, Modules> dicparent = new Dictionary<string, Modules>();
            Dictionary<string, List<Modules>> dicchild = new Dictionary<string, List<Modules>>();
            foreach (Modules d in moles)
            {
                if (!dicparent.ContainsKey(d.parentid))
                    dicparent.Add(d.parentid, d);
                if (d.parentid != d.id)
                {
                    if (!dicchild.ContainsKey(d.parentid))
                    {
                        List<Modules> temp = new List<Modules>();
                        temp.Add(d);
                        dicchild.Add(d.parentid, temp);
                    }
                    else
                    {
                        dicchild[d.parentid].Add(d);
                    }
                }
            }
            //var parent = from m in moles
            //             where m.parentid == m.id
            //             select m;
            //var parent = moles.Find(o => o.parentid!="");
            foreach (Modules mo in dicparent.Values)
            {
                var child = from m in moles
                            where (mo.id == m.parentid && mo.id != m.id)
                            select m;
                var childn = new List<Nodes>();
                bool isParentCheck = true;
                if (dicchild.ContainsKey(mo.parentid))
                {
                    foreach (Modules co in dicchild[mo.parentid])
                    {
                        Nodes leafTwoNode = new Nodes();
                        leafTwoNode.Id = co.id;
                        leafTwoNode.Modulename = co.modulename;
                        leafTwoNode.parentid = mo.parentid;
                        leafTwoNode.remark = co.remark;
                        if (IsSelected(co.id))
                        {
                            leafTwoNode.initSelect = true;
                            leafTwoNode.IsSelected = true;
                        }
                        else
                            isParentCheck = false;
                        //leafTwoNode.Parent = new Nodes();
                        //leafTwoNode.Parent.Id = mo.id;
                        //leafTwoNode.Parent.Modulename = mo.modulename;
                        //if (IsSelected(mo.id))
                        //{
                        //    leafTwoNode.Parent.initSelect = true;
                        //    leafTwoNode.Parent.IsSelected = true;
                        //}
                        if (dicDefaultModulesAction.ContainsKey(co.id))
                        {
                            leafTwoNode.actionst = new List<ModulesAction>();
                            if (dicModulesAction.ContainsKey(co.id))
                            {
                                foreach (ModulesAction m in dicDefaultModulesAction[co.id])
                                {
                                    ModulesAction action = new ModulesAction();
                                    action = m;
                                    foreach (ModulesAction n in dicModulesAction[co.id])
                                    {
                                        if (m.id == n.id)
                                        {
                                            action.IsSelected = true;
                                        }
                                    }
                                    //leafTwoNode.actionst = new List<ModulesAction>();
                                    leafTwoNode.actionst.Add(action);
                                }
                            }
                            else
                                leafTwoNode.actionst.AddRange(dicDefaultModulesAction[co.id]);
                        }
                        childn.Add(leafTwoNode);
                    }
                }
                var node1 = new Nodes();
                node1.Modulename = mo.modulename;
                node1.Childs = childn;
                node1.Id = mo.id;
                node1.remark = mo.remark;
                if (child.Count() > 0)
                {
                    node1.initSelect = isParentCheck;
                    node1.IsSelected = isParentCheck;
                }
                InitialNodeList.Add(node1);
            }
        }

        private bool IsSelected(string id)
        {
            bool isSelect = false;
            if (dicModulesAction.ContainsKey(id))
            {
                isSelect = true;
            }
            return isSelect;
        }
        public List<Roles> getRoles()
        {
            List<Roles> roles = new List<Roles>();
            TRolesQuery Roles = AdminHandler.Instance.rolesQuery();
            if (Roles != null && Roles.Msg.Count > 0)
            {
                return Roles.Msg;
            }
            else
                return new List<Roles>();
        }

        public List<Modules> getModules()
        {
            List<Modules> Modules = new List<Modules>();
            TModulesQuery moles = AdminHandler.Instance.modulesQuery();
            if (moles != null && moles.Msg.Count > 0)
            {
                return moles.Msg;
            }
            else
                return new List<Modules>();
        }

        public List<ModulesAction> getModulesAction()
        {
            List<ModulesAction> Modules = new List<ModulesAction>();
            TModulesActionQuery moles = AdminHandler.Instance.modulesActionQuery();
            if (moles != null && moles.Msg.Count > 0)
            {
                return moles.Msg;
            }
            else
                return new List<ModulesAction>();
        }

        public ICommand SaveActionCommand { get { return new RelayCommand(SaveAction); } }
        // public ICommand SaveActionCommand { get { return new RelayCommand(SaveAction); } }
        public ICommand CancelActionCommand { get { return new RelayCommand(CancelAction); } }
        public ICommand LockCommand { get { return new RelayCommand(LockAction); } }

        public void SaveAction()
        {
            RolesModulesActionUpdateReq req = new RolesModulesActionUpdateReq();
            req.roleid = RoleId;
            foreach (Nodes m in _initialNodeList)
            {
                if (m.IsChange)
                {
                    ModulesInfo info1 = new ModulesInfo();
                    info1.moduleid = m.id;
                    info1.modulesStatus = m.IsSelected.Value ? "1" : "0";
                    req.modulesInfo.Add(info1);
                }
                foreach (Nodes n in m.Childs)
                {
                    //if (n.IsChange|| (n.Actionst != null && n.Actionst.Count > 0))
                    //{
                    ModulesInfo info1 = new ModulesInfo();
                    bool isChange = false;
                    if (n.Actionst != null && n.Actionst.Count > 0)
                    {
                        foreach (ModulesAction action in n.Actionst)
                        {
                            if (action.IsChange == true)
                            {
                                isChange = true;
                                ModulesAction ns = new ModulesAction();
                                ns.id = action.id;
                                ns.moduleid = n.id;
                                ns.actionStatus = action.IsSelected ? "1" : "0";
                                info1.moduleactionInfo.Add(ns);
                            }
                        }
                    }
                    if (n.IsChange || isChange)
                    {
                        info1.moduleid = n.id;
                        info1.modulesStatus = n.IsSelected.Value ? "1" : "0";
                        req.modulesInfo.Add(info1);
                    }
                }
                //}
            }

            PResult result = AdminHandler.Instance.rolesModulesActionUpdate(req);
        }

        public void CancelAction()
        {
        }

        public void LockAction()
        {

        }
        public TRoleUpdate AddRole(RoleUpdateReq req)
        {
            return AdminHandler.Instance.roleUpdate(req);
        }
        public void DeleteRole(RoleUpdateReq req)
        {
            TRoleUpdate result = AdminHandler.Instance.roleUpdate(req);
        }
    }
}