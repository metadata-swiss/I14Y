#!/usr/bin/env python3
"""Evaluate SARIF findings against a small, explicit quality-gate policy."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Iterable


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Fail when SARIF findings match the configured policy."
    )
    parser.add_argument(
        "--input",
        nargs="+",
        required=True,
        metavar="SARIF",
        help="One or more SARIF reports to evaluate.",
    )
    parser.add_argument(
        "--minimum-security-severity",
        type=float,
        help="Fail for results whose SARIF security-severity is at least this value.",
    )
    parser.add_argument(
        "--warning-minimum-security-severity",
        type=float,
        help="Emit a warning for non-blocking results at or above this severity.",
    )
    parser.add_argument(
        "--fail-on-any-result",
        action="store_true",
        help="Fail when any SARIF result is present.",
    )
    parser.add_argument(
        "--label",
        default="SARIF",
        help="Human-readable name used in the log output.",
    )
    arguments = parser.parse_args()

    if not arguments.fail_on_any_result and arguments.minimum_security_severity is None:
        parser.error(
            "Specify --fail-on-any-result or --minimum-security-severity."
        )

    return arguments


def read_sarif(path: Path) -> dict[str, Any]:
    try:
        with path.open(encoding="utf-8") as report:
            document = json.load(report)
    except FileNotFoundError:
        raise ValueError(f"SARIF report does not exist: {path}") from None
    except json.JSONDecodeError as error:
        raise ValueError(f"Invalid SARIF JSON in {path}: {error}") from error

    if not isinstance(document, dict) or not isinstance(document.get("runs"), list):
        raise ValueError(f"{path} is not a SARIF document with a runs array.")

    return document


def rule_properties(run: dict[str, Any], rule_id: str) -> dict[str, Any]:
    driver = run.get("tool", {}).get("driver", {})
    rules = driver.get("rules", [])
    if not isinstance(rules, list):
        return {}

    for rule in rules:
        if isinstance(rule, dict) and rule.get("id") == rule_id:
            properties = rule.get("properties", {})
            return properties if isinstance(properties, dict) else {}
    return {}


def security_severity(result: dict[str, Any], run: dict[str, Any]) -> float | None:
    properties: dict[str, Any] = {}
    result_properties = result.get("properties", {})
    if isinstance(result_properties, dict):
        properties.update(result_properties)
    properties = rule_properties(run, str(result.get("ruleId", ""))) | properties

    for key in ("security-severity", "securitySeverity"):
        value = properties.get(key)
        if value is None:
            continue
        try:
            return float(value)
        except (TypeError, ValueError):
            continue
    return None


def message_text(result: dict[str, Any]) -> str:
    message = result.get("message", {})
    if isinstance(message, dict):
        return str(message.get("text", result.get("ruleId", "Unknown finding")))
    return str(message)


def findings(documents: Iterable[dict[str, Any]]) -> Iterable[tuple[dict[str, Any], dict[str, Any]]]:
    for document in documents:
        for run in document["runs"]:
            if not isinstance(run, dict):
                continue
            results = run.get("results", [])
            if not isinstance(results, list):
                continue
            for result in results:
                if isinstance(result, dict):
                    yield result, run


def main() -> int:
    arguments = parse_arguments()

    try:
        documents = [read_sarif(Path(raw_path)) for raw_path in arguments.input]
    except ValueError as error:
        print(f"::error::{error}", file=sys.stderr)
        return 2

    total = 0
    blocking: list[tuple[dict[str, Any], float | None]] = []
    warnings: list[tuple[dict[str, Any], float]] = []
    for result, run in findings(documents):
        total += 1
        severity = security_severity(result, run)
        if arguments.fail_on_any_result or (
            arguments.minimum_security_severity is not None
            and severity is not None
            and severity >= arguments.minimum_security_severity
        ):
            blocking.append((result, severity))
        elif (
            arguments.warning_minimum_security_severity is not None
            and severity is not None
            and severity >= arguments.warning_minimum_security_severity
        ):
            warnings.append((result, severity))

    print(
        f"{arguments.label}: {total} finding(s), {len(blocking)} blocking, "
        f"{len(warnings)} warning(s)."
    )
    for result, severity in warnings:
        print(
            f"::warning title=High Trivy image vulnerability::[{result.get('ruleId', 'unknown')}, "
            f"security-severity={severity:g}] {message_text(result)}"
        )
    for result, severity in blocking:
        severity_text = f", security-severity={severity:g}" if severity is not None else ""
        print(
            f"::error::[{result.get('ruleId', 'unknown')}{severity_text}] "
            f"{message_text(result)}"
        )

    return 1 if blocking else 0


if __name__ == "__main__":
    sys.exit(main())
