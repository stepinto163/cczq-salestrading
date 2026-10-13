package ficc.qtintake;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class ProtocolRuntimeCheckTest {
    @Test
    void acceptsMatchingRuntime() {
        assertEquals(9, ProtocolRuntimeCheck.verify(getClass().getClassLoader()));
    }

    @Test
    void missingQuoteItemFailsBeforeReceivingQuotesAndPreservesCause() {
        ClassLoader oldRuntime = new ClassLoader(getClass().getClassLoader()) {
            @Override
            public Class<?> loadClass(String name) throws ClassNotFoundException {
                if ("ficc.api.QuoteCreateItem".equals(name)) {
                    throw new ClassNotFoundException(name);
                }
                return super.loadClass(name);
            }
        };
        IllegalStateException error = assertThrows(IllegalStateException.class,
                () -> ProtocolRuntimeCheck.verify(oldRuntime));
        assertTrue(error.getMessage().contains("matching javalib"));
        assertInstanceOf(ClassNotFoundException.class, error.getCause());
    }
}
