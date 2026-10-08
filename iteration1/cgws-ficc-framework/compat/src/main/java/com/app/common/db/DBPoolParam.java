// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.db;

public class DBPoolParam {
    public String SourceName = "";
    public String Driver = "";
    public String Url = "";
    public String Username = "";
    public String Passwd = "";
    public String CheckSql = "";
    public boolean IsEncrypt = false;
    public int MaxCount = 3;
    public int MinCount = 3;
    public int DBConnOutTime = 10000;
    public int DBConnCheckNumber = 60;
    public boolean DBStrategy = false;
    public boolean isDefault = false;
    public int DBAsyncInsert = 1;
    public int DBAsyncInsertMaxInternet = 1;
    public long maxQuerySize = 0x100000L;

    public boolean checkVaild() {
        return true;
    }

    public String toString() {
        String temp = "";
        temp = temp + "********** Database *********\n";
        temp = temp + "SourceName: " + this.SourceName + "\n";
        temp = temp + "    Driver: " + this.Driver + "\n";
        temp = temp + "       URL: " + this.Url + "\n";
        temp = temp + "  UserName: " + this.Username + "\n";
        temp = temp + "  Password: ******\n";
        temp = temp + "  IsEncrypt: " + this.IsEncrypt + "\n";
        temp = temp + "  MaxCount: " + this.MaxCount + "\n";
        temp = temp + "  MinCount: " + this.MinCount + "\n";
        temp = temp + "  DBConnOutTime: " + this.DBConnOutTime + "\n";
        temp = temp + "  DBConnCheckNumber: " + this.DBConnCheckNumber + "\n";
        temp = temp + "  DBStrategy: " + this.DBStrategy + "\n";
        temp = temp + "***************************\n";
        return temp;
    }

    static enum KEY {
        DBCount,
        LogLevel,
        DefSourceName,
        DBSourceName,
        DBDriver,
        DBUrl,
        DBUsername,
        DBPasswd,
        DBCheckSql,
        DBIsEncrypt,
        DBMaxCount,
        DBMinCount,
        DBConnOutTime,
        MaxQuerySize,
        DBConnCheckNumber,
        DBStrategy,
        DBAsyncInsert,
        DBAsyncInsertMaxInternet;
    }
}
