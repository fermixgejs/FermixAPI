#!/usr/bin/env bash
# Downloads the EXILED + LabAPI reference DLLs that FermixAPI compiles
# against and places them in ./refs/.
#
# Usage: bash scripts/fetch-references.sh [exiled_tag] [labapi_version]
#
# Defaults to the versions documented in FermixAPI.csproj.
set -euo pipefail

EXILED_TAG="${1:-v9.13.3}"
LABAPI_VERSION="${2:-1.1.6}"

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
REFS_DIR="$REPO_ROOT/refs"
TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

mkdir -p "$REFS_DIR"

echo "Downloading EXILED $EXILED_TAG..."
curl -sSL "https://github.com/ExMod-Team/EXILED/releases/download/$EXILED_TAG/Exiled.tar.gz" \
    -o "$TMP_DIR/Exiled.tar.gz"

tar -xzf "$TMP_DIR/Exiled.tar.gz" -C "$TMP_DIR"

# Flatten DLLs into refs/
find "$TMP_DIR" -name "*.dll" -exec cp -n {} "$REFS_DIR" \;

echo "Downloading LabAPI $LABAPI_VERSION..."
curl -sSL "https://www.nuget.org/api/v2/package/Northwood.LabAPI/$LABAPI_VERSION" \
    -o "$TMP_DIR/LabApi.nupkg"

unzip -o -q "$TMP_DIR/LabApi.nupkg" -d "$TMP_DIR/labapi"
cp -f "$TMP_DIR/labapi/lib/net48/LabApi.dll" "$REFS_DIR/LabApi.dll"

echo
echo "Note: this script does NOT download the SCP:SL game DLLs"
echo "      (Assembly-CSharp.dll, UnityEngine*.dll, Mirror.dll, etc.)."
echo "      Copy those from a SCP:SL install (SCPSL_Data/Managed/) into"
echo "      $REFS_DIR before building."
echo
echo "References available in:"
ls -1 "$REFS_DIR"
