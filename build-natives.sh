#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RUNTIMES_DIR="$SCRIPT_DIR/src/SvgToDxf/runtimes"

echo "Building native binaries for all platforms..."
echo "================================================"

# Create runtime directories
mkdir -p "$RUNTIMES_DIR/linux-x64/native"
mkdir -p "$RUNTIMES_DIR/linux-arm64/native"
mkdir -p "$RUNTIMES_DIR/linux-musl-x64/native"
mkdir -p "$RUNTIMES_DIR/linux-musl-arm64/native"
mkdir -p "$RUNTIMES_DIR/osx-x64/native"
mkdir -p "$RUNTIMES_DIR/osx-arm64/native"

# Build Linux glibc x64
echo ""
echo "Building linux-x64 (glibc)..."
docker build --platform linux/amd64 -f "$SCRIPT_DIR/Dockerfile.glibc" -o "$RUNTIMES_DIR/linux-x64/native" "$SCRIPT_DIR"

# Build Linux glibc arm64
echo ""
echo "Building linux-arm64 (glibc)..."
docker build --platform linux/arm64 -f "$SCRIPT_DIR/Dockerfile.glibc" -o "$RUNTIMES_DIR/linux-arm64/native" "$SCRIPT_DIR"

# Build Linux musl x64
echo ""
echo "Building linux-musl-x64..."
docker build --platform linux/amd64 -f "$SCRIPT_DIR/Dockerfile.musl" -o "$RUNTIMES_DIR/linux-musl-x64/native" "$SCRIPT_DIR"

# Build Linux musl arm64
echo ""
echo "Building linux-musl-arm64..."
docker build --platform linux/arm64 -f "$SCRIPT_DIR/Dockerfile.musl" -o "$RUNTIMES_DIR/linux-musl-arm64/native" "$SCRIPT_DIR"

# Build macOS - requires native build on macOS or cross-compilation
# Docker cannot build true macOS binaries, so we provide instructions
echo ""
echo "================================================"
echo "Linux binaries built successfully!"
echo ""
echo "For macOS binaries, run the following on a macOS machine:"
echo ""
echo "  # Install dependencies"
echo "  brew install python@3.11"
echo "  pip3 install nuitka ordered-set lxml numpy tinycss2 cssselect pyparsing"
echo ""
echo "  # Clone and build"
echo "  git clone https://gitlab.com/inkscape/extensions.git /tmp/inkscape-extensions"
echo "  cd /tmp/inkscape-extensions"
echo "  git checkout 5e199c99880caa43412b53164a8ccb6baebe6c99"
echo ""
echo "  # Build for current architecture"
echo "  PYTHONPATH=/tmp/inkscape-extensions python3 -m nuitka \\"
echo "    --onefile \\"
echo "    --assume-yes-for-downloads \\"
echo "    --include-package=inkex \\"
echo "    --include-data-files=dxf14_header.txt=dxf14_header.txt \\"
echo "    --include-data-files=dxf14_style.txt=dxf14_style.txt \\"
echo "    --include-data-files=dxf14_footer.txt=dxf14_footer.txt \\"
echo "    dxf_outlines.py"
echo ""
echo "  # Copy to runtimes folder (adjust for your architecture)"
echo "  # For Apple Silicon (arm64):"
echo "  cp dxf_outlines.bin $RUNTIMES_DIR/osx-arm64/native/dxf_outlines-osx-arm64"
echo ""
echo "  # For Intel (x64):"
echo "  cp dxf_outlines.bin $RUNTIMES_DIR/osx-x64/native/dxf_outlines-osx-x64"
echo ""
echo "================================================"

# List built binaries
echo ""
echo "Built binaries:"
find "$RUNTIMES_DIR" -type f -name "dxf_outlines-*" -exec ls -lh {} \;
