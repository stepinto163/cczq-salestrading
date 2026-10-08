// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.util;

import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.MalformedURLException;
import java.net.NetworkInterface;
import java.net.SocketException;
import java.net.URL;
import java.net.UnknownHostException;
import java.util.ArrayList;
import java.util.Enumeration;

public class HostTools {
    public static String[][] getHost(boolean flag) {
        String[][] as;
        block34:
        {
            as = null;
            String s = null;
            String s1 = null;
            try {
                ClassLoader classloader;
                InetAddress[] ainetaddress = null;
                URL url = null;
                Class<HostTools> class1 = HostTools.class;
                String s2 = class1.getName().replace('.', '/') + ".class";
                if (flag) {
                    System.out.println("*** DEBUG *** Class file name [" + s2 + "].");
                }
                if ((classloader = class1.getClassLoader()) == null) {
                    if (flag) {
                        System.out.println("*** DEBUG *** Loaded via bootstrap class loader.");
                    }
                    if ((url = class1.getResource("/" + s2)) == null) {
                        if (flag) {
                            System.out.println(
                                    "*** DEBUG *** Class file URL null when using leading / .");
                        }
                        url = class1.getResource(s2);
                    }
                } else {
                    url = classloader.getResource("/" + s2);
                    if (url == null) {
                        if (flag) {
                            System.out.println(
                                    "*** DEBUG *** Class file URL null when using leading / .");
                        }
                        url = classloader.getResource(s2);
                    }
                }
                if (flag) {
                    System.out.println("*** DEBUG *** Class file URL [" + url + "].");
                }
                if (url == null) {
                    System.out.println("URL is null ");
                    return null;
                }
                if (url.getProtocol().equals("jar")) {
                    if (flag) {
                        System.out.println("*** DEBUG *** Class file URL specified jar protocol..");
                    }
                    url = new URL(url.toString().substring(4));
                }
                boolean flag1 = false;
                if (classloader != null
                        && classloader
                                .getClass()
                                .getName()
                                .equals("com.ms.vm.loader.DLLClassLoader")) {
                    if (flag) {
                        System.out.println("*** DEBUG *** Loaded via MS COM DLL.");
                    }
                    flag1 = true;
                }
                if (url.getHost().length() > 0 && !flag1) {
                    if (flag) {
                        System.out.println("*** DEBUG *** Running as an Applet or via COM.");
                    }
                    s = s1 = url.getHost();
                    if (flag) {
                        System.out.println("*** DEBUG *** Fallback Addr set to[" + s1 + "].");
                        System.out.println("*** DEBUG *** Fallback Name set to[" + s + "].");
                    }
                    ainetaddress = InetAddress.getAllByName(s);
                } else {
                    if (flag) {
                        System.out.println("*** DEBUG *** Running as an Application.");
                    }
                    s1 = InetAddress.getLocalHost().getHostAddress();
                    s = InetAddress.getLocalHost().getHostName();
                    if (flag) {
                        System.out.println("*** DEBUG *** Fallback Addr set to[" + s1 + "].");
                        System.out.println("*** DEBUG *** Fallback Name set to[" + s + "].");
                    }
                    ainetaddress = InetAddress.getAllByName(s1);
                }
                int i = ainetaddress.length;
                if (flag) {
                    System.out.println("*** DEBUG *** [" + i + "] addresses found.");
                }
                as = new String[i][];
                for (int j = 0; j < i; ++j) {
                    InetAddress inetaddress;
                    String s4;
                    as[j] = new String[2];
                    String s3 = ainetaddress[j].getHostAddress();
                    if ((s3 == null || s3.length() == 0) && flag) {
                        System.out.println("*** DEBUG *** Original host address empty or null.");
                    }
                    if (((s4 = (inetaddress = InetAddress.getByName(s3)).getHostName()) == null
                                    || s4.length() == 0)
                            && flag) {
                        System.out.println("*** DEBUG *** Host name empty or null.");
                    }
                    as[j][0] = s4;
                    String s5 = inetaddress.getHostAddress();
                    if ((s5 == null || s5.length() == 0) && flag) {
                        System.out.println("*** DEBUG *** Host address empty or null.");
                    }
                    as[j][1] = s5;
                }
            } catch (SecurityException securityexception) {
                if (s1 != null && s != null) {
                    if (flag) {
                        System.out.println(
                                "*** DEBUG *** Security Exception - using fallback details.");
                    }
                    as = new String[1][2];
                    as[0][0] = s;
                    as[0][1] = s1;
                }
            } catch (MalformedURLException malformedurlexception) {
                if (s1 != null && s != null) {
                    if (flag) {
                        System.out.println(
                                "*** DEBUG *** Unknown Host Exception - using fallback details.");
                    }
                    as = new String[1][2];
                    as[0][0] = s;
                    as[0][1] = s1;
                }
            } catch (UnknownHostException unknownhostexception) {
                if (s1 == null || s == null) break block34;
                if (flag) {
                    System.out.println(
                            "*** DEBUG *** Unknown Host Exception - using fallback details.");
                }
                as = new String[1][2];
                as[0][0] = s;
                as[0][1] = s1;
            }
        }
        return as;
    }

