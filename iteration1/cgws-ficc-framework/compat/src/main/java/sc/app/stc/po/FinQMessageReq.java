// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.po;


public class FinQMessageReq {
    private String content;
    private FinqData data;
    private String fromQB;
    private boolean openChat;
    private String toQQ;

    public String getContent() {
        return this.content;
    }

    public FinqData getData() {
        return this.data;
    }

    public String getFromQB() {
        return this.fromQB;
    }

    public boolean isOpenChat() {
        return this.openChat;
    }

    public String getToQQ() {
        return this.toQQ;
    }

    public void setContent(String content) {
        this.content = content;
    }

    public void setData(FinqData data) {
        this.data = data;
    }

    public void setFromQB(String fromQB) {
        this.fromQB = fromQB;
    }

    public void setOpenChat(boolean openChat) {
        this.openChat = openChat;
    }

    public void setToQQ(String toQQ) {
        this.toQQ = toQQ;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof FinQMessageReq)) {
            return false;
        }
        FinQMessageReq other = (FinQMessageReq) o;
        if (!other.canEqual(this)) {
            return false;
        }
        String this$content = this.getContent();
        String other$content = other.getContent();
        if (this$content == null ? other$content != null : !this$content.equals(other$content)) {
            return false;
        }
        FinqData this$data = this.getData();
        FinqData other$data = other.getData();
        if (this$data == null ? other$data != null : !((Object) this$data).equals(other$data)) {
            return false;
        }
        String this$fromQB = this.getFromQB();
        String other$fromQB = other.getFromQB();
        if (this$fromQB == null ? other$fromQB != null : !this$fromQB.equals(other$fromQB)) {
            return false;
        }
        if (this.isOpenChat() != other.isOpenChat()) {
            return false;
        }
        String this$toQQ = this.getToQQ();
        String other$toQQ = other.getToQQ();
        return !(this$toQQ == null ? other$toQQ != null : !this$toQQ.equals(other$toQQ));
    }

    protected boolean canEqual(Object other) {
        return other instanceof FinQMessageReq;
    }

    public int hashCode() {
        int PRIME = 59;
        int result = 1;
        String $content = this.getContent();
        result = result * 59 + ($content == null ? 43 : $content.hashCode());
        FinqData $data = this.getData();
        result = result * 59 + ($data == null ? 43 : ((Object) $data).hashCode());
        String $fromQB = this.getFromQB();
        result = result * 59 + ($fromQB == null ? 43 : $fromQB.hashCode());
        result = result * 59 + (this.isOpenChat() ? 79 : 97);
        String $toQQ = this.getToQQ();
        result = result * 59 + ($toQQ == null ? 43 : $toQQ.hashCode());
        return result;
    }

    public String toString() {
        return "FinQMessageReq(content="
                + this.getContent()
                + ", data="
                + this.getData()
                + ", fromQB="
                + this.getFromQB()
                + ", openChat="
                + this.isOpenChat()
                + ", toQQ="
                + this.getToQQ()
                + ")";
    }
}
