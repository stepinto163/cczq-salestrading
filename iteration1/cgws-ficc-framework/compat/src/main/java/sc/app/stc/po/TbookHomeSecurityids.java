// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.po;

public class TbookHomeSecurityids {
    public String id;
    public String groupId;
    public String securityid;
    public String symbol;
    public String username;
    public String terminal;
    public String ip;
    public String closeby;
    public String remark;
    public String createtime;
    public String updatetime;
    public String inf1;
    public String inf2;
    public String inf3;
    public String inf4;
    public String inf5;

    public String getId() {
        return this.id;
    }

    public String getGroupId() {
        return this.groupId;
    }

    public String getSecurityid() {
        return this.securityid;
    }

    public String getSymbol() {
        return this.symbol;
    }

    public String getUsername() {
        return this.username;
    }

    public String getTerminal() {
        return this.terminal;
    }

    public String getIp() {
        return this.ip;
    }

    public String getCloseby() {
        return this.closeby;
    }

    public String getRemark() {
        return this.remark;
    }

    public String getCreatetime() {
        return this.createtime;
    }

    public String getUpdatetime() {
        return this.updatetime;
    }

    public String getInf1() {
        return this.inf1;
    }

    public String getInf2() {
        return this.inf2;
    }

    public String getInf3() {
        return this.inf3;
    }

    public String getInf4() {
        return this.inf4;
    }

    public String getInf5() {
        return this.inf5;
    }

    public void setId(String id) {
        this.id = id;
    }

    public void setGroupId(String groupId) {
        this.groupId = groupId;
    }

    public void setSecurityid(String securityid) {
        this.securityid = securityid;
    }

