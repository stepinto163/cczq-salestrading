

using com.gateway.log;
using STC.Common.Model.CRM;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace STC.Data.Plugin.CRM
{
    public class SqliteManager
    {
        private object _crmLock = new object();

        private const string dbFileName = @"data\workbench_0.0.4.db";
        private string dbFile = "data/workbench_0.0.4.db";
        //private const string CreateBondsInfo = "create table if not exists BondInfo(BondID varchar2(32) UNIQUE, BondKey varchar2(25), BondListInfoModifyDate varchar2(60),BondModifyDate varchar2(60),Delflag char(1),MaturityDate varchar2(60),RemaingingTime varchar2(60),ShortName varchar2(60),ListMarket varchar2(10),pinYin varchar2(512),pinYinFull varchar(1024), goodstype varchar2(32), BondSubtype varchar2(100), OptionDate decimal(100), OptionType varchar2(100))";
        //public const string UpdateData = @"REPLACE INTO BondInfo (BondID,BondKey,BondListInfoModifyDate,BondModifyDate,Delflag,MaturityDate,RemaingingTime,ShortName,ListMarket,pinYin,pinYinFull,goodstype, BondSubtype, OptionDate, OptionType)VALUES(@id,@key,@blmd,@bmd,@df,@md,@rt,@sn,@lm,@py,@pyf,@gt,@bs,@od, @ot)";
        //public const string MatchBondsSql = @"SELECT BondID,BondKey,BondListInfoModifyDate,BondModifyDate,Delflag,MaturityDate,RemaingingTime,ShortName,ListMarket,pinYin,pinYinFull,goodstype, BondSubtype, OptionDate, OptionType  FROM BondInfo where Delflag='0' and RemaingingTime <>'0' and BondID like @Match limit 0,20";
        //public const string GetMaxDateSql = @"SELECT max(BondListInfoModifyDate) blmd, max(BondModifyDate) bmd  FROM BondInfo";
        //public const string GetAllBondsSql = @"SELECT BondID,BondKey,BondListInfoModifyDate,BondModifyDate,Delflag,MaturityDate,RemaingingTime,ShortName,ListMarket,pinYin,pinYinFull,goodstype, BondSubtype, OptionDate, OptionType  FROM BondInfo where Delflag='0' and RemaingingTime <>'0'";
        //public const string DeleteBondSql = @"DELETE FROM BondInfo WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='BondInfo')";

        //broker账号同步
        private const string CreateBrokerInfo = "create table if not exists BrokerInfo(Idb_key varchar2(45) UNIQUE, AccountId varchar2(32), Account varchar2(25), AccountName varchar2(60),UpdateTime varchar2(60),PinYin varchar2(512), Term_ids varchar2(512),crmid varchar2(45))";
        public const string BrokerUpdateData = @"REPLACE INTO BrokerInfo (Idb_key, AccountId,Account,AccountName,UpdateTime, PinYin, Term_ids,crmid)VALUES(@Idb_key, @AccountId,@Account,@AccountName,@UpdateTime, @PinYin, @Term_ids,@crmid)";
        public const string GetBrokerMaxDateSql = @"SELECT max(UpdateTime) updateTime FROM BrokerInfo";
        public const string GetAllBrokerSql = @"SELECT Idb_key, AccountId,Account,AccountName,UpdateTime, PinYin, Term_ids,crmid FROM BrokerInfo";
        public const string DeleteBrokerSql = @"DELETE FROM BrokerInfo WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='BrokerInfo')";
        public const string DeleteBrokerByIDSql = @"DELETE FROM BrokerInfo where Idb_key = @idbkey";
        //trader账号同步
        #region Contact
        private const string CreateContacts = "create table if not exists CrmContact(instcode varchar2(60), idbkey varchar2(32) UNIQUE,crmid varchar2(45),id varchar2(45),namecn varchar2(64),instid varchar2(32),instname varchar2(64),insttypevalue varchar2(32),createtime varchar2(45),updatetime varchar2(45), pinyin varchar2(512), pinyinfull varchar2(512), biztype varchar2(512), contactqbid varchar2(512), isntqbid varchar2(512), qqlist varchar2(512), market varchar2(45), commonuse varchar2(45), instfullnamecn varchar2(512))";
        public const string UpdateContactData = @"REPLACE INTO CrmContact (instcode,idbkey,crmid,id,namecn,instid,instname,insttypevalue,createtime,updatetime,pinyin, pinyinfull, biztype, contactqbid, isntqbid, qqlist, market, commonuse, instfullnamecn) VALUES(@instcode,@idbkey,@crmid,@id,@namecn,@instid,@instname,@insttypevalue,@createtime,@updatetime,@pinyin,@pinyinfull,@biztype,@contactqbid,@isntqbid,@qqlist,@market,@commonuse,@instfullnamecn)";
        public const string GetContactMaxDateSql = @"SELECT max(updatetime) bmd  FROM CrmContact";
        public const string GetAllContactsSql = @"SELECT instcode,idbkey,crmid,id,namecn,instid,instname,insttypevalue,createtime,updatetime, pinyin, pinyinfull, biztype, contactqbid, isntqbid, qqlist, market, commonuse, instfullnamecn FROM CrmContact";
        public const string DeleteContactSql = @"DELETE FROM CrmContact WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='CrmContact')";
        public const string DeleteContractByIDSql = @"DELETE FROM CrmContact where idbkey = @idbkey";
        #endregion

        #region Inst
        private const string CreateInsts = "create table if not exists CrmInst(idbkey varchar2(45) UNIQUE, crmid varchar2(45), id varchar2(45),shortnamecn varchar2(45),shortnameen varchar2(45),instcode varchar2(45),cityvalue varchar2(45),insttypevalue varchar2(45),createtime varchar2(45),updatetime varchar2(45),issuer_code varchar2(45),citycode varchar2(45),companyid varchar2(45),pinyin varchar2(1000),pinyinfull varchar(1000), taglist varchar2(512), bncshortnamelist varchar2(512), bcoshortnamelist varchar2(200), ncdshortnamelist varchar2(200), irsshortnamelist varchar2(200), fullnamecn varchar2(256),sseinstitutioncode varchar2(45),szedealercode varchar2(45),ssetradingentitycode varchar2(45))";
        public const string UpdateInstData = @"REPLACE INTO CrmInst (idbkey,crmid,id,shortnamecn,shortnameen,instcode,cityvalue,insttypevalue,createtime,updatetime,issuer_code,citycode,companyid,pinyin,pinyinfull,taglist,bncshortnamelist, bcoshortnamelist, ncdshortnamelist, irsshortnamelist, fullnamecn,sseinstitutioncode,szedealercode,ssetradingentitycode) VALUES(@idbkey,@crmid,@id,@shortnamecn,@shortnameen,@instcode,@cityvalue,@insttypevalue,@createtime,@updatetime,@issuer_code,@citycode,@companyid,@pinyin,@pinyinfull,@taglist,@bncshortnamelist,@bcoshortnamelist,@ncdshortnamelist,@irsshortnamelist,@fullnamecn,@sseinstitutioncode,@szedealercode,@ssetradingentitycode)";
        public const string GetInstMaxDateSql = @"SELECT max(updatetime) bmd  FROM CrmInst";
        public const string GetAllInstsSql = @"SELECT idbkey,crmid,id,shortnamecn,shortnameen,instcode,cityvalue,insttypevalue,createtime,updatetime,issuer_code,citycode,companyid,pinyin,pinyinfull,taglist,bncshortnamelist,bcoshortnamelist,ncdshortnamelist, irsshortnamelist,fullnamecn,sseinstitutioncode,szedealercode,ssetradingentitycode FROM CrmInst";
        public const string DeleteCrmInstSql = @"DELETE FROM CrmInst";
        public const string DeleteCrmInstByIDSql = @"DELETE FROM CrmInst where idbkey = @idbkey";
        #endregion

        private static SqliteManager sqliteHelper = new SqliteManager();
        public static SqliteManager Instance { get { return sqliteHelper; } }

        private SQLiteConnection conn;

        ~SqliteManager()
        {
            this.Close();
        }

        private SqliteManager()
        {
            try
            {
                LogHandler.Info($"删除Crm");
                if (FileHelper.FileIsExists(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\WorkbenchTemp.db")))
                {
                    FileInfo[] files = new DirectoryInfo(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data")).GetFiles("*.db");
                    if (files != null)
                    {
                        foreach (FileInfo fi in files)
                        {
                            try
                            {
                                if (fi.Name == "WorkbenchTemp.db" || fi.Name == Path.GetFileName(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, dbFileName)))
                                {
                                    File.Delete(fi.FullName);
                                    LogHandler.Info($"删除 {fi.FullName}");
                                }
                            }
                            catch (Exception ex1)
                            {
                                LogHandler.Error(ex1);
                            }
                        }
                    }
                }

                if (conn == null || conn.State == ConnectionState.Closed)
                {
                    if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.DataPath))
                        dbFile = Path.Combine(LocalEnv.ConfInfo.DataPath, dbFileName);

                    string connectionString = string.Format("Data Source={0}", dbFile);
                    conn = new SQLiteConnection(connectionString);
                    //conn.SetPassword("aP@ssw0rd");
                    conn.Open();
                    //ExecuteNonQuery(CreateBondsInfo);
                    ExecuteNonQuery(CreateBrokerInfo);
                    ExecuteNonQuery(CreateInsts);
                    ExecuteNonQuery(CreateContacts);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #region Contact

        public void DeleteContact()
        {
            try
            {
                lock (_crmLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        SQLiteCommand cmd = new SQLiteCommand(DeleteContactSql, conn);
                        int result = cmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("DeleteContact exception," + ex.Message);
            }
        }

        public CrmContact GetConyactMaxDate()
        {
            CrmContact po = null;
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetContactMaxDateSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    po = new CrmContact();
                    po.updatetime = GetString(reader, 0);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("can't get ConyactMaxDate," + ex.Message);
            }

            return po;
        }
        public void InsertConyact(List<CrmContact> list)
        {
            try
            {
                lock(_crmLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        foreach (var po in list)
                        {
                            if (string.IsNullOrEmpty(po.crmid))
                            {
                                SQLiteCommand cmd = new SQLiteCommand(DeleteContractByIDSql, conn, tran);
                                cmd.Parameters.AddRange(new[]
                                {
                                new SQLiteParameter("@idbkey",po.idbkey),
                            });
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                SQLiteCommand cmd = new SQLiteCommand(UpdateContactData, conn, tran);
                                cmd.Parameters.AddRange(new[]
                                {
                                new SQLiteParameter("@instcode",po.instcode),
                                new SQLiteParameter("@idbkey",po.idbkey),
                                new SQLiteParameter("@crmid",po.crmid),
                                new SQLiteParameter("@id",po.id),
                                new SQLiteParameter("@namecn",po.namecn),
                                new SQLiteParameter("@instid",po.instid),
                                new SQLiteParameter("@instname",po.instname),
                                new SQLiteParameter("@insttypevalue",po.insttypevalue),
                                new SQLiteParameter("@createtime",po.createtime),
                                new SQLiteParameter("@updatetime",po.updatetime),
                                new SQLiteParameter("@pinyin",po.pinyin),
                                new SQLiteParameter("@pinyinfull",po.pinyinfull),
                                new SQLiteParameter("@biztype",po.biztype),
                                new SQLiteParameter("@contactqbid",po.contact_qb_id),
                                new SQLiteParameter("@isntqbid",po.inst_qb_id),
                                new SQLiteParameter("@qqlist", po.qqlist),
                                new SQLiteParameter("@market", po.market),
                                new SQLiteParameter("@commonuse", po.commonuse),
                                new SQLiteParameter("@instfullnamecn", po.instfullnamecn)
                            });
                                cmd.ExecuteNonQuery();

                            }
                        }

                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }

        public List<CrmContact> GetAllContacts()
        {
            List<CrmContact> list = new List<CrmContact>();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetAllContactsSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    CrmContact po = new CrmContact();
                    po.instcode = GetString(reader, 0);
                    po.idbkey = GetString(reader, 1);
                    po.crmid = GetString(reader, 2);
                    po.id = GetString(reader, 3);
                    po.namecn = GetString(reader, 4);
                    po.instid = GetString(reader, 5);
                    po.instname = GetString(reader, 6);
                    po.insttypevalue = GetString(reader, 7);
                    po.createtime = GetString(reader, 8);
                    po.updatetime = GetString(reader, 9);
                    po.pinyin = GetString(reader, 10);
                    po.pinyinfull = GetString(reader, 11);
                    po.biztype = GetString(reader, 12);
                    po.contact_qb_id = GetString(reader, 13);
                    po.inst_qb_id = GetString(reader, 14);
                    po.qqlist = GetString(reader, 15);
                    po.market = GetString(reader, 16);
                    po.commonuse = GetString(reader, 17);
                    po.instfullnamecn = GetString(reader, 18);
                    list.Add(po);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return list;
        }
        #endregion

        #region Inst

        public void DeleteInst()
        {
            try
            {
                lock (_crmLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        SQLiteCommand cmd = new SQLiteCommand(DeleteCrmInstSql, conn);
                        int result = cmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("DeleteInst exception," + ex.Message);
            }
        }

        public CrmInst GetInstMaxDate()
        {
            CrmInst po = null;
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetInstMaxDateSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    po = new CrmInst();
                    po.updatetime = GetString(reader, 0);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("can't get InstMaxDate," + ex.Message);
            }

            return po;
        }
        public void InsertInst(List<CrmInst> list)
        {
            try
            {
                lock (_crmLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        foreach (var po in list)
                        {
                            if (string.IsNullOrEmpty(po.crmid))
                            {
                                SQLiteCommand cmd = new SQLiteCommand(DeleteCrmInstByIDSql, conn, tran);
                                cmd.Parameters.AddRange(new[]
                                {
                                new SQLiteParameter("@idbkey",po.idbkey),
                            });
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                SQLiteCommand cmd = new SQLiteCommand(UpdateInstData, conn, tran);
                                cmd.Parameters.AddRange(new[]
                                {
                                new SQLiteParameter("@idbkey",po.idbkey),
                                new SQLiteParameter("@crmid",po.crmid),
                                new SQLiteParameter("@id",po.id),
                                new SQLiteParameter("@shortnamecn",po.shortnamecn),
                                new SQLiteParameter("@shortnameen",po.shortnameen),
                                new SQLiteParameter("@instcode",po.instcode),
                                new SQLiteParameter("@cityvalue",po.cityvalue),
                                new SQLiteParameter("@insttypevalue",po.insttypevalue),
                                new SQLiteParameter("@createtime",po.createtime),
                                new SQLiteParameter("@updatetime",po.updatetime),
                                new SQLiteParameter("@issuer_code",po.issuer_code),
                                new SQLiteParameter("@citycode",po.citycode),
                                new SQLiteParameter("@companyid",po.companyid),
                                new SQLiteParameter("@pinyin",po.pinyin),
                                new SQLiteParameter("@pinyinfull",po.pinyinfull),
                                new SQLiteParameter("@taglist",po.taglist),
                                new SQLiteParameter("@bncshortnamelist",po.bncshortnamelist),
                                new SQLiteParameter("@bcoshortnamelist", po.bcoshortnamelist),
                                new SQLiteParameter("@ncdshortnamelist", po.ncdshortnamelist),
                                new SQLiteParameter("@irsshortnamelist", po.irsshortnamelist),
                                new SQLiteParameter("@fullnamecn", po.fullnamecn),
                                new SQLiteParameter("@sseinstitutioncode", po.sseinstitutioncode),
                                new SQLiteParameter("@szedealercode", po.szedealercode),
                                new SQLiteParameter("@ssetradingentitycode", po.ssetradingentitycode),
                            });
                                cmd.ExecuteNonQuery();
                            }


                        }
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }

        public List<CrmInst> GetAllInsts()
        {
            List<CrmInst> list = new List<CrmInst>();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetAllInstsSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    CrmInst po = new CrmInst();
                    po.idbkey = GetString(reader, 0);
                    po.crmid = GetString(reader, 1);
                    po.id = GetString(reader, 2);
                    po.shortnamecn = GetString(reader, 3);
                    po.shortnameen = GetString(reader, 4);
                    po.instcode = GetString(reader, 5);
                    po.cityvalue = GetString(reader, 6);
                    po.insttypevalue = GetString(reader, 7);
                    po.createtime = GetString(reader, 8);
                    po.updatetime = GetString(reader, 9);
                    po.issuer_code = GetString(reader, 10);
                    po.citycode = GetString(reader, 11);
                    po.companyid = GetString(reader, 12);
                    po.pinyin = GetString(reader, 13);
                    po.pinyinfull = GetString(reader, 14);
                    po.taglist = GetString(reader, 15);
                    po.bncshortnamelist = GetString(reader, 16);
                    po.bcoshortnamelist = GetString(reader, 17);
                    po.ncdshortnamelist = GetString(reader, 18);
                    po.irsshortnamelist = GetString(reader, 19);
                    po.fullnamecn = GetString(reader, 20);
                    po.sseinstitutioncode = GetString(reader, 21);
                    po.szedealercode = GetString(reader, 22);
                    po.ssetradingentitycode = GetString(reader, 23);
                    list.Add(po);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return list;
        }
        #endregion

        #region broker操作

        public void DeleteBroker()
        {
            try
            {
                lock (_crmLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        SQLiteCommand cmd = new SQLiteCommand(DeleteBrokerSql, conn);
                        int result = cmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("DeleteBroker exception," + ex.Message);
            }
        }

        public List<BrokerInfoPo> GetAllBroker()
        {
            List<BrokerInfoPo> list = new List<BrokerInfoPo>();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetAllBrokerSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    BrokerInfoPo po = new BrokerInfoPo();
                    po.Idb_key = GetString(reader, 0);
                    po.AccountId = GetString(reader, 1);
                    po.Account = GetString(reader, 2);
                    po.AccountName = GetString(reader, 3);
                    po.UpdateTime = GetString(reader, 4);
                    po.PinYin = GetString(reader, 5);
                    po.Term_ids = GetString(reader, 6);
                    po.Crm_id = GetString(reader, 7);
                    list.Add(po);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return list;
        }

        public BrokerInfoPo GetBrokerMaxDate()
        {
            BrokerInfoPo po = null;
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetBrokerMaxDateSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    po = new BrokerInfoPo();
                    po.UpdateTime = GetString(reader, 0);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("can't get broker MaxDate," + ex.Message);
            }

            return po;
        }

        public void InsertBroker(List<BrokerInfoPo> list)
        {
            try
            {
                lock (_crmLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        foreach (var po in list)
                        {
                            if (string.IsNullOrEmpty(po.Crm_id))
                            {
                                SQLiteCommand cmd = new SQLiteCommand(DeleteBrokerByIDSql, conn, tran);
                                cmd.Parameters.AddRange(new[]
                                {
                                new SQLiteParameter("@idbkey",po.Idb_key),
                            });

                                cmd.ExecuteNonQuery();
                            }
                            else
                            {

                                SQLiteCommand cmd = new SQLiteCommand(BrokerUpdateData, conn, tran);
                                cmd.Parameters.AddRange(new[]
                                {
                                new SQLiteParameter("@Idb_key",po.Idb_key),
                                new SQLiteParameter("@AccountId",po.AccountId),
                                new SQLiteParameter("@Account",po.Account),
                                new SQLiteParameter("@AccountName",po.AccountName),
                                new SQLiteParameter("@UpdateTime",po.UpdateTime),
                                new SQLiteParameter("@PinYin",po.PinYin),
                                new SQLiteParameter("@Term_ids",po.Term_ids),
                                new SQLiteParameter("@crmid",po.Crm_id)
                            });

                                cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }



        #endregion

        #region 债卷操作

        private decimal? GetDecimal(SQLiteDataReader reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return null;
            }
            else
            {
                return reader.GetDecimal(i);
            }
        }

        private string GetString(SQLiteDataReader reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return null;
            }
            else
            {
                return reader.GetString(i);
            }
        }

        private int ExecuteNonQuery(string sql)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return -999;
        }


        public void Test()
        {

        }

        #endregion

        public void Close()
        {
            if (conn != null && conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn?.Dispose();
            conn = null;
        }
    }
}
