#!/usr/bin/env python3
"""Fail a quality gate only when dotnet's JSON report includes critical CVEs."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Iterable


SEVERITY_ORDER = {"low": 1, "moderate": 2, "medium": 2, "high": 3, "critical": 4}


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Evaluate dotnet list package --vulnerable JSON output."
    )
    parser.add_argument("--input", required=True, type=Path)
    parser.add_argument(
        "--block-at",
        choices=("low", "medium", "high", "critical"),
        default="critical",
        help="Lowest severity that fails the command (default: critical).",
    )
    return parser.parse_args()


def package_entries(value: Any) -> Iterable[dict[str, Any]]:
    if isinstance(value, dict):
        vulnerabilities = value.get("vulnerabilities")
        if isinstance(vulnerabilities, list):
            yield value
        for child in value.values():
            yield from package_entries(child)
    elif isinstance(value, list):
        for child in value:
            yield from package_entries(child)


def normalise_severity(value: Any) -> str:
    return str(value or "unknown").strip().lower()


def main() -> int:
    arguments = parse_arguments()
    try:
        report = json.loads(arguments.input.read_text(encoding="utf-8"))
    except FileNotFoundError:
        print(f"::error::Vulnerability report does not exist: {arguments.input}", file=sys.stderr)
        return 2
    except json.JSONDecodeError as error:
        print(f"::error::Invalid vulnerability report: {error}", file=sys.stderr)
        return 2

    threshold = SEVERITY_ORDER[arguments.block_at]
    counts: dict[str, int] = {}
    blocking: list[tuple[str, str, str, str]] = []

    for package in package_entries(report):
        package_id = str(package.get("id", package.get("name", "unknown package")))
        package_version = str(
            package.get("resolvedVersion", package.get("version", "unknown version"))
        )
        vulnerabilities = package.get("vulnerabilities", [])
        if not isinstance(vulnerabilities, list):
            continue

        for vulnerability in vulnerabilities:
            if not isinstance(vulnerability, dict):
                continue
            severity = normalise_severity(vulnerability.get("severity"))
            counts[severity] = counts.get(severity, 0) + 1
            advisory = str(
                vulnerability.get(
                    "advisoryurl", vulnerability.get("advisoryUrl", "unknown advisory")
                )
            )
            if SEVERITY_ORDER.get(severity, 0) >= threshold:
                blocking.append((package_id, package_version, severity, advisory))

    summary = ", ".join(f"{severity}={count}" for severity, count in sorted(counts.items()))
    print(f".NET dependency vulnerabilities: {summary or 'none'}.")
    for package_id, version, severity, advisory in blocking:
        print(
            f"::error::{package_id}@{version} has a {severity} vulnerability: {advisory}"
        )

    return 1 if blocking else 0


if __name__ == "__main__":
    sys.exit(main())