    public void setSymbol(String symbol) {
        this.symbol = symbol;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public void setTerminal(String terminal) {
        this.terminal = terminal;
    }

    public void setIp(String ip) {
        this.ip = ip;
    }

    public void setCloseby(String closeby) {
        this.closeby = closeby;
    }

    public void setRemark(String remark) {
        this.remark = remark;
    }

    public void setCreatetime(String createtime) {
        this.createtime = createtime;
    }

    public void setUpdatetime(String updatetime) {
        this.updatetime = updatetime;
    }

    public void setInf1(String inf1) {
        this.inf1 = inf1;
    }

    public void setInf2(String inf2) {
        this.inf2 = inf2;
    }

    public void setInf3(String inf3) {
        this.inf3 = inf3;
    }

    public void setInf4(String inf4) {
        this.inf4 = inf4;
    }

    public void setInf5(String inf5) {
        this.inf5 = inf5;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof TbookHomeSecurityids)) {
            return false;
        }
        TbookHomeSecurityids other = (TbookHomeSecurityids) o;
        if (!other.canEqual(this)) {
            return false;
        }
        String this$id = this.getId();
        String other$id = other.getId();
        if (this$id == null ? other$id != null : !this$id.equals(other$id)) {
            return false;
        }
        String this$groupId = this.getGroupId();
        String other$groupId = other.getGroupId();
        if (this$groupId == null ? other$groupId != null : !this$groupId.equals(other$groupId)) {
            return false;
        }
        String this$securityid = this.getSecurityid();
        String other$securityid = other.getSecurityid();
        if (this$securityid == null
                ? other$securityid != null
                : !this$securityid.equals(other$securityid)) {
            return false;
        }
        String this$symbol = this.getSymbol();
        String other$symbol = other.getSymbol();
        if (this$symbol == null ? other$symbol != null : !this$symbol.equals(other$symbol)) {
            return false;
        }
        String this$username = this.getUsername();
        String other$username = other.getUsername();
        if (this$username == null
                ? other$username != null
                : !this$username.equals(other$username)) {
            return false;
        }
        String this$terminal = this.getTerminal();
        String other$terminal = other.getTerminal();
        if (this$terminal == null
                ? other$terminal != null
                : !this$terminal.equals(other$terminal)) {
            return false;
        }
        String this$ip = this.getIp();
        String other$ip = other.getIp();
        if (this$ip == null ? other$ip != null : !this$ip.equals(other$ip)) {
            return false;
        }
        String this$closeby = this.getCloseby();
        String other$closeby = other.getCloseby();
        if (this$closeby == null ? other$closeby != null : !this$closeby.equals(other$closeby)) {
            return false;
        }
        String this$remark = this.getRemark();
        String other$remark = other.getRemark();
        if (this$remark == null ? other$remark != null : !this$remark.equals(other$remark)) {
            return false;
        }
        String this$createtime = this.getCreatetime();
        String other$createtime = other.getCreatetime();
        if (this$createtime == null
                ? other$createtime != null
                : !this$createtime.equals(other$createtime)) {
            return false;
        }
        String this$updatetime = this.getUpdatetime();
        String other$updatetime = other.getUpdatetime();
        if (this$updatetime == null
                ? other$updatetime != null
                : !this$updatetime.equals(other$updatetime)) {
            return false;
        }
        String this$inf1 = this.getInf1();
        String other$inf1 = other.getInf1();
        if (this$inf1 == null ? other$inf1 != null : !this$inf1.equals(other$inf1)) {
            return false;
        }
        String this$inf2 = this.getInf2();
        String other$inf2 = other.getInf2();
        if (this$inf2 == null ? other$inf2 != null : !this$inf2.equals(other$inf2)) {
            return false;
        }
        String this$inf3 = this.getInf3();
        String other$inf3 = other.getInf3();
        if (this$inf3 == null ? other$inf3 != null : !this$inf3.equals(other$inf3)) {
            return false;
        }
        String this$inf4 = this.getInf4();
        String other$inf4 = other.getInf4();
        if (this$inf4 == null ? other$inf4 != null : !this$inf4.equals(other$inf4)) {
            return false;
        }
        String this$inf5 = this.getInf5();
        String other$inf5 = other.getInf5();
        return !(this$inf5 == null ? other$inf5 != null : !this$inf5.equals(other$inf5));
    }

    protected boolean canEqual(Object other) {
        return other instanceof TbookHomeSecurityids;
    }

    public int hashCode() {
        int PRIME = 59;
        int result = 1;
        String $id = this.getId();
        result = result * 59 + ($id == null ? 43 : $id.hashCode());
        String $groupId = this.getGroupId();
        result = result * 59 + ($groupId == null ? 43 : $groupId.hashCode());
        String $securityid = this.getSecurityid();
        result = result * 59 + ($securityid == null ? 43 : $securityid.hashCode());
        String $symbol = this.getSymbol();
        result = result * 59 + ($symbol == null ? 43 : $symbol.hashCode());
        String $username = this.getUsername();
        result = result * 59 + ($username == null ? 43 : $username.hashCode());
        String $terminal = this.getTerminal();
        result = result * 59 + ($terminal == null ? 43 : $terminal.hashCode());
        String $ip = this.getIp();
        result = result * 59 + ($ip == null ? 43 : $ip.hashCode());
        String $closeby = this.getCloseby();
        result = result * 59 + ($closeby == null ? 43 : $closeby.hashCode());
        String $remark = this.getRemark();
        result = result * 59 + ($remark == null ? 43 : $remark.hashCode());
        String $createtime = this.getCreatetime();
        result = result * 59 + ($createtime == null ? 43 : $createtime.hashCode());
        String $updatetime = this.getUpdatetime();
        result = result * 59 + ($updatetime == null ? 43 : $updatetime.hashCode());
        String $inf1 = this.getInf1();
        result = result * 59 + ($inf1 == null ? 43 : $inf1.hashCode());
        String $inf2 = this.getInf2();
        result = result * 59 + ($inf2 == null ? 43 : $inf2.hashCode());
        String $inf3 = this.getInf3();
        result = result * 59 + ($inf3 == null ? 43 : $inf3.hashCode());
        String $inf4 = this.getInf4();
        result = result * 59 + ($inf4 == null ? 43 : $inf4.hashCode());
        String $inf5 = this.getInf5();
        result = result * 59 + ($inf5 == null ? 43 : $inf5.hashCode());
        return result;
    }

    public String toString() {
        return "TbookHomeSecurityids(id="
                + this.getId()
                + ", groupId="
                + this.getGroupId()
                + ", securityid="
                + this.getSecurityid()
                + ", symbol="
                + this.getSymbol()
                + ", username="
                + this.getUsername()
                + ", terminal="
                + this.getTerminal()
                + ", ip="
                + this.getIp()
                + ", closeby="
                + this.getCloseby()
                + ", remark="
                + this.getRemark()
                + ", createtime="
                + this.getCreatetime()
                + ", updatetime="
                + this.getUpdatetime()
                + ", inf1="
                + this.getInf1()
                + ", inf2="
                + this.getInf2()
                + ", inf3="
                + this.getInf3()
                + ", inf4="
                + this.getInf4()
                + ", inf5="
                + this.getInf5()
                + ")";
    }
}
