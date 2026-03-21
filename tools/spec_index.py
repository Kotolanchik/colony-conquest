#!/usr/bin/env python3
"""
Сканирует spec/*.md, извлекает первый заголовок уровня 1 и метаданные.
Пишет dev/spec_index.json и dev/spec_index.md.
"""
from __future__ import annotations

import json
import re
import sys
from datetime import date
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SPEC_DIR = ROOT / "spec"
OUT_JSON = ROOT / "dev" / "spec_index.json"
OUT_MD = ROOT / "dev" / "spec_index.md"

H1_RE = re.compile(r"^\s*#\s+(.+?)\s*$", re.MULTILINE)


def extract_h1(text: str) -> str | None:
    m = H1_RE.search(text)
    return m.group(1).strip() if m else None


def main() -> int:
    if not SPEC_DIR.is_dir():
        print(f"Missing directory: {SPEC_DIR}", file=sys.stderr)
        return 1

    rows: list[dict] = []
    for path in sorted(SPEC_DIR.glob("*.md")):
        raw = path.read_text(encoding="utf-8", errors="replace")
        lines = raw.count("\n") + (0 if raw.endswith("\n") and raw else 1)
        h1 = extract_h1(raw)
        rel = path.relative_to(ROOT).as_posix()
        rows.append(
            {
                "path": rel,
                "h1": h1,
                "lines": lines,
                "chars": len(raw),
            }
        )

    payload = {
        "version": 1,
        "generated": date.today().isoformat(),
        "root": "spec",
        "files": rows,
    }

    OUT_JSON.parent.mkdir(parents=True, exist_ok=True)
    OUT_JSON.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    md_lines = [
        "# Индекс спецификаций (автогенерация)",
        "",
        f"Сгенерировано: `{payload['generated']}` — `python tools/spec_index.py`",
        "",
        "| Файл | H1 | Строк |",
        "|------|-----|------|",
    ]
    for r in rows:
        h1 = r["h1"] or "—"
        md_lines.append(f"| `{r['path']}` | {h1} | {r['lines']} |")

    md_lines.append("")
    OUT_MD.write_text("\n".join(md_lines), encoding="utf-8")

    print(f"Wrote {OUT_JSON.relative_to(ROOT)} and {OUT_MD.relative_to(ROOT)} ({len(rows)} files)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
