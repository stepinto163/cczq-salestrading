"""Validate one collaboration task/report without running its commands."""

import argparse
import json
from pathlib import Path, PurePosixPath
import re
import sys


ROOT = Path(__file__).resolve().parents[1]
SHA = re.compile(r"[0-9a-f]{40}")
TASK_ID = re.compile(r"[a-z][a-z0-9-]{2,63}")


def require(condition, message):
    if not condition:
        raise ValueError(message)


def strings(value, field, nonempty=False):
    require(isinstance(value, list), f"{field}: expected array")
    require(not nonempty or bool(value), f"{field}: cannot be empty")
    require(all(isinstance(item, str) and item.strip() for item in value),
            f"{field}: expected nonempty strings")


def relative_path(value):
    require(isinstance(value, str) and value.strip(), "Empty path")
    path = PurePosixPath(value)
    require(not path.is_absolute() and ".." not in path.parts
            and ":" not in value and "\\" not in value and value != ".",
            f"Use a repository-relative path: {value}")
    resolved = (ROOT / value).resolve()
    require(ROOT in resolved.parents, f"Path outside repository: {value}")
    return resolved


def allowed(path, patterns):
    return any(path.startswith(item) if item.endswith("/") else path == item
               for item in patterns)


def load(path):
    data = json.loads(path.read_text(encoding="utf-8-sig"))
    require(isinstance(data, dict), f"Expected JSON object: {path}")
    return data


def validate_task(task, task_id):
    require(task.get("protocolVersion") == 2, "Only new v2 tasks are executable")
    require(task.get("taskId") == task_id, "taskId differs from directory name")
    require(task.get("from") == "codex" and task.get("to") == "workbuddy", "Invalid roles")
    require(task.get("state") in {"draft", "ready", "accepted", "cancelled"}, "Invalid task state")
    require(SHA.fullmatch(task.get("baseCommit", "")), "baseCommit must be a full SHA")
    for field in ("sourceBranch", "workBranch", "goal"):
        require(isinstance(task.get(field), str) and task[field].strip(), f"Missing {field}")
    require(task["workBranch"] == "work/" + task_id, "Unexpected task workBranch")
    require(task["sourceBranch"] != task["workBranch"], "Source/work branch must differ")
    for field in ("readFiles", "writePaths", "constraints", "acceptance", "verification"):
        strings(task.get(field), field, nonempty=True)
    for path in task["readFiles"]:
        require(relative_path(path).is_file(), f"readFiles entry missing: {path}")
    for path in task["writePaths"]:
        relative_path(path)
    report = f"handoff/tasks/{task_id}/result.json"
    require(allowed(report, task["writePaths"]), "writePaths must allow result.json")


def validate_result(result, task):
    require(result.get("protocolVersion") == 2, "Invalid result protocolVersion")
    require(result.get("taskId") == task["taskId"], "Result taskId mismatch")
    require(result.get("from") == "workbuddy", "Invalid result author")
    require(result.get("state") in {"running", "blocked", "completed"}, "Invalid result state")
    for field in ("publicationCommit", "testedCommit"):
        value = result.get(field)
        require(value is None or (isinstance(value, str) and SHA.fullmatch(value)),
                f"{field}: expected full SHA or null")
        if result["state"] == "completed":
            require(value is not None, f"Completed result needs {field}")
    for field in ("summary", "nextAction"):
        require(isinstance(result.get(field), str) and result[field].strip(), f"Missing {field}")
    require(isinstance(result.get("environment"), dict) and result["environment"], "Missing environment")
    for field in ("changedFiles", "blockers", "notTested"):
        strings(result.get(field), field)
    for path in result["changedFiles"]:
        relative_path(path)
        require(allowed(path, task["writePaths"]), f"Reported change out of scope: {path}")
    checks = result.get("checks")
    require(isinstance(checks, list) and bool(checks), "Missing checks")
    for check in checks:
        require(isinstance(check, dict), "Check must be an object")
        require(check.get("status") in {"pass", "fail", "not_run"}, "Invalid check status")
        for field in ("name", "command", "evidence"):
            require(isinstance(check.get(field), str) and check[field].strip(), f"Check missing {field}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("task_id")
    parser.add_argument("--result", action="store_true")
    args = parser.parse_args()
    try:
        require(TASK_ID.fullmatch(args.task_id), "Invalid task ID")
        directory = ROOT / "handoff" / "tasks" / args.task_id
        task = load(directory / "task.json")
        validate_task(task, args.task_id)
        if args.result:
            validate_result(load(directory / "result.json"), task)
        print(f"PASS {args.task_id}: format only; task state={task['state']}; no approval implied")
        return 0
    except (OSError, ValueError, TypeError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
