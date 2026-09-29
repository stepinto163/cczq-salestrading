package ficc.common;

/** Dynamic product-filter field access; business records remain typed. */
public interface FilterValues {
    Object value(String field);
}
