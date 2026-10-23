"""Copy selected VstHelper source bytes and record provenance; never alter the source."""

from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
from urllib.parse import urlsplit, urlunsplit


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--git", default="git")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    source = args.source.resolve()
    target = root / "references" / "VstHelper"
    if target.exists():
        raise SystemExit("Snapshot already exists; review an update as a separate change.")

    def git(*parts: str) -> str:
        return subprocess.check_output([args.git, "-C", str(source), *parts]).decode("utf-8").strip()

    selected = sorted(p for p in source.rglob("*.cs") if not set(p.relative_to(source).parts) & {"bin", "obj", ".git", ".vs"})
    selected += [source / "VstHelper.csproj"]
    head = git("rev-parse", "HEAD")
    status = git("status", "--porcelain=v1")
    remote = git("remote", "get-url", "origin")
    if remote.startswith(("http://", "https://")):
        url = urlsplit(remote)
        remote = urlunsplit((url.scheme, url.hostname or "", url.path, "", ""))
    records = []
    for file in selected:
        relative = file.relative_to(source)
        destination = target / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(file, destination)
        records.append({"path": relative.as_posix(), "sha256": hashlib.sha256(destination.read_bytes()).hexdigest(), "bytes": destination.stat().st_size})
    manifest = {
        "captured_on": "2026-10-23",
        "source_remote": remote,
        "source_head": head,
        "source_working_tree_dirty": bool(status),
        "snapshot_kind": "working-tree bytes, including local modifications; not a clean copy of source_head",
        "original_source": str(source),
        "excluded": ["Git metadata", "build output", "IDE caches", "personal settings", "unrelated documents"],
        "files": records,
    }
    (target / "SNAPSHOT.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Copied {len(records)} source/project files; source working tree dirty: {bool(status)}")


if __name__ == "__main__":
    main()
