package sc.app.stc.parser.utils;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.BufferedReader;
import java.io.File;
import java.io.InputStreamReader;
import java.util.Map;
import java.util.concurrent.TimeUnit;

/**
 * CMD命令行工具类
 *
 * @author enhao.dong
 */
public class CmdUtils {
    private static final Logger logger = LoggerFactory.getLogger(CmdUtils.class);

    public static boolean executeCommand(String command) {
        try {
            // 执行命令
            Process process = Runtime.getRuntime().exec(command);
            // 读取命令输出结果
            BufferedReader reader = new BufferedReader(new InputStreamReader(process.getInputStream(), "GBK"));
            String line;
            while ((line = reader.readLine()) != null) {
                logger.info("execute command: {}", line);
            }
            // 读取子进程的错误输出流
            BufferedReader errorReader = new BufferedReader(new InputStreamReader(process.getErrorStream(), "GBK"));
            String errorLine;
            while ((errorLine = errorReader.readLine()) != null) {
                logger.warn("execute command fail: {}", errorLine);
            }
            // 等待命令执行完成
            boolean exitCode = process.waitFor(30L, TimeUnit.SECONDS);
            logger.info("execute command end, result: {}", exitCode);
            return exitCode;
        } catch (Exception e) {
            e.printStackTrace();
        }
        return false;
    }
}
