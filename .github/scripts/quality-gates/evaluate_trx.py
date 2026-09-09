#!/usr/bin/env python3
"""Emit GitHub annotations for failed VSTest TRX unit tests."""

from __future__ import annotations

import argparse
import re
import sys
import xml.etree.ElementTree as ElementTree
from pathlib import Path
from typing import Iterable


SOURCE_LOCATION = re.compile(
    r"\bin\s+(?P<path>.+?\.(?:cs|fs|vb)):line\s+(?P<line>\d+)"
)


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Emit GitHub workflow annotations for failed TRX test results."
    )
    parser.add_argument(
        "--input",
        nargs="+",
        required=True,
        type=Path,
        metavar="TRX",
        help="One or more VSTest TRX result files.",
    )
    return parser.parse_args()


def local_name(element: ElementTree.Element) -> str:
    return element.tag.rsplit("}", 1)[-1]


def first_descendant_text(element: ElementTree.Element, name: str) -> str:
    for child in element.iter():
        if local_name(child) != name:
            continue
        text = "".join(child.itertext()).strip()
        if text:
            return text
    return ""


def workflow_escape(value: str) -> str:
    return value.replace("%", "%25").replace("\r", "%0D").replace("\n", "%0A")


def source_location(stack_trace: str) -> tuple[str, int] | None:
    match = SOURCE_LOCATION.search(stack_trace)
    if not match:
        return None

    source_path = match.group("path").replace("\\", "/")
    for repository_root in ("backend/", "frontend/"):
        start = source_path.lower().find(repository_root)
        if start >= 0:
            return source_path[start:], int(match.group("line"))
    return None


def emit_failure(project_name: str, result: ElementTree.Element) -> None:
    test_name = result.get("testName", "unknown test")
    error_info = next(
        (
            child
            for child in result.iter()
            if local_name(child) == "ErrorInfo"
        ),
        None,
    )
    message = (
        first_descendant_text(error_info, "Message")
        if error_info is not None
        else ""
    )
    stack_trace = (
        first_descendant_text(error_info, "StackTrace")
        if error_info is not None
        else ""
    )
    if not message:
        message = first_descendant_text(result, "StdOut") or "Test failed."

    details = f"{test_name}: {message}"
    title = f"Failed unit test ({project_name})"
    location = source_location(stack_trace)
    if location is None:
        print(f"::error title={title}::{workflow_escape(details)}")
        return

    file_name, line_number = location
    print(
        f"::error file={workflow_escape(file_name)},line={line_number},"
        f"title={title}::{workflow_escape(details)}"
    )


def failed_results(report: Path) -> Iterable[ElementTree.Element]:
    root = ElementTree.parse(report).getroot()
    for element in root.iter():
        if (
            local_name(element) == "UnitTestResult"
            and element.get("outcome") == "Failed"
        ):
            yield element


def main() -> int:
    arguments = parse_arguments()
    failures = 0

    for report in arguments.input:
        try:
            results = list(failed_results(report))
        except (ElementTree.ParseError, OSError) as error:
            print(
                f"::error title=Unit test results unavailable::"
                f"Could not read {report.name}: {workflow_escape(str(error))}",
                file=sys.stderr,
            )
            continue

        for result in results:
            failures += 1
            emit_failure(report.stem, result)

    print(f"Unit-test result annotations: {failures} failed test(s).")
    return 0


if __name__ == "__main__":
    sys.exit(main())

