package cgws.ficc.parser.dict;

/** One dictionary record coming from the loader (bond_list_info + bond join today). */
public final class DictionaryRow {

    public final String code;
    public final String name;
    public final String key;
    public final boolean invalid;

    public DictionaryRow(String code, String name, String key, boolean invalid) {
        this.code = code;
        this.name = name;
        this.key = key;
        this.invalid = invalid;
    }

    @Override
    public String toString() {
        return code + ":" + name + (invalid ? " [INVALID]" : "");
    }
}
