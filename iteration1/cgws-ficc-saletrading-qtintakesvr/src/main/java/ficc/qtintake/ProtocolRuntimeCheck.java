package ficc.qtintake;

/** Resolve the intake protocol before opening FIX, so mixed deployment JARs fail at startup. */
final class ProtocolRuntimeCheck {
    private ProtocolRuntimeCheck() {
    }

    static int verify(ClassLoader loader) {
        String[] contracts = {
            "ficc.api.QuoteCreateItem", "ficc.api.QuoteCreateRequest", "ficc.api.IntakeIdentity",
            "ficc.api.QuoteOperationResult", "ficc.api.QuoteWriteData", "ficc.api.ApiReply",
            "ficc.api.SalesTradingApi"
        };
        for (String name : contracts) {
            try {
                Class<?> contract = Class.forName(name, true, loader);
                contract.getDeclaredFields();
                contract.getDeclaredMethods();
            } catch (ClassNotFoundException | LinkageError error) {
                throw new IllegalStateException("Incompatible QtIntakeSvr runtime: cannot load "
                        + name + "; deploy the service JAR and its matching javalib together", error);
            }
        }
        return contracts.length;
    }
}
