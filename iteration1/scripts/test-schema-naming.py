"""Verify SQL naming changes in isolated schemas, then roll back every test object."""
from pathlib import Path
from urllib.parse import urlparse
import os
import re
import subprocess
import uuid

ROOT = Path(__file__).resolve().parents[1]


def main():
    configuration = {}
    for line in (ROOT / "runtime/quotesvr.local.properties").read_text(encoding="utf-8-sig").splitlines():
        if "=" in line and not line.lstrip().startswith("#"):
            key, value = line.split("=", 1)
            configuration[key.strip()] = value.strip()
    endpoint = urlparse(configuration["db.url"].removeprefix("jdbc:"))
    if endpoint.hostname not in ("127.0.0.1", "localhost"):
        raise RuntimeError("This bounded check only uses the local development database")
    suffix = uuid.uuid4().hex[:12]
    old_schema = "ficc_naming_old_" + suffix
    new_schema = "ficc_naming_new_" + suffix

    def script(name):
        text = (ROOT / "sql" / name).read_text(encoding="utf-8")
        text = text.replace("ficc_demo", old_schema).replace("ficc_salestrading", new_schema)
        return re.sub(r"(?m)^(?:BEGIN|COMMIT);\s*$", "", text)

    migration = script("003-rename-salestrading-schema.sql")
    initialize = script("001-create-tables.sql")
    sql = f"""
BEGIN;
SET LOCAL statement_timeout = '5s';
CREATE SCHEMA {old_schema};
CREATE TABLE {old_schema}.migration_marker (id text PRIMARY KEY, payload jsonb);
INSERT INTO {old_schema}.migration_marker VALUES ('KEEP-ID', '{{"revision":"7"}}');
{migration}
{migration}
{initialize}
{initialize}
DO $check$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '{old_schema}') THEN
        RAISE EXCEPTION 'Old schema still exists';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM {new_schema}.migration_marker
                   WHERE id = 'KEEP-ID' AND payload->>'revision' = '7') THEN
        RAISE EXCEPTION 'Existing data was not preserved';
    END IF;
    IF (SELECT count(*) FROM information_schema.tables
         WHERE table_schema = '{new_schema}' AND table_name <> 'migration_marker') <> 14 THEN
        RAISE EXCEPTION 'Expected 14 service tables';
    END IF;
END
$check$;
ROLLBACK;
SELECT CASE WHEN EXISTS (SELECT 1 FROM pg_namespace
                         WHERE nspname IN ('{old_schema}', '{new_schema}'))
       THEN 'FAIL: test schemas remain' ELSE 'PASS: data preserved, repeatable SQL, 14 tables, rollback complete' END;
"""
    environment = os.environ.copy()
    environment["PGPASSWORD"] = configuration["db.password"]
    environment["PGCLIENTENCODING"] = "UTF8"
    command = [str(ROOT / "runtime/tools/postgresql/pgsql/bin/psql.exe"),
               "-X", "-v", "ON_ERROR_STOP=1", "-h", endpoint.hostname,
               "-p", str(endpoint.port or 5432), "-U", configuration["db.user"],
               "-d", endpoint.path.lstrip("/"), "-q", "-t", "-A"]
    result = subprocess.run(command, input=sql, encoding="utf-8", env=environment,
                            capture_output=True, timeout=20)
    if result.returncode or "FAIL:" in result.stdout:
        raise RuntimeError(result.stderr or result.stdout)
    print(result.stdout.strip())


if __name__ == "__main__":
    main()
