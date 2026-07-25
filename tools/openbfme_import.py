#!/usr/bin/env python3
"""Repository-stable wrapper for the external retail importer package."""

from __future__ import annotations

from pathlib import Path
import sys


IMPORTER_ROOT = Path(__file__).resolve().parents[1] / "importer"
SITE_PACKAGES = Path(sys.executable).resolve().parent / "Lib" / "site-packages"
sys.path[:0] = [str(IMPORTER_ROOT), str(SITE_PACKAGES)]

from openbfme_importer.cli import main  # noqa: E402


if __name__ == "__main__":
    raise SystemExit(main())
