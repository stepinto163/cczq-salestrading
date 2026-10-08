// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.config;

import ats.foundation.utils.config.mqserver.ClientParams;
import ats.foundation.utils.config.mqserver.MQServer;
import ats.foundation.utils.config.mqserver.RegisterParams;
import ats.foundation.utils.config.mqserver.ServerParams;
import ats.foundation.utils.util.HostTools;

import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

public class PropertiesHelper extends Properties {
    private static final long serialVersionUID = 6951608021703880014L;

    public static void main(String[] args) throws IOException {
        System.out.println(PropertiesHelper.getFullPath("", "./control/DDD.ini"));
        PropertiesHelper properties = new PropertiesHelper();
        try {
            properties.load("./control/DBPoolConfig.ini");
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }
        System.out.println("------------------------------------------");
        DBParam dbParam = properties.findDBParam("JDBC.ATSDB1");
        System.out.println(PropertiesHelper.printParmas(dbParam));
        System.out.println("------------------------------------------");
        dbParam = properties.findDBParam("SERVICENAME.PBOX");
        System.out.println(PropertiesHelper.printParmas(dbParam));
        System.out.println("------------------------------------------");
        System.out.println(
                PropertiesHelper.printParmas(
                        properties.findServerParam("SERVER.AMQIIIServerAdapter")));
        System.out.println(PropertiesHelper.printParmas(properties.findClientParam("CLIENT.xxx")));
        System.out.println(
                PropertiesHelper.printParmas(properties.findClientParam("REGISTER.xxx1")));
        System.out.println(PropertiesHelper.printParmas(properties.findRegisterParams()));
    }

    public static String printParmas(DBParam params) {
        StringBuffer buff = new StringBuffer("DBParam={");
        if (params.Driver.length() <= 0) {
            buff.append("SERVICENAME:").append(params.Driver).append(",");
        } else {
            buff.append("JDBC:").append(params.Driver).append(",");
        }
        buff.append("Url:").append(params.Url).append(",");
        buff.append("UserName:").append(params.UserName).append(",");
        buff.append("Passwd:").append(params.Passwd).append("}");
        return buff.toString();
    }

    public static String printParmas(RegisterParams params) {
        return PropertiesHelper.printParmas((ServerParams) params);
    }

    public static String printParmas(List<RegisterParams> params) {
        StringBuffer buff = new StringBuffer("RegisterList={");
        for (RegisterParams reg : params) {
            buff.append(PropertiesHelper.printParmas((ServerParams) reg));
        }
        buff.append("}");
        return buff.toString();
    }

    public static String printParmas(ClientParams params) {
        return PropertiesHelper.printParmas((ServerParams) params);
    }

    public static String printParmas(ServerParams params) {
        StringBuffer buff = new StringBuffer("ServerParams={");
        buff.append(params.Name).append("=");
        buff.append(params.Host).append(":");
        buff.append(params.Port).append(",");
        buff.append(params.RegType).append(",");
        buff.append(params.LBFactor).append(",");
        buff.append(params.RegisterEnable).append(",");
        buff.append(params.RegisterServerList).append("");
        if (params.RegisterEnable.booleanValue()) {
            buff.append("[");
            for (RegisterParams tmpReg : params.RegisterConfigs) {
                buff.append(tmpReg.Name).append("=");
                buff.append(tmpReg.Host).append(":");
                buff.append(tmpReg.Port).append(";");
            }
            buff.append("]");
        }
        buff.append("}");
        return buff.toString();
    }

    public static String getATS_ROOT() {
        String value = null;
        try {
            value = System.getenv("ATS_ROOT");
        } catch (Exception e) {
            e.printStackTrace();
        }
        return value == null ? "" : value;
    }

    public void load(String path) throws IOException {
        this.load(new FileInputStream(path));
    }

