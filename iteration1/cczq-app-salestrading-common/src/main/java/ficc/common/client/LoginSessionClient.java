package ficc.common.client;

/** Identity boundary for local simulation and real STC sessions. */
public interface LoginSessionClient {
    String loginService() throws Exception;

    String resolveUser(String sessionId) throws Exception;

    default void onInvalidated(java.util.function.Consumer<String> listener) {}
}
