#!/usr/bin/env python3
"""Check newly added GitHub Dependency Graph entries against an SPDX deny-list."""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from pathlib import Path
from typing import Any
from urllib.error import HTTPError, URLError
from urllib.parse import quote
from urllib.request import Request, urlopen


SARIF_SCHEMA = "https://json.schemastore.org/sarif-2.1.0.json"
API_VERSION = "2026-03-10"


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Fail policy evaluator for newly added dependency licenses."
    )
    parser.add_argument("--repository", default=os.getenv("GITHUB_REPOSITORY"))
    parser.add_argument("--base-sha", required=True)
    parser.add_argument("--head-sha", required=True)
    parser.add_argument(
        "--deny-licenses",
        required=True,
        help="Comma-separated SPDX identifiers to deny.",
    )
    parser.add_argument("--output", required=True, type=Path)
    arguments = parser.parse_args()

    if not arguments.repository or "/" not in arguments.repository:
        parser.error("--repository must be in the form OWNER/REPOSITORY.")
    if not os.getenv("GITHUB_TOKEN"):
        parser.error("GITHUB_TOKEN must be available to call the Dependency Graph API.")
    return arguments


def request_dependency_changes(repository: str, base_sha: str, head_sha: str) -> list[dict[str, Any]]:
    base_head = quote(f"{base_sha}...{head_sha}", safe="")
    url = f"https://api.github.com/repos/{repository}/dependency-graph/compare/{base_head}"
    request = Request(
        url,
        headers={
            "Accept": "application/vnd.github+json",
            "Authorization": f"Bearer {os.environ['GITHUB_TOKEN']}",
            "User-Agent": "i14y-quality-gates-license-check",
            "X-GitHub-Api-Version": API_VERSION,
        },
    )

    try:
        with urlopen(request, timeout=30) as response:
            payload = json.load(response)
    except HTTPError as error:
        body = error.read().decode("utf-8", errors="replace")
        raise RuntimeError(
            f"Dependency Graph API returned HTTP {error.code}: {body}"
        ) from error
    except URLError as error:
        raise RuntimeError(f"Dependency Graph API could not be reached: {error}") from error

    if not isinstance(payload, list) or not all(isinstance(item, dict) for item in payload):
        raise RuntimeError("Dependency Graph API returned an unexpected response.")
    return payload


def denied_identifiers(raw_identifiers: str) -> list[str]:
    values = [value.strip().upper() for value in raw_identifiers.split(",") if value.strip()]
    if not values:
        raise ValueError("The license deny-list is empty.")
    return values


def denied_matches(license_expression: str, denied: list[str]) -> list[str]:
    expression = license_expression.upper()
    matches: list[str] = []
    for identifier in denied:
        # GitHub can return the SPDX base identifier, -only, -or-later or legacy + syntax.
        pattern = rf"(?<![A-Z0-9.-]){re.escape(identifier)}(?:-ONLY|-OR-LATER|\+)?(?![A-Z0-9.-])"
        if re.search(pattern, expression):
            matches.append(identifier)
    return matches


def dependency_scope(manifest: str) -> str:
    normalised_manifest = manifest.replace("\\", "/")
    for prefix, scope in (
        ("frontend/admin-ui/", "Admin UI"),
        ("frontend/public-ui/", "Public UI"),
        ("backend/src/Core/", "Core"),
        ("backend/src/Admin/", "Admin"),
        ("backend/src/Partner/", "Partner"),
        ("backend/src/Iri/", "IRI"),
    ):
        if normalised_manifest.startswith(prefix):
            return scope
    return normalised_manifest


def sarif_result(change: dict[str, Any], denied_license: str) -> dict[str, Any]:
    package_name = str(change.get("name", "unknown package"))
    package_version = str(change.get("version", "unknown version"))
    declared_license = str(change.get("license", ""))
    manifest = str(change.get("manifest", "unknown manifest"))
    scope = dependency_scope(manifest)
    return {
        "ruleId": f"license/{denied_license}",
        "level": "error",
        "message": {
            "text": (
                f"[{scope}] {package_name}@{package_version} declares '{declared_license}', "
                f"which matches denied license {denied_license}."
            )
        },
        "locations": [
            {
                "physicalLocation": {
                    "artifactLocation": {"uri": manifest},
                }
            }
        ],
        "properties": {
            "package": package_name,
            "version": package_version,
            "ecosystem": change.get("ecosystem"),
            "declared-license": declared_license,
        },
    }


def create_sarif(results: list[dict[str, Any]], denied: list[str]) -> dict[str, Any]:
    return {
        "$schema": SARIF_SCHEMA,
        "version": "2.1.0",
        "runs": [
            {
                "tool": {
                    "driver": {
                        "name": "I14Y Dependency License Gate",
                        "informationUri": "https://docs.github.com/rest/dependency-graph/dependency-review",
                        "rules": [
                            {
                                "id": f"license/{identifier}",
                                "shortDescription": {
                                    "text": f"Denied dependency license: {identifier}"
                                },
                                "defaultConfiguration": {"level": "error"},
                            }
                            for identifier in denied
                        ],
                    }
                },
                "results": results,
            }
        ],
    }


def main() -> int:
    arguments = parse_arguments()
    try:
        denied = denied_identifiers(arguments.deny_licenses)
        changes = request_dependency_changes(
            arguments.repository, arguments.base_sha, arguments.head_sha
        )
    except (RuntimeError, ValueError) as error:
        print(f"::error::{error}", file=sys.stderr)
        return 2

    results: list[dict[str, Any]] = []
    for change in changes:
        if change.get("change_type") != "added":
            continue
        license_expression = change.get("license")
        if not isinstance(license_expression, str) or not license_expression.strip():
            continue
        for identifier in denied_matches(license_expression, denied):
            results.append(sarif_result(change, identifier))

    arguments.output.parent.mkdir(parents=True, exist_ok=True)
    arguments.output.write_text(
        json.dumps(create_sarif(results, denied), indent=2) + "\n", encoding="utf-8"
    )
    print(
        f"Checked {sum(change.get('change_type') == 'added' for change in changes)} newly "
        f"added dependency entries; found {len(results)} denied license match(es)."
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
