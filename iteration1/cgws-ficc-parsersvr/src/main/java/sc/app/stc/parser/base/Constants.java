package sc.app.stc.parser.base;


/**
 * 常量类
 *
 * @author lemon
 **/
public class Constants {

    public static int OpLogDBThreadNum = 5;
    public final static String selectProvinceInfoSql = "select * from p_province_info where Area_Name != '' and Area_Name is not null";

}