    @Override
    public void load(InputStream inStream) throws IOException {
        Properties temp = new Properties();
        temp.load(inStream);
        for (Object key : temp.keySet()) {
            if (key.toString().toLowerCase().startsWith("@include")) {
                String value = temp.get(key).toString();
                if (value == null || value.trim().compareTo("") == 0) continue;
                value = PropertiesHelper.getFullPath(PropertiesHelper.getATS_ROOT(), value);
                super.load(new FileInputStream(value));
                continue;
            }
            this.put(key, temp.get(key));
        }
    }

    public static String getRealPath(String text) {
        StringBuffer buf = new StringBuffer(text);
        int startIndex = buf.indexOf("${");
        while (startIndex != -1) {
            int endIndex = buf.indexOf("}", startIndex + "${".length());
            if (endIndex != -1) {
                String placeholder = buf.substring(startIndex + "${".length(), endIndex);
                int nextIndex = endIndex + "}".length();
                try {
                    String propVal = System.getProperty(placeholder);
                    if (propVal == null) {
                        propVal = System.getenv(placeholder);
                    }
                    if (propVal != null) {
                        buf.replace(startIndex, endIndex + "}".length(), propVal);
                        nextIndex = startIndex + propVal.length();
                    } else {
                        System.err.println(
                                "Could not resolve placeholder '"
                                        + placeholder
                                        + "' in ["
                                        + text
                                        + "] as system property: neither system property nor"
                                        + " environment variable found");
                    }
                } catch (Throwable ex) {
                    System.err.println(
                            "Could not resolve placeholder '"
                                    + placeholder
                                    + "' in ["
                                    + text
                                    + "] as system property: "
                                    + ex);
                }
                startIndex = buf.indexOf("${", nextIndex);
                continue;
            }
            startIndex = -1;
        }
        return buf.toString();
    }

    public static String getFullPath(String root, String config) {
        if (root.length() > 0) {
            if (root.endsWith("/")) {
                root = root.substring(0, root.length() - 1);
            }
            if (config.startsWith(".")) {
                config = config.substring(1);
            }
            return root + config;
        }
        if (config.startsWith("./")) {
            return config;
        }
        if (config.startsWith("/")) {
            return "." + config;
        }
        return "./" + config;
    }

