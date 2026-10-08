package sc.app.stc.parser.utils;

import ats.foundation.utils.db.base.DBPoolParam;
import ats.foundation.utils.db.base.DBUtils;
import db.DbOperator;
import sc.app.stc.utils.CommonDataUtils;

import java.sql.Connection;
import java.sql.SQLException;
import java.util.Iterator;
import java.util.List;
import java.util.Properties;

/**
 * TODO
 *
 * @author fangfang.zhang
 * @date 2021/6/23
 */
public class DbOperatorUtil extends DbOperator {


    public DbOperatorUtil getInstance(){
        return new DbOperatorUtil();
    }
    public static boolean load(){

        try {
//            Connection connection = CommonDataUtils.dbPoolResource.getConnection();
            List list = DBUtils.readConfig(DBUtils._Configpath);
            Iterator i$ = list.iterator();
            while(i$.hasNext()) {
                DBPoolParam po = (DBPoolParam) i$.next();
                Properties ps = new Properties();
                ps.put("driverClassName", po.Driver);
                ps.put("url", po.Url);
                ps.put("username", po.Username);
                ps.put("password", po.Passwd);
                ps.put("maxActive", Integer.toString(po.MaxCount));
                ps.put("minIdle", Integer.toString(po.MinCount));
                ps.put("initialSize", Integer.toString(po.MinCount / 2));
                if (po.CheckSql != null || "".equalsIgnoreCase(po.CheckSql)) {
                    ps.put("validationQuery", po.CheckSql);
                    ps.put("testOnBorrow", "true");
                }

                ps.put("filters", "stat,wall");
                ps.put("removeAbandoned", "true");
                ps.put("removeAbandonedTimeout", Integer.toString(po.DBConnOutTime / 1000));
                ps.put("logAbandoned", "true");
                ps.put("maxWait", "60000");
                ps.put("timeBetweenEvictionRunsMillis", Integer.toString(po.DBConnCheckNumber * 1000));
                ps.put("minEvictableIdleTimeMillis", Integer.toString(po.DBConnCheckNumber * 1000));
                ps.put("numTestsPerEvictionRun", "10");
                if(po.SourceName.equals(Consts.dbpoolDefault)){
                    URL = po.Url +"&user="+po.Username+"&password="+po.Passwd+"&useUnicode=true&characterEncoding=UTF8";

                }
            }
//            Properties properties = connection.getClientInfo();//DBUtils.getDatabaseConnection().getConnection().getClientInfo();
//            URL = properties.getProperty("URL");
            /*Properties properties1 =  DBUtils.getDatabaseConnection().getConnection().getClientInfo();
            URL = properties1.getProperty("URL");*/
        } catch (SQLException e) {
            e.printStackTrace();
            System.out.println("SQLException:"+e.getMessage());
        } catch (Exception e) {
            e.printStackTrace();
        }
        return true;
    }
}
