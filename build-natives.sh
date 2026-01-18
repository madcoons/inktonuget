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
mkdir -p "$RUNTIMES_DIR/osx/native"

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

# Build macOS - requires native build on macOS
# Docker cannot build true macOS binaries, so we provide instructions
echo ""
echo "================================================"
echo "Linux binaries built successfully!"
echo ""
echo "For macOS universal binary, run on a macOS machine:"
echo ""
echo "  ./build-natives-macos.sh"
echo ""
echo "Or manually:"
echo ""
echo "  pip3 install nuitka ordered-set lxml numpy tinycss2 cssselect pyparsing"
echo "  git clone https://gitlab.com/inkscape/extensions.git /tmp/inkscape-extensions"
echo "  cd /tmp/inkscape-extensions && git checkout 5e199c99880caa43412b53164a8ccb6baebe6c99"
echo ""
echo "  PYTHONPATH=/tmp/inkscape-extensions python3 -m nuitka \\"
echo "    --onefile --macos-target-arch=universal \\"
echo "    --assume-yes-for-downloads --include-package=inkex \\"
echo "    --include-data-files=dxf14_header.txt=dxf14_header.txt \\"
echo "    --include-data-files=dxf14_style.txt=dxf14_style.txt \\"
echo "    --include-data-files=dxf14_footer.txt=dxf14_footer.txt \\"
echo "    dxf_outlines.py"
echo ""
echo "  cp dxf_outlines.bin $RUNTIMES_DIR/osx/native/dxf_outlines-osx"
echo ""
echo "For Windows binaries, run on a Windows machine:"
echo ""
echo "  pip install nuitka ordered-set lxml numpy tinycss2 cssselect pyparsing"
echo "  git clone https://gitlab.com/inkscape/extensions.git %TEMP%\\inkscape-extensions"
echo "  cd %TEMP%\\inkscape-extensions && git checkout 5e199c99880caa43412b53164a8ccb6baebe6c99"
echo ""
echo "  set PYTHONPATH=%TEMP%\\inkscape-extensions"
echo "  python -m nuitka --onefile --assume-yes-for-downloads --include-package=inkex \\"
echo "    --include-data-files=dxf14_header.txt=dxf14_header.txt \\"
echo "    --include-data-files=dxf14_style.txt=dxf14_style.txt \\"
echo "    --include-data-files=dxf14_footer.txt=dxf14_footer.txt \\"
echo "    --output-filename=dxf_outlines-win-x64.exe dxf_outlines.py"
echo ""
echo "================================================"

# List built binaries
echo ""
echo "Built binaries:"
find "$RUNTIMES_DIR" -type f -name "dxf_outlines-*" -exec ls -lh {} \;
