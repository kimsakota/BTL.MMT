"""Index local course PDFs without copying the slides into the project."""

from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    root = Path(__file__).resolve().parents[1]
    parser.add_argument("--slide-dir", type=Path, default=root.parent / "Slide")
    parser.add_argument("--output-dir", type=Path, default=root / "tmp" / "slides")
    parser.add_argument("--index", type=Path, default=root / "docs" / "tham-khao" / "slides-index.json")
    parser.add_argument("--pdftotext", default=None, help="Explicit Poppler executable")
    args = parser.parse_args()
    converter = args.pdftotext or shutil.which("pdftotext")
    if not converter:
        raise SystemExit("pdftotext is required; use the bundled Poppler runtime or install Poppler.")
    from pypdf import PdfReader

    files = sorted(args.slide_dir.glob("*.pdf"))
    if not files:
        raise SystemExit(f"No PDFs found in {args.slide_dir}")
    args.output_dir.mkdir(parents=True, exist_ok=True)
    args.index.parent.mkdir(parents=True, exist_ok=True)
    manifest = {"source_directory": "../Slide", "page_numbering": "PDF page, 1-based", "files": []}
    catalog = []
    for pdf in files:
        reader = PdfReader(pdf)
        text_file = args.output_dir / f"{pdf.stem}.txt"
        converted = subprocess.run([converter, "-layout", "-", "-"], input=pdf.read_bytes(), capture_output=True, check=True)
        pages = converted.stdout.decode("utf-8").split("\f")
        if pages and not pages[-1].strip():
            pages.pop()
        if len(pages) != len(reader.pages):
            raise RuntimeError(f"Page count mismatch for {pdf.name}: {len(pages)} vs {len(reader.pages)}")
        record = {"file": pdf.name, "pages": len(pages), "sha256": hashlib.sha256(pdf.read_bytes()).hexdigest()}
        manifest["files"].append(record)
        page_records = []
        with text_file.open("w", encoding="utf-8", newline="\n") as out:
            for number, text in enumerate(pages, 1):
                out.write(f"PAGE {number}\n{text.rstrip()}\n\n")
                lines = [line.strip() for line in text.splitlines() if line.strip()]
                page_records.append({"page": number, "heading": " | ".join(lines[:3]), "text": text})
        catalog.append({**record, "page_records": page_records})
        print(f"{pdf.name}: {len(pages)} pages")
    manifest["total_pages"] = sum(item["pages"] for item in manifest["files"])
    args.index.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (args.output_dir / "page-catalog.json").write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Indexed {len(files)} PDFs, {manifest['total_pages']} pages.")


if __name__ == "__main__":
    main()
