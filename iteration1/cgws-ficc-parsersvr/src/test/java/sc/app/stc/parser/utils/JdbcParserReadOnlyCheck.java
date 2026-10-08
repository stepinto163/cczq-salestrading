package sc.app.stc.parser.utils;

import cgws.ficc.parser.parse.MessageReferenceData;
import cgws.ficc.parser.parse.QuoteMessageParser;
import cgws.ficc.parser.model.AllQuote;
import java.io.File;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Proxy;
import java.sql.Connection;
import java.sql.DriverManager;
import java.time.LocalDate;
import javax.xml.parsers.DocumentBuilderFactory;
import org.w3c.dom.Element;

/** Explicit local-only functional check. Limits both loader queries to two bonds; no mutations. */
public final class JdbcParserReadOnlyCheck {
    public static void main(String[] args) throws Exception {
        Element config = (Element) DocumentBuilderFactory.newInstance().newDocumentBuilder()
                .parse(new File(args[0])).getElementsByTagName("Connect").item(0);
        String server = config.getAttribute("server");
        if (!(server.startsWith("127.0.0.1:") || server.startsWith("localhost:"))) {
            throw new IllegalArgumentException("This check only accepts a local PostgreSQL server");
        }
        String url = "jdbc:postgresql://" + server + "/" + config.getAttribute("db")
                + "?connectTimeout=3&socketTimeout=10";
        JdbcParserReferenceLoader loader = new JdbcParserReferenceLoader(() -> {
            Connection connection = DriverManager.getConnection(url, config.getAttribute("user"), config.getAttribute("pass"));
            connection.setReadOnly(true);
            return (Connection) Proxy.newProxyInstance(Connection.class.getClassLoader(), new Class<?>[]{Connection.class},
                    (proxy, method, arguments) -> {
                        if ("prepareStatement".equals(method.getName()) && arguments[0] instanceof String) {
                            String sql = (String) arguments[0];
                            if (sql.equals(JdbcParserReferenceLoader.BONDS)) {
                                arguments[0] = sql + " WHERE l.bond_id IN ('240210','240210.IB','240215','240215.IB')";
                            } else if (sql.equals(JdbcParserReferenceLoader.VALUATIONS)) {
                                arguments[0] = sql.replace(" ORDER BY", " WHERE bond_id IN ('240210','240210.IB','240215','240215.IB') ORDER BY");
                            } else {
                                throw new IllegalStateException("Unexpected query in limited reference check");
                            }
                        }
                        try {
                            return method.invoke(connection, arguments);
                        } catch (InvocationTargetException error) {
                            throw error.getCause();
                        }
                    });
        });
        MessageReferenceData reference = loader.load();
        AllQuote result = new QuoteMessageParser(reference).parse("240210 2.5/2.4 1000;240215 2.3 2000 bid", LocalDate.now(), true);
        if (result.lstQuote.size() != 3 || !"2.4".equals(result.lstQuote.get(1).Price)) {
            throw new AssertionError("Unexpected parsing result from local STC reference data: " + result.lstQuote);
        }
        System.out.println("PASS local PostgreSQL reference loading and message parsing; quotes=3");
    }
}
