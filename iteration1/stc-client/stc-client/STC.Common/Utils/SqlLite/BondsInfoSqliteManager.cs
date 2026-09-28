using com.gateway.log;
using STC.Common.Model.Base;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace STC.Common.Utils.SqlLite
{
    public class BondsInfoSqliteManager
    {
        private object _bondLock = new object();

        private const string dbFileName = @"data\STC_1.0.16.db";
        private string dbFile = "data/STC_1.0.16.db";
        private const string CreateBondsInfo = "create table if not exists BondInfo(BondID varchar2(32) UNIQUE, BondKey varchar2(25), BondListInfoModifyDate varchar2(60),BondModifyDate varchar2(60),Delflag char(1)," +
            "MaturityDate varchar2(60),RemaingingTime varchar2(60),ShortName varchar2(60),ListMarket varchar2(10),pinYin varchar2(512),pinYinFull varchar(1024), goodstype varchar2(32), BondSubtype varchar2(100), " +
            "OptionDate decimal(100), OptionType varchar2(100), RatingCurrent varchar2(32),SecurityTypeId varchar2(20), FullNameCN varchar2(200), FullNameEN varchar2(200), Flag varchar2(10), " +
            "CfetsModifyDate varchar2(50), EntCor varchar2(50), IssuerRatingCurrentNPY varchar2(50), MarketIndicator varchar2(10), securitydesc varchar2(100), issuedate varchar2(50), issuer varchar2(50), SecurityTerm varchar2(50), " +
            "CNShortName varchar2(100),Coupon_Rate_Current DECIMAL(7,4),Min_change varchar2(10), IssuerRatingCurrent varchar2(50),IssuerCode varchar2(50), Fund_Objectives_Category varchar2(200),Category_flag varchar2(200),Asset_Status varchar2(50),Issue_Amount DECIMAL(16, 4),symbolType varchar2(50), bondSigns text, cdcAuth varchar2(50))";
        public const string UpdateData = @"REPLACE INTO BondInfo (BondID,BondKey,BondListInfoModifyDate,BondModifyDate,Delflag,MaturityDate,RemaingingTime,ShortName,ListMarket,pinYin,pinYinFull,goodstype, BondSubtype, OptionDate, OptionType, " +
            "RatingCurrent, SecurityTypeId, FullNameCN, FullNameEN, Flag, CfetsModifyDate, EntCor, IssuerRatingCurrentNPY, MarketIndicator, securitydesc, issuedate, issuer, SecurityTerm, " +
            "CNShortName,Coupon_Rate_Current,Min_change, IssuerRatingCurrent,IssuerCode,Fund_Objectives_Category,Category_flag,Asset_Status,Issue_Amount,symbolType, bondSigns, cdcAuth) " +
            "VALUES(@id,@key,@blmd,@bmd,@df,@md,@rt,@sn,@lm,@py,@pyf,@gt,@bs,@od, @ot,@rc, @st, @fnc, @fne, @flag,@Cftes, @ec, @irc, @mki, @secd, @issd, @issu, @sect, @cnsn,@crc,@mchange,@irct,@ircode,@foc,@cateflag,@assetstatus,@issueamount,@symboltype,@bondSigns, @cdcAuth)";
        public const string MatchBondsSql = @"SELECT BondID,BondKey,BondListInfoModifyDate,BondModifyDate,Delflag,MaturityDate,RemaingingTime,ShortName,ListMarket,pinYin,pinYinFull,goodstype, " +
            "BondSubtype, OptionDate, OptionType, RatingCurrent, SecurityTypeId, FullNameCN, FullNameEN, Flag, CfetsModifyDate, EntCor, IssuerRatingCurrentNPY, MarketIndicator, securitydesc, " +
            "issuedate, issuer, SecurityTerm, CNShortName,Coupon_Rate_Current,Min_change, IssuerRatingCurrent,IssuerCode,Fund_Objectives_Category,Category_flag,Asset_Status,Issue_Amount,symbolType,bondSigns, cdcAuth) FROM BondInfo where Delflag='0' and RemaingingTime <>'0' and BondID like @Match limit 0,20";
        public const string GetMaxDateSql = @"SELECT max(BondListInfoModifyDate) blmd, max(BondModifyDate) bmd, max(CfetsModifyDate) cmd  FROM BondInfo";
        public const string GetAllBondsSql = @"SELECT BondID,BondKey,BondListInfoModifyDate,BondModifyDate,Delflag,MaturityDate,RemaingingTime,ShortName,ListMarket,pinYin,pinYinFull,goodstype, " +
            "BondSubtype, OptionDate, OptionType, RatingCurrent, SecurityTypeId, FullNameCN, FullNameEN, Flag, CfetsModifyDate, EntCor, IssuerRatingCurrentNPY, MarketIndicator, securitydesc, " +
            "issuedate, issuer, SecurityTerm, CNShortName,Coupon_Rate_Current, Min_change, IssuerRatingCurrent,IssuerCode,Fund_Objectives_Category,Category_flag,Asset_Status,Issue_Amount,symbolType,bondSigns,cdcAuth FROM BondInfo";// where Delflag='0' and RemaingingTime <>'0'";
        public const string GetBondBySecurityIdSql = @"SELECT BondID,BondKey,BondListInfoModifyDate,BondModifyDate,Delflag,MaturityDate,RemaingingTime,ShortName,ListMarket,pinYin,pinYinFull,goodstype, " +
            "BondSubtype, OptionDate, OptionType, RatingCurrent, SecurityTypeId, FullNameCN, FullNameEN, Flag, CfetsModifyDate, EntCor, IssuerRatingCurrentNPY, MarketIndicator, securitydesc, " +
            "issuedate, issuer, SecurityTerm, CNShortName,Coupon_Rate_Current, Min_change, IssuerRatingCurrent,IssuerCode,Fund_Objectives_Category,Category_flag,Asset_Status,Issue_Amount,symbolType,bondSigns,cdcAuth FROM BondInfo where BondID=@id";
        public const string GetAllSimpleBondsSql = @"SELECT BondID,MaturityDate,ShortName,ListMarket,pinYin,pinYinFull," +
            "OptionDate, OptionType, SecurityTypeId, FullNameEN, Flag, MarketIndicator,symbolType FROM BondInfo";
        public const string DeleteBondSql = @"DELETE FROM BondInfo";

        //broker账号同步
        private const string CreateBrokerInfo = "create table if not exists BrokerInfo(Idb_key varchar2(45) UNIQUE, AccountId varchar2(32), Account varchar2(25), AccountName varchar2(60),UpdateTime varchar2(60),PinYin varchar2(512), Term_ids varchar2(512))";
        public const string BrokerUpdateData = @"REPLACE INTO BrokerInfo (Idb_key, AccountId,Account,AccountName,UpdateTime, PinYin, Term_ids)VALUES(@Idb_key, @AccountId,@Account,@AccountName,@UpdateTime, @PinYin, @Term_ids)";
        public const string GetBrokerMaxDateSql = @"SELECT max(UpdateTime) updateTime FROM BrokerInfo";
        public const string GetAllBrokerSql = @"SELECT Idb_key, AccountId,Account,AccountName,UpdateTime, PinYin, Term_ids FROM BrokerInfo";
        public const string DeleteBrokerSql = @"DELETE FROM BrokerInfo";

        //trader账号同步
        private const string CreateContacts = "create table if not exists CrmContact(instcode varchar2(60), idbkey varchar2(32) UNIQUE,crmid varchar2(45),id varchar2(45),namecn varchar2(64),instid varchar2(32),instname varchar2(64),insttypevalue varchar2(32),createtime varchar2(45),updatetime varchar2(45), pinyin varchar2(512), pinyinfull varchar2(512), biztype varchar2(512), contactqbid varchar2(512), isntqbid varchar2(512))";
        public const string UpdateContactData = @"REPLACE INTO CrmContact (instcode,idbkey,crmid,id,namecn,instid,instname,insttypevalue,createtime,updatetime,pinyin, pinyinfull, biztype, contactqbid, isntqbid) VALUES(@instcode,@idbkey,@crmid,@id,@namecn,@instid,@instname,@insttypevalue,@createtime,@updatetime,@pinyin,@pinyinfull,@biztype,@contactqbid,@isntqbid)";
        public const string GetContactMaxDateSql = @"SELECT max(updatetime) bmd  FROM CrmContact";
        public const string GetAllContactsSql = @"SELECT instcode,idbkey,crmid,id,namecn,instid,instname,insttypevalue,createtime,updatetime, pinyin, pinyinfull, biztype, contactqbid, isntqbid FROM CrmContact";
        public const string DeleteContactSql = @"DELETE FROM CrmContact";


        #region Inst
        private const string CreateInsts = "create table if not exists CrmInst(Id varchar2(45), CnFullName varchar2(200), CnShortName varchar2(100),PartyCode21 varchar2(45),PartyCode6 varchar2(45) UNIQUE,UpdateTime varchar2(45))";
        public const string UpdateInstData = @"REPLACE INTO CrmInst (Id,CnFullName,CnShortName,PartyCode21,PartyCode6,UpdateTime) VALUES(@id, @cfn, @csn, @pc21, @pc6, @ut)";
        public const string GetInstMaxDateSql = @"SELECT max(UpdateTime) bmd  FROM CrmInst";
        public const string GetAllInstsSql = @"SELECT Id,CnFullName,CnShortName,PartyCode21,PartyCode6,UpdateTime FROM CrmInst";
        public const string DeleteCrmInstSql = @"DELETE FROM CrmInst";

        #endregion

        private static BondsInfoSqliteManager sqliteHelper = new BondsInfoSqliteManager();
        public static BondsInfoSqliteManager Instance { get { return sqliteHelper; } }

        private SQLiteConnection conn;

        ~BondsInfoSqliteManager()
        {
            this.Close();
        }

        private BondsInfoSqliteManager()
        {
            try
            {
                LogHandler.Info($"删除Base");
                if (FileHelper.FileIsExists(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\STCTemp.db")))
                {
                    FileInfo[] files = new DirectoryInfo(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data")).GetFiles("*.db");
                    if (files != null)
                    {
                        foreach (FileInfo fi in files)
                        {
                            try
                            {
                                if (fi.Name == "STCTemp.db" || fi.Name == Path.GetFileName(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, dbFileName)))
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

                    string connectionString = string.Format("Data Source={0};Version=3;", dbFile);
                    
                    LogHelper.Info(connectionString);
                    conn = new SQLiteConnection(connectionString);
                    //conn.SetPassword("aP@ssw0rd");
                    conn.Open();
                    ExecuteNonQuery(CreateBondsInfo);
                    //ExecuteNonQuery(CreateBrokerInfo);
                    //ExecuteNonQuery(CreateInsts);
                    //ExecuteNonQuery(CreateContacts);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
        #region 债卷操作

        public void DeleteBond()
        {
            try
            {
                lock (_bondLock)
                {
                    using (SQLiteTransaction tran = GetSqliteConnection().BeginTransaction())
                    {
                        SQLiteCommand cmd = new SQLiteCommand(DeleteBondSql, conn);
                        int result = cmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("DeleteBond exception," + ex.Message);
            }
        }


        public void Insert(List<BondsInfoPo> list)
        {
            try
            {
                lock (_bondLock)
                {
                    //LogHelper.Info("Auto commit:" + conn.AutoCommit);
                    using (SQLiteTransaction tran = GetSqliteConnection().BeginTransaction())
                    {
                        foreach (var po in list)
                        {
                            //SQLiteCommand cmd = new SQLiteCommand(UpdateData, conn, tran);
                            SQLiteCommand cmd = new SQLiteCommand(UpdateData, conn);
                            cmd.Parameters.AddRange(new[]
                            {
                                new SQLiteParameter("@id",po.Bond_ID),
                                new SQLiteParameter("@key",po.Bond_Key),
                                new SQLiteParameter("@blmd",po.bondlistinfo_modify_date),
                                new SQLiteParameter("@bmd",po.bond_modify_date),
                                new SQLiteParameter("@df",po.delflag),
                                new SQLiteParameter("@md",po.Maturity_Date),
                                new SQLiteParameter("@rt", (!string.IsNullOrEmpty(po.MarketIndicator) && po.MarketIndicator.Equals("4")? STCHelper.SetShowTenor(po):string.Empty)),
                                new SQLiteParameter("@sn",po.Short_Name),
                                new SQLiteParameter("@lm",po.listed_Market),
                                new SQLiteParameter("@py",po.pinYin),
                                new SQLiteParameter("@pyf",po.pinyin_full),
                                new SQLiteParameter("@gt", po.SECURITY_TYPE),
                                new SQLiteParameter("@bs", po.Bond_Subtype),
                                new SQLiteParameter("@od", po.Option_Date),
                                new SQLiteParameter("@ot", po.Option_Type),
                                new SQLiteParameter("@rc", po.Rating_Current),

                                new SQLiteParameter("@st", po.Security_Type_Id),
                                //new SQLiteParameter("@sd", po.Security_Dec),
                                new SQLiteParameter("@fnc", po.Full_Name_CN),
                                new SQLiteParameter("@fne", po.Full_Name_EN),
                                new SQLiteParameter("@flag", po.flag),
                                new SQLiteParameter("@Cftes", po.bondmaster_modify_date),
                                new SQLiteParameter("@ec", po.Ent_Cor),
                                new SQLiteParameter("@irc", po.Issuer_Rating_Current_NPY),
                                new SQLiteParameter("@mki", po.MarketIndicator),
                                new SQLiteParameter("@secd", po.SECURITY_DEC),
                                new SQLiteParameter("@issd", po.ISSUE_DATE),
                                new SQLiteParameter("@issu", po.issuer),
                                new SQLiteParameter("@sect", po.Security_Term),
                                new SQLiteParameter("@cnsn", po.CN_Short_Name),
                                new SQLiteParameter("@crc", po.Coupon_Rate_Current)
                                ,new SQLiteParameter("@mchange", po.Min_change)
                                ,new SQLiteParameter("@irct", po.Issuer_Rating_Current)
                                ,new SQLiteParameter("@ircode", po.Issuer_Code)
                                ,new SQLiteParameter("@foc", po.Fund_Objectives_Category)
                                ,new SQLiteParameter("@cateflag", po.Category_flag)
                                ,new SQLiteParameter("@assetstatus", po.Asset_Status)
                                ,new SQLiteParameter("@issueamount", po.Issue_Amount)
                                ,new SQLiteParameter("@symboltype", po.symbolType)
                                ,new SQLiteParameter("@bondSigns", po.bondSigns)
                                ,new SQLiteParameter("@cdcAuth", po.cdcAuth)
                            });
                            cmd.ExecuteNonQuery();
                        }
                        //if (!conn.AutoCommit)
                        //{
                        //    SQLiteTransaction tran = conn.BeginTransaction();
                        //    tran.Commit();
                        //}
                        tran.Commit();
                    }
                }
            }
            catch (SQLiteException ex)
            {
                LogHelper.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public List<BondsInfoPo> MatchBonds(string match)
        {
            List<BondsInfoPo> list = new List<BondsInfoPo>();
            try
            {
                match = string.Format("%{0}%", match);
                SQLiteCommand cmd = new SQLiteCommand(MatchBondsSql, GetSqliteConnection());
                cmd.Parameters.Add(new SQLiteParameter("@Match", match));
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    BondsInfoPo po = new BondsInfoPo();
                    po.Bond_ID = GetString(reader, 0);
                    po.Bond_Key = GetString(reader, 1);
                    po.bondlistinfo_modify_date = GetString(reader, 2);
                    po.bond_modify_date = GetString(reader, 3);
                    po.delflag = GetString(reader, 4);
                    po.Maturity_Date = GetString(reader, 5);

                    po.Short_Name = GetString(reader, 7);
                    po.listed_Market = GetString(reader, 8);
                    po.pinYin = GetString(reader, 9);
                    po.pinyin_full = GetString(reader, 10);
                    po.SECURITY_TYPE = GetString(reader, 11);
                    po.Bond_Subtype = GetString(reader, 12);
                    po.Option_Date = GetDecimal(reader, 13);
                    po.Option_Type = GetString(reader, 14);
                    po.Rating_Current = GetString(reader, 15);

                    po.Security_Type_Id = GetString(reader, 16);
                    //po.Security_Dec = GetString(reader, 17);
                    po.Full_Name_CN = GetString(reader, 17);
                    po.Full_Name_EN = GetString(reader, 18);
                    po.flag = GetString(reader, 19);
                    po.bondmaster_modify_date = GetString(reader, 20);
                    po.Ent_Cor = GetString(reader, 21);
                    po.Issuer_Rating_Current_NPY = GetString(reader, 22);
                    po.MarketIndicator = GetString(reader, 23);

                    po.SECURITY_DEC = GetString(reader, 24);
                    po.ISSUE_DATE = GetString(reader, 25);
                    po.issuer = GetString(reader, 26);
                    po.Security_Term = GetString(reader, 27);

                    po.CN_Short_Name = GetString(reader, 28);
                    po.Coupon_Rate_Current = GetString(reader, 29);
                    po.RemaingingTime = (!string.IsNullOrEmpty(po.MarketIndicator) && po.MarketIndicator.Equals("4") ? STCHelper.SetShowTenor(po) : string.Empty);//   STCHelper.SetShowTenor(po);
                    po.Min_change = GetString(reader, 30);
                    po.Issuer_Rating_Current = GetString(reader, 31);
                    po.Issuer_Code = GetString(reader, 32);
                    po.Fund_Objectives_Category = GetString(reader, 33);
                    po.Category_flag = GetString(reader, 34);
                    po.Asset_Status = GetString(reader, 35);
                    po.Issue_Amount = GetDecimal(reader, 36);
                    po.symbolType = GetString(reader, 37);
                    po.bondSigns = GetString(reader, 38);
                    po.cdcAuth = GetString(reader, 39);
                    //try
                    //{
                    //    List<string> signlist = new List<string>();
                    //    List<Dictionary<string, string>> signdiclist = new List<Dictionary<string, string>>();
                    //    STC.Data.Plugin.Handler.DataHandler.Instance.ParserBondSigns(po.bondSigns, ref signdiclist, ref signlist);
                    //    po.SignDicList = signdiclist;
                    //    po.SignList = signlist;
                    //}
                    //catch (Exception ex)
                    //{
                    //    LogHelper.Error($"ParserBondSigns Error:{po.Bond_ID} {po.bondSigns} ErrorMessage:{ex.Message}");
                    //}

                    list.Add(po);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return list;
        }


        ///// <summary> 
        ///// 执行一个查询语句，返回一个包含查询结果的DataTable。 
        ///// </summary> 
        ///// <param name="sql">要执行的查询语句。</param> 
        ///// <returns></returns> 
        ///// <exception cref="Exception"></exception>
        //public DataTable ExecuteQuery()
        //{
        //    using (SQLiteConnection connection = new SQLiteConnection(conn))
        //    {
        //        string sql = @"SELECT BondID as Bond_ID, BondKey as Bond_Key, BondListInfoModifyDate as bondlistinfo_modify_date, BondModifyDate as bond_modify_date, Delflag as delflag, MaturityDate as Maturity_Date, RemaingingTime, ShortName as Short_Name, ListMarket as listed_Market, pinYin, pinYinFull as pinyin_full, goodstype as SECURITY_TYPE, " +
        //    "BondSubtype as Bond_Subtype, OptionDate as Option_Date, OptionType as Option_Type, RatingCurrent as Rating_Current, SecurityTypeId as Security_Type_Id, FullNameCN as Full_Name_CN, FullNameEN as Full_Name_EN, Flag as flag, CfetsModifyDate as bondmaster_modify_date, EntCor as Ent_Cor, IssuerRatingCurrentNPY as Issuer_Rating_Current_NPY, MarketIndicator as MarketIndicator, securitydesc as SECURITY_DEC, " +
        //    "issuedate as ISSUE_DATE, issuer, SecurityTerm as Security_Term, CNShortName CN_Short_Name,Coupon_Rate_Current as Coupon_Rate_Current ,Min_change as Min_change FROM BondInfo";
        //        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
        //        {
        //            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
        //            DataTable data = new DataTable();
        //            try { adapter.Fill(data); }
        //            catch (Exception) { throw; }
        //            return data;
        //        }
        //    }
        //}



        public List<SimpleBondsInfoPo> GetAllSimpleBonds()
        {
            List<SimpleBondsInfoPo> list = new List<SimpleBondsInfoPo>();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetAllSimpleBondsSql, GetSqliteConnection());
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    SimpleBondsInfoPo po = new SimpleBondsInfoPo();
                    po.Bond_ID = GetString(reader, 0);
                    po.Maturity_Date = GetString(reader, 1);
                    po.Short_Name = GetString(reader, 2);
                    po.listed_Market = GetString(reader, 3);
                    po.pinYin = GetString(reader, 4);
                    po.pinyin_full = GetString(reader, 5);
                    po.Option_Date = GetDecimal(reader, 6);
                    po.Option_Type = GetString(reader, 7);
                    po.Security_Type_Id = GetString(reader, 8);
                    po.Full_Name_EN = GetString(reader, 9);
                    po.flag = GetString(reader, 10);
                    po.MarketIndicator = GetString(reader, 11);
                    po.symbolType = GetString(reader, 12);
                    list.Add(po);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return list;
        }

        public List<BondsInfoPo> GetAllBonds()
        {
            List<BondsInfoPo> list = new List<BondsInfoPo>();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetAllBondsSql, GetSqliteConnection());
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    BondsInfoPo po = new BondsInfoPo();
                    po.Bond_ID = GetString(reader, 0);
                    po.Bond_Key = GetString(reader, 1);
                    po.bondlistinfo_modify_date = GetString(reader, 2);
                    po.bond_modify_date = GetString(reader, 3);
                    po.delflag = GetString(reader, 4);
                    po.Maturity_Date = GetString(reader, 5);

                    po.Short_Name = GetString(reader, 7);
                    po.listed_Market = GetString(reader, 8);
                    po.pinYin = GetString(reader, 9);
                    po.pinyin_full = GetString(reader, 10);
                    po.SECURITY_TYPE = GetString(reader, 11);
                    po.Bond_Subtype = GetString(reader, 12);
                    po.Option_Date = GetDecimal(reader, 13);
                    po.Option_Type = GetString(reader, 14);
                    po.Rating_Current = GetString(reader, 15);

                    po.Security_Type_Id = GetString(reader, 16);
                    //po.Security_Dec = GetString(reader, 17);
                    po.Full_Name_CN = GetString(reader, 17);
                    po.Full_Name_EN = GetString(reader, 18);
                    po.flag = GetString(reader, 19);
                    po.bondmaster_modify_date = GetString(reader, 20);
                    po.Ent_Cor = GetString(reader, 21);
                    po.Issuer_Rating_Current_NPY = GetString(reader, 22);
                    po.MarketIndicator = GetString(reader, 23);

                    po.SECURITY_DEC = GetString(reader, 24);
                    po.ISSUE_DATE = GetString(reader, 25);

                    po.issuer = GetString(reader, 26);
                    po.Security_Term = GetString(reader, 27);

                    po.CN_Short_Name = GetString(reader, 28);
                    po.Coupon_Rate_Current = reader["Coupon_Rate_Current"].ToString();
                    po.RemaingingTime = (!string.IsNullOrEmpty(po.MarketIndicator) && po.MarketIndicator.Equals("4") ? STCHelper.SetShowTenor(po) : string.Empty);//  STCHelper.SetShowTenor(po);
                    po.Min_change = GetString(reader, 30);
                    po.Issuer_Rating_Current = GetString(reader, 31);
                    po.Issuer_Code = GetString(reader, 32);
                    po.Fund_Objectives_Category = GetString(reader, 33);
                    po.Category_flag = GetString(reader, 34);
                    po.Asset_Status = GetString(reader, 35);
                    po.Issue_Amount = GetDecimal(reader, 36);
                    po.symbolType = GetString(reader, 37);
                    po.bondSigns = GetString(reader, 38);
                    po.cdcAuth = GetString(reader, 39);
                    list.Add(po);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return list;
        }

        private SQLiteConnection GetSqliteConnection()
        {
            if (conn == null || conn.State == ConnectionState.Closed)
            {
                
                if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.DataPath))
                    dbFile = Path.Combine(LocalEnv.ConfInfo.DataPath, dbFileName);

                string connectionString = string.Format("Data Source={0};Version=3;", dbFile);
                conn = new SQLiteConnection(connectionString);
                conn.Open();

                LogHelper.Info("Reconnect Sqlite.");
            }

            return conn;
        }


        public BondsInfoPo GetBondsInfoBySecurityID(string securityID)
        {
            if (string.IsNullOrEmpty(securityID))
            {
                return null;
            }

            List<BondsInfoPo> list = new List<BondsInfoPo>();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetBondBySecurityIdSql, GetSqliteConnection());
                cmd.Parameters.AddRange(new[]
                {
                    new SQLiteParameter("@id",securityID),
                });


                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    BondsInfoPo po = new BondsInfoPo();
                    po.Bond_ID = GetString(reader, 0);
                    po.Bond_Key = GetString(reader, 1);
                    po.bondlistinfo_modify_date = GetString(reader, 2);
                    po.bond_modify_date = GetString(reader, 3);
                    po.delflag = GetString(reader, 4);
                    po.Maturity_Date = GetString(reader, 5);

                    po.Short_Name = GetString(reader, 7);
                    po.listed_Market = GetString(reader, 8);
                    po.pinYin = GetString(reader, 9);
                    po.pinyin_full = GetString(reader, 10);
                    po.SECURITY_TYPE = GetString(reader, 11);
                    po.Bond_Subtype = GetString(reader, 12);
                    po.Option_Date = GetDecimal(reader, 13);
                    po.Option_Type = GetString(reader, 14);
                    po.Rating_Current = GetString(reader, 15);

                    po.Security_Type_Id = GetString(reader, 16);
                    //po.Security_Dec = GetString(reader, 17);
                    po.Full_Name_CN = GetString(reader, 17);
                    po.Full_Name_EN = GetString(reader, 18);
                    po.flag = GetString(reader, 19);
                    po.bondmaster_modify_date = GetString(reader, 20);
                    po.Ent_Cor = GetString(reader, 21);
                    po.Issuer_Rating_Current_NPY = GetString(reader, 22);
                    po.MarketIndicator = GetString(reader, 23);

                    po.SECURITY_DEC = GetString(reader, 24);
                    po.ISSUE_DATE = GetString(reader, 25);

                    po.issuer = GetString(reader, 26);
                    po.Security_Term = GetString(reader, 27);

                    po.CN_Short_Name = GetString(reader, 28);
                    po.Coupon_Rate_Current = reader["Coupon_Rate_Current"].ToString();
                    po.RemaingingTime = (!string.IsNullOrEmpty(po.MarketIndicator) && po.MarketIndicator.Equals("4") ? STCHelper.SetShowTenor(po) : string.Empty);//  STCHelper.SetShowTenor(po);
                    po.Min_change = GetString(reader, 30);
                    po.Issuer_Rating_Current = GetString(reader, 31);
                    po.Issuer_Code = GetString(reader, 32);
                    po.Fund_Objectives_Category = GetString(reader, 33);
                    po.Category_flag = GetString(reader, 34);
                    po.Asset_Status = GetString(reader, 35);
                    po.Issue_Amount = GetDecimal(reader, 36);
                    po.symbolType = GetString(reader, 37);
                    po.bondSigns = GetString(reader, 38);
                    po.cdcAuth = GetString(reader, 39);

                    return po;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return null;
        }

        public DataTable GetAllBonds2()
        {
            DataSet ds = new DataSet();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetAllBondsSql, GetSqliteConnection());
                SQLiteDataReader reader = cmd.ExecuteReader();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);

                adapter.Fill(ds);
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
        }

        public BondsInfoPo GetMaxDate()
        {
            BondsInfoPo po = null;
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetMaxDateSql, GetSqliteConnection());
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    po = new BondsInfoPo();
                    po.bondlistinfo_modify_date = GetString(reader, 0);
                    po.bond_modify_date = GetString(reader, 1);
                    po.bondmaster_modify_date = GetString(reader, 2);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("can't get MaxDate," + ex.Message);
            }

            return po;
        }

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
                SQLiteCommand command = new SQLiteCommand(sql, GetSqliteConnection());
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
        }
    }
}