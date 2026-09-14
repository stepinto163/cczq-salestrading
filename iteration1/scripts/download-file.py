"""Download an HTTPS file with normal certificate verification and a socket timeout."""
import shutil
import sys
import urllib.request
from pathlib import Path

destination = Path(sys.argv[2])
temporary = destination.with_suffix(destination.suffix + '.part')
with urllib.request.urlopen(sys.argv[1], timeout=60) as response:
    with temporary.open('wb') as output:
        shutil.copyfileobj(response, output)
temporary.replace(destination)
