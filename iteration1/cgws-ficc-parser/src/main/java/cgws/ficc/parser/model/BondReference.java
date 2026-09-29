package cgws.ficc.parser.model;

/** Immutable reference data needed by message parsing, independent of the STC transport. */
public final class BondReference {
    public final String code;
    public final String name;
    public final String key;
    public final String bondType;
    public final boolean shortCodeEligible;
    public final boolean invalid;
    public final boolean hasOption;
    public final String valuationYield;

    public BondReference(String code, String name, String key, String bondType,
            boolean shortCodeEligible, boolean invalid, boolean hasOption, String valuationYield) {
        if (code == null || code.trim().isEmpty() || name == null) {
            throw new IllegalArgumentException("Bond code and name are required");
        }
        this.code = code.trim().toLowerCase(java.util.Locale.ROOT);
        this.name = name;
        this.key = key;
        this.bondType = bondType == null ? "BCO" : bondType;
        this.shortCodeEligible = shortCodeEligible;
        this.invalid = invalid;
        this.hasOption = hasOption;
        this.valuationYield = valuationYield == null ? "" : valuationYield;
    }
}
