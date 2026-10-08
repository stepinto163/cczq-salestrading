// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.db.base;

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
        DBCount {

            public String toString() {
                return "DBPOOL.DBCount";
            }
        },
        LogLevel {

            public String toString() {
                return "DBPOOL.LogLevel";
            }
        },
        DefSourceName {

            public String toString() {
                return "DBPOOL.DefSourceName";
            }
        },
        DBSourceName {

            public String toString() {
                return "DBPOOL.DBSourceName_%s";
            }
        },
        DBDriver {

            public String toString() {
                return "DBPOOL.DBDriver_%s";
            }
        },
        DBUrl {

            public String toString() {
                return "DBPOOL.DBUrl_%s";
            }
        },
        DBUsername {

            public String toString() {
                return "DBPOOL.DBUsername_%s";
            }
        },
        DBPasswd {

            public String toString() {
                return "DBPOOL.DBPasswd_%s";
            }
        },
        DBCheckSql {

            public String toString() {
                return "DBPOOL.DBCheckSql_%s";
            }
        },
        DBIsEncrypt {

            public String toString() {
                return "DBPOOL.DBIsEncrypt_%s";
            }
        },
        DBMaxCount {

            public String toString() {
                return "DBPOOL.DBMaxCount_%s";
            }
        },
        DBMinCount {

            public String toString() {
                return "DBPOOL.DBMinCount_%s";
            }
        },
        DBConnOutTime {

            public String toString() {
                return "DBPOOL.DBConnOutTime_%s";
            }
        },
        DBConnCheckNumber {

            public String toString() {
                return "DBPOOL.DBConnCheckNumber_%s";
            }
        },
        DBStrategy {

            public String toString() {
                return "DBPOOL.DBStrategy_%s";
            }
        };
    }
}
