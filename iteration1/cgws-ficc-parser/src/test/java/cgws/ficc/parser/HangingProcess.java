package cgws.ficc.parser;

/**
 * Test only: a child JVM that never finishes on its own, used to prove that the process helper
 * kills a hung child at its deadline instead of blocking forever. It sleeps, it does no work.
 */
public final class HangingProcess {

    public static void main(String[] args) throws Exception {
        System.out.println("HANGING_PROCESS_STARTED");
        System.out.flush();
        Thread.sleep(120000L);
    }
}