    public static String getEnvPath(String root) {
        String value = null;
        try {
            value = System.getenv(root);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return value == null ? "" : value;
    }

    public static String getFullPath(String config) {
        String retval = config;
        if (config.startsWith("$")) {
            String root = config.substring(1, config.indexOf("/"));
            String temp = config.substring(config.indexOf("/"));
            retval = PropertiesHelper.getFullPath(PropertiesHelper.getEnvPath(root), temp);
        }
        return retval;
    }

    @Override
    public void loadFromXML(InputStream inStream) throws IOException {
        Properties temp = new Properties();
        temp.loadFromXML(inStream);
        for (Object key : temp.keySet()) {
            if (key.toString().toLowerCase().startsWith("@include")) {
                String value = temp.get(key).toString();
                if (value == null || value.trim().compareTo("") == 0) continue;
                value = PropertiesHelper.getFullPath(PropertiesHelper.getATS_ROOT(), value);
                super.loadFromXML(new FileInputStream(value.toString()));
                continue;
            }
            this.put(key, temp.get(key));
        }
    }

    public DBParam findDBParam(String name) throws IOException {
        DBParam dbParam = new DBParam();
        String[] nameSplit = (name + "").split("\\.");
        if (nameSplit.length >= 1) {
            if (nameSplit[0].equalsIgnoreCase(DBParam.TYPE.JDBC.toString())) {
                dbParam.Driver = this.getProperty((name + ".Driver").trim());
                dbParam.Url = this.getProperty((name + ".Url").trim());
                dbParam.UserName = this.getProperty((name + ".UserName").trim());
                dbParam.Passwd = this.getProperty((name + ".Passwd").trim());
                return dbParam;
            }
            if (nameSplit[0].equalsIgnoreCase(DBParam.TYPE.ServiceName.toString())) {
                dbParam.Url = this.getProperty((name + ".Url").trim());
                dbParam.UserName = this.getProperty((name + ".UserName").trim());
                dbParam.Passwd = this.getProperty((name + ".Passwd").trim());
                return dbParam;
            }
        }
        return null;
    }

    public List<RegisterParams> findRegisterParams() throws IOException {
        String temp = this.getProperty("REGISTER.ServerList".trim()) + "";
        String[] array = temp.split(",");
        ArrayList<RegisterParams> registerConfigs = new ArrayList<RegisterParams>();
        for (String str : array) {
            RegisterParams tmpParams;
            if (str == null
                    || str.length() <= 0
                    || (tmpParams = this.findRegisterParam(str)) == null) continue;
            registerConfigs.add(tmpParams);
        }
        return registerConfigs;
    }

    public RegisterParams findRegisterParam(String name) throws IOException {
        ServerParams params = this.findServerParam(name);
        return params;
    }

    public ClientParams findClientParam(String name) throws IOException {
        ServerParams params = this.findServerParam(name);
        return params;
    }

    private boolean isYes(String str) {
        if (str == null) {
            return false;
        }
        return "1".equals(str = str.trim())
                || "y".equalsIgnoreCase(str)
                || "yes".equalsIgnoreCase(str)
                || "true".equalsIgnoreCase(str);
    }

    public ServerParams findServerParam(String name) throws IOException {
        ServerParams dbParam = new ServerParams();
        String[] nameSplit = (name + "").split("\\.");
        if (nameSplit[0].equalsIgnoreCase(MQServer.TYPE.Regiser.toString())) {
            this.setHostInfo(name, dbParam);
        }
        if (nameSplit[0].equalsIgnoreCase(MQServer.TYPE.Server.toString())
                || nameSplit[0].equalsIgnoreCase(MQServer.TYPE.Client.toString())) {
            this.setHostInfo(name, dbParam);
            dbParam.ServiceName = this.getProperty((name + ".ServiceName").trim());
            String temp = this.getProperty((name + ".RegisterEnable").trim()) + "";
            dbParam.RegisterEnable = this.isYes(temp);
            dbParam.RegisterServerList = this.getProperty(name + ".RegisterServerList");
            this.setInfoAboutRegistry(
                    name, dbParam, nameSplit[0].equalsIgnoreCase(MQServer.TYPE.Server.toString()));
        }
        return dbParam;
    }

    private void setInfoAboutRegistry(String name, ServerParams dbParam, boolean isSvr)
            throws IOException {
        String[] array;
        if (!dbParam.RegisterEnable.booleanValue()) {
            return;
        }
        if (dbParam.ServiceName == null || dbParam.ServiceName.length() == 0) {
            throw new IOException("RegisterEnable is set. But no ServiceName - " + name);
        }
        if (dbParam.RegisterServerList == null || dbParam.RegisterServerList.length() == 0) {
            throw new IOException("RegisterEnable is set. But no RegisterServerList - " + name);
        }
        for (String str : array = dbParam.RegisterServerList.split(",")) {
            RegisterParams tmpParams;
            if (str == null
                    || str.length() <= 0
                    || (tmpParams = this.findRegisterParam(str)) == null) continue;
            dbParam.RegisterConfigs.add(tmpParams);
        }
        if (isSvr) {
            try {
                String temp = this.getProperty((name + ".RegType").trim()) + "";
                dbParam.RegType = Integer.valueOf(temp);
                temp = this.getProperty((name + ".LBFactor").trim()) + "";
                dbParam.LBFactor = Integer.valueOf(temp);
            } catch (NumberFormatException e) {
                throw new IOException(
                        "RegType or LBFactor is invalid when RegisterEnable is set. - " + name, e);
            }
        }
    }

    private void setHostInfo(String name, ServerParams params) throws IOException {
        params.Name = this.getProperty((name + ".Name").trim());
        String hostStr = this.getProperty((name + ".Host").trim());
        String hostMode = this.getProperty((name + ".HostMode").trim());
        if (hostStr == null) {
            throw new IOException(name + ".Host is empty, MUST set it.");
        }
        try {
            if (hostMode == null || (hostMode = hostMode.trim()).length() == 0) {
                this.noHostMode(hostStr, params);
            } else {
                this.hostMode(hostMode, hostStr, params);
            }
        } catch (NumberFormatException e) {
            throw new IOException(
                    name
                            + ".Host format is invalid, pls check the port. - Host="
                            + hostStr
                            + ",HostMode="
                            + hostMode);
        } catch (HostTools.InvalidHostException e) {
            throw new IOException(e);
        }
    }

    private void noHostMode(String hostStr, ServerParams params) {
        if (hostStr.indexOf(58) > 0) {
            String[] tempArray = hostStr.split(":");
            params.Host = tempArray[0];
            params.Port = Integer.valueOf(tempArray[1]);
        } else {
            params.Host = hostStr;
            params.Port = 80;
        }
    }

    private void hostMode(String hostMode, String hostStr, ServerParams params)
            throws HostTools.InvalidHostException {
        params.Port =
                hostStr.indexOf(58) > 0
                        ? Integer.valueOf(hostStr.split(":")[1]).intValue()
                        : Integer.valueOf(hostStr).intValue();
        String[] m = hostMode.split(":");
        String mode = m[0];
        if ("host".equalsIgnoreCase(mode)) {
            params.Host = HostTools.getHostName();
        } else if ("ip".equalsIgnoreCase(mode)) {
            this.setIp(m, params);
        } else if ("hostmap".equalsIgnoreCase(mode)) {
            this.setHostMap(m, params);
        } else {
            throw new HostTools.InvalidHostException("Unknown host mode : " + mode);
        }
    }

    private void setIp(String[] modes, ServerParams params) throws HostTools.InvalidHostException {
        if (modes.length == 1) {
            Object[] ips = HostTools.getAllIP();
            if (ips.length == 0) {
                throw new HostTools.InvalidHostException("Can't get ip. Maybe no ip is set.");
            }
            if (ips.length != 1) {
                this.moreThanOneException(ips, params.Name);
            }
            params.Host = (String) ips[0];
        } else {
            String filter = modes[1];
            params.Host = HostTools.getIP(filter);
        }
    }

    private void setHostMap(String[] modes, ServerParams params)
            throws HostTools.InvalidHostException {
        String name = null;
        String filter = null;
        if (modes.length == 1) {
            name = HostTools.getHostName();
        }
        if (modes.length >= 2 && (name = modes[1].trim()).charAt(0) == '$') {
            name = HostTools.getHostName();
        }
        if (modes.length >= 3) {
            filter = modes[2].trim();
        }
        Object[] adds = HostTools.getAddress(name);
        if (filter == null) {
            if (adds.length != 1) {
                this.moreThanOneException(adds, name);
            } else {
                params.Host = (String) adds[0];
            }
        } else {
            ArrayList<Object> list = new ArrayList<Object>();
            for (Object ip : adds) {
                if (!((String) ip).startsWith(filter)) continue;
                list.add(ip);
            }
            if (list.size() == 1) {
                params.Host = (String) list.get(0);
            } else {
                if (list.size() == 0) {
                    throw new HostTools.InvalidHostException(
                            "Can't get the address for name=" + name + ",filter=" + filter);
                }
                this.moreThanOneException(list.toArray(new String[list.size()]), name);
            }
        }
    }

    private void moreThanOneException(Object[] adds, String name)
            throws HostTools.InvalidHostException {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < adds.length; ++i) {
            sb.append(adds[i]).append(",");
        }
        throw new HostTools.InvalidHostException(
                "More than one address in hosts about '" + name + "' : " + sb);
    }
}
