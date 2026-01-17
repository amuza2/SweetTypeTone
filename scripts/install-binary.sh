#!/bin/bash

# SweetTypeTone Binary Installation Script
# Run this after extracting the .tar.gz archive

set -e

INSTALL_DIR="/opt/sweettypetone"
BIN_DIR="/usr/local/bin"
DESKTOP_DIR="/usr/share/applications"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== SweetTypeTone Installation ==="
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "This script must be run as root (use sudo)"
    echo "Usage: sudo ./install.sh"
    exit 1
fi

# Get the actual user (not root when using sudo)
ACTUAL_USER="${SUDO_USER:-$USER}"

# Create installation directory
echo "Installing SweetTypeTone..."
mkdir -p "$INSTALL_DIR"

# Copy all files from current directory
cp -r "$SCRIPT_DIR"/* "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/SweetTypeTone"

# Create launcher script
echo "Creating launcher..."
cat > "$BIN_DIR/sweettypetone" << 'EOF'
#!/bin/bash

# Check if user has input group access
if ! groups | grep -q '\binput\b'; then
    echo "Setting up permissions (one-time setup)..."
    sudo usermod -aG input "$USER"
    echo ""
    echo "============================================"
    echo "Permissions configured!"
    echo ""
    echo "IMPORTANT: You must log out and log back in"
    echo "for the changes to take effect."
    echo ""
    echo "After logging back in, run: sweettypetone"
    echo "============================================"
    exit 0
fi

# Run the application
exec /opt/sweettypetone/SweetTypeTone "$@"
EOF

chmod 755 "$BIN_DIR/sweettypetone"

# Create desktop entry
echo "Creating desktop entry..."
cat > "$DESKTOP_DIR/sweettypetone.desktop" << EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=SweetTypeTone
Comment=Play keyboard sounds as you type
Exec=/usr/local/bin/sweettypetone
Icon=$INSTALL_DIR/icon.png
Terminal=false
Categories=AudioVideo;Audio;
StartupNotify=true
EOF

echo ""
echo "=== Installation Complete ==="
echo ""
echo "To run SweetTypeTone:"
echo "  1. Type 'sweettypetone' in terminal, or"
echo "  2. Find 'SweetTypeTone' in your application menu"
echo ""
echo "On first run, you'll be prompted to setup permissions."
echo "After granting permission, log out and log back in."
echo ""
