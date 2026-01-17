#!/bin/bash
# Build macOS native binary for the current architecture
# Run this script on a macOS machine

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RUNTIMES_DIR="$SCRIPT_DIR/src/SvgToDxf/runtimes"
TEMP_DIR="/tmp/inkscape-extensions"

# Detect architecture
ARCH=$(uname -m)
if [ "$ARCH" = "x86_64" ]; then
    RID="osx-x64"
elif [ "$ARCH" = "arm64" ]; then
    RID="osx-arm64"
else
    echo "Unsupported architecture: $ARCH"
    exit 1
fi

echo "Building macOS native binary for $RID..."
echo "================================================"

# Check for Python 3
if ! command -v python3 &> /dev/null; then
    echo "Python 3 is required. Install with: brew install python@3.11"
    exit 1
fi

# Install dependencies
echo "Installing Python dependencies..."
pip3 install --quiet nuitka ordered-set lxml numpy tinycss2 cssselect pyparsing

# Clone repository if needed
if [ ! -d "$TEMP_DIR" ]; then
    echo "Cloning Inkscape extensions repository..."
    git clone --quiet https://gitlab.com/inkscape/extensions.git "$TEMP_DIR"
fi

cd "$TEMP_DIR"
git checkout --quiet 5e199c99880caa43412b53164a8ccb6baebe6c99

# Build with Nuitka
echo "Building with Nuitka (this may take several minutes)..."
PYTHONPATH="$TEMP_DIR" python3 -m nuitka \
    --onefile \
    --assume-yes-for-downloads \
    --include-package=inkex \
    --include-data-files=dxf14_header.txt=dxf14_header.txt \
    --include-data-files=dxf14_style.txt=dxf14_style.txt \
    --include-data-files=dxf14_footer.txt=dxf14_footer.txt \
    dxf_outlines.py

# Create output directory and copy binary
mkdir -p "$RUNTIMES_DIR/$RID/native"
cp dxf_outlines.bin "$RUNTIMES_DIR/$RID/native/dxf_outlines-$RID"
chmod +x "$RUNTIMES_DIR/$RID/native/dxf_outlines-$RID"

echo ""
echo "================================================"
echo "Successfully built: $RUNTIMES_DIR/$RID/native/dxf_outlines-$RID"
ls -lh "$RUNTIMES_DIR/$RID/native/dxf_outlines-$RID"
