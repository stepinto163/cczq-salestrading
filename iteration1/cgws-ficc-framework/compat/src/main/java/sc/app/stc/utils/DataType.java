// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

public enum DataType {
    Bond("1"),
    BondInfo("2"),
    InstAuth("3"),
    HolidayQuery("4"),
    ChianondInfo("5"),
    Partyinfo("6"),
    Partydealerinfo("7"),
    VenueSetting("8"),
    Load("9");

    private String key;

    private DataType(String key) {
        this.key = key;
    }

    public static DataType parse(String str) {
        DataType orderType = null;
        for (DataType s : DataType.values()) {
            if (!s.key.equals(str)) continue;
            orderType = s;
        }
        if (orderType == null) {
            throw new IllegalArgumentException("DataType enum parse failed:" + str);
        }
        return orderType;
    }

    public String toString() {
        return String.valueOf(this.key);
    }
}
