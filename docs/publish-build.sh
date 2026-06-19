#!/usr/bin/env bash
# Publish the current ./web build as a phase snapshot the codex tracker links to.
#   bash docs/publish-build.sh phase1
set -euo pipefail
NAME="${1:?usage: publish-build.sh <phaseN>}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT/web"
npm run build
mkdir -p "$ROOT/docs/builds"
cp "$ROOT/web/dist/index.html" "$ROOT/docs/builds/$NAME.html"
echo "published docs/builds/$NAME.html ($(wc -c < "$ROOT/docs/builds/$NAME.html") bytes)"
