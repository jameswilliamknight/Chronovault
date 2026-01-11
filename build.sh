#!/usr/bin/env bash

set -e

# Chronovault - Build & Package Script
# Usage: ./build.sh [version] [output-path]
# Example: ./build.sh 0.1.0
# Example: ./build.sh 0.1.0 /mnt/c/builds/

VERSION="${1:-0.0.1}"
OUTPUT_PATH="${2:-}"
SCRIPT_DIR="$(dirname "$(realpath "$0")")"

echo "🔨 Chronovault - Build & Package"
echo "======================================"
echo "📁 Project directory: ${SCRIPT_DIR}"
echo "🏷️  Version: ${VERSION}"
if [ -n "${OUTPUT_PATH}" ]; then
    echo "📤 Output path: ${OUTPUT_PATH}"
fi
echo ""

# Step 1: Restore and build the solution
echo "📦 Restoring NuGet packages..."
dotnet restore "${SCRIPT_DIR}/Chronovault.sln" --verbosity minimal

echo ""
echo "🏗️  Building solution..."
dotnet build "${SCRIPT_DIR}/Chronovault.sln" --configuration Release --no-restore --verbosity minimal

# Step 2: Run the packager (already built as part of solution above)
echo ""
echo "📦 Running packager..."
if [ -n "${OUTPUT_PATH}" ]; then
    dotnet run --project "${SCRIPT_DIR}/src/Chronovault.Packager/Chronovault.Packager.csproj" \
        --configuration Release \
        --no-build \
        -- "${VERSION}" "${OUTPUT_PATH}"
else
    dotnet run --project "${SCRIPT_DIR}/src/Chronovault.Packager/Chronovault.Packager.csproj" \
        --configuration Release \
        --no-build \
        -- "${VERSION}"
fi

echo ""
echo "✅ Build complete!"
