package sc.app.stc.parser.utils;

import cgws.ficc.parser.model.BondReference;
import cgws.ficc.parser.parse.JavaBondQuoteEngine;
import cgws.ficc.parser.parse.MessageReferenceData;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

/** Uses the already configured STC DB pool; no extra credentials or native database client. */
public final class JdbcParserReferenceLoader implements JavaBondQuoteEngine.ReferenceLoader {
    public interface ConnectionProvider {
        Connection open() throws SQLException;
    }

    static final String BONDS = "SELECT l.bond_id,l.short_name,l.bond_key,l.listed_market,"
            + "l.delisted_date,b.maturity_date,b.bond_type,b.bond_subtype,b.option_type "
            + "FROM bond_list_info l LEFT JOIN bond b ON b.bond_key=l.bond_key";
    static final String VALUATIONS = "SELECT bond_id,listed_market,valuation_yield "
            + "FROM china_bond_valuation ORDER BY valuation_date";
    private final ConnectionProvider connections;
    // The STC schema does not include the legacy parser's optional type dictionary tables.
    // Preserve its documented default BNC classification, separately from short-code eligibility.
    private static final Set<String> BNC_SUBTYPES = new HashSet<String>(Arrays.asList(
            "BGB", "SGB", "LLB", "EGB", "TLB", "SCB", "PBB", "PSB"));

    public JdbcParserReferenceLoader(ConnectionProvider connections) {
        this.connections = connections;
    }

    @Override
    public MessageReferenceData load() throws SQLException {
        String today = LocalDate.now().format(DateTimeFormatter.BASIC_ISO_DATE);
        List<BondReference> records = new ArrayList<BondReference>();
        try (Connection connection = connections.open()) {
            Map<String, String> yields = valuations(connection);
            try (PreparedStatement query = connection.prepareStatement(BONDS)) {
                query.setQueryTimeout(30);
                try (ResultSet rows = query.executeQuery()) {
                    while (rows.next()) {
                        String code = code(rows.getString("bond_id"), rows.getString("listed_market"));
                        String name = rows.getString("short_name");
                        if (code.isEmpty() || name == null) {
                            continue;
                        }
                        String subtype = value(rows.getString("bond_subtype"));
                        boolean shortCode = subtype.equalsIgnoreCase("bgb") || subtype.equalsIgnoreCase("pbb")
                                || subtype.equalsIgnoreCase("psb");
                        boolean invalid = expired(rows.getString("delisted_date"), today)
                                || expired(rows.getString("maturity_date"), today);
                        String type = value(rows.getString("bond_type"));
                        String option = value(rows.getString("option_type"));
                        records.add(new BondReference(code, name, rows.getString("bond_key"),
                                "NCD".equalsIgnoreCase(type) ? "NCD"
                                        : BNC_SUBTYPES.contains(subtype.toUpperCase(Locale.ROOT)) ? "BNC" : "BCO",
                                shortCode, invalid, !option.isEmpty() && !"NON".equalsIgnoreCase(option), yields.get(code)));
                    }
                }
            }
        }
        return new MessageReferenceData(records, "STC-" + today);
    }

    private static Map<String, String> valuations(Connection connection) throws SQLException {
        Map<String, String> values = new HashMap<String, String>();
        try (PreparedStatement query = connection.prepareStatement(VALUATIONS)) {
            query.setQueryTimeout(30);
            try (ResultSet rows = query.executeQuery()) {
                while (rows.next()) {
                    String yield = rows.getString("valuation_yield");
                    if (yield != null && !yield.trim().isEmpty()) {
                        values.put(code(rows.getString("bond_id"), rows.getString("listed_market")), yield.trim());
                    }
                }
            }
        }
        return values;
    }

    static String code(String raw, String market) {
        String code = value(raw).toLowerCase(Locale.ROOT);
        if (code.isEmpty() || code.contains(".")) {
            return code;
        }
        String venue = value(market).toUpperCase(Locale.ROOT);
        String suffix = "CIB".equals(venue) ? ".ib" : "SSE".equals(venue) ? ".sh"
                : "SZE".equals(venue) || "SZSE".equals(venue) ? ".sz" : "BSE".equals(venue) ? ".bj" : "";
        return code + suffix;
    }

    private static boolean expired(String date, String today) {
        String value = value(date).replace("-", "");
        return value.length() == 8 && value.compareTo(today) < 0;
    }

    private static String value(String text) {
        return text == null ? "" : text.trim();
    }
}
