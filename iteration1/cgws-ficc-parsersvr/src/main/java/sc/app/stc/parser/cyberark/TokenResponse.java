package sc.app.stc.parser.cyberark;

public class TokenResponse {

    private String ret;
    private String msg;
    private AccessTokenResponse data;

    public String getRet() {
        return ret;
    }

    public void setRet(String ret) {
        this.ret = ret;
    }

    public String getMsg() {
        return msg;
    }

    public void setMsg(String msg) {
        this.msg = msg;
    }

    public AccessTokenResponse getData() {
        return data;
    }

    public void setData(AccessTokenResponse data) {
        this.data = data;
    }
}
