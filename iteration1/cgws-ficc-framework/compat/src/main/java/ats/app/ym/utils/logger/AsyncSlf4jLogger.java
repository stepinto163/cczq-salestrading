// Recovered compatibility source; provenance: recovered-support.json.
package ats.app.ym.utils.logger;

import org.slf4j.Logger;
import org.slf4j.helpers.MarkerIgnoringBase;

public class AsyncSlf4jLogger extends MarkerIgnoringBase {
    protected static final Object[] emptyObjectArray = new Object[0];
    protected Logger slf4jLogger;

    public AsyncSlf4jLogger(Logger slf4jLogger) {
        this.slf4jLogger = slf4jLogger;
    }

    public Logger getSlf4jLogger() {
        return this.slf4jLogger;
    }

    public void trace(String msg) {
        this.trace(msg, emptyObjectArray);
    }

    public void trace(String format, Object arg) {
        this.trace(format, new Object[] {arg});
    }

    public void trace(String format, Object arg1, Object arg2) {
        this.trace(format, new Object[] {arg1, arg2});
    }

    public void trace(String format, Object... arguments) {
        this.log(format, 10000, null, arguments);
    }

    public void trace(String msg, Throwable t) {
        this.log(msg, 10000, t, null);
    }

    public void debug(String msg) {
        this.debug(msg, emptyObjectArray);
    }

    public void debug(String format, Object arg) {
        this.debug(format, new Object[] {arg});
    }

    public void debug(String format, Object arg1, Object arg2) {
        this.debug(format, new Object[] {arg1, arg2});
    }

    public void debug(String format, Object... arguments) {
        this.log(format, 10000, null, arguments);
    }

    public void debug(String msg, Throwable t) {
        this.log(msg, 10000, t, null);
    }

    public void info(String msg) {
        this.info(msg, emptyObjectArray);
    }

    public void info(String format, Object arg) {
        this.info(format, new Object[] {arg});
    }

    public void info(String format, Object arg1, Object arg2) {
        this.info(format, new Object[] {arg1, arg2});
    }

    public void info(String format, Object... arguments) {
        this.log(format, 20000, null, arguments);
    }

    public void info(String msg, Throwable t) {
        this.log(msg, 20000, t, null);
    }

    public void warn(String msg) {
        this.warn(msg, emptyObjectArray);
    }

    public void warn(String format, Object arg) {
        this.warn(format, new Object[] {arg});
    }

    public void warn(String format, Object arg1, Object arg2) {
        this.warn(format, new Object[] {arg1, arg2});
    }

    public void warn(String format, Object... arguments) {
        this.log(format, 30000, null, arguments);
    }

    public void warn(String msg, Throwable t) {
        this.log(msg, 30000, t, null);
    }

    public void error(String msg) {
        this.error(msg, emptyObjectArray);
    }

    public void error(String format, Object arg) {
        this.error(format, new Object[] {arg});
    }

    public void error(String format, Object arg1, Object arg2) {
        this.error(format, new Object[] {arg1, arg2});
    }

    public void error(String format, Object... arguments) {
        this.log(format, 40000, null, arguments);
    }

    public void error(String msg, Throwable t) {
        this.log(msg, 40000, t, null);
    }

    private void log(String format, int logLevel, Throwable t, Object[] arguments) {
        boolean writeLog = false;
        switch (logLevel) {
            case 10000:
                {
                    writeLog = this.isDebugEnabled();
                    break;
                }
            case 20000:
                {
                    writeLog = this.isInfoEnabled();
                    break;
                }
            case 30000:
                {
                    writeLog = this.isWarnEnabled();
                    break;
                }
            case 40000:
                {
                    writeLog = this.isErrorEnabled();
                    break;
                }
        }
        if (writeLog) {
            AsyncSlf4jLoggerFactory.asyncLog(
                    (AsyncSlf4jLogger) this,
                    (String) format,
                    (int) logLevel,
                    (Throwable) t,
                    (Object[]) arguments,
                    (long) System.currentTimeMillis(),
                    (boolean) true);
        }
    }

    public boolean isTraceEnabled() {
        return this.slf4jLogger.isTraceEnabled();
    }

    public boolean isDebugEnabled() {
        return this.slf4jLogger.isDebugEnabled();
    }

    public boolean isInfoEnabled() {
        return this.slf4jLogger.isInfoEnabled();
    }

    public boolean isWarnEnabled() {
        return this.slf4jLogger.isWarnEnabled();
    }

    public boolean isErrorEnabled() {
        return this.slf4jLogger.isErrorEnabled();
    }
}
