// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.po.dist;

import java.util.ArrayList;
import java.util.List;

public class QMSendParams {
    public boolean no_wait;
    public List<String> qm_ids = new ArrayList<String>();
    public int create_room;
    public String data;
    public String body;
    public String from;

    public boolean isNo_wait() {
        return this.no_wait;
    }

    public List<String> getQm_ids() {
        return this.qm_ids;
    }

    public int getCreate_room() {
        return this.create_room;
    }

    public String getData() {
        return this.data;
    }

    public String getBody() {
        return this.body;
    }

    public String getFrom() {
        return this.from;
    }

    public void setNo_wait(boolean no_wait) {
        this.no_wait = no_wait;
    }

    public void setQm_ids(List<String> qm_ids) {
        this.qm_ids = qm_ids;
    }

    public void setCreate_room(int create_room) {
        this.create_room = create_room;
    }

    public void setData(String data) {
        this.data = data;
    }

    public void setBody(String body) {
        this.body = body;
    }

    public void setFrom(String from) {
        this.from = from;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof QMSendParams)) {
            return false;
        }
        QMSendParams other = (QMSendParams) o;
        if (!other.canEqual(this)) {
            return false;
        }
        if (this.isNo_wait() != other.isNo_wait()) {
            return false;
        }
        List<String> this$qm_ids = this.getQm_ids();
        List<String> other$qm_ids = other.getQm_ids();
        if (this$qm_ids == null
                ? other$qm_ids != null
                : !((Object) this$qm_ids).equals(other$qm_ids)) {
            return false;
        }
        if (this.getCreate_room() != other.getCreate_room()) {
            return false;
        }
        String this$data = this.getData();
        String other$data = other.getData();
        if (this$data == null ? other$data != null : !this$data.equals(other$data)) {
            return false;
        }
        String this$body = this.getBody();
        String other$body = other.getBody();
        if (this$body == null ? other$body != null : !this$body.equals(other$body)) {
            return false;
        }
        String this$from = this.getFrom();
        String other$from = other.getFrom();
        return !(this$from == null ? other$from != null : !this$from.equals(other$from));
    }

    protected boolean canEqual(Object other) {
        return other instanceof QMSendParams;
    }

    public int hashCode() {
        int PRIME = 59;
        int result = 1;
        result = result * 59 + (this.isNo_wait() ? 79 : 97);
        List<String> $qm_ids = this.getQm_ids();
        result = result * 59 + ($qm_ids == null ? 43 : ((Object) $qm_ids).hashCode());
        result = result * 59 + this.getCreate_room();
        String $data = this.getData();
        result = result * 59 + ($data == null ? 43 : $data.hashCode());
        String $body = this.getBody();
        result = result * 59 + ($body == null ? 43 : $body.hashCode());
        String $from = this.getFrom();
        result = result * 59 + ($from == null ? 43 : $from.hashCode());
        return result;
    }

    public String toString() {
        return "QMSendParams(no_wait="
                + this.isNo_wait()
                + ", qm_ids="
                + this.getQm_ids()
                + ", create_room="
                + this.getCreate_room()
                + ", data="
                + this.getData()
                + ", body="
                + this.getBody()
                + ", from="
                + this.getFrom()
                + ")";
    }
}
