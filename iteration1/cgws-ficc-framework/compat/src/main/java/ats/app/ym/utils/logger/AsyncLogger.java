// Recovered compatibility source; provenance: recovered-support.json.
package ats.app.ym.utils.logger;

import org.slf4j.Logger;

import java.text.SimpleDateFormat;
import java.util.Date;

public class AsyncLogger {
    private static boolean writeTime = false;

    public static boolean isWriteTime() {
        return writeTime;
    }

    public static void setWriteTime(boolean writeTime) {
        AsyncLogger.writeTime = writeTime;
    }

    public static void debug(
            Logger logger, String format, Throwable t, Object[] arguments, long time) {
        if (writeTime) {
            String timeString = AsyncLogger.formatTime(time);
            format = String.valueOf(timeString) + format;
        }
        if (arguments != null) {
            if (arguments.length == 0) {
                logger.debug(format);
            } else {
                logger.debug(format, arguments);
            }
        } else {
            logger.debug(format, t);
        }
    }

    private static String formatTime(long time) {
        SimpleDateFormat sdf = new SimpleDateFormat("HH:mm:ss.SSS ");
        return sdf.format(new Date(time));
    }

    public static void info(
            Logger logger, String format, Throwable t, Object[] arguments, long time) {
        if (writeTime) {
            String timeString = AsyncLogger.formatTime(time);
            format = String.valueOf(timeString) + format;
        }
        if (arguments != null) {
            if (arguments.length == 0) {
                logger.info(format);
            } else {
                logger.info(format, arguments);
            }
        } else {
            logger.info(format, t);
        }
    }

    public static void warn(
            Logger logger, String format, Throwable t, Object[] arguments, long time) {
        if (writeTime) {
            String timeString = AsyncLogger.formatTime(time);
            format = String.valueOf(timeString) + format;
        }
        if (arguments != null) {
            if (arguments.length == 0) {
                logger.warn(format);
            } else {
                logger.warn(format, arguments);
            }
        } else {
            logger.warn(format, t);
        }
    }

    public static void error(
            Logger logger, String format, Throwable t, Object[] arguments, long time) {
        if (writeTime) {
            String timeString = AsyncLogger.formatTime(time);
            format = String.valueOf(timeString) + format;
        }
        if (arguments != null) {
            if (arguments.length == 0) {
                logger.error(format);
            } else {
                logger.error(format, arguments);
            }
        } else {
            logger.error(format, t);
        }
    }
}