    public static String[] getAddress(String name) throws InvalidHostException {
        if (name == null || (name = name.trim()).length() == 0) {
            name = HostTools.getHostName();
        }
        InetAddress[] addressArr = null;
        try {
            addressArr = InetAddress.getAllByName(name);
        } catch (UnknownHostException e) {
            throw new InvalidHostException(e);
        }
        if (addressArr == null || addressArr.length == 0) {
            throw new InvalidHostException("Can't get the ip for " + name);
        }
        String[] r = new String[addressArr.length];
        for (int i = 0; i < addressArr.length; ++i) {
            r[i] = addressArr[i].getHostAddress();
        }
        return r;
    }

    public static String getHostName() throws InvalidHostException {
        InetAddress a = null;
        try {
            a = InetAddress.getLocalHost();
        } catch (UnknownHostException e) {
            throw new InvalidHostException(e);
        }
        String hostName = a.getHostName();
        if (hostName.indexOf(32) > 0) {
            throw new InvalidHostException("Contains space character. - " + hostName);
        }
        return hostName;
    }

    public static String getIP(String filter) throws InvalidHostException {
        String[] ips = HostTools.getAllIP();
        if (ips.length == 0) {
            throw new InvalidHostException("Can't get ip. Maybe no ip is set.");
        }
        ArrayList<String> al = new ArrayList<String>();
        for (String ip : ips) {
            if (!ip.startsWith(filter)) continue;
            al.add(ip);
        }
        if (al.size() == 0) {
            throw new InvalidHostException("Can't get the ip for " + filter);
        }
        if (al.size() == 1) {
            return (String) al.get(0);
        }
        throw new InvalidHostException("More than one ip for " + filter);
    }

    public static String[] getAllIP() throws InvalidHostException {
        Enumeration<NetworkInterface> allNetInterfaces = null;
        try {
            allNetInterfaces = NetworkInterface.getNetworkInterfaces();
        } catch (SocketException e) {
            throw new InvalidHostException(e);
        }
        ArrayList<String> all = new ArrayList<String>();
        while (allNetInterfaces.hasMoreElements()) {
            NetworkInterface netInterface = allNetInterfaces.nextElement();
            Enumeration<InetAddress> addresses = netInterface.getInetAddresses();
            while (addresses.hasMoreElements()) {
                String ipStr;
                InetAddress ip = addresses.nextElement();
                if (ip == null
                        || !(ip instanceof Inet4Address)
                        || (ipStr = ip.getHostAddress()).equals("127.0.0.1")) continue;
                all.add(ipStr);
            }
        }
        return all.toArray(new String[all.size()]);
    }

    public static class InvalidHostException extends Exception {
        public InvalidHostException(Exception e) {
            super(e);
        }

        public InvalidHostException(String str) {
            super(str);
        }
    }
}
