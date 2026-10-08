from pathlib import Path
import re, shutil, json, time, os

ROOT = Path.cwd()
SOURCE = Path(os.environ["FICC_FRAMEWORK_ATS_ROOT"])
TARGET = ROOT / "iteration1/runtime/framework-live" / time.strftime("%Y%m%d-%H%M%S")
TARGET.mkdir(parents=True)


def write(path, text):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")


def update(text, key, value):
    pattern = r"(?m)^" + re.escape(key) + r"=.*$"
    return (
        re.sub(pattern, lambda m: key + "=" + value, text)
        if re.search(pattern, text)
        else text + "\n" + key + "=" + value + "\n"
    )


shutil.copytree(SOURCE / "control", TARGET / "control")
auth = TARGET / "control/QtIntakeSvrAuth.properties"
if auth.is_file():
    credentials = auth.read_text(encoding="utf-8-sig")
    credentials = update(credentials, "fix.username", "framework-loopback")
    credentials = update(credentials, "fix.password", "framework-loopback")
    write(auth, credentials)
(TARGET / "log").mkdir()
services = {
    "QuoteSvr": 32980,
    "NotificationSvr": 32981,
    "CRMSvr": 32983,
    "ParserSvr": 32943,
    "QtIntakeSvr": 32982,
    "LoginSvr": 32931,
    "GW": 32900,
}
ats = (TARGET / "control/ATSConfig.ini").read_text(encoding="utf-8-sig")
for name, port in services.items():
    ats = update(
        ats,
        "SERVER." + name + ".ServiceName",
        "FrameworkTest" + TARGET.name.replace("-", "") + name,
    )
    ats = update(ats, "SERVER." + name + ".Host", "127.0.0.1:" + str(port))
    ats = update(ats, "SERVER." + name + ".Port", str(port))
    standby = os.environ.get("FICC_FRAMEWORK_STANDBY_TEST") == "1" and name in (
        "QuoteSvr",
        "NotificationSvr",
    )
    ats = update(ats, "SERVER." + name + ".RegType", "2" if standby else "1")
# The long-running local BaseSvr has lost its old-framework registry node.
# Exercise its unchanged DPS endpoint without modifying the user's registry or process.
ats = update(ats, "SERVER.BaseSvr.RegisterEnable", "0")
write(TARGET / "control/ATSConfig.ini", ats)
# No credentials are displayed or committed. Local settings are copied into this ignored test folder.
schema = "ficc_framework_" + str(int(time.time()))
for name in services:
    old = SOURCE / "stc" / name
    new = TARGET / "stc" / name
    shutil.copytree(old / "control", new / "control")
    if (old / "config").exists():
        shutil.copytree(old / "config", new / "config")
    if name == "ParserSvr":
        for library in old.glob("*.dll"):
            shutil.copy2(library, new / library.name)
        shutil.copytree(old / "data", new / "data")
        hanlp = (
            ROOT / "iteration1/cgws-ficc-parsersvr/src/main/resources/hanlp.properties"
        ).read_text(encoding="utf-8-sig")
        write(
            new / "config/hanlp.properties", update(hanlp, "root", new.as_posix() + "/")
        )
    for path in new.rglob("*"):
        if path.is_file() and path.suffix in (".ini", ".properties", ".xml"):
            content = path.read_text(encoding="utf-8-sig")
            content = content.replace(
                str(SOURCE).replace("\\", "/"), str(TARGET).replace("\\", "/")
            )
            content = content.replace(str(SOURCE), str(TARGET))
            if name in ("QuoteSvr", "NotificationSvr") and "db.schema=" in content:
                content = update(content, "db.schema", schema)
            if "log4j.appender." in content:
                content = re.sub(
                    r"(?m)^(log4j.appender.\w+.File)=.*$",
                    lambda m: m[1]
                    + "="
                    + str(
                        TARGET / "log" / (name + "-" + m[1].split(".")[2] + ".log")
                    ).replace("\\", "/"),
                    content,
                )
            if name == "GW":
                content = content.replace("28600", "32900").replace("28601", "32901")
            write(path, content)
    (new / "log").mkdir(exist_ok=True)
    (new / "logs").mkdir(exist_ok=True)
    if name == "QtIntakeSvr":
        write(
            new / "config/qtrade-fix.cfg",
            """[default]
ConnectionType=initiator
StartTime=00:00:00
EndTime=00:00:00
HeartBtInt=30
ReconnectInterval=1
UseDataDictionary=N
ValidateIncomingMessage=N
ValidateUserDefinedFields=N
PersistMessages=N
FileStorePath=./data/fix-store
ResetOnLogon=Y
[session]
BeginString=FIX.4.4
SenderCompID=FRAMEWORK_CLIENT
TargetCompID=FRAMEWORK_SERVER
SocketConnectHost=127.0.0.1
SocketConnectPort=32984
""",
        )
        config = new / "config/service.properties"
        content = config.read_text(encoding="utf-8")
        content = update(content, "fix.settings", "./config/qtrade-fix.cfg")
        content = update(
            content,
            "owner.mapFile",
            str(TARGET / "control/FrameworkOwnerMap.properties").replace("\\", "/"),
        )
        content = update(content, "owner.defaultUserId", "")
        content = update(content, "fix.replayOnStartup", "false")
        write(config, content)
        write(
            TARGET / "control/FrameworkOwnerMap.properties",
            "# Filled by the authenticated test probe.\n",
        )
write(ROOT / "iteration1/runtime/framework-live-current.txt", str(TARGET))
write(
    TARGET / "test-settings.json",
    json.dumps({"services": services, "schema": schema}, indent=2),
)
print(TARGET)
print(
    "Isolated service names, ports, log files and quote/notification schema prepared."
)
