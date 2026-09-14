package ficc.common.client;

/** Identity boundary: simulation today, STC LoginSvrClient integration later. */
public interface LoginSessionClient {
    String loginService() throws Exception;

    String resolveUser(String sessionId) throws Exception;
}
