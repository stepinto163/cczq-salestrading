// Recovered compatibility source; provenance: recovered-support.json.
package ats.app.ym.utils.db;

public class UpdateListResult {
    private final int[] results;
    private int cur = 0;

    public UpdateListResult(int total) {
        this.results = new int[total];
        this.cur = 0;
    }

    public void add(int[] sub) {
        System.arraycopy(sub, 0, this.results, this.cur, sub.length);
        this.cur += sub.length;
    }

    public int[] getResults() {
        return this.results;
    }
}
